using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading;
using Metasia.Core.Sounds;
using Miniaudio.Net;

namespace Metasia.Editor.Services;

public sealed class MiniaudioService : IAudioService
{
    private MiniaudioEngine _engine;
    private MiniaudioStreamingSound _stream;
    private uint _sampleRate;
    private uint _channels;
    private AudioFormat _format;
    private readonly uint _bufferCapacityInFrames;
    private readonly object _gate = new();
    private bool _disposed;

    public MiniaudioService()
        : this(sampleRate: 44100, channels: 2)
    {
    }

    public MiniaudioService(uint sampleRate, uint channels, uint bufferCapacityInFrames = 8192)
    {
        if (sampleRate == 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (channels == 0) throw new ArgumentOutOfRangeException(nameof(channels));
        if (bufferCapacityInFrames == 0) throw new ArgumentOutOfRangeException(nameof(bufferCapacityInFrames));

        _sampleRate = sampleRate;
        _channels = channels;
        _format = new AudioFormat((int)sampleRate, (int)channels);
        _bufferCapacityInFrames = bufferCapacityInFrames;

        (_engine, _stream) = CreateResources(sampleRate, channels, bufferCapacityInFrames);
    }

    public IAudioFormat Format => _format;

    /// <inheritdoc />
    public bool EnsureFormat(int sampleRate, int channelCount)
    {
        if (sampleRate <= 0 || channelCount <= 0)
        {
            return false;
        }

        lock (_gate)
        {
            ThrowIfDisposed();

            if (_sampleRate == (uint)sampleRate && _channels == (uint)channelCount)
            {
                return true;
            }

            try
            {
                ReplaceResources((uint)sampleRate, (uint)channelCount);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MiniaudioService の出力フォーマット切り替えに失敗: {ex.Message}");
            }

            if (channelCount == 2)
            {
                return false;
            }

            // マルチチャンネルを受け付けない環境ではステレオへ戻して再生を継続する
            try
            {
                ReplaceResources((uint)sampleRate, 2);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MiniaudioService のステレオへの復帰に失敗: {ex.Message}");
            }

            return false;
        }
    }

    private void ReplaceResources(uint sampleRate, uint channels)
    {
        (MiniaudioEngine engine, MiniaudioStreamingSound stream) = CreateResources(sampleRate, channels, _bufferCapacityInFrames);

        MiniaudioEngine oldEngine = _engine;
        MiniaudioStreamingSound oldStream = _stream;

        _engine = engine;
        _stream = stream;
        _sampleRate = sampleRate;
        _channels = channels;
        _format = new AudioFormat((int)sampleRate, (int)channels);

        DisposeResources(oldStream, oldEngine);
    }

    private static (MiniaudioEngine Engine, MiniaudioStreamingSound Stream) CreateResources(
        uint sampleRate,
        uint channels,
        uint bufferCapacityInFrames)
    {
        MiniaudioEngine? engine = null;
        try
        {
            engine = MiniaudioEngine.Create(new MiniaudioEngineOptions
            {
                SampleRate = sampleRate,
                Channels = channels,
            });
            engine.Start();

            MiniaudioStreamingSound stream = engine.CreateStreamingSound(channels, sampleRate, bufferCapacityInFrames);
            stream.Start();

            return (engine, stream);
        }
        catch
        {
            if (engine is not null)
            {
                try
                {
                    engine.Stop();
                }
                catch
                {
                    // ignore
                }

                engine.Dispose();
            }

            throw;
        }
    }

    private static void DisposeResources(MiniaudioStreamingSound stream, MiniaudioEngine engine)
    {
        try
        {
            stream.Stop();
        }
        catch
        {
            // ignore
        }

        stream.Dispose();

        try
        {
            engine.Stop();
        }
        catch
        {
            // ignore
        }

        engine.Dispose();
    }

    public void InsertQueue(IAudioChunk chunk)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(chunk);

        if (chunk.Format.SampleRate != (int)_sampleRate || chunk.Format.ChannelCount != (int)_channels)
        {
            throw new InvalidOperationException("チャンネル数またはサンプルレートが一致していません");
        }

        var source = chunk.Samples;
        if (source.Length == 0)
        {
            return;
        }

        var pool = ArrayPool<float>.Shared;
        var buffer = pool.Rent(source.Length);
        try
        {
            for (var i = 0; i < source.Length; i++)
            {
                var sample = source[i];
                if (sample > 1d) sample = 1d;
                else if (sample < -1d) sample = -1d;
                buffer[i] = (float)sample;
            }

            var offset = 0;
            while (offset < source.Length)
            {
                var remaining = source.Length - offset;
                var remainingFrames = (ulong)(remaining / (int)_channels);
                if (remainingFrames == 0)
                {
                    break;
                }

                ulong availableFrames;
                lock (_gate)
                {
                    availableFrames = _stream.AvailableFramesToWrite;
                }

                if (availableFrames == 0)
                {
                    Thread.Yield();
                    continue;
                }

                var framesToWrite = Math.Min(availableFrames, remainingFrames);
                var samplesToWrite = checked((int)(framesToWrite * _channels));

                ulong writtenFrames;
                lock (_gate)
                {
                    writtenFrames = _stream.AppendPcmFrames(buffer.AsSpan(offset, samplesToWrite));
                }

                if (writtenFrames == 0)
                {
                    Thread.Yield();
                    continue;
                }

                offset += checked((int)(writtenFrames * _channels));
            }
        }
        finally
        {
            pool.Return(buffer);
        }
    }

    public void ClearQueue()
    {
        ThrowIfDisposed();
        lock (_gate)
        {
            _stream.Stop();
            _stream.ResetBuffer();
            _stream.ClearEndOfStream();
            _stream.Start();
        }
    }

    public long GetQueuedSamplesCount()
    {
        ThrowIfDisposed();
        lock (_gate)
        {
            return (long)_stream.QueuedFrames;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        lock (_gate)
        {
            DisposeResources(_stream, _engine);
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MiniaudioService));
        }
    }
}

