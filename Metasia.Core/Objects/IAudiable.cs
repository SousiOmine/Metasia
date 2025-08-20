using Metasia.Core.Project;
using Metasia.Core.Sounds;

namespace Metasia.Core.Objects
{
    public interface IAudiable
    {
        public double Volume { get; set; }

        /// <summary>
        /// 指定した範囲の音声チャンクを取得する
        /// </summary>
        /// <param name="format">音声フォーマット情報</param>
        /// <param name="startSample">オブジェクトの始端を基準とした取得開始位置</param>
        /// <param name="length">取得する音声チャンクの長さ</param>
        /// <returns>指定された範囲の音声チャンク</returns>
        public AudioChunk GetAudioChunk(AudioFormat format, long startSample, long length);
    }
}