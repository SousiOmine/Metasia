using Metasia.Core.Encode;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Sounds;
using SkiaSharp;

namespace Metasia.Core.Tests.Encode;

[TestFixture]
public class EncoderBaseTests
{
    [Test]
    public void Initialize_WhenSelectionIsDefault_UsesTimelineLastFrame()
    {
        var encoder = new TestEncoder();
        var timeline = CreateTimelineWithLastFrame(120);
        var project = new MetasiaProject(new ProjectInfo(60, new SKSize(1920, 1080), 48000, 2));

        encoder.Initialize(project, timeline, new FakeMediaAccessor(), new FakeMediaAccessor(), new FakeMediaAccessor(), ".", "out.mp4");

        Assert.That(encoder.ExposedFrameCount, Is.EqualTo(121));
    }

    [Test]
    public void Initialize_WhenSelectionRangeIsReversed_NormalizesRange()
    {
        var encoder = new TestEncoder();
        var timeline = CreateTimelineWithLastFrame(120);
        var project = new MetasiaProject(new ProjectInfo(60, new SKSize(1920, 1080), 48000, 2));
        timeline.SelectionStart = 80;
        timeline.SelectionEnd = 20;

        encoder.Initialize(project, timeline, new FakeMediaAccessor(), new FakeMediaAccessor(), new FakeMediaAccessor(), ".", "out.mp4");

        Assert.That(encoder.ExposedFrameCount, Is.EqualTo(61));
    }

    private static TimelineObject CreateTimelineWithLastFrame(int lastFrame)
    {
        var timeline = new TimelineObject("timeline");
        var layer = new LayerObject();
        var clip = new ImageObject
        {
            StartFrame = 0,
            EndFrame = lastFrame,
            ImagePath = MediaPath.CreateFromPath(".", "image.png")
        };

        layer.Objects.Add(clip);
        timeline.Layers.Add(layer);
        return timeline;
    }

    private sealed class TestEncoder : EncoderBase
    {
        public int ExposedFrameCount => FrameCount;

        public override double ProgressRate { get; protected set; }
        public override event EventHandler<EventArgs> StatusChanged = delegate { };
        public override event EventHandler<EventArgs> EncodeStarted = delegate { };
        public override event EventHandler<EventArgs> EncodeCompleted = delegate { };
        public override event EventHandler<EventArgs> EncodeFailed = delegate { };

        public override void CancelRequest()
        {
        }

        public override void Start()
        {
        }
    }

    private sealed class FakeMediaAccessor : IImageFileAccessor, IVideoFileAccessor, IAudioFileAccessor
    {
        public Task<ImageFileAccessorResult> GetImageAsync(string path)
            => Task.FromResult(new ImageFileAccessorResult { IsSuccessful = false, Image = null });

        public Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time)
            => Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false, Image = null });

        public Task<VideoFileAccessorResult> GetImageAsync(string path, int frame)
            => Task.FromResult(new VideoFileAccessorResult { IsSuccessful = false, Image = null });

        public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
            => Task.FromResult(new AudioFileAccessorResult { IsSuccessful = false, Chunk = null });

        public Task<AudioSampleResult> GetAudioBySampleAsync(string path, long startSample, long sampleCount, int sampleRate)
            => Task.FromResult(new AudioSampleResult { IsSuccessful = false, Chunk = null });
    }
}
