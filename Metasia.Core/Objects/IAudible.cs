using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Project;
using Metasia.Core.Sounds;

namespace Metasia.Core.Objects
{
    public interface IAudible
    {
        double Volume { get; set; }

        List<AudioEffectBase> AudioEffects { get; }

        /// <summary>
        /// 指定した範囲の音声チャンクを取得する
        /// </summary>
        /// <param name="context">音声の要求に必要な情報群</param>
        /// <returns>指定された範囲の音声チャンク</returns>
        IAudioChunk GetAudioChunk(GetAudioContext context);
    }
}
