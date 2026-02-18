using Metasia.Core.Render;
using Metasia.Core.Xml;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
using Metasia.Core.Sounds;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using Metasia.Core.Objects.AudioEffects;
using Metasia.Core.Attributes;

namespace Metasia.Core.Objects
{
    [Serializable]
    [ClipTypeIdentifier("HelloObject")]
    public class kariHelloObject : ClipObject, IRenderable, IAudible, IDisposable
    {
        [EditableProperty("BlendMode")]
        public BlendModeParam BlendMode { get; set; } = new BlendModeParam();

        [EditableProperty("X")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> X { get; set; } = new MetaNumberParam<double>(0);
        [EditableProperty("Y")]
        [ValueRange(-99999, 99999, -2000, 2000)]
        public MetaNumberParam<double> Y { get; set; } = new MetaNumberParam<double>(0);
        [EditableProperty("Scale")]
        [ValueRange(0, 99999, 0, 1000)]
        public MetaNumberParam<double> Scale { get; set; } = new MetaNumberParam<double>(100);
        [EditableProperty("Alpha")]
        [ValueRange(0, 100, 0, 100)]
        public MetaNumberParam<double> Alpha { get; set; } = new MetaNumberParam<double>(0);
        [EditableProperty("Rotation")]
        [ValueRange(-99999, 99999, 0, 360)]
        public MetaNumberParam<double> Rotation { get; set; } = new MetaNumberParam<double>(0);
        [EditableProperty("AudioVolume")]
        [ValueRange(0, 99999, 0, 200)]
        public MetaDoubleParam Volume { get; set; } = new MetaDoubleParam(100);
        public List<AudioEffectBase> AudioEffects { get; set; } = new();

        private SKImage? myImage;
        private bool disposed;
        private int audio_offset = 0;

        public kariHelloObject()
        {
            InitializeBitmap();
        }

        public kariHelloObject(string id) : base(id)
        {
            InitializeBitmap();
        }

        ~kariHelloObject()
        {
            Dispose(false);
        }

        private void InitializeBitmap()
        {
            var skFont = new SKFont(SKTypeface.Default, 80);
            var skPaint = new SKPaint()
            {
                Color = SKColors.Red
            };
            var info = new SKImageInfo(200, 200, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            using var canvas = surface.Canvas;
            canvas.Clear(SKColors.Brown);
            canvas.DrawText("Hello", new SKPoint(100, 100), SKTextAlign.Center, skFont, skPaint);
            myImage = surface.Snapshot();
        }

        public Task<IRenderNode> RenderAsync(RenderContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //このオブジェクトのStartFrameを基準としたフレーム
            int relativeFrame = context.Frame - StartFrame;
            int clipLength = EndFrame - StartFrame + 1;

            var transform = new Transform()
            {
                Position = new SKPoint((float)X.Get(relativeFrame, clipLength), (float)Y.Get(relativeFrame, clipLength)),
                Scale = (float)Scale.Get(relativeFrame, clipLength) / 100,
                Rotation = (float)Rotation.Get(relativeFrame, clipLength),
                Alpha = (100.0f - (float)Alpha.Get(relativeFrame, clipLength)) / 100,
            };

            return Task.FromResult<IRenderNode>(new NormalRenderNode()
            {
                Image = myImage,
                LogicalSize = new SKSize(200, 200),
                Transform = transform,
                BlendMode = BlendMode.Value,
            });
        }

        public Task<IAudioChunk> GetAudioChunkAsync(GetAudioContext context)
        {
            IAudioChunk chunk = new AudioChunk(context.Format, context.RequiredLength);
            double frequency = 440;

            for (long i = 0; i < context.RequiredLength; i++)
            {
                // currentSampleは、このオブジェクトの先頭からのサンプル位置
                long currentSample = context.StartSamplePosition + i;

                var time = currentSample / (double)context.Format.SampleRate;
                var pulse = Math.Sin(time * (frequency * 2.0 * Math.PI)) * 0.5 * Volume.Value / 100;

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

            return Task.FromResult(chunk);
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
            int clipLength = EndFrame - StartFrame + 1;

            // Xプロパティの分割
            var (firstX, secondX) = X.Split(relativeSplitFrame, clipLength);
            firstHello.X = firstX;
            secondHello.X = secondX;

            // Yプロパティの分割
            var (firstY, secondY) = Y.Split(relativeSplitFrame, clipLength);
            firstHello.Y = firstY;
            secondHello.Y = secondY;

            // Scaleプロパティの分割
            var (firstScale, secondScale) = Scale.Split(relativeSplitFrame, clipLength);
            firstHello.Scale = firstScale;
            secondHello.Scale = secondScale;

            // Alphaプロパティの分割
            var (firstAlpha, secondAlpha) = Alpha.Split(relativeSplitFrame, clipLength);
            firstHello.Alpha = firstAlpha;
            secondHello.Alpha = secondAlpha;

            // Rotationプロパティの分割
            var (firstRotation, secondRotation) = Rotation.Split(relativeSplitFrame, clipLength);
            firstHello.Rotation = firstRotation;
            secondHello.Rotation = secondRotation;

            // Volumeプロパティの分割
            var (firstVolume, secondVolume) = Volume.Split(relativeSplitFrame);
            firstHello.Volume = firstVolume;
            secondHello.Volume = secondVolume;

            var (firstBlendMode, secondBlendMode) = BlendMode.Split();
            firstHello.BlendMode = firstBlendMode;
            secondHello.BlendMode = secondBlendMode;

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                myImage?.Dispose();
                myImage = null;
            }

            disposed = true;
        }
    }
}
