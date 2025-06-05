
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
    public class kariHelloObject : MetasiaObject, IMetaCoordable, IMetaAudiable
    {
        public MetaDoubleParam X { get; set; }
        public MetaDoubleParam Y { get; set; }
        public MetaDoubleParam Scale { get; set; }
        public MetaDoubleParam Alpha { get; set; }
        public MetaDoubleParam Rotation { get; set; }

        private SKBitmap myBitmap = new(200, 200);
        private int audio_offset = 0;
        private IAudioSource _audioSource;

        [JsonConstructor]
        public kariHelloObject()
        {
            InitializeBitmap();
            _audioSource = new SineWaveSource(440.0, 0.5);
        }

        public kariHelloObject(string id) : base(id)
        {
            InitializeBitmap();
            _audioSource = new SineWaveSource(440.0, 0.5);
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

        public double Volume { get; set; } = 1.0;
        public void AudioExpresser(ref AudioExpresserArgs e, int frame)
        {
            // Generate audio frame from the audio source
            AudioFrame audioFrame = _audioSource.GenerateAudioFrame(
                e.AudioChannel,
                e.SoundSampleRate,
                (ushort)e.FPS,
                frame
            );

            // Adjust volume
            if (Volume != 1.0)
            {
                audioFrame = AudioUtils.ChangeVolume(audioFrame, (double)Volume);
            }

            // Convert to MetasiaSound for compatibility
            e.Sound = new MetasiaSound(
                audioFrame.Pulse.ToArray(),
                audioFrame.Channel,
                audioFrame.SampleRate,
                audioFrame.FPS
            );
        }
    }
}
