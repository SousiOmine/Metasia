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
    /// 複数オブジェクトを格納するオブジェクト
    /// </summary>
    public class ListObject : MetasiaObject
	{
		public List<MetasiaObject> Objects { get; set; } = new();

		public ListObject(string id) : base(id)
		{
		}

		public override void Expression(ref ExpresserArgs e, int frame)
		{
			//ここでObjectsを座標を考慮し描写する

			foreach (var o in Objects)
			{
				if (frame < o.StartFrame || frame > o.EndFrame) continue;

				using (SKCanvas canvas = new SKCanvas(e.bitmap))
				{
					ExpresserArgs express = new();
					o.Expression(ref express, frame);
					canvas.DrawBitmap(express.bitmap, 0, 0);
				}

			}

			base.Expression(ref e, frame);
		}
	}
}
