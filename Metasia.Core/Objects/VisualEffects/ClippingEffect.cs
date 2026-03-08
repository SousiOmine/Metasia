using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// クリッピングエフェクト - 画像の上下左右を切り取る
    /// </summary>
    [VisualEffectIdentifier("ClippingEffect", DisplayKey = "effect.visual.clipping.name", FallbackText = "クリッピング")]
    public class ClippingEffect : VisualEffectBase
    {
        [EditableProperty("ClipTop", DisplayKey = "property.effect.clip.top", FallbackText = "上")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Top { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ClipBottom", DisplayKey = "property.effect.clip.bottom", FallbackText = "下")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Bottom { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ClipLeft", DisplayKey = "property.effect.clip.left", FallbackText = "左")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Left { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ClipRight", DisplayKey = "property.effect.clip.right", FallbackText = "右")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Right { get; set; } = new MetaNumberParam<double>(0);

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            if (input is null) return new VisualEffectResult(input, context.TargetImageCacheKey);

            int relativeFrame = context.RelativeFrame;
            int clipLength = context.ClipLength;

            int top = (int)Top.Get(relativeFrame, clipLength);
            int bottom = (int)Bottom.Get(relativeFrame, clipLength);
            int left = (int)Left.Get(relativeFrame, clipLength);
            int right = (int)Right.Get(relativeFrame, clipLength);

            if (top == 0 && bottom == 0 && left == 0 && right == 0)
            {
                return new VisualEffectResult(input, context.TargetImageCacheKey);
            }

            if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
            {
                long cacheKey = GetImageHashCode(context);
                var cachedImage = context.ImageCache?.TryGet(cacheKey);
                if (cachedImage is not null)
                {
                    return new VisualEffectResult(cachedImage, cacheKey);
                }
            }

            int srcWidth = input.Width;
            int srcHeight = input.Height;

            int newWidth = Math.Max(1, srcWidth - left - right);
            int newHeight = Math.Max(1, srcHeight - top - bottom);

            var info = new SKImageInfo(srcWidth, srcHeight, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            // クリッピング領域を設定して描画
            canvas.ClipRect(new SKRect(left, top, srcWidth - right, srcHeight - bottom));
            canvas.DrawImage(input, 0, 0);

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
            hash.Add(nameof(ClippingEffect));
            hash.Add(context.TargetImageCacheKey);
            hash.Add(Top.Get(context.RelativeFrame, context.ClipLength));
            hash.Add(Bottom.Get(context.RelativeFrame, context.ClipLength));
            hash.Add(Left.Get(context.RelativeFrame, context.ClipLength));
            hash.Add(Right.Get(context.RelativeFrame, context.ClipLength));
            return hash.ToHashCode();
        }
    }
}
