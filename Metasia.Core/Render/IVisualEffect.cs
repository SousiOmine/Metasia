using Metasia.Core.Objects;
using SkiaSharp;

namespace Metasia.Core.Render
{
    /// <summary>
    /// ビジュアルエフェクトのインターフェース
    /// </summary>
    public interface IVisualEffect : IMetasiaObject
    {
        /// <summary>
        /// 入力画像に対してビジュアルエフェクトを適用する
        /// </summary>
        /// <param name="input">入力画像</param>
        /// <param name="context">エフェクトのコンテキスト</param>
        /// <returns>適用後の画像</returns>
        SKImage Apply(SKImage input, VisualEffectContext context);
    }
}
