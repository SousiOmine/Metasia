using Metasia.Core.Render;

namespace Metasia.Core.Objects
{
    /// <summary>
    /// 描画機能を持つオブジェクト用のインターフェース
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// 描画情報をRenderNodeとして非同期に生成する
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns></returns>
        Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default);
    }
}