using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("MonochromeEffect", DisplayKey = "effect.visual.monochrome.name", FallbackText = "単色化")]
public class MonochromeEffect : VisualEffectBase
{
    [EditableProperty("TintColor", DisplayKey = "property.effect.monochrome.tint_color", FallbackText = "着色色")]
    public ColorRgb8 TintColor { get; set; } = new ColorRgb8(255, 255, 255);

    [EditableProperty("Intensity", DisplayKey = "property.effect.monochrome.intensity", FallbackText = "強度")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Intensity { get; set; } = new MetaNumberParam<double>(100);

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        double intensity = Intensity.Get(relativeFrame, clipLength);
        if (intensity <= 0)
        {
            return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize);
        }

        if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
        {
            long cacheKey = GetImageHashCode(context);
            var cachedImage = context.ImageCache?.TryGet(cacheKey);
            if (cachedImage is not null)
            {
                return new VisualEffectResult(cachedImage, cacheKey, context.LogicalSize);
            }
        }

        float intensityF = (float)(intensity / 100.0);

        float luminR = 0.2126f;
        float luminG = 0.7152f;
        float luminB = 0.0722f;

        float tR = TintColor.R / 255f;
        float tG = TintColor.G / 255f;
        float tB = TintColor.B / 255f;

        float grayR = intensityF * tR;
        float grayG = intensityF * tG;
        float grayB = intensityF * tB;

        float identFactor = 1f - intensityF;

        float[] combinedMatrix = new float[20];
        combinedMatrix[0] = grayR * luminR + identFactor; combinedMatrix[1] = grayR * luminG; combinedMatrix[2] = grayR * luminB; combinedMatrix[3] = 0; combinedMatrix[4] = 0;
        combinedMatrix[5] = grayG * luminR; combinedMatrix[6] = grayG * luminG + identFactor; combinedMatrix[7] = grayG * luminB; combinedMatrix[8] = 0; combinedMatrix[9] = 0;
        combinedMatrix[10] = grayB * luminR; combinedMatrix[11] = grayB * luminG; combinedMatrix[12] = grayB * luminB + identFactor; combinedMatrix[13] = 0; combinedMatrix[14] = 0;
        combinedMatrix[15] = 0; combinedMatrix[16] = 0; combinedMatrix[17] = 0; combinedMatrix[18] = 1; combinedMatrix[19] = 0;

        using var colorFilter = SKColorFilter.CreateColorMatrix(combinedMatrix);

        var info = new SKImageInfo(input.Width, input.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = context.SurfaceFactory.CreateSurface(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var drawImage = context.SurfaceFactory.GetDrawImage(input);
        try
        {
            using var paint = new SKPaint();
            paint.ColorFilter = colorFilter;
            canvas.DrawImage(drawImage, 0, 0, paint);
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
            return new VisualEffectResult(result, cacheKey, context.LogicalSize);
        }
        else
        {
            return new VisualEffectResult(result, IRenderImageCache.NO_CACHE_KEY, context.LogicalSize);
        }
    }

    private long GetImageHashCode(VisualEffectContext context)
    {
        var hash = new HashCode();
        hash.Add(nameof(MonochromeEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(Intensity.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(TintColor.R);
        hash.Add(TintColor.G);
        hash.Add(TintColor.B);
        return hash.ToHashCode();
    }
}