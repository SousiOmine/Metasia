using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// モーションブラーエフェクト - 指定した角度と強度で方向性のあるブラーを適用する
    /// </summary>
    [VisualEffectIdentifier("MotionBlurEffect", DisplayKey = "effect.visual.motion_blur.name", FallbackText = "モーションブラー")]
    public class MotionBlurEffect : VisualEffectBase
    {
        [EditableProperty("BlurAngle", DisplayKey = "property.effect.motion_blur.angle", FallbackText = "角度")]
        [ValueRange(-180, 180, -180, 180)]
        public MetaNumberParam<double> Angle { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("BlurStrength", DisplayKey = "property.effect.motion_blur.strength", FallbackText = "強度")]
        [ValueRange(0, 200, 0, 100)]
        public MetaNumberParam<double> Strength { get; set; } = new MetaNumberParam<double>(10);

        public override VisualEffectResult Apply(SKImage input, VisualEffectContext context)
        {
            ArgumentNullException.ThrowIfNull(input);

            int relativeFrame = context.RelativeFrame;
            int clipLength = context.ClipLength;

            float strength = (float)Strength.Get(relativeFrame, clipLength);
            if (strength <= 0) return new VisualEffectResult(input, context.TargetImageCacheKey, context.LogicalSize);

            float angle = (float)Angle.Get(relativeFrame, clipLength);

            if (context.TargetImageCacheKey != IRenderImageCache.NO_CACHE_KEY)
            {
                long cacheKey = GetImageHashCode(context);
                var cachedImage = context.ImageCache?.TryGet(cacheKey);
                if (cachedImage != null)
                {
                    return new VisualEffectResult(cachedImage, cacheKey, context.LogicalSize);
                }
            }

            int width = input.Width;
            int height = input.Height;

            float logicalScaleX = context.LogicalSize.Width > 0 ? width / context.LogicalSize.Width : 1f;
            float logicalScaleY = context.LogicalSize.Height > 0 ? height / context.LogicalSize.Height : 1f;

            float radians = angle * MathF.PI / 180f;
            float sigmaX = MathF.Abs(MathF.Cos(radians)) * strength * logicalScaleX;
            float sigmaY = MathF.Abs(MathF.Sin(radians)) * strength * logicalScaleY;

            sigmaX = MathF.Max(sigmaX, 0.1f);
            sigmaY = MathF.Max(sigmaY, 0.1f);

            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = context.SurfaceFactory.CreateSurface(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            var drawImage = context.SurfaceFactory.GetDrawImage(input);
            try
            {
                using var blurFilter = SKImageFilter.CreateBlur(sigmaX, sigmaY);
                using var paint = new SKPaint();
                paint.ImageFilter = blurFilter;

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
            hash.Add(nameof(MotionBlurEffect));
            hash.Add(context.TargetImageCacheKey);
            hash.Add(Angle.Get(context.RelativeFrame, context.ClipLength));
            hash.Add(Strength.Get(context.RelativeFrame, context.ClipLength));
            return hash.ToHashCode();
        }
    }
}
