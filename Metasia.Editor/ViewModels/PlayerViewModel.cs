using System;
using System.Threading;
using System.Timers;
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
		private bool _isPlaying = false;

		private int frame = 0;
		public int audioVolume { get; set; } = 100;
		private int sliderMaximum = 100;
		private int sliderMinimum = 0;
		private System.Timers.Timer? timer;

		public bool IsPlaying 
		{ 
			get => _isPlaying;
			set => this.RaiseAndSetIfChanged(ref _isPlaying, value); 
		}

		public int Frame
		{
			get => frame;
			set {
				this.RaiseAndSetIfChanged(ref frame, value);
				ViewPaintRequest?.Invoke();
			}
		}

		public int SliderMaximum
		{
			get => sliderMaximum;
			set => this.RaiseAndSetIfChanged(ref sliderMaximum, value);
		}

		public int SliderMinimum
		{
			get => sliderMinimum;
			set => this.RaiseAndSetIfChanged(ref sliderMinimum, value);
		}

		public Action? ViewPaintRequest;

        public ICommand NextFrame { get; }
        public ICommand PreviousFrame { get; }
		public ICommand Play { get; }
		public ICommand Pause { get; }
		public PlayerViewModel()
        {

            NextFrame = ReactiveCommand.Create(() =>
            {
                Frame++;
            });
            PreviousFrame = ReactiveCommand.Create(() =>
            {
                Frame--;
            });
			Play = ReactiveCommand.Create(() =>
			{
				if(MetasiaProvider.MetasiaProject is null) return;
				timer = new System.Timers.Timer(1000 / MetasiaProvider.MetasiaProject.Info.Framerate);
				timer.Elapsed += Timer_Elapsed;
				timer.Start();
				IsPlaying = true;
			});
			Pause = ReactiveCommand.Create(() =>
			{
				if(timer is not null) timer.Stop();
				IsPlaying = false;
			});


			ProjectInfo info = new ProjectInfo()
		    {
	    	    Framerate = 60,
	    		Size = new SKSize(3840, 2160),
	    	};
	    	MetasiaProvider.MetasiaProject = new MetasiaProject(info);
			MetasiaProvider.MetasiaProject.LastFrame = 120;

			kariHelloObject kariHello = new kariHelloObject("karihello")
	    	{ 
	    		EndFrame = 120,
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
			kariHello.Rotation_Points.Add(new CoordPoint(){Value = 90, Frame = 120});
	    	TimelineObject mainTL = new TimelineObject("RootTimeline");
	    	mainTL.Objects.Add(kariHello);
	    	mainTL.Objects.Add(kariHello2);
	    	MetasiaProvider.MetasiaProject.Timelines.Add(mainTL);

			NotifyProjectChanged();
		}

		private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
		{
			Frame++;
		}

		/// <summary>
		/// プロジェクトに変更が加わったらこれを呼び出す
		/// </summary>
		public void NotifyProjectChanged()
		{
			//再生されてなければ再描写する
			if(IsPlaying == false) ViewPaintRequest?.Invoke();

			if(MetasiaProvider.MetasiaProject is not null)
			{
				//スライダーの最大値を変更
				SliderMaximum = MetasiaProvider.MetasiaProject.LastFrame;
			}
		}

	}
}