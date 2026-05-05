using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Objects.Parameters;
using NUnit.Framework;

namespace Metasia.Core.Tests.Objects.Clips;

[TestFixture]
public class AudioObjectTests
{
    [Test]
    public void SplitAtFrame_WithSplitContext_AdjustsAudioStartSecondsForSecondClip()
    {
        var audio = new AudioObject("test-audio")
        {
            StartFrame = 10,
            EndFrame = 100,
            AudioStartSeconds = new(3.0)
        };

        var context = new SplitContext { FrameRate = 30 };
        var (firstClip, secondClip) = audio.SplitAtFrame(50, context);

        var first = (AudioObject)firstClip;
        var second = (AudioObject)secondClip;

        Assert.That(first.AudioStartSeconds.Get(0, 1), Is.EqualTo(3.0).Within(0.001));
        double expectedOffset = (50.0 - 10.0) / 30.0;
        Assert.That(second.AudioStartSeconds.Get(0, 1), Is.EqualTo(3.0 + expectedOffset).Within(0.001));
    }

    [Test]
    public void SplitAtFrame_WithoutSplitContext_DoesNotAdjustAudioStartSeconds()
    {
        var audio = new AudioObject("test-audio")
        {
            StartFrame = 10,
            EndFrame = 100,
            AudioStartSeconds = new(3.0)
        };

        var (firstClip, secondClip) = audio.SplitAtFrame(50);

        var first = (AudioObject)firstClip;
        var second = (AudioObject)secondClip;

        Assert.That(first.AudioStartSeconds.Get(0, 1), Is.EqualTo(3.0).Within(0.001));
        Assert.That(second.AudioStartSeconds.Get(0, 1), Is.EqualTo(3.0).Within(0.001));
    }

    [Test]
    public void SplitAtFrame_WithSplitContext_PreservesFirstClipStartSeconds()
    {
        var audio = new AudioObject("test-audio")
        {
            StartFrame = 20,
            EndFrame = 80,
            AudioStartSeconds = new(8.0)
        };

        var context = new SplitContext { FrameRate = 60 };
        var (firstClip, _) = audio.SplitAtFrame(40, context);

        var first = (AudioObject)firstClip;
        Assert.That(first.AudioStartSeconds.Get(0, 1), Is.EqualTo(8.0).Within(0.001));
    }

    [Test]
    public void SplitAtFrame_WithMovableAudioStartSeconds_RebasesSecondClipBeforeOffset()
    {
        var startSeconds = new MetaNumberParam<double>(0.0)
        {
            IsMovable = true
        };
        startSeconds.AddPoint(new CoordPoint { Frame = 40, Value = 6.0 });

        var audio = new AudioObject("test-audio")
        {
            StartFrame = 10,
            EndFrame = 100,
            AudioStartSeconds = startSeconds
        };

        var context = new SplitContext { FrameRate = 30 };
        var (_, secondClip) = audio.SplitAtFrame(50, context);

        var second = (AudioObject)secondClip;
        double expectedOffset = (50.0 - 10.0) / 30.0;
        Assert.That(second.AudioStartSeconds.Get(0, 51), Is.EqualTo(6.0 + expectedOffset).Within(0.001));
    }

    [Test]
    public void SplitAtFrame_WithSpeed_AdjustsAudioStartSecondsBySourceTime()
    {
        var audio = new AudioObject("test-audio")
        {
            StartFrame = 10,
            EndFrame = 100,
            AudioStartSeconds = new(3.0),
            Speed = 200
        };

        var context = new SplitContext { FrameRate = 30 };
        var (_, secondClip) = audio.SplitAtFrame(40, context);

        var second = (AudioObject)secondClip;
        double expectedOffset = ((40.0 - 10.0) / 30.0) * 2.0;
        Assert.That(second.AudioStartSeconds.Get(0, 1), Is.EqualTo(3.0 + expectedOffset).Within(0.001));
    }

    [Test]
    public void Speed_DefaultValue_Is100()
    {
        var audio = new AudioObject("test-audio");
        Assert.That(audio.Speed.Value, Is.EqualTo(100));
    }

    [Test]
    public void SplitAtFrame_PreservesSpeedValue()
    {
        var audio = new AudioObject("test-audio")
        {
            StartFrame = 10,
            EndFrame = 100,
            Speed = 50
        };

        var (firstClip, secondClip) = audio.SplitAtFrame(50);
        var first = (AudioObject)firstClip;
        var second = (AudioObject)secondClip;

        Assert.That(first.Speed.Value, Is.EqualTo(50));
        Assert.That(second.Speed.Value, Is.EqualTo(50));
    }
}
