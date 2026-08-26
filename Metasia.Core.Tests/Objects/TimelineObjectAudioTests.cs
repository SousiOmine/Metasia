using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Sounds;
using NUnit.Framework;

namespace Metasia.Core.Tests.Objects;

/// <summary>
/// タイムラインの音声ミキシング（タイムライン音量・グループ制御の音量/エフェクト）のテスト。
/// </summary>
[TestFixture]
public class TimelineObjectAudioTests
{
    private const int SampleRate = 44100;
    private const long FrameRate = 60;

    private static GetAudioContext CreateContext(long startSample, long length, double durationSeconds, IAudioFileAccessor accessor)
    {
        return new GetAudioContext(
            new AudioFormat(SampleRate, 2),
            startSample,
            length,
            FrameRate,
            durationSeconds,
            accessor,
            null);
    }

    private static TimelineObject CreateTimelineWithAudioClip(int clipStartFrame, int clipEndFrame, TimelineObject? timeline = null)
    {
        timeline ??= new TimelineObject("timeline");
        var audioLayer = new LayerObject("audio-layer", "audio");
        var clip = new AudioObject("clip")
        {
            StartFrame = clipStartFrame,
            EndFrame = clipEndFrame,
            AudioPath = MediaPath.CreateFromPath(".", "audio.wav"),
            Volume = 100,
        };
        audioLayer.Objects.Add(clip);
        timeline.Layers.Add(audioLayer);
        return timeline;
    }

    [Test]
    public async Task GetAudioChunkAsync_TimelineVolume_IsAppliedToMixedResult()
    {
        var accessor = new FakeAudioFileAccessor(1.0);
        var timeline = CreateTimelineWithAudioClip(0, 119, new TimelineObject("timeline") { Volume = 50 });

        var chunk = await timeline.GetAudioChunkAsync(CreateContext(0, SampleRate, 2.0, accessor));

        Assert.That(chunk.Length, Is.EqualTo(SampleRate));
        Assert.That(chunk.Samples.All(s => Math.Abs(s - 0.5) < 0.0001), Is.True);
    }

    [Test]
    public async Task GetAudioChunkAsync_GroupControlVolumeAndTimelineVolume_AreAppliedOnceEach()
    {
        var accessor = new FakeAudioFileAccessor(1.0);
        var timeline = new TimelineObject("timeline") { Volume = 50 };

        // グループ制御レイヤー（0）と音声レイヤー（1）
        var controlLayer = new LayerObject("control-layer", "control");
        var control = new GroupControlObject("control")
        {
            StartFrame = 0,
            EndFrame = 119,
            Volume = 50,
        };
        controlLayer.Objects.Add(control);
        timeline.Layers.Add(controlLayer);

        var audioLayer = new LayerObject("audio-layer", "audio");
        var clip = new AudioObject("clip")
        {
            StartFrame = 0,
            EndFrame = 119,
            AudioPath = MediaPath.CreateFromPath(".", "audio.wav"),
            Volume = 100,
        };
        audioLayer.Objects.Add(clip);
        timeline.Layers.Add(audioLayer);

        var chunk = await timeline.GetAudioChunkAsync(CreateContext(0, SampleRate, 2.0, accessor));

        // グループ制御 0.5 × タイムライン 0.5 = 0.25（それぞれ1回ずつ適用）
        Assert.That(chunk.Samples.All(s => Math.Abs(s - 0.25) < 0.0001), Is.True);
    }

    [Test]
    public async Task GetAudioChunkAsync_GroupControlEffect_UsesControlStartAsTimeBase()
    {
        var accessor = new FakeAudioFileAccessor(1.0);
        var timeline = new TimelineObject("timeline");

        // グループ制御はフレーム60（1秒位置）から開始し、1秒のフェードインを持つ
        var controlLayer = new LayerObject("control-layer", "control");
        var control = new GroupControlObject("control")
        {
            StartFrame = 60,
            EndFrame = 179,
            Volume = 100,
        };
        control.AudioEffects.Add(new VolumeFadeEffect { In = 1.0f, Out = 0f });
        controlLayer.Objects.Add(control);
        timeline.Layers.Add(controlLayer);

        // 対象の音声レイヤー（制御と同じ期間）
        var audioLayer = new LayerObject("audio-layer", "audio");
        var clip = new AudioObject("clip")
        {
            StartFrame = 60,
            EndFrame = 179,
            AudioPath = MediaPath.CreateFromPath(".", "audio.wav"),
            Volume = 100,
        };
        audioLayer.Objects.Add(clip);
        timeline.Layers.Add(audioLayer);

        // タイムライン位置60フレーム（=44100サンプル）から1秒分を要求
        var chunk = await timeline.GetAudioChunkAsync(CreateContext(SampleRate, SampleRate, 2.0, accessor));

        // エフェクトの時間基準はグループ制御の開始位置（フレーム60）のため、
        // チャンク先頭はフェードイン開始（係数0）、0.05秒後は係数0.05
        Assert.That(chunk.Samples[0], Is.EqualTo(0.0).Within(0.0001));
        Assert.That(chunk.Samples[2205 * 2], Is.EqualTo(0.05).Within(0.001));
    }

    private sealed class FakeAudioFileAccessor(double sampleValue = 1.0) : IAudioFileAccessor
    {
        public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
            => Task.FromResult(new AudioFileAccessorResult { IsSuccessful = false, Chunk = null });

        public Task<AudioSampleResult> GetAudioBySampleAsync(string path, long startSample, long sampleCount, int sampleRate)
        {
            var samples = new double[sampleCount * 2];
            Array.Fill(samples, sampleValue);
            return Task.FromResult(new AudioSampleResult
            {
                IsSuccessful = true,
                Chunk = new AudioChunk(new AudioFormat(sampleRate, 2), samples),
            });
        }

        public Task<AudioMediaInfoResult?> GetAudioMediaInfoAsync(string path)
            => Task.FromResult<AudioMediaInfoResult?>(null);
    }
}