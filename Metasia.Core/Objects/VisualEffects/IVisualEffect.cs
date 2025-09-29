using Metasia.Core.Render;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// 描画エフェクトのインターフェース
    /// </summary>
    public interface IVisualEffect : IMetasiaObject
    {
        /// <summary>
        /// エフェクトを描画ノードに適用する
        /// </summary>
        /// <param name="input">入力描画ノード</param>
        /// <param name="context">エフェクトコンテキスト</param>
        /// <returns>エフェクト適用後の描画ノード</returns>
        RenderNode Apply(RenderNode input, VisualEffectContext context);
    }
}