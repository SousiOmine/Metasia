using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Render
{
    /// <summary>
    /// ビジュアルエフェクトの適用結果を表すクラス
    /// </summary>
    public class VisualEffectResult
    {
        /// <summary>
        /// エフェクト適用後の画像
        /// </summary>
        public SKImage Image { get; }

        public long ImageCacheKey { get; set; } = IRenderImageCache.NO_CACHE_KEY;

        /// <summary>
        /// エフェクト適用後の論理サイズ
        /// </summary>
        public SKSize LogicalSize { get; }

        /// <summary>
        /// エフェクトがクリップのTransformに適用するオフセット（加算/乗算）。nullの場合は変更なし。
        /// </summary>
        public Transform? TransformOffset { get; set; }

        public VisualEffectResult(SKImage image, long imageCacheKey, SKSize? logicalSize = null)
        {
            ArgumentNullException.ThrowIfNull(image);
            Image = image;
            ImageCacheKey = imageCacheKey;
            LogicalSize = logicalSize ?? new SKSize(image.Width, image.Height);
        }
    }
}