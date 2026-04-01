using System.Reflection.Metadata;
using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Objects.Parameters.Color;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// 縁取りエフェクト - オブジェクトの輪郭に沿って縁取りを描画する
    /// </summary>
    [VisualEffectIdentifier("BorderEffect", DisplayKey = "effect.visual.border.name", FallbackText = "縁取り")]
    public class BorderEffect : VisualEffectBase
    {
        [EditableProperty("BorderSize", DisplayKey = "property.effect.border.size", FallbackText = "縁取りサイズ")]
        [ValueRange(0, 500, 0, 50)]
        public MetaNumberParam<double> Size { get; set; } = new MetaNumberParam<double>(3);

        [EditableProperty("BorderColor", DisplayKey = "property.effect.border.color", FallbackText = "縁取り色")]
        public ColorRgb8 Color { get; set; } = new ColorRgb8(0, 0, 0);

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            if (input is null) return new VisualEffectResult(input, context.TargetImageCacheKey);

            int relativeFrame = context.RelativeFrame;
            int clipLength = context.ClipLength;

            float size = (float)Size.Get(relativeFrame, clipLength);
            if (size <= 0) return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize);

            int width = input.Width;
            int height = input.Height;

            int expand = (int)Math.Ceiling(size);
            int newWidth = width + expand * 2;
            int newHeight = height + expand * 2;
            var newLogicalSize = new SKSize(newWidth, newHeight);

            if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
            {
                long cacheKey = GetImageHashCode(context);
                var cachedImage = context.ImageCache?.TryGet(cacheKey);
                if (cachedImage is not null)
                {
                    return new VisualEffectResult(cachedImage, cacheKey, newLogicalSize);
                }
            }

            SKColor color = new SKColor(Color.R, Color.G, Color.B, 255);

            var info = new SKImageInfo(newWidth, newHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            using var dilateFilter = SKImageFilter.CreateDilate((int)Math.Ceiling(size), (int)Math.Ceiling(size));
            using var colorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcIn);

            using var borderPaint = new SKPaint();
            borderPaint.ImageFilter = dilateFilter;
            borderPaint.ColorFilter = colorFilter;
            canvas.DrawImage(input, expand, expand, borderPaint);

            canvas.DrawImage(input, expand, expand);

            var result = surface.Snapshot();

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
            hash.Add(nameof(BorderEffect));
            hash.Add(context.TargetImageCacheKey);
            hash.Add(Size.Get(context.RelativeFrame, context.ClipLength));
            hash.Add(Color.R);
            hash.Add(Color.G);
            hash.Add(Color.B);
            return hash.ToHashCode();
        }
    }
}
