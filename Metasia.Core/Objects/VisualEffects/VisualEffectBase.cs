using Metasia.Core.Render;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// 描画エフェクトの基底クラス
    /// </summary>
    public abstract class VisualEffectBase : IVisualEffect
    {
        public string Id { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public abstract RenderNode Apply(RenderNode input, VisualEffectContext context);
    }
}