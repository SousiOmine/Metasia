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
        public double Volume { get; set; } = 100;

        /// <summary>
        /// レイヤー名
        /// </summary>
        public string Name { get; set; }

        public LayerObject(string id, string LayerName) : base(id)
        {
            Name = LayerName;
            StartFrame = 0;
            EndFrame = int.MaxValue;
        }

        public LayerObject()
        {
            StartFrame = 0;
            EndFrame = int.MaxValue;
        }

        public void DrawExpresser(ref DrawExpresserArgs e, int frame)
        {
            double resolution_level_x = e.ActualResolution.Width / e.TargetResolution.Width;
            double resolution_level_y = e.ActualResolution.Height / e.TargetResolution.Height;
            
            List<MetasiaObject> ApplicateObjects = new();
            foreach (var obj in Objects) 
            {
                if(obj.IsExistFromFrame(frame) && obj is IMetaDrawable && obj.IsActive)
                {
                    ApplicateObjects.Add(obj);
                }
            }

            if (ApplicateObjects.Count == 0) return;

            if (e.Bitmap is null) e.Bitmap = new SKBitmap((int)(e.ActualResolution.Width), (int)(e.ActualResolution.Height));

            

            foreach (var obj in ApplicateObjects)
            {
                IMetaDrawable drawObject = (IMetaDrawable)obj;
                DrawExpresserArgs express = new()
                {
                    ActualResolution = e.ActualResolution,
                    TargetResolution = e.TargetResolution,
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

                        if (rotate != 0)
                        {
                            express.Bitmap = MetasiaBitmap.Rotate(express.Bitmap, rotate);
                            express.TargetSize = new SKSize(
                                (int)(express.TargetSize.Value.Width *
                                      (express.Bitmap.Width / express.ActualSize.Value.Width)),
                                (int)(express.TargetSize.Value.Height *
                                      (express.Bitmap.Height / express.ActualSize.Value.Height))
                            );
                            express.ActualSize = new SKSize(express.Bitmap.Width, express.Bitmap.Height);
                        }
                        if (alpha != 0.0) express.Bitmap = MetasiaBitmap.Transparency(express.Bitmap, (100 - alpha) / 100);

                        double width = express.TargetSize.Value.Width * (scale / 100f);
                        double height = express.TargetSize.Value.Height * (scale / 100f);
                        
                        SKRect drawPos = new SKRect()
                        {
                            Left = (float)((e.TargetResolution.Width - width) / 2 + x) * (e.ActualResolution.Width / e.TargetResolution.Width),
                            Top = (float)((e.TargetResolution.Height - height) / 2 - y) * (e.ActualResolution.Height / e.TargetResolution.Height),
                            Right = (float)(((e.TargetResolution.Width - width) / 2 + x + width) * (e.ActualResolution.Width / e.TargetResolution.Width)),
                            Bottom = (float)(((e.TargetResolution.Height - height) / 2 - y + height) * (e.ActualResolution.Height / e.TargetResolution.Height)),
                        };

                        canvas.DrawBitmap(express.Bitmap, drawPos);
                    }
                }

                express.Dispose();
            }
            
            e.ActualSize = new SKSize(e.Bitmap.Width, e.Bitmap.Height);
            e.TargetSize = e.TargetResolution;
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
