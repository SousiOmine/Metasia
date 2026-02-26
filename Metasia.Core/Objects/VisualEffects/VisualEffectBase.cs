using Metasia.Core.Render;
using SkiaSharp;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// ビジュアルエフェクトの基底クラス
    /// </summary>
    public abstract class VisualEffectBase : IVisualEffect
    {
        public string Id { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public abstract SKImage Apply(SKImage input, VisualEffectContext context);
    }
}
