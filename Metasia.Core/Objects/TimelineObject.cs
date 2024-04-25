using Metasia.Core.Graphics;
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
	/// タイムライン専用のオブジェクト
	/// </summary>
	public class TimelineObject : MetasiaObject
	{
		public List<MetasiaObject> Objects { get; set; } = new();

		public TimelineObject(string id) : base(id)
		{
		}

		public override void Expression(ref ExpresserArgs e, int frame)
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
		}
	}
}
