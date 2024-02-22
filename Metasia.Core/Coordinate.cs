using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core
{
	public class Coordinate
	{
		/// <summary>
		/// 中央を0としたX座標
		/// </summary>
		public float X = 0;

		/// <summary>
		/// 中央を0としたY座標
		/// </summary>
		public float Y = 0;

		/// <summary>
		/// 100を等倍とした拡大率
		/// </summary>
		public float Scale = 100;

		/// <summary>
		/// 0で不透過、100で透明になる透明度
		/// </summary>
		public float Alpha = 0;

		/// <summary>
		/// 数字が増えると時計回りに回転する回転角
		/// </summary>
		public float Rotation = 0;
	}
}
