using Metasia.Core.Render;
using Metasia.Core.Xml;
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
		[ValueRange(-99999, 99999, -2000, 2000)]
		public MetaNumberParam<double> X { get; set; }
		[EditableProperty("Y")]
		[ValueRange(-99999, 99999, -2000, 2000)]
		public MetaNumberParam<double> Y { get; set; }
		[EditableProperty("Scale")]
		[ValueRange(0, 99999, 0, 1000)]
		public MetaNumberParam<double> Scale { get; set; }
		[EditableProperty("Alpha")]
		[ValueRange(0, 100, 0, 100)]
		public MetaNumberParam<double> Alpha { get; set; }
		[EditableProperty("Rotation")]
		[ValueRange(-99999, 99999, 0, 360)]
		public MetaNumberParam<double> Rotation { get; set; }
		[EditableProperty("AudioVolume")]
		[ValueRange(0, 99999, 0, 200)]
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
			X = new MetaNumberParam<double>(0);
			Y = new MetaNumberParam<double>(0);
			Scale = new MetaNumberParam<double>(100);
			Alpha = new MetaNumberParam<double>(0);
			Rotation = new MetaNumberParam<double>(0);
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
			//このオブジェクトのStartFrameを基準としたフレーム
			int relativeFrame = context.Frame - StartFrame;
			var bitmap = new SKBitmap(200, 200);
			
			using (SKCanvas canvas = new SKCanvas(bitmap))
			{
				canvas.DrawBitmap(myBitmap, (bitmap.Width - myBitmap.Width) / 2, (bitmap.Height - myBitmap.Height) / 2);
			}

			var transform = new Transform()
			{
				Position = new SKPoint((float)X.Get(relativeFrame), (float)Y.Get(relativeFrame)),
				Scale = (float)Scale.Get(relativeFrame) / 100,
				Rotation = (float)Rotation.Get(relativeFrame),
				Alpha = (100.0f - (float)Alpha.Get(relativeFrame)) / 100,
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

        /// <summary>
        /// 指定したフレームでHelloオブジェクトを分割する
        /// </summary>
        /// <param name="splitFrame">分割フレーム</param>
        /// <returns>分割後の2つのHelloオブジェクト（前半と後半）</returns>
        public override (ClipObject firstClip, ClipObject secondClip) SplitAtFrame(int splitFrame)
        {
            var result = base.SplitAtFrame(splitFrame);

            var firstHello = (kariHelloObject)result.firstClip;
            var secondHello = (kariHelloObject)result.secondClip;

            firstHello.Id = Id + "_part1";
            secondHello.Id = Id + "_part2";

            // MetaNumberParamプロパティの分割
            // 相対フレーム位置で分割するため、オブジェクトの開始フレームを基準とした相対位置で分割
            int relativeSplitFrame = splitFrame - StartFrame;

            // Xプロパティの分割
            var (firstX, secondX) = X.Split(relativeSplitFrame);
            firstHello.X = firstX;
            secondHello.X = secondX;

            // Yプロパティの分割
            var (firstY, secondY) = Y.Split(relativeSplitFrame);
            firstHello.Y = firstY;
            secondHello.Y = secondY;

            // Scaleプロパティの分割
            var (firstScale, secondScale) = Scale.Split(relativeSplitFrame);
            firstHello.Scale = firstScale;
            secondHello.Scale = secondScale;

            // Alphaプロパティの分割
            var (firstAlpha, secondAlpha) = Alpha.Split(relativeSplitFrame);
            firstHello.Alpha = firstAlpha;
            secondHello.Alpha = secondAlpha;

            // Rotationプロパティの分割
            var (firstRotation, secondRotation) = Rotation.Split(relativeSplitFrame);
            firstHello.Rotation = firstRotation;
            secondHello.Rotation = secondRotation;

            return (firstHello, secondHello);
        }

        /// <summary>
        /// Helloオブジェクトのコピーを作成する
        /// </summary>
        /// <returns>コピーされたHelloオブジェクト</returns>
        protected override ClipObject CreateCopy()
        {
            var xml = MetasiaObjectXmlSerializer.Serialize(this);
            var copy = MetasiaObjectXmlSerializer.Deserialize<kariHelloObject>(xml);
            copy.Id = Id + "_copy";
            return copy;
        }
    }
}
