using System.Diagnostics;
using Metasia.Core.Attributes;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using SkiaSharp;

namespace Metasia.Core.Objects;

[ClipTypeIdentifier("TimelineReferenceObject", DisplayKey = "clip.timeline_reference.name", FallbackText = "タイムライン参照")]
public class TimelineReferenceObject : ClipObject, IRenderable, IAudible
{
    [EditableProperty("BlendMode", DisplayKey = "property.common.blend_mode", FallbackText = "合成モード")]
    public BlendModeParam BlendMode { get; set; } = new();

    [EditableProperty("X", DisplayKey = "property.common.x", FallbackText = "X")]
    [ValueRange(-99999, 99999, -2000, 2000)]
    public MetaNumberParam<double> X { get; set; } = new(0);

    [EditableProperty("Y", DisplayKey = "property.common.y", FallbackText = "Y")]
    [ValueRange(-99999, 99999, -2000, 2000)]
    public MetaNumberParam<double> Y { get; set; } = new(0);

    [EditableProperty("Scale", DisplayKey = "property.common.scale", FallbackText = "拡大率")]
    [ValueRange(0, 99999, 0, 1000)]
    public MetaNumberParam<double> Scale { get; set; } = new(100);

    [EditableProperty("Alpha", DisplayKey = "property.common.alpha", FallbackText = "透明度")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Alpha { get; set; } = new(0);

    [EditableProperty("Rotation", DisplayKey = "property.common.rotation", FallbackText = "回転")]
    [ValueRange(-99999, 99999, 0, 360)]
    public MetaNumberParam<double> Rotation { get; set; } = new(0);

    [EditableProperty("TargetTimelineId", DisplayKey = "property.timeline_reference.target_timeline_id", FallbackText = "参照タイムラインID")]
    public string TargetTimelineId { get; set; } = string.Empty;

    [EditableProperty("SourceStartFrame", DisplayKey = "property.timeline_reference.source_start_frame", FallbackText = "参照開始フレーム")]
    [ValueRange(0, 99999, 0, 10000)]
    public MetaDoubleParam SourceStartFrame { get; set; } = new(0);

    [EditableProperty("AudioVolume", DisplayKey = "property.common.audio_volume", FallbackText = "音量")]
    [ValueRange(0, 99999, 0, 200)]
    public MetaDoubleParam Volume { get; set; } = new(100);

    public List<AudioEffectBase> AudioEffects { get; set; } = new();

    public List<VisualEffectBase> VisualEffects { get; set; } = new();

    public TimelineReferenceObject()
    {
    }

    public TimelineReferenceObject(string id) : base(id)
    {
    }

    public override (ClipObject firstClip, ClipObject secondClip) SplitAtFrame(int splitFrame)
    {
        var (firstClip, secondClip) = base.SplitAtFrame(splitFrame);

        var firstReference = (TimelineReferenceObject)firstClip;
        var secondReference = (TimelineReferenceObject)secondClip;

        firstReference.Id = Id + "_part1";
        secondReference.Id = Id + "_part2";

        int relativeSplitFrame = splitFrame - StartFrame;
        int clipLength = EndFrame - StartFrame + 1;

        var (firstX, secondX) = X.Split(relativeSplitFrame, clipLength);
        firstReference.X = firstX;
        secondReference.X = secondX;

        var (firstY, secondY) = Y.Split(relativeSplitFrame, clipLength);
        firstReference.Y = firstY;
        secondReference.Y = secondY;

        var (firstScale, secondScale) = Scale.Split(relativeSplitFrame, clipLength);
        firstReference.Scale = firstScale;
        secondReference.Scale = secondScale;

        var (firstAlpha, secondAlpha) = Alpha.Split(relativeSplitFrame, clipLength);
        firstReference.Alpha = firstAlpha;
        secondReference.Alpha = secondAlpha;

        var (firstRotation, secondRotation) = Rotation.Split(relativeSplitFrame, clipLength);
        firstReference.Rotation = firstRotation;
        secondReference.Rotation = secondRotation;

        var (firstVolume, secondVolume) = Volume.Split(relativeSplitFrame);
        firstReference.Volume = firstVolume;
        secondReference.Volume = secondVolume;

        firstReference.SourceStartFrame = new MetaDoubleParam(SourceStartFrame.Value);
        secondReference.SourceStartFrame = new MetaDoubleParam(SourceStartFrame.Value + relativeSplitFrame);

        var (firstBlendMode, secondBlendMode) = BlendMode.Split();
        firstReference.BlendMode = firstBlendMode;
        secondReference.BlendMode = secondBlendMode;

        return (firstReference, secondReference);
    }

    public async Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryResolveTimeline(context, out TimelineObject? targetTimeline))
        {
            return new NormalRenderNode();
        }

        int relativeFrame = context.Frame - StartFrame;
        int clipLength = EndFrame - StartFrame + 1;
        int sourceFrame = (int)Math.Floor(SourceStartFrame.Value);
        int targetFrame = relativeFrame + sourceFrame;
        int lastFrame = targetTimeline.GetLastFrameOfClips();
        if (targetFrame < 0 || targetFrame > lastFrame)
        {
            return new NormalRenderNode();
        }

        try
        {
            var referencedContext = context.CreateReferencedTimelineContext(targetTimeline, targetFrame);
            var referencedNode = await targetTimeline.RenderAsync(referencedContext, cancellationToken);

            var info = new SKImageInfo(
                (int)context.RenderResolution.Width,
                (int)context.RenderResolution.Height,
                SKColorType.Rgba8888,
                SKAlphaType.Premul);

            using var surface = SKSurface.Create(info)
                ?? throw new InvalidOperationException($"Failed to create SKSurface with dimensions {info.Width}x{info.Height}");

            surface.Canvas.Clear(SKColors.Transparent);

            var compositor = new Compositor();
            await compositor.ProcessNodeAsync(
                surface.Canvas,
                referencedNode,
                context.ProjectResolution,
                context.RenderResolution,
                cancellationToken);

            var image = surface.Snapshot();
            var logicalSize = new SKSize(context.ProjectResolution.Width, context.ProjectResolution.Height);
            var transform = new Transform()
            {
                Position = new SKPoint((float)X.Get(relativeFrame, clipLength), (float)Y.Get(relativeFrame, clipLength)),
                Scale = (float)Scale.Get(relativeFrame, clipLength) / 100,
                Rotation = (float)Rotation.Get(relativeFrame, clipLength),
                Alpha = (100.0f - (float)Alpha.Get(relativeFrame, clipLength)) / 100,
            };

            long imageCacheKey = GetImageCacheKey(targetTimeline.Id, targetFrame);
            var finalResult = VisualEffectPipeline.ApplyEffects(
                image,
                VisualEffects,
                context,
                StartFrame,
                EndFrame,
                logicalSize,
                imageCacheKey);

            return new NormalRenderNode()
            {
                Image = finalResult.Image,
                LogicalSize = finalResult.LogicalSize,
                Transform = transform,
                BlendMode = BlendMode.Value,
                ImageCacheKey = finalResult.ImageCacheKey,
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to render referenced timeline '{TargetTimelineId}': {ex.Message}");
            return new NormalRenderNode();
        }
    }

    public async Task<IAudioChunk> GetAudioChunkAsync(GetAudioContext context)
    {
        IAudioChunk result = new AudioChunk(context.Format, context.RequiredLength);

        if (context.RequiredLength <= 0)
        {
            return ApplyAudioEffects(result, context);
        }

        if (!TryResolveTimeline(context, out TimelineObject? targetTimeline))
        {
            return ApplyAudioEffects(result, context);
        }

        int clipLength = EndFrame - StartFrame + 1;
        int relativeFrame = (int)Math.Floor(context.StartSamplePosition * context.ProjectFrameRate / context.Format.SampleRate);
        int sourceFrame = (int)Math.Floor(SourceStartFrame.Value);
        if (sourceFrame < 0)
        {
            return ApplyAudioEffects(result, context);
        }

        try
        {
            long sourceStartSampleOffset = (long)Math.Floor(sourceFrame * (context.Format.SampleRate / context.ProjectFrameRate));
            long referencedStartSample = context.StartSamplePosition + sourceStartSampleOffset;
            if (referencedStartSample < 0)
            {
                return ApplyAudioEffects(result, context);
            }

            double targetDuration = Math.Max(0, targetTimeline.GetLastFrameOfClips()) / context.ProjectFrameRate;
            var referencedContext = context.CreateReferencedTimelineContext(
                targetTimeline,
                referencedStartSample,
                context.RequiredLength,
                targetDuration);

            result = await targetTimeline.GetAudioChunkAsync(referencedContext);

            double gain = Volume.Value / 100.0;
            for (long i = 0; i < result.Samples.Length; i++)
            {
                result.Samples[i] = Math.Clamp(result.Samples[i] * gain, -1.0, 1.0);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to render referenced timeline audio '{TargetTimelineId}': {ex.Message}");
            return ApplyAudioEffects(result, context);
        }

        return ApplyAudioEffects(result, context);
    }

    private bool TryResolveTimeline(RenderContext context, out TimelineObject? timeline)
    {
        timeline = null;

        if (!context.TryResolveTimeline(TargetTimelineId, out timeline) || timeline is null || !timeline.IsActive)
        {
            return false;
        }

        return !context.IsTimelineInReferenceStack(timeline.Id);
    }

    private bool TryResolveTimeline(GetAudioContext context, out TimelineObject? timeline)
    {
        timeline = null;

        if (!context.TryResolveTimeline(TargetTimelineId, out timeline) || timeline is null || !timeline.IsActive)
        {
            return false;
        }

        return !context.IsTimelineInReferenceStack(timeline.Id);
    }

    private IAudioChunk ApplyAudioEffects(IAudioChunk chunk, GetAudioContext context)
    {
        IAudioChunk result = chunk;
        AudioEffectContext effectContext = new(this, context);

        foreach (var effect in AudioEffects)
        {
            if (!effect.IsActive) continue;
            result = effect.Apply(result, effectContext);
        }

        return result;
    }

    private long GetImageCacheKey(string timelineId, int targetFrame)
    {
        var hash = new HashCode();
        hash.Add(nameof(TimelineReferenceObject));
        hash.Add(Id);
        hash.Add(timelineId);
        hash.Add(targetFrame);
        return hash.ToHashCode();
    }

    protected override ClipObject CreateCopy()
    {
        var xml = Xml.MetasiaObjectXmlSerializer.Serialize(this);
        var copy = Xml.MetasiaObjectXmlSerializer.Deserialize<TimelineReferenceObject>(xml);
        copy.Id = Id + "_copy";
        return copy;
    }
}
