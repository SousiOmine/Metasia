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
        /// エフェクト適用対象のオブジェクトの長さ（秒）
        /// </summary>
        public double ObjectDurationInSeconds { get; }

        /// <summary>
        /// 現在処理中のチャンクが、音源のどの位置から開始しているか
        /// </summary>
        public long CurrentSamplePosition { get; }

        /// <summary>
        /// プロジェクトのフレームレート
        /// </summary>
        public double ProjectFrameRate { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="source">エフェクトが適用される音源ソース</param>
        /// <param name="getAudioContext">音声コンテキスト情報</param>

        public AudioEffectContext(IAudible source, GetAudioContext getAudioContext)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(getAudioContext);
            Source = source;
            Format = getAudioContext.Format;
            CurrentSamplePosition = getAudioContext.StartSamplePosition;
            ObjectDurationInSeconds = getAudioContext.ObjectDurationInSeconds;
            ProjectFrameRate = getAudioContext.ProjectFrameRate;
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

            var audioContext = new GetAudioContext(Format, startPosition, endPosition - startPosition, ProjectFrameRate, ObjectDurationInSeconds);
            var chunk = Source.GetAudioChunk(audioContext);
            return chunk;
        }
    }
}
