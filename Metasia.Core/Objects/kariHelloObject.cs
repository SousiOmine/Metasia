using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
	public class kariHelloObject : CoordObject
	{
		private SKBitmap myBitmap = new(400, 300);

		public kariHelloObject(string id) : base(id)
		{
			var skPaint = new SKPaint()
			{
				TextSize = 80,
				TextAlign = SKTextAlign.Center,
				Color = SKColors.Red
			};
			using (SKCanvas canvas = new SKCanvas(myBitmap))
			{
				canvas.DrawText("Hello", 100, 100, skPaint);
			}
		}


		public override void Expression(ref ExpresserArgs e, int frame)
		{


			using (SKCanvas canvas = new SKCanvas(e.bitmap))
			{
				canvas.DrawBitmap(myBitmap, 0, 0);
			}

			base.Expression(ref e, frame);
		}
	}
}
