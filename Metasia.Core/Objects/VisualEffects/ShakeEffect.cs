using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("ShakeEffect", DisplayKey = "effect.visual.shake.name", FallbackText = "振動")]
public class ShakeEffect : VisualEffectBase
{
    [EditableProperty("Strength", DisplayKey = "property.effect.shake.strength", FallbackText = "強さ")]
    [ValueRange(0, 500, 0, 100)]
    public MetaNumberParam<double> Strength { get; set; } = new MetaNumberParam<double>(10);

    [EditableProperty("Seed", DisplayKey = "property.effect.shake.seed", FallbackText = "シード")]
    [ValueRange(0, 10000, 0, 10000)]
    public MetaIntParam Seed { get; set; } = new MetaIntParam(0);

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        float strength = (float)Strength.Get(relativeFrame, clipLength);
        if (strength <= 0) return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize);

        int width = input.Width;
        int height = input.Height;

        float logicalScaleX = context.LogicalSize.Width > 0 ? width / context.LogicalSize.Width : 1f;
        float logicalScaleY = context.LogicalSize.Height > 0 ? height / context.LogicalSize.Height : 1f;

        float scaledStrengthX = strength * logicalScaleX;
        float scaledStrengthY = strength * logicalScaleY;

        int seed = Seed.Value;

        float offsetX = GetShakeOffset(seed, relativeFrame, 0) * scaledStrengthX;
        float offsetY = GetShakeOffset(seed, relativeFrame, 1) * scaledStrengthY;

        int expandX = Math.Max(1, (int)Math.Ceiling(Math.Abs(scaledStrengthX)));
        int expandY = Math.Max(1, (int)Math.Ceiling(Math.Abs(scaledStrengthY)));

        int newWidth = width + expandX * 2;
        int newHeight = height + expandY * 2;
        float expandLogicalX = strength;
        float expandLogicalY = strength;
        var newLogicalSize = new SKSize(
            context.LogicalSize.Width + expandLogicalX * 2,
            context.LogicalSize.Height + expandLogicalY * 2);

        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            var cachedImage = context.ImageCache?.TryGet(cacheKey);
            if (cachedImage != null)
            {
                return new VisualEffectResult(cachedImage, cacheKey, newLogicalSize);
            }
        }

        var info = new SKImageInfo(newWidth, newHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = context.SurfaceFactory.CreateSurface(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var drawImage = context.SurfaceFactory.GetDrawImage(input);
        try
        {
            canvas.DrawImage(drawImage, expandX + offsetX, expandY + offsetY);
        }
        finally
        {
            if (!ReferenceEquals(drawImage, input))
            {
                drawImage.Dispose();
            }
        }

        var result = context.SurfaceFactory.Snapshot(surface, context.PreferRasterOutput);
        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            context.ImageCache?.Set(cacheKey, result);
            return new VisualEffectResult(result, cacheKey, newLogicalSize);
        }
        else
        {
            return new VisualEffectResult(result, IRenderImageCache.NO_CACHE_KEY, newLogicalSize);
        }
    }

    private static float GetShakeOffset(int seed, int frame, int componentIndex)
    {
        uint h = (uint)seed * 374761393u
               + (uint)frame * 668265263u
               + (uint)componentIndex * 1274126177u;
        h ^= h >> 13;
        h *= 1274126177u;
        h ^= h >> 16;
        return (h / (float)uint.MaxValue) * 2f - 1f;
    }

    private long GetImageHashCode(VisualEffectContext context)
    {
        var hash = new HashCode();
        hash.Add(nameof(ShakeEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(Strength.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(Seed.Value);
        hash.Add(context.RelativeFrame);
        return hash.ToHashCode();
    }
}
