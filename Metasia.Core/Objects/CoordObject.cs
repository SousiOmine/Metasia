using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Coordinate;
using Metasia.Core.Render;

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
			if (X_Points.Count == 0) X_Points.Add(new CoordPoint(){Value = X});
			if (Y_Points.Count == 0) Y_Points.Add(new CoordPoint(){Value = Y});
			if (Scale_Points.Count == 0) Scale_Points.Add(new CoordPoint(){Value = Scale});
			if (Alpha_Points.Count == 0) Alpha_Points.Add(new CoordPoint(){Value = Alpha});
			if (Rotation_Points.Count == 0) Rotation_Points.Add(new CoordPoint(){Value = Rotation});
		}

		public List<CoordPoint> X_Points = new();

		public List<CoordPoint> Y_Points = new();

		public List<CoordPoint> Scale_Points = new();


		public List<CoordPoint> Alpha_Points = new();

		public List<CoordPoint> Rotation_Points = new();

		public override void Expression(ref ExpresserArgs e, int frame)
		{
			X = CalculateMidValue(X_Points, frame);
			Y = CalculateMidValue(Y_Points, frame);
			Scale = CalculateMidValue(Scale_Points, frame);
			Alpha = CalculateMidValue(Alpha_Points, frame);
			Rotation = CalculateMidValue(Rotation_Points, frame);

			base.Expression(ref e, frame);
		}

		protected float CalculateMidValue(List<CoordPoint> points, int frame)
		{
			//CoordPointのFrameはオブジェクトの始点基準なので合わせる
			frame -= StartFrame;
			//pointsをFrameの昇順に並べ替え
			points.Sort((a, b) => a.Frame - b.Frame);
			CoordPoint startPoint = points.Last();
			CoordPoint endPoint = startPoint;

			//frameを含む前後２つのポイントを取得
			for(int i = 0; i < points.Count; i++)
			{
				if (points[i].Frame >= frame)
				{
					endPoint = points[i];
					if(i > 0) startPoint = points[i - 1];
					else startPoint = endPoint;
					break;
				}
			}
			float midValue = startPoint.PointLogic.GetBetweenPoint(startPoint.Value, endPoint.Value, frame, startPoint.Frame, endPoint.Frame);
			
			return midValue;
		}
	}
}
