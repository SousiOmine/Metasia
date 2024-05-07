using System;
using Metasia.Core.Graphics;
using Metasia.Core.Objects;
using SkiaSharp;

namespace Metasia.Core.Render
{
    public class LayoutsExpresser
    {
        /// <summary>
        /// 複数オブジェクトを、指定されたフレームでExpresserArgsに描写する
        /// </summary>
        /// <param name="objects">描写したいオブジェクトのリスト</param>
        /// <param name="e">描写先のExpresserArgs</param>
        /// <param name="frame">描写するフレーム</param>
        public static void DrawObjects(List<MetasiaObject> objects, ref ExpresserArgs e, int frame)
        {
            //Layerの昇順に並び替える
            objects = objects.OrderBy(o => o.Layer).ToList();

            foreach (var o in objects)
			{
				using (SKCanvas canvas = new SKCanvas(e.bitmap))
				{
					ExpresserArgs express = new()
					{
						targetSize = e.targetSize,
						ResolutionLevel = e.ResolutionLevel,
						SoundChannel = e.SoundChannel,
						SoundSampleRate = e.SoundSampleRate,
						Fps = e.Fps
					};
					o.Expression(ref express, frame);

					//もし帰ってきたデータに画像があれば描写
					if (express.bitmap is not null)
					{
						if (o.Rotation != 0) express.bitmap = MetasiaBitmap.Rotate(express.bitmap, o.Rotation);
						if (o.Alpha != 100) express.bitmap = MetasiaBitmap.Transparency(express.bitmap, o.Alpha / 100);
					
					
						// オブジェクト画像の大きさを指定して描写
						float width = express.bitmap.Width * (o.Scale / 100f);
						float height = express.bitmap.Height * (o.Scale / 100f);
						SKRect drawPos = new SKRect(
							((e.targetSize.Width - width) / 2 + o.X) * e.ResolutionLevel, 
							((e.targetSize.Height - height) / 2 - o.Y) * e.ResolutionLevel, 
							((e.targetSize.Width - width) / 2 + o.X) * e.ResolutionLevel + width * e.ResolutionLevel, 
							((e.targetSize.Height - height) / 2 - o.Y) * e.ResolutionLevel + height * e.ResolutionLevel
						);
					
						canvas.DrawBitmap(express.bitmap, drawPos);
					}
					
					//もし帰ってきたデータに音声があれば合成
					if (express.sound is not null)
					{
						for(int i = 0; i < express.sound.Pulse.Length; i++)
						{
							int sum = e.sound.Pulse[i] + express.sound.Pulse[i];
							if(sum > short.MaxValue) sum = short.MaxValue;
							else if (sum < short.MinValue) sum = short.MinValue;
							else e.sound.Pulse[i] = (short)sum;
						}
					}
					
					express.Dispose();
				}

			}
        }
    }
}