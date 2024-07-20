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
		
		public void BitmapRender(ref DrawExpresserArgs args, int frame)
		{
			//Listの中からMainTimelineというIDを持つListObjectを取得する
			RootTimeline = Project.Timelines.Find(x => x.Id == "RootTimeline");
			if (RootTimeline is null) return;

			//下地は黒で塗りつぶす
			using (SKCanvas canvas = new SKCanvas(args.Bitmap))
			{
				canvas.Clear(SKColors.Black);
			}

			RootTimeline.DrawExpresser(ref args, frame);
		}
		
		public void AudioRender(ref AudioExpresserArgs args, int frame)
		{
			//Listの中からMainTimelineというIDを持つListObjectを取得する
			RootTimeline = Project.Timelines.Find(x => x.Id == "RootTimeline");
			if (RootTimeline is null) return;

			RootTimeline.AudioExpresser(ref args, frame);
		}

	}
}
