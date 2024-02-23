using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
    /// <summary>
    /// 座標とか拡大率とかを書き換え可能なオブジェクト
    /// </summary>
    public class CoordObject : MetasiaObject
	{
		//public new Coordinate Coord { get; set; } = new();

		public CoordObject(string id) : base(id)
		{
		}

		public void SetX(float x)
		{
			X = x;
		}

		public void SetY(float y)
		{
			Y = y;
		}

		public void SetScale(float scale)
		{
			Scale = scale;
		}

		public void SetAlpha(float alpha)
		{
			Alpha = alpha;
		}

		public void SetRotation(float rotation)
		{
			Rotation = rotation;
		}

		
	}
}
