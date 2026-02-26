using SkiaSharp;

namespace Metasia.Core.Render
{
    /// <summary>
    /// ビジュアルエフェクト適用時のコンテキスト情報
    /// </summary>
    public class VisualEffectContext
    {
        /// <summary>
        /// 現在のフレーム番号（タイムライン上の絶対フレーム）
        /// </summary>
        public int Frame { get; }

        /// <summary>
        /// クリップ内の相対フレーム（クリップ先頭からのフレーム数）
        /// </summary>
        public int RelativeFrame { get; }

        /// <summary>
        /// クリップの長さ（フレーム数）
        /// </summary>
        public int ClipLength { get; }

        /// <summary>
        /// プロジェクトの解像度
        /// </summary>
        public SKSize ProjectResolution { get; }

        /// <summary>
        /// レンダリング解像度
        /// </summary>
        public SKSize RenderResolution { get; }

        /// <summary>
        /// エフェクト適用前の画像の論理サイズ
        /// </summary>
        public SKSize LogicalSize { get; }

        public VisualEffectContext(
            int frame,
            int relativeFrame,
            int clipLength,
            SKSize projectResolution,
            SKSize renderResolution,
            SKSize logicalSize)
        {
            Frame = frame;
            RelativeFrame = relativeFrame;
            ClipLength = clipLength;
            ProjectResolution = projectResolution;
            RenderResolution = renderResolution;
            LogicalSize = logicalSize;
        }

        /// <summary>
        /// RenderContextとクリップ情報からVisualEffectContextを生成する
        /// </summary>
        public static VisualEffectContext FromRenderContext(RenderContext renderContext, int startFrame, int endFrame, SKSize logicalSize)
        {
            int relativeFrame = renderContext.Frame - startFrame;
            int clipLength = endFrame - startFrame + 1;
            return new VisualEffectContext(
                renderContext.Frame,
                relativeFrame,
                clipLength,
                renderContext.ProjectResolution,
                renderContext.RenderResolution,
                logicalSize);
        }
    }
}
