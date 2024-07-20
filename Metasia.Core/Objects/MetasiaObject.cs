using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
    /// <summary>
    ///	Metasiaのタイムラインや動画、音声といったオブジェクトの基底クラス
    /// </summary>
    public class MetasiaObject
    {
        /// <summary>
        /// オブジェクト固有のID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 子オブジェクト１つ
        /// </summary>
        public MetasiaObject? Child;

        /// <summary>
        /// オブジェクトの先頭フレーム
        /// </summary>
        public int StartFrame = 0;

        /// <summary>
        /// オブジェクトの終端フレーム
        /// </summary>
        public int EndFrame = 100;

        /// <summary>
        /// オブジェクトが配置されるレイヤーの番号 大きいほど後に描画される
        /// </summary>
        public int Layer = 0;

		/// <summary>
		/// オブジェクトの初期化
		/// </summary>
		/// <param name="id">オブジェクト固有のID</param>

		public MetasiaObject(string id)
        {
            Id = id;
        }
    }
}
