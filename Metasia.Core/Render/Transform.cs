using SkiaSharp;

namespace Metasia.Core.Render
{
    public class Transform
    {
        /// <summary>
        /// 論理座標(中央が0)
        /// </summary>
        public SKPoint Position { get; set; } = SKPoint.Empty;

        /// <summary>
        /// スケール(1.0で100%)
        /// </summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>
        /// 回転角度(度数法)
        /// </summary>
        public float Rotation { get; set; } = 0.0f;

        /// <summary>
        /// 不透明度(0.0で透明、1.0で不透明)
        /// </summary>
        public float Alpha { get; set; } = 1.0f;

        /// <summary>
        /// 移動無し、等倍、回転無し、不透明
        /// </summary>
        public static Transform Identify { get; } = new Transform();
    }
}