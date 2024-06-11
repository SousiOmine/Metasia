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
    public class CoordObject : MetasiaObject, IMetaCoordable
	{
		//public new Coordinate Coord { get; set; } = new();

		public CoordObject(string id) : base(id)
		{
			X_Points = new();
			Y_Points = new();
			Scale_Points = new();
			Alpha_Points = new();
			Rotation_Points = new();
			X_Points.Add(new CoordPoint(){Value = X});
			Y_Points.Add(new CoordPoint(){Value = Y});
			Scale_Points.Add(new CoordPoint(){Value = Scale});
			Alpha_Points.Add(new CoordPoint(){Value = Alpha});
			Rotation_Points.Add(new CoordPoint(){Value = Rotation});
		}

		/*public override void Expression(ref ExpresserArgs e, int frame)
		{
			X = CalculateMidValue(X_Points, frame);
			Y = CalculateMidValue(Y_Points, frame);
			Scale = CalculateMidValue(Scale_Points, frame);
			Alpha = CalculateMidValue(Alpha_Points, frame);
			Rotation = CalculateMidValue(Rotation_Points, frame);

			base.Expression(ref e, frame);
		}*/

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

		public virtual void DrawExpresser(ref DrawExpresserArgs e, int frame)
		{
			X = CalculateMidValue(X_Points, frame);
			Y = CalculateMidValue(Y_Points, frame);
			Scale = CalculateMidValue(Scale_Points, frame);
			Alpha = CalculateMidValue(Alpha_Points, frame);
			Rotation = CalculateMidValue(Rotation_Points, frame);
			
			if (frame < StartFrame || frame > EndFrame) return;
			if (Child is not null && Child is IMetaDrawable)
			{
				IMetaDrawable drawChild = (IMetaDrawable)Child;
				Child.StartFrame = this.StartFrame;
				Child.EndFrame = this.EndFrame;
				drawChild.DrawExpresser(ref e, frame);
			}
		}

		public List<CoordPoint> X_Points { get; set; }
		public List<CoordPoint> Y_Points { get; set; }
		public List<CoordPoint> Scale_Points { get; set; }
		public List<CoordPoint> Alpha_Points { get; set; }
		public List<CoordPoint> Rotation_Points { get; set; }
		public float X { get; set; } = 0;
		public float Y { get; set; } = 0;
		public float Scale { get; set; } = 100;
		public float Alpha { get; set; } = 100;
		public float Rotation { get; set; } = 0;
	}
}
