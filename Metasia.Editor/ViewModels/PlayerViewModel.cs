﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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


		public TimelineObject TargetTimeline;

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
		
		public ObservableCollection<MetasiaObject> TargetObjects = new();

		public Action? ViewPaintRequest;
		public Action? PlayStart;

        public ICommand NextFrame { get; }
        public ICommand PreviousFrame { get; }
		public ICommand Play { get; }
		public ICommand Pause { get; }
		public PlayerViewModel(TimelineObject targetTimeline)
		{
			TargetTimeline = targetTimeline;

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