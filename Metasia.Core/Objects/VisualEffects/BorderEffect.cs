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
    [VisualEffectIdentifier("BorderEffect")]
    public class BorderEffect : VisualEffectBase
    {
        [EditableProperty("BorderSize")]
        [ValueRange(0, 500, 0, 50)]
        public MetaNumberParam<double> Size { get; set; } = new MetaNumberParam<double>(3);

        [EditableProperty("BorderColor")]
        public ColorRgb8 Color { get; set; } = new ColorRgb8(0, 0, 0);

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            if (input is null) return new VisualEffectResult(input, context.TargetImageCacheKey);

            if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
            {
                long cacheKey = GetImageHashCode(context);
                var cachedImage = context.ImageCache?.TryGet(cacheKey);
                if (cachedImage is not null)
                {
                    return new VisualEffectResult(cachedImage, cacheKey);
                }
            }
            

            int relativeFrame = context.RelativeFrame;
            int clipLength = context.ClipLength;

            float size = (float)Size.Get(relativeFrame, clipLength);
            if (size <= 0) return new VisualEffectResult(input, context.TargetImageCacheKey);

            SKColor color = new SKColor(Color.R, Color.G, Color.B, 255);

            int width = input.Width;
            int height = input.Height;

            // 縁取り分だけ大きいサーフェスを作成
            int expand = (int)Math.Ceiling(size);
            int newWidth = width + expand * 2;
            int newHeight = height + expand * 2;

            var info = new SKImageInfo(newWidth, newHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            // 膨張（Dilate）フィルタで縁取りを作成
            using var dilateFilter = SKImageFilter.CreateDilate((int)Math.Ceiling(size), (int)Math.Ceiling(size));
            using var colorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcIn);

            // 1. 膨張した画像を色付きで描画（縁取り部分）
            using var borderPaint = new SKPaint();
            borderPaint.ImageFilter = dilateFilter;
            borderPaint.ColorFilter = colorFilter;
            canvas.DrawImage(input, expand, expand, borderPaint);

            // 2. 元の画像を上に重ねる
            canvas.DrawImage(input, expand, expand);

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
