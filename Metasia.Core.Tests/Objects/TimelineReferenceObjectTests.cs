using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using NUnit.Framework;
using SkiaSharp;

namespace Metasia.Core.Tests.Objects;

[TestFixture]
public class TimelineReferenceObjectTests
{
    [Test]
    public async Task RenderAsync_WithReferencedTimeline_ReturnsImageNode()
    {
        var referencedTimeline = CreateReferencedTimeline("TargetTimeline");
        var clip = new TimelineReferenceObject("ref")
        {
            StartFrame = 0,
            EndFrame = 30,
            TargetTimelineId = referencedTimeline.Id,
            SourceStartFrame = new(0)
        };

        var context = CreateRenderContext(referencedTimeline);

        var result = await clip.RenderAsync(context);

        Assert.That(result, Is.InstanceOf<NormalRenderNode>());
        var node = (NormalRenderNode)result;
        Assert.That(node.Image, Is.Not.Null);
        Assert.That(node.LogicalSize.Width, Is.EqualTo(1920).Within(0.01f));
        Assert.That(node.LogicalSize.Height, Is.EqualTo(1080).Within(0.01f));
    }

    [Test]
    public async Task RenderAsync_WithRecursiveReference_ReturnsEmptyNode()
    {
        var rootTimeline = new TimelineObject("RootTimeline");
        var clip = new TimelineReferenceObject("ref")
        {
            StartFrame = 0,
            EndFrame = 30,
            TargetTimelineId = rootTimeline.Id
        };

        var context = CreateRenderContext(rootTimeline, [rootTimeline.Id]);

        var result = await clip.RenderAsync(context);

        Assert.That(result, Is.InstanceOf<NormalRenderNode>());
        var node = (NormalRenderNode)result;
        Assert.That(node.Image, Is.Null);
    }

    [Test]
    public async Task GetAudioChunkAsync_WithReferencedTimeline_ReturnsAudibleChunk()
    {
        var referencedTimeline = CreateReferencedTimeline("AudioTimeline");
        var clip = new TimelineReferenceObject("ref")
        {
            StartFrame = 0,
            EndFrame = 60,
            TargetTimelineId = referencedTimeline.Id,
            SourceStartFrame = new(5)
        };

        var context = CreateAudioContext(referencedTimeline);

        var result = await clip.GetAudioChunkAsync(context);

        Assert.That(result.Samples.Any(sample => Math.Abs(sample) > 0.0001), Is.True);
    }

    [Test]
    public async Task GetAudioChunkAsync_WithRecursiveReference_ReturnsSilentChunk()
    {
        var rootTimeline = new TimelineObject("RootTimeline");
        var clip = new TimelineReferenceObject("ref")
        {
            StartFrame = 0,
            EndFrame = 60,
            TargetTimelineId = rootTimeline.Id
        };

        var context = CreateAudioContext(rootTimeline, [rootTimeline.Id]);

        var result = await clip.GetAudioChunkAsync(context);

        Assert.That(result.Samples.All(sample => Math.Abs(sample) < 0.0001), Is.True);
    }

    [Test]
    public void SplitAtFrame_AdvancesSourceStartFrameForSecondClip()
    {
        var clip = new TimelineReferenceObject("ref")
        {
            StartFrame = 10,
            EndFrame = 30,
            SourceStartFrame = new MetaDoubleParam(24),
            TargetTimelineId = "TargetTimeline"
        };

        var (firstClip, secondClip) = clip.SplitAtFrame(18);

        Assert.That(firstClip, Is.InstanceOf<TimelineReferenceObject>());
        Assert.That(secondClip, Is.InstanceOf<TimelineReferenceObject>());

        var firstReference = (TimelineReferenceObject)firstClip;
        var secondReference = (TimelineReferenceObject)secondClip;

        Assert.That(firstReference.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(secondReference.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(firstReference.Id, Is.Not.EqualTo(secondReference.Id));
        Assert.That(Guid.TryParse(firstReference.Id, out _), Is.True);
        Assert.That(Guid.TryParse(secondReference.Id, out _), Is.True);
        Assert.That(firstReference.SourceStartFrame.Value, Is.EqualTo(24).Within(0.001));
        Assert.That(secondReference.SourceStartFrame.Value, Is.EqualTo(32).Within(0.001));
        Assert.That(firstReference.StartFrame, Is.EqualTo(10));
        Assert.That(firstReference.EndFrame, Is.EqualTo(17));
        Assert.That(secondReference.StartFrame, Is.EqualTo(18));
        Assert.That(secondReference.EndFrame, Is.EqualTo(30));
    }

    private static TimelineObject CreateReferencedTimeline(string id)
    {
        var timeline = new TimelineObject(id);
        var layer = new LayerObject("layer1", "Layer 1");
        layer.Objects.Add(new kariHelloObject("hello")
        {
            StartFrame = 0,
            EndFrame = 120
        });
        timeline.Layers.Add(layer);
        return timeline;
    }

    private static RenderContext CreateRenderContext(TimelineObject timeline, IReadOnlyList<string>? stack = null)
    {
        return new RenderContext(
            frame: 10,
            projectResolution: new SKSize(1920, 1080),
            renderResolution: new SKSize(1920, 1080),
            imageFileAccessor: new EmptyImageFileAccessor(),
            videoFileAccessor: new EmptyVideoFileAccessor(),
            projectInfo: new ProjectInfo(60, new SKSize(1920, 1080), 44100, 2),
            projectPath: string.Empty,
            availableTimelines: new Dictionary<string, TimelineObject>(StringComparer.OrdinalIgnoreCase)
            {
                [timeline.Id] = timeline
            },
            timelineReferenceStack: stack);
    }

    private static GetAudioContext CreateAudioContext(TimelineObject timeline, IReadOnlyList<string>? stack = null)
    {
        return new GetAudioContext(
            new AudioFormat(44100, 2),
            startSamplePosition: 0,
            requiredLength: 4096,
            projectFrameRate: 60,
            objectDurationInSeconds: 2.0,
            audioFileAccessor: null,
            projectPath: string.Empty,
            availableTimelines: new Dictionary<string, TimelineObject>(StringComparer.OrdinalIgnoreCase)
            {
                [timeline.Id] = timeline
            },
            timelineReferenceStack: stack);
    }
}
