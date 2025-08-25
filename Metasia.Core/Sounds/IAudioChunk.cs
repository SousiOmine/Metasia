namespace Metasia.Core.Sounds
{
    /// <summary>
    /// 音声チャンクを定義するインターフェース
    /// </summary>
    public interface IAudioChunk
    {
        /// <summary>
        /// 音声サンプル配列
        /// </summary>
        double[] Samples { get; }
        
        /// <summary>
        /// 音声フォーマット
        /// </summary>
        IAudioFormat Format { get; }
        
        /// <summary>
        /// 音声の長さ（サンプル数）
        /// </summary>
        long Length { get; }
    }
}
