using Metasia.Core.Render;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// 透明度を変化させるエフェクト
    /// </summary>
    public class OpacityEffect : VisualEffectBase
    {
        /// <summary>
        /// 透明度 (0.0 = 完全透明, 1.0 = 完全不透明)
        /// </summary>
        public double Opacity { get; set; } = 1.0;

        public override RenderNode Apply(RenderNode input, VisualEffectContext context)
        {
            if (!IsActive || Opacity >= 1.0)
                return input;

            var clonedNode = input.Clone();
            clonedNode.Opacity *= Opacity;

            return clonedNode;
        }
    }
}