using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Sounds;
using NUnit.Framework;

namespace Metasia.Core.Tests.Objects;

[TestFixture]
public class MediaAudioObjectTests
{
    [Test]
    public async Task AudioObject_GetAudioChunk_UsesAccessorAndAppliesVolume()
    {
        var accessor = new FakeAudioFileAccessor(
            new AudioSampleResult
            {
                IsSuccessful = true,
                Chunk = new AudioChunk(new AudioFormat(44100, 2), [1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0]),
            });

        var obj = new AudioObject("audio")
        {
            AudioPath = MediaPath.CreateFromPath(Path.GetTempPath(), "audio.wav"),
            Volume = 50,
        };
        obj.AudioStartSeconds.SetSinglePoint(2.0);

        var context = new GetAudioContext(
            new AudioFormat(44100, 2),
            44100,
            4,
            60,
            1,
            accessor,
            null);

        var chunk = await obj.GetAudioChunkAsync(context);

        Assert.That(accessor.LastStartSample, Is.EqualTo(44100 * 2 + 44100));
        Assert.That(accessor.LastSampleCount, Is.EqualTo(4));
        Assert.That(chunk.Length, Is.EqualTo(4));
        Assert.That(chunk.Samples.All(x => Math.Abs(x - 0.5) < 0.0001), Is.True);
    }

    [Test]
    public async Task VideoObject_GetAudioChunk_UsesVideoAudioSource()
    {
        var accessor = new FakeAudioFileAccessor(
            new AudioSampleResult
            {
                IsSuccessful = true,
                Chunk = new AudioChunk(new AudioFormat(44100, 2), [0.8, 0.8, 0.8, 0.8]),
            });

        var obj = new VideoObject("video")
        {
            VideoPath = MediaPath.CreateFromPath(Path.GetTempPath(), "video.mp4"),
            Volume = 25,
        };

        var context = new GetAudioContext(
            new AudioFormat(44100, 2),
            0,
            2,
            60,
            1,
            accessor,
            null);

        var chunk = await obj.GetAudioChunkAsync(context);

        Assert.That(chunk.Length, Is.EqualTo(2));
        Assert.That(chunk.Samples.All(x => Math.Abs(x - 0.2) < 0.0001), Is.True);
    }

    [Test]
    public void AudioObject_Constructor_SetsAudioMediaType()
    {
        var obj = new AudioObject("audio");

        Assert.That(obj.AudioPath.Types, Is.EqualTo(new[] { MediaType.Audio }));
    }

    private sealed class FakeAudioFileAccessor(AudioSampleResult result) : IAudioFileAccessor
    {
        private readonly AudioSampleResult _result = result;

        public long LastStartSample { get; private set; }
        public long LastSampleCount { get; private set; }

        public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
        {
            return Task.FromResult(new AudioFileAccessorResult { IsSuccessful = false, Chunk = null });
        }
        
        public Task<AudioSampleResult> GetAudioBySampleAsync(string path, long startSample, long sampleCount, int sampleRate)
        {
            LastStartSample = startSample;
            LastSampleCount = sampleCount;
            return Task.FromResult(_result);
        }
    }
}
