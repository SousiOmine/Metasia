using System;
using System.Windows.Input;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models;
using ReactiveUI;
using SkiaSharp;

namespace Metasia.Editor.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        public int frame { get; set; } = 0;
        public Action ViewPaintRequest;

        public ICommand NextFrame { get; }
        public ICommand PreviousFrame { get; }
        public PlayerViewModel()
        {

            NextFrame = ReactiveCommand.Create(() =>
            {
                frame++;
                ViewPaintRequest?.Invoke();
            });
            PreviousFrame = ReactiveCommand.Create(() =>
            {
                frame--;
                ViewPaintRequest?.Invoke();
            });
            
            ProjectInfo info = new ProjectInfo()
		    {
	    	    Framerate = 60,
	    		Size = new SKSize(500, 300),
	    	};
	    	MetasiaProvider.MetasiaProject = new MetasiaProject(info);

	    	kariHelloObject kariHello = new kariHelloObject("karihello")
	    	{ 
	    		EndFrame = 10,
	    		Layer = 1
	    	};
	    	kariHelloObject kariHello2 = new kariHelloObject("karihello2")
	    	{
	    		EndFrame = 10,
	    		Layer = 2
	    	};

			kariHello2.Y_Points[0].Value = 300;
			kariHello2.Rotation_Points[0].Value = 45;
			kariHello2.Alpha_Points[0].Value = 50;
			kariHello2.Scale_Points[0].Value = 50;
			kariHello.Rotation_Points.Add(new CoordPoint(){Value = 90, Frame = 10});
	    	TimelineObject mainTL = new TimelineObject("RootTimeline");
	    	mainTL.Objects.Add(kariHello);
	    	mainTL.Objects.Add(kariHello2);
	    	MetasiaProvider.MetasiaProject.Timelines.Add(mainTL);
        }
    }
}