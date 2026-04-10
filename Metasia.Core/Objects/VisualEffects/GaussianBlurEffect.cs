using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("GaussianBlurEffect", DisplayKey = "effect.visual.gaussian_blur.name", FallbackText = "ぼかし")]
public class GaussianBlurEffect : VisualEffectBase
{
    [EditableProperty("Strength", DisplayKey = "property.effect.gaussian_blur.strength", FallbackText = "強度")]
    [ValueRange(0, 200, 0, 100)]
    public MetaNumberParam<double> Strength { get; set; } = new MetaNumberParam<double>(10);

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

        float expandLogical = strength * 3;
        int expandX = Math.Max(1, (int)Math.Ceiling(expandLogical * logicalScaleX));
        int expandY = Math.Max(1, (int)Math.Ceiling(expandLogical * logicalScaleY));

        int newWidth = width + expandX * 2;
        int newHeight = height + expandY * 2;
        var newLogicalSize = new SKSize(context.LogicalSize.Width + expandLogical * 2, context.LogicalSize.Height + expandLogical * 2);

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
            using var blurFilter = SKImageFilter.CreateBlur(strength, strength);
            using var paint = new SKPaint();
            paint.ImageFilter = blurFilter;

            canvas.DrawImage(drawImage, expandX, expandY, paint);
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

    private long GetImageHashCode(VisualEffectContext context)
    {
        var hash = new HashCode();
        hash.Add(nameof(GaussianBlurEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(Strength.Get(context.RelativeFrame, context.ClipLength));
        return hash.ToHashCode();
    }
}