using Metasia.Core.Graphics;
using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Metasia.Core.Sounds;

namespace Metasia.Core.Objects
{
	/// <summary>
	/// タイムライン専用のオブジェクト
	/// </summary>
	public class TimelineObject : MetasiaObject, IMetaDrawable, IMetaAudiable
	{
		//public List<MetasiaObject> Objects { get; protected set; } = new();
		public List<LayerObject> Layers { get; protected set; } = new();

		public double Volume { get; set; }
		
		public TimelineObject(string id) : base(id)
		{
			Volume = 100;
		}

		public void DrawExpresser(ref DrawExpresserArgs e, int frame)
		{
			//DrawExpresserArgsのSKBitmapのインスタンスがなかったら生成
			if (e.Bitmap is null) e.Bitmap = new SKBitmap((int)(e.TargetSize.Width * e.ResolutionLevel), (int)(e.TargetSize.Height * e.ResolutionLevel));

			foreach (var layer in Layers)
			{
				if (!layer.IsActive) continue;
                DrawExpresserArgs express = new()
                {
                    TargetSize = e.TargetSize,
                    ResolutionLevel = e.ResolutionLevel,
                    FPS = e.FPS
                };
				layer.DrawExpresser(ref express, frame);
				if (express.Bitmap is null) continue;

                using (SKCanvas canvas = new SKCanvas(e.Bitmap))
				{
					canvas.DrawBitmap(express.Bitmap, 0, 0);
				}

				express.Dispose();
            }
		}
		

		public void AudioExpresser(ref AudioExpresserArgs e, int frame)
		{
			//AudioExpresserArgsのMetasiaSoundのインスタンスがなかったら生成
			if (e.Sound is null) e.Sound = new MetasiaSound(e.AudioChannel, e.SoundSampleRate, (ushort)e.FPS);

			foreach (var layer in Layers)
			{
                if (!layer.IsActive) continue;
                AudioExpresserArgs express = new()
                {
                    AudioChannel = e.AudioChannel,
                    SoundSampleRate = e.SoundSampleRate,
                    FPS = e.FPS
                };
				layer.AudioExpresser(ref express, frame);

				if(express.Sound is null) continue;

                if (layer.Volume != 100)
                {
                    express.Sound = MetasiaSound.VolumeChange(express.Sound, layer.Volume / 100);
                }

                e.Sound = MetasiaSound.SynthesisPulse(e.AudioChannel, e.Sound, express.Sound);

				express.Dispose();
            }
			
		}
	}
}
