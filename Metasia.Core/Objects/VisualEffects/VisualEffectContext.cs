using Metasia.Core.Project;

namespace Metasia.Core.Objects.VisualEffects
{
    /// <summary>
    /// 描画エフェクトの適用に必要な情報群
    /// </summary>
    public class VisualEffectContext
    {
        /// <summary>
        /// 現在の時間
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// プロジェクト情報
        /// </summary>
        public ProjectInfo Project { get; set; }

        /// <summary>
        /// 元のオブジェクトの位置
        /// </summary>
        public System.Numerics.Vector2 OriginalPosition { get; set; }

        /// <summary>
        /// 元のオブジェクトのサイズ
        /// </summary>
        public System.Numerics.Vector2 OriginalSize { get; set; }
    }
}