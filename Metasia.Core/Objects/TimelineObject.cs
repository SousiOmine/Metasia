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
	/// タイムライン専用のオブジェクト ObjectsにはLayerObjectのみが格納される前提で描写が行われる
	/// </summary>
	public class TimelineObject : ListObject
	{

		public TimelineObject(string id) : base(id)
		{
		}

		public override void Expression(ref ExpresserArgs e, int frame)
		{
			if (e.bitmap is null) e.bitmap = new SKBitmap((int)(e.targetSize.Width * e.ResolutionLevel), (int)(e.targetSize.Height * e.ResolutionLevel));

			//ここでObjectsを各座標とかを考慮し描写する

			foreach (var o in Objects)
			{
				if (frame < o.StartFrame || frame > o.EndFrame) continue;



				using (SKCanvas canvas = new SKCanvas(e.bitmap))
				{
					ExpresserArgs express = new()
					{
						//bitmap = new(300, 300),
						targetSize = e.targetSize,
						ResolutionLevel = e.ResolutionLevel
					};
					o.Expression(ref express, frame);

					//float startx = ((e.bitmap.Width - express.bitmap.Width) / 2 + o.X) * e.ResolutionLevel;
					//float starty = ((e.bitmap.Height - express.bitmap.Height) / 2 - o.Y) * e.ResolutionLevel;
					//float endx = ((e.bitmap.Width - express.bitmap.Width) / 2 + o.X + express.bitmap.Width) * e.ResolutionLevel;
					//float endy = ((e.bitmap.Height - express.bitmap.Height) / 2 - o.Y + express.bitmap.Height) * e.ResolutionLevel;

					//SKRect drawPos = new SKRect(startx, starty, endx, endy);
					//SKRect drawPos = new SKRect(0, 0, 600, 300);

					canvas.DrawBitmap(express.bitmap, (e.bitmap.Width - express.bitmap.Width) / 2 + o.X, (e.bitmap.Height - express.bitmap.Height) / 2 - o.Y);
					//canvas.DrawBitmap(express.bitmap, drawPos);
					express.Dispose();
				}

			}
		}
	}
}
