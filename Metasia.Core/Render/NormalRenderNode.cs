using SkiaSharp;

namespace Metasia.Core.Render
{
    public class NormalRenderNode : IRenderNode
    {
        /// <summary>
        /// 描画されるピクセルデータ
        /// </summary>
        public SKImage? Image { get; init; }

        /// <summary>
        /// Imageがプロジェクト解像度において持つべき論理的なサイズ
        /// </summary>
        public SKSize LogicalSize { get; init; }

        /// <summary>
        /// 描画オブジェクトの位置や角度、スケール、不透明度など
        /// </summary>
        public Transform Transform { get; set; } = Transform.Identity;

        /// <summary>
        /// ブレンドモード
        /// </summary>
        public BlendModeKind BlendMode { get; init; } = BlendModeKind.SrcOver;

        /// <summary>
        /// 子ノードの描画情報リスト
        /// </summary>
        public IReadOnlyList<IRenderNode> Children { get; init; } = new List<IRenderNode>();
    }
}