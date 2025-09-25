using Metasia.Core.Render;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// 4方向からクリッピングをかけるエフェクト
    /// </summary>
    public class ClipEffect : VisualEffectBase
    {
        /// <summary>
        /// 左からのクリッピング量 (0.0 = クリップなし, 1.0 = 完全にクリップ)
        /// </summary>
        public double Left { get; set; } = 0.0;

        /// <summary>
        /// 右からのクリッピング量 (0.0 = クリップなし, 1.0 = 完全にクリップ)
        /// </summary>
        public double Right { get; set; } = 0.0;

        /// <summary>
        /// 上からのクリッピング量 (0.0 = クリップなし, 1.0 = 完全にクリップ)
        /// </summary>
        public double Top { get; set; } = 0.0;

        /// <summary>
        /// 下からのクリッピング量 (0.0 = クリップなし, 1.0 = 完全にクリップ)
        /// </summary>
        public double Bottom { get; set; } = 0.0;

        public override RenderNode Apply(RenderNode input, VisualEffectContext context)
        {
            if (!IsActive || (Left == 0.0 && Right == 0.0 && Top == 0.0 && Bottom == 0.0))
                return input;

            var clonedNode = input.Clone();

            // クリッピング矩形を計算
            var originalWidth = context.OriginalSize.X;
            var originalHeight = context.OriginalSize.Y;

            var clipLeft = originalWidth * Left;
            var clipRight = originalWidth * Right;
            var clipTop = originalHeight * Top;
            var clipBottom = originalHeight * Bottom;

            // 有効なクリッピング領域を計算
            var validLeft = Math.Max(0, clipLeft);
            var validTop = Math.Max(0, clipTop);
            var validRight = Math.Max(0, originalWidth - clipRight);
            var validBottom = Math.Max(0, originalHeight - clipBottom);

            // クリッピング矩形が有効かチェック
            if (validRight > validLeft && validBottom > validTop)
            {
                var clipRect = new SkiaSharp.SKRect(
                    (float)validLeft,
                    (float)validTop,
                    (float)validRight,
                    (float)validBottom
                );

                // RenderNodeにクリッピング情報を設定
                clonedNode.ClipRect = clipRect;
            }
            else
            {
                // クリッピング領域が無効な場合は完全に非表示
                clonedNode.IsVisible = false;
            }

            return clonedNode;
        }
    }
}