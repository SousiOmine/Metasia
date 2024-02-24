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
		TimelineObject? RootTimeline;

		public ProjectRenderer(MetasiaProject project) 
		{
			Project = project;
			
		}

		public void Render(ref SKCanvas canvas, int frame)
		{
			//Listの中からMainTimelineというIDを持つListObjectを取得する
			RootTimeline = Project.Timelines.Find(x => x.Id == "RootTimeline");
			if (RootTimeline is null) return;

			var bmp = new SKBitmap(384, 216);

			using (SKCanvas canvas2 = new SKCanvas(bmp))
			{
				canvas2.Clear(SKColors.Black);
			}

			ExpresserArgs args = new()
			{
				bitmap = bmp,
				targetSize = new SKSize(3840, 2160),
				ResolutionLevel = 0.1f
			};

			RootTimeline.Expression(ref args, frame);

			canvas.DrawBitmap(bmp, 0, 0);
			bmp.Dispose();

		}
	}
}
