namespace Metasia.Core.Sounds
{
    /// <summary>
    /// 音声フォーマットを定義するインターフェース
    /// </summary>
    public interface IAudioFormat
    {
        /// <summary>
        /// サンプルレート（Hz）
        /// </summary>
        int SampleRate { get; }

        /// <summary>
        /// チャンネル数
        /// </summary>
        int ChannelCount { get; }
    }
}
