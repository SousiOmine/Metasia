using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("ScaleEffect", DisplayKey = "effect.visual.scale.name", FallbackText = "拡大率")]
public class ScaleEffect : VisualEffectBase
{
    [EditableProperty("ScaleX", DisplayKey = "property.effect.scale.x", FallbackText = "X方向")]
    [ValueRange(0, 1000, 0, 500)]
    public MetaNumberParam<double> ScaleX { get; set; } = new(100);

    [EditableProperty("ScaleY", DisplayKey = "property.effect.scale.y", FallbackText = "Y方向")]
    [ValueRange(0, 1000, 0, 500)]
    public MetaNumberParam<double> ScaleY { get; set; } = new(100);

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        double scaleX = ScaleX.Get(relativeFrame, clipLength) / 100.0;
        double scaleY = ScaleY.Get(relativeFrame, clipLength) / 100.0;

        if (Math.Abs(scaleX - 1.0) < double.Epsilon && Math.Abs(scaleY - 1.0) < double.Epsilon)
        {
            return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize);
        }

        int srcWidth = input.Width;
        int srcHeight = input.Height;

        int newWidth = Math.Max(1, (int)Math.Round(srcWidth * scaleX));
        int newHeight = Math.Max(1, (int)Math.Round(srcHeight * scaleY));

        var newLogicalSize = new SKSize(
            (float)(context.LogicalSize.Width * scaleX),
            (float)(context.LogicalSize.Height * scaleY));

        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            var cachedImage = context.ImageCache?.TryGet(cacheKey);
            if (cachedImage is not null)
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
            using var paint = new SKPaint();
            paint.IsAntialias = true;
            var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);
            var destRect = SKRect.Create(0, 0, newWidth, newHeight);
            canvas.DrawImage(drawImage, destRect, sampling, paint);
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

        return new VisualEffectResult(result, IRenderImageCache.NO_CACHE_KEY, newLogicalSize);
    }

    private long GetImageHashCode(VisualEffectContext context)
    {
        var hash = new HashCode();
        hash.Add(nameof(ScaleEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(ScaleX.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(ScaleY.Get(context.RelativeFrame, context.ClipLength));
        return hash.ToHashCode();
    }
}
