using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Coordinate;
using Metasia.Core.Sounds;
using System.Diagnostics;

namespace Metasia.Core.Objects
{
	public class kariHelloObject : MetasiaObject, IMetaCoordable, IMetaAudiable
	{
		public MetaDoubleParam X { get; set; }
		public MetaDoubleParam Y { get; set; }
		public MetaDoubleParam Scale { get; set; }
		public MetaDoubleParam Alpha { get; set; }
		public MetaDoubleParam Rotation { get; set; }
		
		private SKBitmap myBitmap = new(200, 200);
		
		private int audio_offset = 0;


		public kariHelloObject()
		{

		}
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
			X = new MetaDoubleParam(this, 0);
			Y = new MetaDoubleParam(this, 0);
			Scale = new MetaDoubleParam(this, 100);
			Alpha = new MetaDoubleParam(this, 0);
			Rotation = new MetaDoubleParam(this, 0);
		}


		public void DrawExpresser(ref DrawExpresserArgs e, int frame)
		{
            if (frame < StartFrame || frame > EndFrame) return;
			
			e.Bitmap = new SKBitmap(200, 200);
			
			using (SKCanvas canvas = new SKCanvas(e.Bitmap))
			{
				
				canvas.DrawBitmap(myBitmap, (e.Bitmap.Width - myBitmap.Width) / 2, (e.Bitmap.Height - myBitmap.Height) / 2);
			}
			
			if (Child is not null && Child is IMetaDrawable)
			{
				IMetaDrawable drawChild = (IMetaDrawable)Child;
				Child.StartFrame = this.StartFrame;
				Child.EndFrame = this.EndFrame;
				drawChild.DrawExpresser(ref e, frame);
			}
			
			e.ActualSize = new SKSize(e.Bitmap.Width, e.Bitmap.Height);
			e.TargetSize = new SKSize(200, 200);
		}


		public double Volume { get; set; } = 100;
		public void AudioExpresser(ref AudioExpresserArgs e, int frame)
		{
			MetasiaSound sound = new(e.AudioChannel, 44100, 60);
			audio_offset = frame * sound.Pulse.Length;
			for (int i = 0; i < sound.Pulse.Length; i+=2)
			{
				sound.Pulse[i] = Math.Sin(((i + audio_offset)/2 * (1.0 / 44100)) * (440.0 * 2.0 * Math.PI)) * 0.5;
				sound.Pulse[i + 1] = Math.Sin(((i + audio_offset)/2 * (1.0 / 44100)) * (440.0 * 2.0 * Math.PI)) * 0.5;
			}
			//audio_offset += sound.Pulse.Length;
			
			e.Sound = sound;
		}

	}
}
