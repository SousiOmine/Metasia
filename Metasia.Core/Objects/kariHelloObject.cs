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
using System.Text.Json.Serialization;

namespace Metasia.Core.Objects
{
	public class kariHelloObject : MetasiaObject, IRenderable, IAudiable
	{
		public MetaDoubleParam X { get; set; }
		public MetaDoubleParam Y { get; set; }
		public MetaDoubleParam Scale { get; set; }
		public MetaDoubleParam Alpha { get; set; }
		public MetaDoubleParam Rotation { get; set; }
		
		public double Volume { get; set; } = 100;
		
		private SKBitmap myBitmap = new(200, 200);
		private int audio_offset = 0;

		[JsonConstructor]
		public kariHelloObject()
		{
			InitializeBitmap();
		}

		public kariHelloObject(string id) : base(id)
		{
			InitializeBitmap();
			X = new MetaDoubleParam(this, 0);
			Y = new MetaDoubleParam(this, 0);
			Scale = new MetaDoubleParam(this, 100);
			Alpha = new MetaDoubleParam(this, 0);
			Rotation = new MetaDoubleParam(this, 0);
		}

		private void InitializeBitmap()
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

		public RenderNode Render(RenderContext context)
		{
			var bitmap = new SKBitmap(200, 200);
			
			using (SKCanvas canvas = new SKCanvas(bitmap))
			{
				canvas.DrawBitmap(myBitmap, (bitmap.Width - myBitmap.Width) / 2, (bitmap.Height - myBitmap.Height) / 2);
			}

			var transform = new Transform()
			{
				Position = new SKPoint((float)X.Get(context.Frame), (float)Y.Get(context.Frame)),
				Scale = (float)Scale.Get(context.Frame) / 100,
				Rotation = (float)Rotation.Get(context.Frame),
				Alpha = (100.0f - (float)Alpha.Get(context.Frame)) / 100,
			};
			
			if (Child is not IRenderable renderableChild)
			{
				return new RenderNode()
				{
					Bitmap = bitmap,
					LogicalSize = new SKSize(bitmap.Width, bitmap.Height),
					Transform = transform,
				};
			}

			var childNode = renderableChild.Render(context);
			return new RenderNode()
			{
				Bitmap = bitmap,
				Children = new List<RenderNode>() { childNode },
				LogicalSize = new SKSize(bitmap.Width, bitmap.Height),
				Transform = transform,
			};
		}


        public AudioChunk GetAudioChunk(AudioFormat format, long startSample, long length)
        {
			var chunk = new AudioChunk(format, length);
			double frequency = 440;

			for (long i = 0; i < length; i++)
			{
				// currentSampleは、このオブジェクトの先頭からのサンプル位置
				long currentSample = startSample + i;
        
				var time = currentSample / (double)format.SampleRate;
				var pulse = Math.Sin(time * (frequency * 2.0 * Math.PI)) * 0.5;

				for (int ch = 0; ch < format.ChannelCount; ch++)
				{
					chunk.Samples[i * format.ChannelCount + ch] = pulse;
				}
			}

			return chunk;
        }
    }
}
