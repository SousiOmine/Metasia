using Metasia.Core.Objects.VisualEffects;
using SkiaSharp;

namespace Metasia.Core.Render
{
    /// <summary>
    /// ビジュアルエフェクトのパイプライン処理を行うユーティリティクラス
    /// </summary>
    public static class VisualEffectPipeline
    {
        /// <summary>
        /// エフェクトリストを順番に適用し、最終的な画像を返す
        /// </summary>
        /// <param name="input">入力画像</param>
        /// <param name="effects">適用するエフェクトのリスト</param>
        /// <param name="renderContext">レンダリングコンテキスト</param>
        /// <param name="startFrame">クリップの開始フレーム</param>
        /// <param name="endFrame">クリップの終了フレーム</param>
        /// <param name="logicalSize">元画像の論理サイズ</param>
        /// <returns>エフェクト適用後の画像</returns>
        public static SKImage ApplyEffects(
            SKImage input,
            IReadOnlyList<VisualEffectBase> effects,
            RenderContext renderContext,
            int startFrame,
            int endFrame,
            SKSize logicalSize)
        {
            if (effects is null || effects.Count == 0)
            {
                return input;
            }

            var context = VisualEffectContext.FromRenderContext(renderContext, startFrame, endFrame, logicalSize);
            return ApplyEffects(input, effects, context);
        }

        public static SKImage ApplyEffects(
            SKImage input,
            IReadOnlyList<VisualEffectBase> effects,
            VisualEffectContext context)
        {
            if (effects is null || effects.Count == 0)
            {
                return input;
            }

            SKImage current = input;

            foreach (var effect in effects)
            {
                if (effect.IsActive)
                {
                    current = effect.Apply(current, context);
                }
            }

            return current;
        }
    }
}
