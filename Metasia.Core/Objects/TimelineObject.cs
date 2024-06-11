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
		public List<MetasiaObject> Objects { get; protected set; } = new();

		public float Volume { get; set; }
		
		public TimelineObject(string id) : base(id)
		{
			Volume = 100;
		}

		/*public override void Expression(ref ExpresserArgs e, int frame)
		{
			if (e.bitmap is null) e.bitmap = new SKBitmap((int)(e.targetSize.Width * e.ResolutionLevel), (int)(e.targetSize.Height * e.ResolutionLevel));

			//描写対象のオブジェクトを抽出し、Layerの昇順に並び替える
			List<MetasiaObject> ApplicateObjects = new();
			foreach (var o in Objects)
			{
				if (frame < o.StartFrame || frame > o.EndFrame) continue;
				ApplicateObjects.Add(o);
			}

			LayoutsExpresser.DrawObjects(ApplicateObjects, ref e, frame);

			base.Expression(ref e, frame);
		}*/

		public void DrawExpresser(ref DrawExpresserArgs e, int frame)
		{
			if (e.Bitmap is null) e.Bitmap = new SKBitmap((int)(e.TargetSize.Width * e.ResolutionLevel), (int)(e.TargetSize.Height * e.ResolutionLevel));
			
			List<MetasiaObject> ApplicateObjects = new();
			foreach (var o in Objects)
			{
				if (frame < o.StartFrame || frame > o.EndFrame) continue;
				if(o is IMetaDrawable) ApplicateObjects.Add(o);
			}

			foreach (var o in ApplicateObjects)
			{
				IMetaDrawable drawObject = (IMetaDrawable)o;
				DrawExpresserArgs express = new()
				{
					TargetSize = e.TargetSize,
					ResolutionLevel = e.ResolutionLevel,
					FPS = e.FPS
				};
				drawObject.DrawExpresser(ref express, frame);

				if (express.Bitmap is not null)
				{
					float x = 0;
					float y = 0;
					float rotate = 0;
					float alpha = 0;
					float scale = 100;
					
					using (SKCanvas canvas = new SKCanvas(e.Bitmap))
					{
						if (drawObject is IMetaCoordable)
						{
							IMetaCoordable coordObject = (IMetaCoordable)drawObject;
							x = coordObject.X;
							y = coordObject.Y;
							rotate = coordObject.Rotation;
							alpha = coordObject.Alpha;
							scale = coordObject.Scale;
						}
						
						if(rotate != 0) express.Bitmap = MetasiaBitmap.Rotate(express.Bitmap, rotate);
						if(alpha != 100) express.Bitmap = MetasiaBitmap.Transparency(express.Bitmap, alpha / 100);
						
						float width = express.Bitmap.Width * (scale / 100f);
						float height = express.Bitmap.Height * (scale / 100f);
						SKRect drawPos = new SKRect()
						{
							Left = ((e.TargetSize.Width - width) / 2 + x) * e.ResolutionLevel,
							Top = ((e.TargetSize.Height - height) / 2 - y) * e.ResolutionLevel,
							Right = ((e.TargetSize.Width - width) / 2 + x) * e.ResolutionLevel + width * e.ResolutionLevel,
							Bottom = ((e.TargetSize.Height - height) / 2 - y) * e.ResolutionLevel + height * e.ResolutionLevel
						};
						
						canvas.DrawBitmap(express.Bitmap, drawPos);
					}
				}
				
				express.Dispose();
			}
			
		}
		

		public void AudioExpresser(ref AudioExpresserArgs e, int frame)
		{
			if (e.Sound is null) e.Sound = new MetasiaSound(e.AudioChannel, e.SoundSampleRate, (ushort)e.FPS);
			
			List<MetasiaObject> ApplicateObjects = new();
			foreach (var o in Objects)
			{
				if (frame < o.StartFrame || frame > o.EndFrame) continue;
				if(o is IMetaAudiable) ApplicateObjects.Add(o);
			}

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
					
					foreach (var val in express.Sound.Pulse)
					{
						//Console.WriteLine(val);
					}

					e.Sound = MetasiaSound.SynthesisPulse(e.AudioChannel, e.Sound, express.Sound);
				}
				
				express.Dispose();
			}
		}
	}
}
