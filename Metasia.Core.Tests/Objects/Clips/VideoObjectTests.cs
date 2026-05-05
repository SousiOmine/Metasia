using Metasia.Core.Coordinate;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Sounds;
using NUnit.Framework;

namespace Metasia.Core.Tests.Objects.Clips;

[TestFixture]
public class VideoObjectTests
{
    [Test]
    public void SplitAtFrame_WithSplitContext_AdjustsVideoStartSecondsForSecondClip()
    {
        var video = new VideoObject("test-video")
        {
            StartFrame = 10,
            EndFrame = 100,
            VideoStartSeconds = new(5.0)
        };

        var context = new SplitContext { FrameRate = 30 };
        var (firstClip, secondClip) = video.SplitAtFrame(50, context);

        var first = (VideoObject)firstClip;
        var second = (VideoObject)secondClip;

        Assert.That(first.VideoStartSeconds.Get(0, 1), Is.EqualTo(5.0).Within(0.001));
        double expectedOffset = (50.0 - 10.0) / 30.0;
        Assert.That(second.VideoStartSeconds.Get(0, 1), Is.EqualTo(5.0 + expectedOffset).Within(0.001));
    }

    [Test]
    public void SplitAtFrame_WithoutSplitContext_DoesNotAdjustVideoStartSeconds()
    {
        var video = new VideoObject("test-video")
        {
            StartFrame = 10,
            EndFrame = 100,
            VideoStartSeconds = new(5.0)
        };

        var (firstClip, secondClip) = video.SplitAtFrame(50);

        var first = (VideoObject)firstClip;
        var second = (VideoObject)secondClip;

        Assert.That(first.VideoStartSeconds.Get(0, 1), Is.EqualTo(5.0).Within(0.001));
        Assert.That(second.VideoStartSeconds.Get(0, 1), Is.EqualTo(5.0).Within(0.001));
    }

    [Test]
    public void SplitAtFrame_WithSplitContext_PreservesFirstClipStartSeconds()
    {
        var video = new VideoObject("test-video")
        {
            StartFrame = 20,
            EndFrame = 80,
            VideoStartSeconds = new(10.0)
        };

        var context = new SplitContext { FrameRate = 60 };
        var (firstClip, _) = video.SplitAtFrame(40, context);

        var first = (VideoObject)firstClip;
        Assert.That(first.VideoStartSeconds.Get(0, 1), Is.EqualTo(10.0).Within(0.001));
    }

    [Test]
    public void SplitAtFrame_WithMovableVideoStartSeconds_RebasesSecondClipBeforeOffset()
    {
        var startSeconds = new MetaNumberParam<double>(0.0)
        {
            IsMovable = true
        };
        startSeconds.AddPoint(new CoordPoint { Frame = 40, Value = 7.0 });

        var video = new VideoObject("test-video")
        {
            StartFrame = 10,
            EndFrame = 100,
            VideoStartSeconds = startSeconds
        };

        var context = new SplitContext { FrameRate = 30 };
        var (_, secondClip) = video.SplitAtFrame(50, context);

        var second = (VideoObject)secondClip;
        double expectedOffset = (50.0 - 10.0) / 30.0;
        Assert.That(second.VideoStartSeconds.Get(0, 51), Is.EqualTo(7.0 + expectedOffset).Within(0.001));
    }

    [Test]
    public void SplitAtFrame_WithSpeed_AdjustsVideoStartSecondsBySourceTime()
    {
        var video = new VideoObject("test-video")
        {
            StartFrame = 10,
            EndFrame = 100,
            VideoStartSeconds = new(5.0),
            Speed = 50
        };

        var context = new SplitContext { FrameRate = 30 };
        var (_, secondClip) = video.SplitAtFrame(40, context);

        var second = (VideoObject)secondClip;
        double expectedOffset = ((40.0 - 10.0) / 30.0) * 0.5;
        Assert.That(second.VideoStartSeconds.Get(0, 1), Is.EqualTo(5.0 + expectedOffset).Within(0.001));
    }

    [Test]
    public void Speed_DefaultValue_Is100()
    {
        var video = new VideoObject("test-video");
        Assert.That(video.Speed.Value, Is.EqualTo(100));
    }

    [Test]
    public void SplitAtFrame_PreservesSpeedValue()
    {
        var video = new VideoObject("test-video")
        {
            StartFrame = 10,
            EndFrame = 100,
            Speed = 200
        };

        var (firstClip, secondClip) = video.SplitAtFrame(50);
        var first = (VideoObject)firstClip;
        var second = (VideoObject)secondClip;

        Assert.That(first.Speed.Value, Is.EqualTo(200));
        Assert.That(second.Speed.Value, Is.EqualTo(200));
    }

    [Test]
    public async Task GetAudioChunkAsync_Speed200_AdjustsSourcePositionAndLength()
    {
        var accessor = new FakeAudioFileAccessor(
            new AudioSampleResult
            {
                IsSuccessful = true,
                Chunk = new AudioChunk(new AudioFormat(44100, 2), new double[44100 * 2]),
            });

        var obj = new VideoObject("video")
        {
            VideoPath = MediaPath.CreateFromPath(Path.GetTempPath(), "video.mp4"),
            Speed = 200,
        };

        var context = new GetAudioContext(
            new AudioFormat(44100, 2),
            44100,
            4,
            60,
            1,
            accessor,
            null);

        var chunk = await obj.GetAudioChunkAsync(context);

        // speed 2.0 => source start = 44100 * 2, source length = 4 * 2 = 8
        Assert.That(accessor.LastStartSample, Is.EqualTo(44100 * 2));
        Assert.That(accessor.LastSampleCount, Is.EqualTo(8));
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

        public Task<AudioMediaInfoResult?> GetAudioMediaInfoAsync(string path)
        {
            return Task.FromResult<AudioMediaInfoResult?>(null);
        }
    }
}
