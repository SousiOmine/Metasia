using Metasia.Core.Objects;

namespace Metasia.Core.Sounds
{
    public interface IAudioEffect : IMetasiaObject
    {
        /// <summary>
        /// 入力チャンクに対してエフェクトを適用する
        /// </summary>
        /// <param name="input">入力チャンク</param>
        /// <param name="context">エフェクトのコンテキスト</param>
        /// <returns>適用後のチャンク</returns>
        IAudioChunk Apply(IAudioChunk input, AudioEffectContext context);
    }
}
