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

        public VisualEffectResult(SKImage image, long imageCacheKey)
        {
            Image = image;
            ImageCacheKey = imageCacheKey;
        }
    }
}