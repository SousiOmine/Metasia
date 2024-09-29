using Metasia.Core.Graphics;
using Metasia.Core.Render;
using Metasia.Core.Sounds;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
    public class LayerObject : MetasiaObject, IMetaDrawable, IMetaAudiable
    {
        /// <summary>
        /// レイヤーに属するオブジェクト 原則同じフレームに2個以上オブジェクトがあってはならない
        /// </summary>
        public ObservableCollection<MetasiaObject> Objects { get; protected set; } = new();
        public double Volume { get; set; }

        /// <summary>
        /// レイヤー名
        /// </summary>
        public string Name { get; set; }

        public LayerObject(string id, string LayerName) : base(id)
        {
            Volume = 100;
            Name = LayerName;
            StartFrame = 0;
            EndFrame = int.MaxValue;
        }

        public void DrawExpresser(ref DrawExpresserArgs e, int frame)
        {
            
            List<MetasiaObject> ApplicateObjects = new();
            foreach (var obj in Objects) 
            {
                if(obj.IsExistFromFrame(frame) && obj is IMetaDrawable && obj.IsActive)
                {
                    ApplicateObjects.Add(obj);
                }
            }

            if (ApplicateObjects.Count == 0) return;

            if (e.Bitmap is null) e.Bitmap = new SKBitmap((int)(e.TargetSize.Width * e.ResolutionLevel), (int)(e.TargetSize.Height * e.ResolutionLevel));

            

            foreach (var obj in ApplicateObjects)
            {
                IMetaDrawable drawObject = (IMetaDrawable)obj;
                DrawExpresserArgs express = new()
                {
                    TargetSize = e.TargetSize,
                    ResolutionLevel = e.ResolutionLevel,
                    FPS = e.FPS
                };
                drawObject.DrawExpresser(ref express, frame);


                if (express.Bitmap is not null)
                {
                    double x = 0;
                    double y = 0;
                    double rotate = 0;
                    double alpha = 0;
                    double scale = 100;

                    using (SKCanvas canvas = new SKCanvas(e.Bitmap))
                    {
                        //座標持ってたら反映
                        if (drawObject is IMetaCoordable)
                        {
                            IMetaCoordable coordObject = (IMetaCoordable)drawObject;
                            x = coordObject.X.Get(frame);
                            y = coordObject.Y.Get(frame);
                            rotate = coordObject.Rotation.Get(frame);
                            alpha = coordObject.Alpha.Get(frame);
                            scale = coordObject.Scale.Get(frame);
                        }

                        if (rotate != 0) express.Bitmap = MetasiaBitmap.Rotate(express.Bitmap, rotate);
                        if (alpha != 0.0) express.Bitmap = MetasiaBitmap.Transparency(express.Bitmap, (100 - alpha) / 100);


                        //中央を座標0,0とするために位置調整
                        double width = express.Bitmap.Width * (scale / 100f);
                        double height = express.Bitmap.Height * (scale / 100f);
                        SKRect drawPos = new SKRect()
                        {
                            Left = (float)(((e.TargetSize.Width - width) / 2 + x) * e.ResolutionLevel),
                            Top = (float)(((e.TargetSize.Height - height) / 2 - y) * e.ResolutionLevel),
                            Right = (float)(((e.TargetSize.Width - width) / 2 + x) * e.ResolutionLevel + width * e.ResolutionLevel),
                            Bottom = (float)(((e.TargetSize.Height - height) / 2 - y) * e.ResolutionLevel + height * e.ResolutionLevel)
                        };

                        canvas.DrawBitmap(express.Bitmap, drawPos);
                    }
                }

                express.Dispose();
            }
        }

        public void AudioExpresser(ref AudioExpresserArgs e, int frame)
        {
            List<MetasiaObject> ApplicateObjects = new();
            foreach (var obj in Objects)
            {
                if (obj.IsExistFromFrame(frame) && obj is IMetaAudiable && obj.IsActive)
                {
                    ApplicateObjects.Add(obj);
                }
            }
            if (ApplicateObjects.Count == 0) return;

            if (e.Sound is null) e.Sound = new MetasiaSound(e.AudioChannel, e.SoundSampleRate, (ushort)e.FPS);

            foreach (var o in ApplicateObjects)
            {
                IMetaAudiable audiableObject = (IMetaAudiable)o;
                AudioExpresserArgs express = new()
                {
                    AudioChannel = e.AudioChannel,
                    SoundSampleRate = e.SoundSampleRate,
                    FPS = e.FPS
                };
                audiableObject.AudioExpresser(ref express, frame);

                if (express.Sound is not null)
                {

                    if (audiableObject.Volume != 100)
                    {
                        express.Sound = MetasiaSound.VolumeChange(express.Sound, audiableObject.Volume / 100);
                    }

                    e.Sound = MetasiaSound.SynthesisPulse(e.AudioChannel, e.Sound, express.Sound);
                }

                express.Dispose();
            }
        }

        
    }
}
