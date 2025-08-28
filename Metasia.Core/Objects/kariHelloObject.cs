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
using System.Xml.Serialization;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Attributes;

namespace Metasia.Core.Objects
{
	[Serializable]
	public class kariHelloObject : ClipObject, IRenderable, IAudible
	{
		[EditableProperty("X")]
		public MetaNumberParam<double> X { get; set; }
		[EditableProperty("Y")]
		public MetaNumberParam<double> Y { get; set; }
		[EditableProperty("Scale")]
		public MetaNumberParam<double> Scale { get; set; }
		[EditableProperty("Alpha")]
		public MetaNumberParam<double> Alpha { get; set; }
		[EditableProperty("Rotation")]
		public MetaNumberParam<double> Rotation { get; set; }
		[EditableProperty("AudioVolume")]
		public double Volume { get; set; } = 100;
		public List<AudioEffectBase> AudioEffects { get; set; } = new();
		
		private SKBitmap myBitmap = new(200, 200);
		private int audio_offset = 0;

		public kariHelloObject()
		{
			InitializeBitmap();
		}

		public kariHelloObject(string id) : base(id)
		{
			InitializeParameters();
			InitializeBitmap();
		}

		private void InitializeParameters()
		{
			X = new MetaNumberParam<double>(this, 0);
			Y = new MetaNumberParam<double>(this, 0);
			Scale = new MetaNumberParam<double>(this, 100);
			Alpha = new MetaNumberParam<double>(this, 0);
			Rotation = new MetaNumberParam<double>(this, 0);
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
			
			return new RenderNode()
			{
				Bitmap = bitmap,
				LogicalSize = new SKSize(bitmap.Width, bitmap.Height),
				Transform = transform,
			};
		}


        public IAudioChunk GetAudioChunk(GetAudioContext context)
        {
			IAudioChunk chunk = new AudioChunk(context.Format, context.RequiredLength);
			double frequency = 440;

			for (long i = 0; i < context.RequiredLength; i++)
			{
				// currentSampleは、このオブジェクトの先頭からのサンプル位置
				long currentSample = context.StartSamplePosition + i;
        
				var time = currentSample / (double)context.Format.SampleRate;
				var pulse = Math.Sin(time * (frequency * 2.0 * Math.PI)) * 0.5 * Volume / 100;

				for (int ch = 0; ch < context.Format.ChannelCount; ch++)
				{
					chunk.Samples[i * context.Format.ChannelCount + ch] = pulse;
				}
			}

			AudioEffectContext effectContext = new AudioEffectContext(this, context);

			foreach (var effect in AudioEffects)
			{
				chunk = effect.Apply(chunk, effectContext);
			}

			return chunk;
        }
    }
}
