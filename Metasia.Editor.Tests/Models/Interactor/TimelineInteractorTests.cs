using Metasia.Core.Attributes;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using Metasia.Editor.Models.Interactor;
using SkiaSharp;

namespace Metasia.Editor.Tests.Models.Interactor;

[TestFixture]
public class TimelineInteractorTests
{
    [Test]
    public void EnumerateEditableMetaNumberParams_IncludesClipAndNestedEffects()
    {
        var clip = new FakeTimelineClip();
        clip.ClipValue.IsMovable = true;
        clip.ClipValue.AddPoint(new CoordPoint { Frame = 10, Value = 10 });

        var visualEffect = new FakeVisualEffect();
        visualEffect.VisualValue.IsMovable = true;
        visualEffect.VisualValue.AddPoint(new CoordPoint { Frame = 20, Value = 20 });
        clip.VisualEffects.Add(visualEffect);

        var audioEffect = new FakeAudioEffect();
        audioEffect.AudioValue.IsMovable = true;
        audioEffect.AudioValue.AddPoint(new CoordPoint { Frame = 30, Value = 30 });
        clip.AudioEffects.Add(audioEffect);

        var results = TimelineInteractor.EnumerateEditableMetaNumberParams(clip).ToList();

        Assert.That(results.Select(x => x.PropertyIdentifier), Is.EquivalentTo(new[]
        {
            "ClipValue",
            "VisualValue",
            "AudioValue"
        }));
        Assert.That(results.All(x => x.PropertyValue.Params.Count == 1), Is.True);
    }

    [Test]
    public void EnumerateEditableMetaNumberParams_SkipsNonMovableParams()
    {
        var clip = new FakeTimelineClip();
        clip.ClipValue.IsMovable = true;
        clip.ClipValue.AddPoint(new CoordPoint { Frame = 10, Value = 10 });

        var visualEffect = new FakeVisualEffect();
        visualEffect.VisualValue.IsMovable = false;
        visualEffect.VisualValue.AddPoint(new CoordPoint { Frame = 20, Value = 20 });
        clip.VisualEffects.Add(visualEffect);

        var results = TimelineInteractor.EnumerateEditableMetaNumberParams(clip).ToList();

        Assert.That(results.Select(x => x.PropertyIdentifier), Is.EquivalentTo(new[]
        {
            "ClipValue"
        }));
    }

    public sealed class FakeTimelineClip : ClipObject, IRenderable, IAudible
    {
        [EditableProperty("ClipValue")]
        public MetaNumberParam<double> ClipValue { get; set; } = new(0);

        public MetaDoubleParam Volume { get; set; } = new(100);

        public List<AudioEffectBase> AudioEffects { get; set; } = new();

        public List<VisualEffectBase> VisualEffects { get; set; } = new();

        public Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IRenderNode>(new NormalRenderNode());
        }

        public Task<IAudioChunk> GetAudioChunkAsync(GetAudioContext context)
        {
            return Task.FromResult<IAudioChunk>(new AudioChunk(context.Format, context.RequiredLength));
        }
    }

    public sealed class FakeVisualEffect : VisualEffectBase
    {
        [EditableProperty("VisualValue")]
        public MetaNumberParam<double> VisualValue { get; set; } = new(0);

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            return new VisualEffectResult(input, context.TargetImageCacheKey);
        }
    }

    public sealed class FakeAudioEffect : AudioEffectBase
    {
        [EditableProperty("AudioValue")]
        public MetaNumberParam<double> AudioValue { get; set; } = new(0);

        public override IAudioChunk Apply(IAudioChunk input, AudioEffectContext context)
        {
            return input;
        }
    }
}
