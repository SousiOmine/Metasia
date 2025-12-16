using System;
using System.Buffers;
using System.Threading;
using Metasia.Core.Sounds;
using Miniaudio.Net;

namespace Metasia.Editor.Services;

public sealed class MiniaudioService : IAudioService
{
    private readonly MiniaudioEngine _engine;
    private readonly MiniaudioStreamingSound _stream;
    private readonly uint _sampleRate;
    private readonly uint _channels;
    private readonly object _gate = new();
    private bool _disposed;

    public MiniaudioService()
        : this(sampleRate: 44100, channels: 2)
    {
    }

    public MiniaudioService(uint sampleRate, uint channels, uint bufferCapacityInFrames = 131072)
    {
        if (sampleRate == 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));
        if (channels == 0) throw new ArgumentOutOfRangeException(nameof(channels));
        if (bufferCapacityInFrames == 0) throw new ArgumentOutOfRangeException(nameof(bufferCapacityInFrames));

        _sampleRate = sampleRate;
        _channels = channels;

        _engine = MiniaudioEngine.Create(new MiniaudioEngineOptions
        {
            SampleRate = sampleRate,
            Channels = channels,
        });
        _engine.Start();

        _stream = _engine.CreateStreamingSound(channels, sampleRate, bufferCapacityInFrames);
        _stream.Start();
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
                    Thread.Sleep(1);
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
                    Thread.Sleep(1);
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
            try
            {
                _stream.Stop();
            }
            catch
            {
                // ignore
            }

            _stream.Dispose();

            try
            {
                _engine.Stop();
            }
            catch
            {
                // ignore
            }

            _engine.Dispose();
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

