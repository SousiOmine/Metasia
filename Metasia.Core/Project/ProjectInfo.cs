using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Project
{
    /// <summary>
    /// MetasiaProjectのオブジェクト以外の情報を格納するクラス
    /// </summary>
    public class ProjectInfo
    {
        private int _framerate;
        private SKSize _size;
        private int _audioSamplingRate;


        /// <summary>
        /// 一秒間に何フレームを含むかの値
        /// </summary>
        public int Framerate
        {
            get => _framerate;
            set => _framerate = value;
        }

        /// <summary>
        /// プロジェクトの縦横の解像度
        /// </summary>
        public SKSize Size
        {
            get => _size;
            set => _size = value;
        }

        /// <summary>
        /// 音声のサンプリングレート（Hz）
        /// </summary>
        public int AudioSamplingRate
        {
            get => _audioSamplingRate;
            set => _audioSamplingRate = value;
        }

        /// <summary>
        /// デフォルトコンストラクタ 非推奨
        /// </summary>
        [Obsolete("Use ProjectInfo(int framerate, SKSize size, int audioSamplingRate) instead")]
        public ProjectInfo()
        {

            Framerate = 30;
            Size = new SKSize(1920, 1080);
            AudioSamplingRate = 44100;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="framerate">フレームレート</param>
        /// <param name="size">プロジェクトの解像度</param>
        /// <param name="audioSamplingRate">音声のサンプリングレート（Hz）</param>
        public ProjectInfo(int framerate, SKSize size, int audioSamplingRate)
        {
            Framerate = framerate;
            Size = size;
            AudioSamplingRate = audioSamplingRate;
        }
    }
}
