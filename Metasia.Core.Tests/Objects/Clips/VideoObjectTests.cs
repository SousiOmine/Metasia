using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Objects.Parameters;
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
}
