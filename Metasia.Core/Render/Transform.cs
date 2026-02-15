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
        /// スケール(1.0で100%) 0%以下にはならない
        /// </summary>
        public float Scale
        {
            get => _scale;
            set => _scale = Math.Max(value, 0f);
        }

        /// <summary>
        /// 回転角度(度数法)
        /// </summary>
        public float Rotation { get; set; } = 0.0f;

        /// <summary>
        /// 不透明度(0.0で透明、1.0で不透明)
        /// </summary>
        public float Alpha
        {
            get => _alpha;
            set => _alpha = Math.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// 移動無し、等倍、回転無し、不透明
        /// </summary>
        public static Transform Identity { get; } = new Transform();

        private float _scale = 1.0f;
        private float _alpha = 1.0f;

        public Transform Add(Transform transform)
        {
            ArgumentNullException.ThrowIfNull(transform);
            return new Transform
            {
                Position = this.Position + transform.Position,
                Scale = this.Scale * transform.Scale,
                Rotation = this.Rotation + transform.Rotation,
                Alpha = this.Alpha * transform.Alpha
            };
        }
    }
}
