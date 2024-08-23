using System;
using System.Threading;
using System.Timers;
using System.Windows.Input;
using DynamicData;
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

		/// <summary>
		/// 再生中であるか否か
		/// </summary>
		public bool IsPlaying 
		{ 
			get => _isPlaying;
			set => this.RaiseAndSetIfChanged(ref _isPlaying, value); 
		}

		/// <summary>
		/// 現在表示しているフレーム 変更すると再描写する
		/// </summary>
		public int Frame
		{
			get => frame;
			set {
				this.RaiseAndSetIfChanged(ref frame, value);
				ViewPaintRequest?.Invoke();
			}
		}

		/// <summary>
		/// 再生バーの最大値
		/// </summary>
		public int SliderMaximum
		{
			get => sliderMaximum;
			set => this.RaiseAndSetIfChanged(ref sliderMaximum, value);
		}

		/// <summary>
		/// 再生バーの最小値
		/// </summary>
		public int SliderMinimum
		{
			get => sliderMinimum;
			set => this.RaiseAndSetIfChanged(ref sliderMinimum, value);
		}

		public Action? ViewPaintRequest;
		public Action? PlayStart;

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
				PlayStart?.Invoke();
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
			MetasiaProvider.MetasiaProject.LastFrame = 239;

			kariHelloObject kariHello = new kariHelloObject("karihello")
	    	{ 
	    		EndFrame = 120,
	    	};
            kariHello.Rotation.Params.Add(new CoordPoint() { Value = 90, Frame = 120 });

            kariHelloObject kariHello2 = new kariHelloObject("karihello2")
	    	{
	    		EndFrame = 10,
	    	};
            kariHello2.Y.Params[0].Value = 300;
            kariHello2.Rotation.Params[0].Value = 45;
            kariHello2.Alpha.Params[0].Value = 50;
            kariHello2.Scale.Params[0].Value = 50;
            kariHello2.X.Params.Add(new CoordPoint() { Value = 1000, Frame = 10 });

            Text text = new Text("konnichiwa")
			{
                EndFrame = 120,
				TypefaceName = "LINE Seed JP_TTF",
                Contents = "こんにちは Hello",
			};
            text.TextSize.Params[0].Value = 400;

            Text onesec = new Text("sec1")
			{
                EndFrame = 59,
                TypefaceName = "LINE Seed JP_TTF",
                Contents = "1",
            };
			onesec.TextSize.Params[0].Value = 200;
			onesec.X.Params[0].Value = -1800;
			onesec.Y.Params[0].Value = 900;

			Text twosec = new Text("sec2")
			{
				StartFrame = 60,
				EndFrame = 119,
				TypefaceName = "LINE Seed JP_TTF",
				Contents = "2",
			};
            twosec.TextSize.Params[0].Value = 200;
            twosec.X.Params[0].Value = -1800;
            twosec.Y.Params[0].Value = 900;

			Text foursec = new Text("sec4")
			{
				StartFrame = 180,
				EndFrame = 239,
				TypefaceName = "LINE Seed JP_TTF",
				Contents = "4",
			};
            foursec.TextSize.Params[0].Value = 200;
            foursec.X.Params[0].Value = -1800;
            foursec.Y.Params[0].Value = 900;

            LayerObject layer1 = new LayerObject("layer1", "Layer 1");
			LayerObject layer2 = new LayerObject("layer2", "Layer 2");
			LayerObject layer3 = new LayerObject("layer3", "Layer 3");
			LayerObject layer4 = new LayerObject("layer4", "Layer 4");
		    
			
			
			
	    	TimelineObject mainTL = new TimelineObject("RootTimeline");

			layer1.Objects.Add(kariHello);
			layer2.Objects.Add(kariHello2);
			layer3.Objects.Add(text);
			layer4.Objects.Add(onesec);
			layer4.Objects.Add(twosec);
            layer4.Objects.Add(foursec);
            mainTL.Layers.Add(layer1);
			mainTL.Layers.Add(layer2);
			mainTL.Layers.Add(layer3);
			mainTL.Layers.Add(layer4);

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