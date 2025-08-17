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
        public Transform Transform { get; init; } = Transform.Identify;

        /// <summary>
        /// 子ノードの描画情報リスト
        /// </summary>
        public IReadOnlyList<RenderNode> Children { get; init; } = new List<RenderNode>();
    }
}