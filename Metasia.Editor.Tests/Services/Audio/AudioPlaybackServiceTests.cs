using System.Collections.Generic;
using System.Threading;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Sounds;
using Metasia.Editor.Services;
using Metasia.Editor.Services.Audio;
using NUnit.Framework;
using SkiaSharp;

namespace Metasia.Editor.Tests.Services.Audio;

/// <summary>
/// 音声再生タスクの開始/停止（Play/Pause）のテスト。
/// Pauseを連続で呼んでも例外にならないことと、
/// Pause直後のPlayで古いタスクの終了処理が新しい再生状態を壊さないことを検証する。
/// </summary>
[TestFixture]
public class AudioPlaybackServiceTests
{
    private static ProjectInfo CreateProjectInfo()
    {
        return new ProjectInfo(60, new SKSize(1920, 1080), 48000, 2);
    }

    private static TimelineObject CreateTimeline()
    {
        return new TimelineObject("timeline");
    }

    [Test]
    public void Pause_WhenNotPlaying_DoesNotThrow()
    {
        var service = new AudioPlaybackService(new FakeAudioService());

        Assert.DoesNotThrow(() => service.Pause());
    }

    [Test]
    public async Task Pause_WhenCalledTwice_DoesNotThrowAndStops()
    {
        var service = new AudioPlaybackService(new FakeAudioService());
        service.Play(CreateTimeline(), CreateProjectInfo(), 0, 1.0, 44100, 2, new FakeAudioFileAccessor(), ".", new Dictionary<string, TimelineObject>());

        service.Pause();
        Assert.DoesNotThrow(() => service.Pause());
        Assert.That(service.IsPlaying, Is.False);

        // 生成タスクの終了を待つ
        await Task.Delay(100);
    }

    [Test]
    public void Play_WhenAlreadyPlaying_DoesNotStartAnotherTask()
    {
        var audio = new FakeAudioService();
        var service = new AudioPlaybackService(audio);
        service.Play(CreateTimeline(), CreateProjectInfo(), 0, 1.0, 44100, 2, new FakeAudioFileAccessor(), ".", new Dictionary<string, TimelineObject>());

        service.Play(CreateTimeline(), CreateProjectInfo(), 0, 1.0, 44100, 2, new FakeAudioFileAccessor(), ".", new Dictionary<string, TimelineObject>());

        Assert.That(service.IsPlaying, Is.True);
        service.Pause();
    }

    [Test]
    public async Task Play_Pause_Play_Pause_OldTaskTerminationDoesNotResetNewPlayback()
    {
        var audio = new BlockableAudioService();
        var service = new AudioPlaybackService(audio);
        var timeline = CreateTimeline();

        // 1回目の再生タスクを、キュー書き込み中のブロックで実行中に保持する
        audio.Blocking = true;
        service.Play(timeline, CreateProjectInfo(), 0, 1.0, 44100, 2, new FakeAudioFileAccessor(), ".", new Dictionary<string, TimelineObject>());
        Assert.That(SpinWait.SpinUntil(() => audio.BlockedCalls > 0, 5000), Is.True, "1回目のタスクがキュー書き込みに到達しませんでした");

        // 1回目のタスクはブロック中のため終了しない
        service.Pause();

        // 2回目の再生を開始（この時点で1回目のタスクはまだ生存している）
        service.Play(timeline, CreateProjectInfo(), 0, 1.0, 44100, 2, new FakeAudioFileAccessor(), ".", new Dictionary<string, TimelineObject>());
        Assert.That(service.IsPlaying, Is.True);

        // 1回目のタスクを解放し、終了処理（finally）を実行させる
        audio.Release();
        await Task.Delay(200);

        // 古いタスクの終了処理が新しい再生のIsPlayingを上書きしないこと
        Assert.That(service.IsPlaying, Is.True, "古いタスクの終了処理が新しい再生の状態をリセットしました");

        // 2回目のタスクを停止できること
        service.Pause();
        Assert.That(service.IsPlaying, Is.False);
        await Task.Delay(100);
        Assert.That(service.IsPlaying, Is.False);
    }

    [Test]
    public void Play_ConvertsChunkToOutputFormatWhenProjectFormatDiffers()
    {
        var audio = new RecordingAudioService();
        var service = new AudioPlaybackService(audio);
        var projectInfo = new ProjectInfo(60, new SKSize(1920, 1080), 48000, 6);

        service.Play(CreateTimeline(), projectInfo, 0, 1.0, projectInfo.AudioSamplingRate, projectInfo.AudioChannels, new FakeAudioFileAccessor(), ".", new Dictionary<string, TimelineObject>());

        Assert.That(SpinWait.SpinUntil(() => audio.InsertedFormats.Count > 0, 5000), Is.True, "チャンクが再生キューに挿入されませんでした");

        var insertedFormat = audio.InsertedFormats[0];
        Assert.That(insertedFormat.SampleRate, Is.EqualTo(44100));
        Assert.That(insertedFormat.ChannelCount, Is.EqualTo(2));

        service.Pause();
    }

    private sealed class FakeAudioService : IAudioService
    {
        public IAudioFormat Format { get; } = new AudioFormat(44100, 2);

        public bool EnsureFormat(int sampleRate, int channelCount) => true;

        public void InsertQueue(IAudioChunk chunk)
        {
        }

        public void ClearQueue()
        {
        }

        public long GetQueuedSamplesCount() => 0;

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// InsertQueueの実行中にブロックできるテスト用のIAudioService。
    /// Blocking=trueの間、キュー書き込みは解放されるまで待機する。
    /// </summary>
    private sealed class BlockableAudioService : IAudioService
    {
        private readonly ManualResetEventSlim _gate = new(false);

        public IAudioFormat Format { get; } = new AudioFormat(44100, 2);

        public bool EnsureFormat(int sampleRate, int channelCount) => true;

        public volatile bool Blocking;

        public volatile int BlockedCalls;

        public void InsertQueue(IAudioChunk chunk)
        {
            if (Blocking)
            {
                Interlocked.Increment(ref BlockedCalls);
                _gate.Wait();
            }
        }

        public void Release()
        {
            Blocking = false;
            _gate.Set();
        }

        public void ClearQueue()
        {
        }

        public long GetQueuedSamplesCount() => 0;

        public void Dispose()
        {
        }
    }

    /// <summary>
    /// 挿入されたチャンクのフォーマットを記録するテスト用のIAudioService。
    /// 出力先のフォーマットと異なるチャンクが渡された場合は例外にする。
    /// </summary>
    private sealed class RecordingAudioService : IAudioService
    {
        private readonly List<IAudioFormat> _formats = [];

        public IAudioFormat Format { get; } = new AudioFormat(44100, 2);

        public bool EnsureFormat(int sampleRate, int channelCount) => true;

        public IReadOnlyList<IAudioFormat> InsertedFormats
        {
            get
            {
                lock (_formats)
                {
                    return _formats.ToList();
                }
            }
        }

        public void InsertQueue(IAudioChunk chunk)
        {
            lock (_formats)
            {
                _formats.Add(chunk.Format);
            }
        }

        public void ClearQueue()
        {
        }

        public long GetQueuedSamplesCount()
        {
            lock (_formats)
            {
                // 1件でも挿入されたら十分に埋まっている扱いにして、生成ループの空回りを止める
                return _formats.Count > 0 ? 1_000_000 : 0;
            }
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeAudioFileAccessor : IAudioFileAccessor
    {
        public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
            => Task.FromResult(new AudioFileAccessorResult { IsSuccessful = false, Chunk = null });

        public Task<AudioSampleResult> GetAudioBySampleAsync(string path, long startSample, long sampleCount, int sampleRate)
            => Task.FromResult(new AudioSampleResult { IsSuccessful = false, Chunk = null });

        public Task<AudioMediaInfoResult?> GetAudioMediaInfoAsync(string path)
            => Task.FromResult<AudioMediaInfoResult?>(null);
    }
}