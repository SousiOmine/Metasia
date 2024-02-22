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
        private float _framerate;
        private SKSize _size;


        /// <summary>
        /// 一秒間に何フレームを含むかの値
        /// </summary>
        public float Framerate
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
    }
}
