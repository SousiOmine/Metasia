using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("GradientOverlayEffect", DisplayKey = "effect.visual.gradient_overlay.name", FallbackText = "グラデーション")]
public class GradientOverlayEffect : VisualEffectBase
{
    [EditableProperty("StartColor", DisplayKey = "property.effect.gradient_overlay.start_color", FallbackText = "開始色")]
    public ColorRgb8 StartColor { get; set; } = new ColorRgb8(0, 0, 0);

    [EditableProperty("EndColor", DisplayKey = "property.effect.gradient_overlay.end_color", FallbackText = "終了色")]
    public ColorRgb8 EndColor { get; set; } = new ColorRgb8(255, 255, 255);

    [EditableProperty("Angle", DisplayKey = "property.effect.gradient_overlay.angle", FallbackText = "角度")]
    [ValueRange(0, 360, 0, 360)]
    public MetaNumberParam<double> Angle { get; set; } = new MetaNumberParam<double>(0);

    [EditableProperty("Opacity", DisplayKey = "property.effect.gradient_overlay.opacity", FallbackText = "不透明度")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Opacity { get; set; } = new MetaNumberParam<double>(50);

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        double opacity = Opacity.Get(relativeFrame, clipLength);
        if (opacity <= 0)
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

        float angle = (float)Angle.Get(relativeFrame, clipLength);
        float opacityF = (float)(opacity / 100.0);

        int width = input.Width;
        int height = input.Height;

        float centerX = width / 2f;
        float centerY = height / 2f;
        float rad = angle * MathF.PI / 180f;
        float halfDiagonal = MathF.Sqrt(centerX * centerX + centerY * centerY);

        float startX = centerX - MathF.Sin(rad) * halfDiagonal;
        float startY = centerY + MathF.Cos(rad) * halfDiagonal;
        float endX = centerX + MathF.Sin(rad) * halfDiagonal;
        float endY = centerY - MathF.Cos(rad) * halfDiagonal;

        var startColor = new SKColor(StartColor.R, StartColor.G, StartColor.B, (byte)(opacityF * 255));
        var endColor = new SKColor(EndColor.R, EndColor.G, EndColor.B, (byte)(opacityF * 255));

        var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var surface = context.SurfaceFactory.CreateSurface(info);
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        var drawImage = context.SurfaceFactory.GetDrawImage(input);
        try
        {
            canvas.DrawImage(drawImage, 0, 0);

            using var gradientPaint = new SKPaint();
            gradientPaint.IsAntialias = true;
            gradientPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(startX, startY),
                new SKPoint(endX, endY),
                new[] { startColor, endColor },
                SKShaderTileMode.Clamp);
            gradientPaint.BlendMode = SKBlendMode.SrcOver;

            canvas.DrawRect(0, 0, width, height, gradientPaint);
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
        hash.Add(nameof(GradientOverlayEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(Angle.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(Opacity.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(StartColor.R);
        hash.Add(StartColor.G);
        hash.Add(StartColor.B);
        hash.Add(EndColor.R);
        hash.Add(EndColor.G);
        hash.Add(EndColor.B);
        return hash.ToHashCode();
    }
}