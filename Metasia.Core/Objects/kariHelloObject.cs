



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

            // Set up the audio source
            var sineWaveSource = new SineWaveSource(440.0, 0.5);
            AudioSource = sineWaveSource;
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

        // Implement IMetaAudiable
        public double Volume { get; set; } = 1.0;

        // Use the new AudioClipObject pattern
        public IAudioSource AudioSource { get; set; } = new SilenceSource();

        public void AudioExpresser(ref AudioExpresserArgs e, int frame)
        {
            if (frame < StartFrame || frame > EndFrame)
            {
                // Return silence if outside the object's time range
                e.Sound = null;
                return;
            }

            // Get the audio frame from the audio source
            AudioFrame audioFrame = AudioSource.GetAudioFrame(
                e.AudioChannel,
                e.SoundSampleRate,
                (ushort)e.FPS,
                frame - StartFrame
            );

            // Adjust the volume if needed
            if (Volume != 1.0)
            {
                audioFrame = AudioUtils.ChangeVolume(audioFrame, Volume);
            }

            // Convert to MetasiaSound for backward compatibility
            e.Sound = new MetasiaSound(audioFrame);
        }
    }
}


