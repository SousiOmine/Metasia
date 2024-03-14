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
					
					
					// オブジェクト画像の大きさを指定して描写
					float width = express.bitmap.Width * (o.Scale / 100f);
					float height = express.bitmap.Height * (o.Scale / 100f);
					SKRect drawPos = new SKRect(
						((e.targetSize.Width - width) / 2 + o.X) * e.ResolutionLevel, 
						((e.targetSize.Height - height) / 2 - o.Y) * e.ResolutionLevel, 
						((e.targetSize.Width - width) / 2 + o.X) * e.ResolutionLevel + width * e.ResolutionLevel, 
						((e.targetSize.Height - height) / 2 - o.Y) * e.ResolutionLevel + height * e.ResolutionLevel
					);
					
					canvas.DrawBitmap(express.bitmap, drawPos);

					express.Dispose();
				}

			}

			base.Expression(ref e, frame);
		}
	}
}
