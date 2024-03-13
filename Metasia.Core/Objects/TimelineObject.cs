using Metasia.Core.Graphics;
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
	/// タイムライン専用のオブジェクト
	/// </summary>
	public class TimelineObject : MetasiaObject
	{
		public List<MetasiaObject> Objects { get; set; } = new();

		public TimelineObject(string id) : base(id)
		{
		}

		public override void Expression(ref ExpresserArgs e, int frame)
		{
			if (e.bitmap is null) e.bitmap = new SKBitmap((int)(e.targetSize.Width * e.ResolutionLevel), (int)(e.targetSize.Height * e.ResolutionLevel));

			//描写対象のオブジェクトを抽出し、Layerの昇順に並び替える
			List<MetasiaObject> ApplicateObjects = new();
			foreach (var o in Objects)
			{
				if (frame < o.StartFrame || frame > o.EndFrame) continue;
				ApplicateObjects.Add(o);
			}
			ApplicateObjects = ApplicateObjects.OrderBy(o => o.Layer).ToList();

			//ここでObjectsを各座標とかを考慮し描写する
			foreach (var o in ApplicateObjects)
			{
				using (SKCanvas canvas = new SKCanvas(e.bitmap))
				{
					ExpresserArgs express = new()
					{
						targetSize = e.targetSize,
						ResolutionLevel = e.ResolutionLevel
					};
					o.Expression(ref express, frame);

					if (o.Rotation != 0) express.bitmap = MetasiaBitmap.Rotate(express.bitmap, o.Rotation);
					if (o.Alpha != 100) express.bitmap = MetasiaBitmap.Transparency(express.bitmap, o.Alpha / 100);

					//オブジェクト画像の大きさを指定して描写
					float startx = ((e.targetSize.Width - express.bitmap.Width) / 2 + o.X) * e.ResolutionLevel;
					float starty = ((e.targetSize.Height - express.bitmap.Height) / 2 - o.Y) * e.ResolutionLevel;
					float endx = ((e.targetSize.Width - express.bitmap.Width) / 2 + o.X + express.bitmap.Width) * e.ResolutionLevel;
					float endy = ((e.targetSize.Height - express.bitmap.Height) / 2 - o.Y + express.bitmap.Height) * e.ResolutionLevel;

					SKRect drawPos = new SKRect(startx, starty, endx, endy);

					canvas.DrawBitmap(express.bitmap, drawPos);

					express.Dispose();
				}

			}

			base.Expression(ref e, frame);
		}
	}
}
