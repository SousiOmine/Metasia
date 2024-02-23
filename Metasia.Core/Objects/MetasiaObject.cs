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

        public virtual Coordinate Coord { get; } = new();


        public MetasiaObject(string id)
        {
            Id = id;
        }

        /// <summary>
        /// 動画や音声の表現はこのメソッドでオブジェクトごとに行う
        /// </summary>
        /// <param name="e"></param>
        /// <param name="frame">要求するフレーム</param>
        public virtual void Expression(ref ExpresserArgs e, int frame)
        {
            if (frame < StartFrame || frame > EndFrame) return;
            if (Child is not null)
            {
                Child.StartFrame = this.StartFrame;
				Child.EndFrame = this.EndFrame;
				Child.Expression(ref e, frame);
            }
        }
    }
}
