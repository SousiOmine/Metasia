using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Sounds;

namespace Metasia.Core.Objects
{
	public class kariHelloObject : CoordObject
	{
		private SKBitmap myBitmap = new(200, 200);
		
		private int audio_offset = 0;

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
				canvas.Clear(SKColors.Brown);
				canvas.DrawText("Hello", 100, 100, skPaint);
			}
		}


		public override void Expression(ref ExpresserArgs e, int frame)
		{
			e.bitmap = new SKBitmap(200, 200);

			MetasiaSound sound = new(1, 44100, 60);
			for (int i = 0; i < sound.Pulse.Length - 1; i+=2)
			{
				sound.Pulse[i] = Math.Sin(((i + audio_offset) * (1.0 / 44100)) * (440.0 * 2.0 * Math.PI));
				sound.Pulse[i + 1] = Math.Sin(((i + audio_offset) * (1.0 / 44100)) * (440.0 * 2.0 * Math.PI));
			}
			audio_offset += sound.Pulse.Length;
			
			e.sound = sound;

			using (SKCanvas canvas = new SKCanvas(e.bitmap))
			{
				
				canvas.DrawBitmap(myBitmap, (e.bitmap.Width - myBitmap.Width) / 2, (e.bitmap.Height - myBitmap.Height) / 2);
			}

			base.Expression(ref e, frame);
		}
	}
}
