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
		public List<LayerObject> Layers { get; set; } = new();

		public TimelineObject(string id) : base(id)
		{
		}

		public override void Expression(ref ExpresserArgs e, int frame)
		{
			if (e.bitmap is null) e.bitmap = new SKBitmap((int)(e.targetSize.Width * e.ResolutionLevel), (int)(e.targetSize.Height * e.ResolutionLevel));

			//ここでObjectsを各座標とかを考慮し描写する

			foreach (var o in Layers)
			{
				if (frame < o.StartFrame || frame > o.EndFrame) continue;

				using (SKCanvas canvas = new SKCanvas(e.bitmap))
				{
					ExpresserArgs express = new()
					{
						targetSize = e.targetSize,
						ResolutionLevel = e.ResolutionLevel
					};
					o.Expression(ref express, frame);

					canvas.DrawBitmap(express.bitmap, (e.bitmap.Width - express.bitmap.Width) / 2 + o.X, (e.bitmap.Height - express.bitmap.Height) / 2 - o.Y);

					express.Dispose();
				}

			}

			base.Expression(ref e, frame);
		}
	}
}
