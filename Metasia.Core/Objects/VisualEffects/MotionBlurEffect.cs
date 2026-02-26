using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// モーションブラーエフェクト - 指定した角度と強度で方向性のあるブラーを適用する
    /// </summary>
    [VisualEffectIdentifier("MotionBlurEffect")]
    public class MotionBlurEffect : VisualEffectBase
    {
        [EditableProperty("BlurAngle")]
        [ValueRange(-180, 180, -180, 180)]
        public MetaNumberParam<double> Angle { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("BlurStrength")]
        [ValueRange(0, 200, 0, 100)]
        public MetaNumberParam<double> Strength { get; set; } = new MetaNumberParam<double>(10);

        public override SKImage Apply(SKImage input, VisualEffectContext context)
        {
            if (input is null) return input;

            int relativeFrame = context.RelativeFrame;
            int clipLength = context.ClipLength;

            float strength = (float)Strength.Get(relativeFrame, clipLength);
            if (strength <= 0) return input;

            float angle = (float)Angle.Get(relativeFrame, clipLength);

            // 角度をラジアンに変換してX/Y方向のブラー量を計算
            float radians = angle * MathF.PI / 180f;
            float sigmaX = MathF.Abs(MathF.Cos(radians)) * strength;
            float sigmaY = MathF.Abs(MathF.Sin(radians)) * strength;

            // 最低限のブラー量を確保
            sigmaX = MathF.Max(sigmaX, 0.1f);
            sigmaY = MathF.Max(sigmaY, 0.1f);

            int width = input.Width;
            int height = input.Height;

            var info = new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);

            using var blurFilter = SKImageFilter.CreateBlur(sigmaX, sigmaY);
            using var paint = new SKPaint();
            paint.ImageFilter = blurFilter;

            canvas.DrawImage(input, 0, 0, paint);

            return surface.Snapshot();
        }
    }
}
