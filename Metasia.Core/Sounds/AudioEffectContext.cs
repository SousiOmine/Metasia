using Metasia.Core.Objects;

namespace Metasia.Core.Sounds
{
    public class AudioEffectContext
    {
        /// <summary>
        /// エフェクトが適用される音源ソース
        /// </summary>
        public IAudible Source { get; }

        /// <summary>
        /// 音声フォーマット
        /// </summary>
        public AudioFormat Format { get; }

        /// <summary>
        /// 現在処理中のチャンクが、音源のどの位置から開始しているか
        /// </summary>
        public long CurrentSamplePosition { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="source">エフェクトが適用される音源ソース</param>
        /// <param name="format">音声フォーマット</param>
        /// <param name="currentSamplePosition">現在処理中のチャンクが、音源のどの位置から開始しているか</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>

        public AudioEffectContext(IAudible source, AudioFormat format, long currentSamplePosition)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(format);
            if (currentSamplePosition < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(currentSamplePosition), "currentSamplePosition must be non-negative");
            }
            Source = source;
            Format = format;
            CurrentSamplePosition = currentSamplePosition;
        }

        /// <summary>
        /// 指定された範囲の音声データを取得する
        /// </summary>
        /// <param name="startPosition">音源の始端を基準とした開始位置</param>
        /// <param name="endPosition">音源の始端を基準とした終了位置</param>
        /// <returns>指定された範囲の音声データ</returns>
        public AudioChunk GetSourceAudio(long startPosition, long endPosition)
        {
            if (startPosition < CurrentSamplePosition)
            {
                throw new ArgumentException("startPosition must be greater than or equal to CurrentSamplePosition");
            }
            if (endPosition < startPosition)
            {
                throw new ArgumentException("endPosition must be greater than or equal to startPosition");
            }
            var chunk = Source.GetAudioChunk(Format, startPosition, endPosition - startPosition);
            return chunk;
        }
    }
}