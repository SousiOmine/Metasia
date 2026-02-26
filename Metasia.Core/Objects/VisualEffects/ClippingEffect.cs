using Metasia.Core.Attributes;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// クリッピングエフェクト - 画像の上下左右を切り取る
    /// </summary>
    [VisualEffectIdentifier("ClippingEffect")]
    public class ClippingEffect : VisualEffectBase
    {
        [EditableProperty("ClipTop")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Top { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ClipBottom")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Bottom { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ClipLeft")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Left { get; set; } = new MetaNumberParam<double>(0);

        [EditableProperty("ClipRight")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Right { get; set; } = new MetaNumberParam<double>(0);

        public override SKImage Apply(SKImage input, VisualEffectContext context)
        {
            if (input is null) return input;

            int relativeFrame = context.RelativeFrame;
            int clipLength = context.ClipLength;

            int top = (int)Top.Get(relativeFrame, clipLength);
            int bottom = (int)Bottom.Get(relativeFrame, clipLength);
            int left = (int)Left.Get(relativeFrame, clipLength);
            int right = (int)Right.Get(relativeFrame, clipLength);

            if (top == 0 && bottom == 0 && left == 0 && right == 0)
            {
                return input;
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

            return surface.Snapshot();
        }
    }
}
