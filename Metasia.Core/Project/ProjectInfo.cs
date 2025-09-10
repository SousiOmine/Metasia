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
        /// パラメータなしのコンストラクタ
        /// </summary>
        [Obsolete("パラメータなしのコンストラクタは非推奨です。FramerateとSizeを指定して初期化してください。")]
        public ProjectInfo()
        {
            // デフォルト値を設定
            Framerate = 30;
            Size = new SKSize(1920, 1080);
        }

        public ProjectInfo(int framerate, SKSize size)
        {
            Framerate = framerate;
            Size = size;
        }
    }
}
