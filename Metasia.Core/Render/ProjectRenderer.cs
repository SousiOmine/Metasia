using Metasia.Core.Objects;
using Metasia.Core.Project;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Core.Render
{
	public class ProjectRenderer
	{
		MetasiaProject Project;
		ListObject MainTimeline;

		public ProjectRenderer(MetasiaProject project) 
		{
			Project = project;
			
		}

		public void Render(ref SKCanvas canvas, int frame)
		{
			//Listの中からMainTimelineというIDを持つListObjectを取得する
			MainTimeline = Project.Timelines.Find(x => x.Id == "MainTimeline");
			if (MainTimeline is null) return;

			var bmp = new SKBitmap(300, 300);

			using (SKCanvas canvas2 = new SKCanvas(bmp))
			{
				canvas2.Clear(SKColors.Black);
			}

			ExpresserArgs args = new()
			{
				bitmap = bmp,
				targetSize = new SKSize(300, 300),
				ResolutionLevel = 1
			};

			MainTimeline.Expression(ref args, frame);

			canvas.DrawBitmap(args.bitmap, 0, 0);
			bmp.Dispose();

		}
	}
}
