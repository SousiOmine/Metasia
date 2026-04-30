using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("BasicTransformEffect", DisplayKey = "effect.visual.basic_transform.name", FallbackText = "基本移動")]
public class BasicTransformEffect : VisualEffectBase
{
    [EditableProperty("PositionX", DisplayKey = "property.effect.basic_transform.position_x", FallbackText = "X位置")]
    [ValueRange(-10000, 10000, -2000, 2000)]
    public MetaNumberParam<double> PositionX { get; set; } = new(0);

    [EditableProperty("PositionY", DisplayKey = "property.effect.basic_transform.position_y", FallbackText = "Y位置")]
    [ValueRange(-10000, 10000, -2000, 2000)]
    public MetaNumberParam<double> PositionY { get; set; } = new(0);

    [EditableProperty("Scale", DisplayKey = "property.effect.basic_transform.scale", FallbackText = "拡大率")]
    [ValueRange(0, 1000, 0, 500)]
    public MetaNumberParam<double> Scale { get; set; } = new(100);

    [EditableProperty("Rotation", DisplayKey = "property.effect.basic_transform.rotation", FallbackText = "回転")]
    [ValueRange(-360, 360, -180, 180)]
    public MetaNumberParam<double> Rotation { get; set; } = new(0);

    [EditableProperty("Alpha", DisplayKey = "property.effect.basic_transform.alpha", FallbackText = "不透明度")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Alpha { get; set; } = new(100);

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        float posX = (float)PositionX.Get(relativeFrame, clipLength);
        float posY = (float)PositionY.Get(relativeFrame, clipLength);
        float scale = (float)Scale.Get(relativeFrame, clipLength) / 100f;
        float rotation = (float)Rotation.Get(relativeFrame, clipLength);
        float alpha = (float)Alpha.Get(relativeFrame, clipLength) / 100f;

        var transformOffset = new Transform
        {
            Position = new SKPoint(posX, posY),
            Scale = scale,
            Rotation = rotation,
            Alpha = alpha,
        };

        return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize)
        {
            TransformOffset = transformOffset
        };
    }
}
