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
      
        private float _x = 0;
		private float _y = 0;
		private float _scale = 100;
		private float _alpha = 100;
		private float _rotation = 0;

		/// <summary>
		/// 中央を0としたX座標
		/// </summary>
		public virtual float X { get { return _x; } protected set { _x = value; } }

		/// <summary>
		/// 中央を0としたY座標
		/// </summary>
		public virtual float Y { get { return _y; } protected set { _y = value; } }

		/// <summary>
		/// 100を等倍とした拡大率
		/// </summary>
		public virtual float Scale { get { return _scale; } protected set { _scale = value; } }

		/// <summary>
		/// 0で不透過、100で透明になる透明度
		/// </summary>
		public virtual float Alpha { get { return _alpha; } protected set { _alpha = value; } }

		/// <summary>
		/// 数字が増えると時計回りに回転する回転角
		/// </summary>
		public virtual float Rotation { get { return _rotation; } protected set { _rotation = value; } }


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
