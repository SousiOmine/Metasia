using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects;

[VisualEffectIdentifier("DropShadowEffect", DisplayKey = "effect.visual.drop_shadow.name", FallbackText = "ドロップシャドウ")]
public class DropShadowEffect : VisualEffectBase
{
    [EditableProperty("OffsetX", DisplayKey = "property.effect.drop_shadow.offset_x", FallbackText = "Xオフセット")]
    [ValueRange(-500, 500, -100, 100)]
    public MetaNumberParam<double> OffsetX { get; set; } = new MetaNumberParam<double>(5);

    [EditableProperty("OffsetY", DisplayKey = "property.effect.drop_shadow.offset_y", FallbackText = "Yオフセット")]
    [ValueRange(-500, 500, -100, 100)]
    public MetaNumberParam<double> OffsetY { get; set; } = new MetaNumberParam<double>(5);

    [EditableProperty("BlurSize", DisplayKey = "property.effect.drop_shadow.blur", FallbackText = "ぼかし")]
    [ValueRange(0, 200, 0, 50)]
    public MetaNumberParam<double> BlurSize { get; set; } = new MetaNumberParam<double>(5);

    [EditableProperty("Opacity", DisplayKey = "property.effect.drop_shadow.opacity", FallbackText = "不透明度")]
    [ValueRange(0, 100, 0, 100)]
    public MetaNumberParam<double> Opacity { get; set; } = new MetaNumberParam<double>(50);

    [EditableProperty("ShadowColor", DisplayKey = "property.effect.drop_shadow.color", FallbackText = "シャドウ色")]
    public ColorRgb8 Color { get; set; } = new ColorRgb8(0, 0, 0);

    public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
    {
        ArgumentNullException.ThrowIfNull(input);

        int relativeFrame = context.RelativeFrame;
        int clipLength = context.ClipLength;

        float offsetX = (float)OffsetX.Get(relativeFrame, clipLength);
        float offsetY = (float)OffsetY.Get(relativeFrame, clipLength);
        float blurSize = (float)BlurSize.Get(relativeFrame, clipLength);
        float opacity = (float)Opacity.Get(relativeFrame, clipLength);

        if (opacity <= 0) return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize);

        int width = input.Width;
        int height = input.Height;

        float logicalScaleX = context.LogicalSize.Width > 0 ? width / context.LogicalSize.Width : 1f;
        float logicalScaleY = context.LogicalSize.Height > 0 ? height / context.LogicalSize.Height : 1f;

        float blurMarginLogical = blurSize * 3;
        float expandLogicalLeft = (offsetX < 0 ? Math.Abs(offsetX) : 0) + blurMarginLogical;
        float expandLogicalRight = (offsetX > 0 ? Math.Abs(offsetX) : 0) + blurMarginLogical;
        float expandLogicalTop = (offsetY < 0 ? Math.Abs(offsetY) : 0) + blurMarginLogical;
        float expandLogicalBottom = (offsetY > 0 ? Math.Abs(offsetY) : 0) + blurMarginLogical;

        int expandLeft = Math.Max(1, (int)Math.Ceiling(expandLogicalLeft * logicalScaleX));
        int expandTop = Math.Max(1, (int)Math.Ceiling(expandLogicalTop * logicalScaleY));
        int expandRight = Math.Max(1, (int)Math.Ceiling(expandLogicalRight * logicalScaleX));
        int expandBottom = Math.Max(1, (int)Math.Ceiling(expandLogicalBottom * logicalScaleY));

        int newWidth = width + expandLeft + expandRight;
        int newHeight = height + expandTop + expandBottom;
        var newLogicalSize = new SKSize(
            context.LogicalSize.Width + expandLogicalLeft + expandLogicalRight,
            context.LogicalSize.Height + expandLogicalTop + expandLogicalBottom
        );

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
            float scaledBlurX = blurSize * logicalScaleX;
            float scaledBlurY = blurSize * logicalScaleY;

            int pixelOffsetX = offsetX != 0 ? (int)Math.Round(offsetX * logicalScaleX) : 0;
            int pixelOffsetY = offsetY != 0 ? (int)Math.Round(offsetY * logicalScaleY) : 0;

            int shadowX = expandLeft + pixelOffsetX;
            int shadowY = expandTop + pixelOffsetY;

            byte shadowAlpha = (byte)Math.Clamp(opacity * 255 / 100, 0, 255);
            var shadowColor = new SKColor(Color.R, Color.G, Color.B, shadowAlpha);

            using var blurFilter = blurSize > 0
                ? SKImageFilter.CreateBlur(scaledBlurX, scaledBlurY)
                : null;
            using var colorFilter = SKColorFilter.CreateBlendMode(shadowColor, SKBlendMode.SrcIn);
            using var shadowPaint = new SKPaint();
            shadowPaint.ImageFilter = blurFilter;
            shadowPaint.ColorFilter = colorFilter;

            canvas.DrawImage(drawImage, shadowX, shadowY, shadowPaint);

            canvas.DrawImage(drawImage, expandLeft, expandTop);
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
        hash.Add(nameof(DropShadowEffect));
        hash.Add(context.TargetImageCacheKey);
        hash.Add(OffsetX.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(OffsetY.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(BlurSize.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(Opacity.Get(context.RelativeFrame, context.ClipLength));
        hash.Add(Color.R);
        hash.Add(Color.G);
        hash.Add(Color.B);
        return hash.ToHashCode();
    }
}
