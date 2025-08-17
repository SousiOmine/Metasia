using Metasia.Core.Render;

namespace Metasia.Core.Objects
{
    /// <summary>
    /// 描画機能を持つオブジェクト用のインターフェース
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// 描画情報をRenderNodeとして生成する
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        RenderNode Render(RenderContext context);
    }
}