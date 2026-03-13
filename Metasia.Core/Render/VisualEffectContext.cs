using Metasia.Core.Render.Cache;
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

        /// <summary>
        /// 画像キャッシュ
        /// </summary>
        public IRenderImageCache? ImageCache { get; }

        /// <summary>
        /// レンダリングサーフェスファクトリ
        /// </summary>
        public IRenderSurfaceFactory SurfaceFactory { get; }

        /// <summary>
        /// エフェクト出力をラスタ画像として保持するか
        /// </summary>
        public bool PreferRasterOutput { get; }

        /// <summary>
        /// 適用対象のSKImageに割り当てられたキャッシュキー
        /// </summary>
        public long TargetImageCacheKey { get; set; } = 0;

        public VisualEffectContext(
            int frame,
            int relativeFrame,
            int clipLength,
            SKSize projectResolution,
            SKSize renderResolution,
            SKSize logicalSize,
            IRenderImageCache? imageCache = null,
            IRenderSurfaceFactory? surfaceFactory = null,
            bool preferRasterOutput = false,
            long targetImageCacheKey = IRenderImageCache.NO_CACHE_KEY)
        {
            Frame = frame;
            RelativeFrame = relativeFrame;
            ClipLength = clipLength;
            ProjectResolution = projectResolution;
            RenderResolution = renderResolution;
            LogicalSize = logicalSize;
            ImageCache = imageCache;
            SurfaceFactory = surfaceFactory ?? new NullRenderSurfaceFactory();
            PreferRasterOutput = preferRasterOutput;
            TargetImageCacheKey = targetImageCacheKey;
        }

        /// <summary>
        /// RenderContextとクリップ情報からVisualEffectContextを生成する
        /// </summary>
        public static VisualEffectContext FromRenderContext(
            RenderContext renderContext,
            int startFrame,
            int endFrame,
            SKSize logicalSize,
            IRenderImageCache? imageCache = null,
            long targetImageCacheKey = IRenderImageCache.NO_CACHE_KEY)
        {
            int relativeFrame = renderContext.Frame - startFrame;
            int clipLength = endFrame - startFrame + 1;
            return new VisualEffectContext(
                renderContext.Frame,
                relativeFrame,
                clipLength,
                renderContext.ProjectResolution,
                renderContext.RenderResolution,
                logicalSize,
                imageCache ?? renderContext.ImageCache ?? null,
                renderContext.SurfaceFactory,
                renderContext.PreferRasterOutput,
                targetImageCacheKey);
        }
    }
}
