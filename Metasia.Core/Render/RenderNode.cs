using SkiaSharp;

namespace Metasia.Core.Render
{
    public class RenderNode
    {
        /// <summary>
        /// 描画されるピクセルデータ
        /// </summary>
        public SKBitmap? Bitmap { get; init; }

        /// <summary>
        /// Bitmapがプロジェクト解像度において持つべき論理的なサイズ
        /// </summary>
        public SKSize LogicalSize { get; init; }

        /// <summary>
        /// 描画オブジェクトの位置や角度、スケール、不透明度など
        /// </summary>
        public Transform Transform { get; init; } = Transform.Identity;

        /// <summary>
        /// 子ノードの描画情報リスト
        /// </summary>
        public IReadOnlyList<RenderNode> Children { get; init; } = new List<RenderNode>();

        /// <summary>
        /// エフェクト用の透明度
        /// </summary>
        public double Opacity { get; set; } = 1.0;

        /// <summary>
        /// クリッピング矩形
        /// </summary>
        public SKRect? ClipRect { get; set; }

        /// <summary>
        /// 表示状態
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// RenderNodeのクローンを作成
        /// </summary>
        public RenderNode Clone()
        {
            return new RenderNode
            {
                Bitmap = Bitmap,
                LogicalSize = LogicalSize,
                Transform = new Transform
                {
                    Position = Transform.Position,
                    Scale = Transform.Scale,
                    Rotation = Transform.Rotation,
                    Alpha = Transform.Alpha
                },
                Children = Children.ToList(),
                Opacity = Opacity,
                ClipRect = ClipRect,
                IsVisible = IsVisible
            };
        }
    }
}