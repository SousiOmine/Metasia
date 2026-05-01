using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects
{
    [VisualEffectIdentifier("AreaExpansionEffect", DisplayKey = "effect.visual.area_expansion.name", FallbackText = "領域拡張")]
    public class AreaExpansionEffect : VisualEffectBase
    {
        [EditableProperty("ExpandTop", DisplayKey = "property.effect.area_expansion.top", FallbackText = "上")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Top { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ExpandBottom", DisplayKey = "property.effect.area_expansion.bottom", FallbackText = "下")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Bottom { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ExpandLeft", DisplayKey = "property.effect.area_expansion.left", FallbackText = "左")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Left { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ExpandRight", DisplayKey = "property.effect.area_expansion.right", FallbackText = "右")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Right { get; set; } = new MetaNumberParam<double>(0);

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            ArgumentNullException.ThrowIfNull(input);

            int relativeFrame = context.RelativeFrame;
            int clipLength = context.ClipLength;

            int topValue = (int)Top.Get(relativeFrame, clipLength);
            int bottomValue = (int)Bottom.Get(relativeFrame, clipLength);
            int leftValue = (int)Left.Get(relativeFrame, clipLength);
            int rightValue = (int)Right.Get(relativeFrame, clipLength);

            if (topValue == 0 && bottomValue == 0 && leftValue == 0 && rightValue == 0)
            {
                return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize);
            }

            int srcWidth = input.Width;
            int srcHeight = input.Height;

            float logicalScaleX = context.LogicalSize.Width > 0 ? srcWidth / context.LogicalSize.Width : 1f;
            float logicalScaleY = context.LogicalSize.Height > 0 ? srcHeight / context.LogicalSize.Height : 1f;

            int expandTop = (int)(topValue * logicalScaleY);
            int expandBottom = (int)(bottomValue * logicalScaleY);
            int expandLeft = (int)(leftValue * logicalScaleX);
            int expandRight = (int)(rightValue * logicalScaleX);

            int newWidth = srcWidth + expandLeft + expandRight;
            int newHeight = srcHeight + expandTop + expandBottom;
            var newLogicalSize = new SKSize(
                context.LogicalSize.Width + leftValue + rightValue,
                context.LogicalSize.Height + topValue + bottomValue);

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
            hash.Add(nameof(AreaExpansionEffect));
            hash.Add(context.TargetImageCacheKey);
            hash.Add(Top.Get(context.RelativeFrame, context.ClipLength));
            hash.Add(Bottom.Get(context.RelativeFrame, context.ClipLength));
            hash.Add(Left.Get(context.RelativeFrame, context.ClipLength));
            hash.Add(Right.Get(context.RelativeFrame, context.ClipLength));
            return hash.ToHashCode();
        }
    }
}
