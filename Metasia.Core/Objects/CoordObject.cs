using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Coordinate;

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

		public List<CoordPoint> X_Points = new();
	
		public void SetY(float y)
		{
			Y = y;
		}

		public List<CoordPoint> Y_Points = new();

		public void SetScale(float scale)
		{
			Scale = scale;
		}

		public List<CoordPoint> Scale_Points = new();

		public void SetAlpha(float alpha)
		{
			Alpha = alpha;
		}

		public List<CoordPoint> Alpha_Points = new();

		public void SetRotation(float rotation)
		{
			Rotation = rotation;
		}

		public List<CoordPoint> Rotation_Points = new();

		
	}
}
