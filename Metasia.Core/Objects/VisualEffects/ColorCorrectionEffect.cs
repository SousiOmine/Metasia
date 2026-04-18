using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("ColorCorrectionEffect", DisplayKey = "effect.visual.color_correction.name", FallbackText = "色調補正")]
public class ColorCorrectionEffect : VisualEffectBase
{
    [EditableProperty("Brightness", DisplayKey = "property.effect.color_correction.brightness", FallbackText = "明るさ")]
    [ValueRange(0, 200, 0, 200)]
    public MetaNumberParam<double> Brightness { get; set; } = new MetaNumberParam<double>(100);

    [EditableProperty("Contrast", DisplayKey = "property.effect.color_correction.contrast", FallbackText = "コントラスト")]
    [ValueRange(-100, 100, -100, 100)]
    public MetaNumberParam<double> Contrast { get; set; } = new MetaNumberParam<double>(0);

    [EditableProperty("Saturation", DisplayKey = "property.effect.color_correction.saturation", FallbackText = "彩度")]
    [ValueRange(-100, 100, -100, 100)]
    public MetaNumberParam<double> Saturation { get; set; } = new MetaNumberParam<double>(0);

    [EditableProperty("HueShift", DisplayKey = "property.effect.color_correction.hue_shift", FallbackText = "色相シフト")]
    [ValueRange(-180, 180, -180, 180)]
    public MetaNumberParam<double> HueShift { get; set; } = new MetaNumberParam<double>(0);

    [EditableProperty("Gamma", DisplayKey = "property.effect.color_correction.gamma", FallbackText = "ガンマ")]
    [ValueRange(0.1, 10, 0.1, 5)]
    public MetaNumberParam<double> Gamma { get; set; } = new MetaNumberParam<double>(1);

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        double brightness = Brightness.Get(relativeFrame, clipLength);
        double contrast = Contrast.Get(relativeFrame, clipLength);
        double saturation = Saturation.Get(relativeFrame, clipLength);
        double hueShift = HueShift.Get(relativeFrame, clipLength);
        double gamma = Gamma.Get(relativeFrame, clipLength);

        if (brightness == 100 && contrast == 0 && saturation == 0 && hueShift == 0 && gamma == 1.0)
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

        using var colorFilter = CreateColorFilter(brightness, contrast, saturation, hueShift, gamma);

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

    private static SKColorFilter CreateColorFilter(double brightness, double contrast, double saturation, double hueShift, double gamma)
    {
        float b = (float)(brightness / 100.0);
        float c = (float)((contrast + 100.0) / 100.0);
        float s = (float)((saturation + 100.0) / 100.0);
        float g = (float)gamma;

        float sR = (1f - s) * 0.2126f;
        float sG = (1f - s) * 0.7152f;
        float sB = (1f - s) * 0.0722f;

        float midPoint = 0.5f;
        float cOffset = midPoint * (1f - c) * b;

        float[] matrix = new float[20];
        matrix[0] = c * b * (sR + s); matrix[1] = c * b * sG; matrix[2] = c * b * sB; matrix[3] = 0; matrix[4] = cOffset;
        matrix[5] = c * b * sR; matrix[6] = c * b * (sG + s); matrix[7] = c * b * sB; matrix[8] = 0; matrix[9] = cOffset;
        matrix[10] = c * b * sR; matrix[11] = c * b * sG; matrix[12] = c * b * (sB + s); matrix[13] = 0; matrix[14] = cOffset;
        matrix[15] = 0; matrix[16] = 0; matrix[17] = 0; matrix[18] = 1; matrix[19] = 0;

        SKColorFilter result = SKColorFilter.CreateColorMatrix(matrix);

        if (Math.Abs(hueShift) >= 0.01)
        {
            result = SKColorFilter.CreateCompose(result, CreateHueShiftFilter((float)hueShift));
        }

        if (Math.Abs(gamma - 1.0) >= 0.01)
        {
            result = SKColorFilter.CreateCompose(result, CreateGammaFilter(1f / g));
        }

        return result;
    }

    private static SKColorFilter CreateHueShiftFilter(float degrees)
    {
        float rad = degrees * MathF.PI / 180f;
        float cosA = MathF.Cos(rad);
        float sinA = MathF.Sin(rad);

        float a00 = 0.213f + cosA * 0.787f - sinA * 0.213f;
        float a01 = 0.715f - cosA * 0.715f - sinA * 0.715f;
        float a02 = 0.072f - cosA * 0.072f + sinA * 0.928f;
        float a10 = 0.213f - cosA * 0.213f + sinA * 0.143f;
        float a11 = 0.715f + cosA * 0.285f + sinA * 0.140f;
        float a12 = 0.072f - cosA * 0.072f - sinA * 0.283f;
        float a20 = 0.213f - cosA * 0.213f - sinA * 0.787f;
        float a21 = 0.715f - cosA * 0.715f + sinA * 0.715f;
        float a22 = 0.072f + cosA * 0.928f + sinA * 0.072f;

        return SKColorFilter.CreateColorMatrix(new float[]
        {
            a00, a01, a02, 0, 0,
            a10, a11, a12, 0, 0,
            a20, a21, a22, 0, 0,
            0, 0, 0, 1, 0
        });
    }

    private static SKColorFilter CreateGammaFilter(float gammaInv)
    {
        byte[] table = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            float normalized = i / 255f;
            float corrected = MathF.Pow(normalized, gammaInv);
            table[i] = (byte)Math.Clamp((int)(corrected * 255f), 0, 255);
        }
        return SKColorFilter.CreateTable(table);
    }

    private long GetImageHashCode(VisualEffectContext context)
    {
        var hash = new HashCode();
        hash.Add(nameof(ColorCorrectionEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(Brightness.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(Contrast.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(Saturation.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(HueShift.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(Gamma.Get(context.RelativeFrame, context.ClipLength));
        return hash.ToHashCode();
    }
}