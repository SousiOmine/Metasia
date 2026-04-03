using Metasia.Core.Attributes;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

/// <summary>
/// フリップエフェクト - 水平方向と垂直方向の反転をつけることができる
/// </summary>
[VisualEffectIdentifier("FlipEffect", DisplayKey = "effect.visual.flip.name", FallbackText = "反転")]
public class FlipEffect : VisualEffectBase
{
    [EditableProperty("FlipHorizontal", DisplayKey = "property.effect.flip.horizontal", FallbackText = "左右反転")]
    public bool FlipHorizontal { get; set; } = false;

    [EditableProperty("FlipVertical", DisplayKey = "property.effect.flip.vertical", FallbackText = "上下反転")]
    public bool FlipVertical { get; set; } = false;

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            ArgumentNullException.ThrowIfNull(input);

        if (!FlipHorizontal && !FlipVertical)
        {
            return new VisualEffectResult(input, context.TargetImageCacheKey);
        }

        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            var cachedImage = context.ImageCache?.TryGet(cacheKey);
            if (cachedImage != null)
            {
                return new VisualEffectResult(cachedImage, cacheKey);
            }
        }


        int width = input.Width;
        int height = input.Height;

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = SKSurface.Create(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint();
        paint.IsAntialias = true;
        float scaleX = FlipHorizontal ? -1 : 1;
        float scaleY = FlipVertical ? -1 : 1;
        if (FlipHorizontal)
        {
            canvas.Translate(width, 0);
        }
        if (FlipVertical)
        {
            canvas.Translate(0, height);
        }
        canvas.Scale(scaleX, scaleY);
        canvas.DrawImage(input, 0, 0, paint);

        var result = surface.Snapshot();
        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            context.ImageCache?.Set(cacheKey, result);
            return new VisualEffectResult(result, cacheKey);
        }
        else
        {
            return new VisualEffectResult(result, IRenderImageCache.NO_CACHE_KEY);
        }
    }

    private long GetImageHashCode(VisualEffectContext context)
    {
        var hash = new HashCode();
        hash.Add(nameof(FlipEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(FlipHorizontal);
        hash.Add(FlipVertical);
        return hash.ToHashCode();
    }
}
