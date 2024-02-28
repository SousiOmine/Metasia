﻿using Metasia.Core.Graphics;
using Metasia.Core.Render;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Objects
{
	/// <summary>
	/// レイヤー専用のオブジェクト ObjectsをResolutionLevelに沿った解像度で描画できる
	/// </summary>
	public class LayerObject : MetasiaObject
	{
		public List<MetasiaObject> Objects { get; set; } = new();

		public LayerObject(string id) : base(id)
		{
		}

		public override void Expression(ref ExpresserArgs e, int frame)
		{
			if (e.bitmap is null) e.bitmap = new SKBitmap((int)(e.targetSize.Width * e.ResolutionLevel), (int)(e.targetSize.Height * e.ResolutionLevel));

			//ここでObjectsを各座標とかを考慮し描写する

			foreach (var o in Objects)
			{
				if (frame < o.StartFrame || frame > o.EndFrame) continue;

				using (SKCanvas canvas = new SKCanvas(e.bitmap))
				{
					ExpresserArgs express = new()
					{
						targetSize = e.targetSize,
						ResolutionLevel = e.ResolutionLevel
					};
					o.Expression(ref express, frame);

					//回転
					if (o.Rotation != 0) express.bitmap = MetasiaBitmap.Rotate(express.bitmap, o.Rotation);

					//オブジェクト画像の大きさを指定して描写
					float startx = ((e.targetSize.Width - express.bitmap.Width) / 2 + o.X) * e.ResolutionLevel;
					float starty = ((e.targetSize.Height - express.bitmap.Height) / 2 - o.Y) * e.ResolutionLevel;
					float endx = ((e.targetSize.Width - express.bitmap.Width) / 2 + o.X + express.bitmap.Width) * e.ResolutionLevel;
					float endy = ((e.targetSize.Height - express.bitmap.Height) / 2 - o.Y + express.bitmap.Height) * e.ResolutionLevel;

					SKRect drawPos = new SKRect(startx, starty, endx, endy);

					canvas.DrawBitmap(express.bitmap, drawPos);
					express.Dispose();
				}

			}

			base.Expression(ref e, frame);
		}
	}
}
