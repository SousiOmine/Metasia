using Metasia.Core.Objects.VisualEffects;
using Metasia.Core.Render.Cache;
using SkiaSharp;

namespace Metasia.Core.Render
{
    /// <summary>
    /// ビジュアルエフェクトのパイプライン処理を行うユーティリティクラス
    /// </summary>
    public static class VisualEffectPipeline
    {
        /// <summary>
        /// エフェクトリストを順番に適用し、最終的な結果を返す
        /// </summary>
        /// <param name="input">入力画像</param>
        /// <param name="effects">適用するエフェクトのリスト</param>
        /// <param name="renderContext">レンダリングコンテキスト</param>
        /// <param name="startFrame">クリップの開始フレーム</param>
        /// <param name="endFrame">クリップの終了フレーム</param>
        /// <param name="logicalSize">元画像の論理サイズ</param>
        /// <returns>エフェクト適用後の結果</returns>
        public static VisualEffectResult ApplyEffects(
            SKImage input,
            IReadOnlyList<VisualEffectBase> effects,
            RenderContext renderContext,
            int startFrame,
            int endFrame,
            SKSize logicalSize,
            long imageCacheKey = IRenderImageCache.NO_CACHE_KEY)
        {
            if (effects is null || effects.Count == 0)
            {
                return new VisualEffectResult(input, imageCacheKey);
            }

            var context = VisualEffectContext.FromRenderContext(renderContext, startFrame, endFrame, logicalSize, targetImageCacheKey: imageCacheKey);
            return ApplyEffects(input, effects, context);
        }

        /// <summary>
        /// エフェクトリストを順番に適用し、最終的な結果を返す
        /// </summary>
        /// <param name="input">入力画像</param>
        /// <param name="effects">適用するエフェクトのリスト</param>
        /// <param name="context">ビジュアルエフェクトコンテキスト</param>
        /// <returns>エフェクト適用後の結果</returns>
        public static VisualEffectResult ApplyEffects(
            SKImage input,
            IReadOnlyList<VisualEffectBase> effects,
            VisualEffectContext context)
        {
            if (effects is null || effects.Count == 0)
            {
                return new VisualEffectResult(input, context.TargetImageCacheKey);
            }

            SKImage current = input;
            long currentCacheKey = context.TargetImageCacheKey;
            SKSize currentLogicalSize = context.LogicalSize;

            foreach (var effect in effects)
            {
                if (effect.IsActive)
                {
                    var result = effect.Apply(current, context);
                    current = result.Image;
                    currentCacheKey = result.ImageCacheKey;
                    currentLogicalSize = result.LogicalSize;
                    context = new VisualEffectContext(
                        context.Frame,
                        context.RelativeFrame,
                        context.ClipLength,
                        context.ProjectResolution,
                        context.RenderResolution,
                        currentLogicalSize,
                        context.ImageCache,
                        currentCacheKey);
                }
            }

            return new VisualEffectResult(current, currentCacheKey, currentLogicalSize);
        }
    }
}