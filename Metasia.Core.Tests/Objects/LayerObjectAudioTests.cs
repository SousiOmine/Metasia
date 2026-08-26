using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Sounds;
using NUnit.Framework;

namespace Metasia.Core.Tests.Objects;

/// <summary>
/// レイヤーの音声ミキシング（音量適用・エフェクトの時間基準）のテスト。
/// クリップ音量は各クリップ内で適用済みのため、レイヤーでは二重適用されないことを検証する。
/// </summary>
[TestFixture]
public class LayerObjectAudioTests
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

    private static AudioObject CreateAudioClip(int startFrame, int endFrame, double volume = 100)
    {
        return new AudioObject($"clip-{startFrame}-{endFrame}")
        {
            StartFrame = startFrame,
            EndFrame = endFrame,
            AudioPath = MediaPath.CreateFromPath(".", "audio.wav"),
            Volume = volume,
        };
    }

    [Test]
    public async Task GetAudioChunkAsync_ClipVolume50WithLayerVolume100_IsAppliedOnlyOnce()
    {
        var accessor = new FakeAudioFileAccessor(1.0);
        var layer = new LayerObject("layer", "layer");
        layer.Objects.Add(CreateAudioClip(0, 119, volume: 50));
        // レイヤー音量が100でもクリップ音量50は0.5のまま（二重適用されない）
        layer.Volume = 100;

        var chunk = await layer.GetAudioChunkAsync(CreateContext(0, SampleRate, 2.0, accessor));

        Assert.That(chunk.Length, Is.EqualTo(SampleRate));
        Assert.That(chunk.Samples.All(s => Math.Abs(s - 0.5) < 0.0001), Is.True);
    }

    [Test]
    public async Task GetAudioChunkAsync_ClipVolumeAndLayerVolume_AreAppliedOnceEach()
    {
        var accessor = new FakeAudioFileAccessor(1.0);
        var layer = new LayerObject("layer", "layer");
        layer.Objects.Add(CreateAudioClip(0, 119, volume: 50));
        layer.Volume = 50;

        var chunk = await layer.GetAudioChunkAsync(CreateContext(0, SampleRate, 2.0, accessor));

        // クリップ音量 0.5 × レイヤー音量 0.5 = 0.25（それぞれ1回ずつ適用）
        Assert.That(chunk.Samples.All(s => Math.Abs(s - 0.25) < 0.0001), Is.True);
    }

    [Test]
    public async Task GetAudioChunkAsync_LayerVolume_IsAppliedToMixedResult()
    {
        var accessor = new FakeAudioFileAccessor(1.0);
        var layer = new LayerObject("layer", "layer");
        layer.Objects.Add(CreateAudioClip(0, 119, volume: 100));
        layer.Volume = 50;

        var chunk = await layer.GetAudioChunkAsync(CreateContext(0, SampleRate, 2.0, accessor));

        Assert.That(chunk.Samples.All(s => Math.Abs(s - 0.5) < 0.0001), Is.True);
    }

    [Test]
    public async Task GetAudioChunkAsync_LayerVolume_IsAppliedOnlyToClipOverlap()
    {
        var accessor = new FakeAudioFileAccessor(1.0);
        var layer = new LayerObject("layer", "layer");
        // クリップはフレーム60〜119（1秒〜2秒）のみ
        layer.Objects.Add(CreateAudioClip(60, 119, volume: 100));
        layer.Volume = 50;

        // タイムライン先頭0.5秒（クリップ前）と、クリップ期間を含む1.5秒を要求
        var chunk = await layer.GetAudioChunkAsync(CreateContext(0, (long)(SampleRate * 1.5), 2.0, accessor));

        // クリップ前(先頭1秒分)は無音、クリップ期間(1秒以降)はレイヤー音量0.5が適用される
        Assert.That(chunk.Samples.Take(SampleRate * 2).All(s => Math.Abs(s) < 0.0001), Is.True);
        Assert.That(chunk.Samples.Skip(SampleRate * 2).All(s => Math.Abs(s - 0.5) < 0.0001), Is.True);
    }

    [Test]
    public async Task GetAudioChunkAsync_LayerEffect_UsesLayerStartAsTimeBase()
    {
        var accessor = new FakeAudioFileAccessor(1.0);
        var layer = new LayerObject("layer", "layer");
        // クリップはフレーム60（1秒位置）から開始
        layer.Objects.Add(CreateAudioClip(60, 179, volume: 100));
        layer.AudioEffects.Add(new VolumeFadeEffect { In = 1.0f, Out = 0f });

        // タイムライン位置60フレーム（=44100サンプル）から1秒分を要求
        var chunk = await layer.GetAudioChunkAsync(CreateContext(SampleRate, SampleRate, 2.0, accessor));

        // エフェクトの時間基準はレイヤー先頭（クリップ開始位置=フレーム60）のため、
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