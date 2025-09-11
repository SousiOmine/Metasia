using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DynamicData;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.Services.Audio;
using ReactiveUI;

namespace Metasia.Editor.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
		private bool _isPlaying = false;

		private int frame = 0;
		public int audioVolume { get; set; } = 100;
		private int sliderMaximum = 100;
		private int sliderMinimum = 0;
		
		public ProjectInfo TargetProjectInfo { get; private set; }
		
		public TimelineObject TargetTimeline { get; private set; }
		

		public event EventHandler? ProjectChanged;

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
				playbackState.RequestReRendering();
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
		
		public ObservableCollection<ClipObject> SelectingObjects = new();

		public Action? PlayStart;

        public ICommand NextFrame { get; }
        public ICommand PreviousFrame { get; }
		public ICommand Play { get; }
		public ICommand Pause { get; }
		public ICommand PlayPauseToggle { get; }
		private IPlaybackState playbackState;
		private IProjectState projectState;
		private IEditCommandManager _editCommandManager;
		public PlayerViewModel(TimelineObject targetTimeline, 
			ProjectInfo projectInfo, 
			ISelectionState selectionState,
			IPlaybackState playbackState,
			IProjectState projectState,
			IEditCommandManager editCommandManager)
		{
			TargetTimeline = targetTimeline;
			TargetProjectInfo = projectInfo;

			this.playbackState = playbackState;
			this.projectState = projectState;
			_editCommandManager = editCommandManager;
			selectionState.SelectionChanged += () =>
			{
				SelectingObjects.Clear();
				SelectingObjects.AddRange(selectionState.SelectedClips);
			};

			// コマンドが実行されたりUndoRedoされたときに再描画&タイムライン変更を通知する
			_editCommandManager.CommandExecuted += (sender, command) =>
			{
				playbackState.RequestReRendering();
				projectState.NotifyTimelineChanged();
			};
			_editCommandManager.CommandUndone += (sender, command) =>
			{
				playbackState.RequestReRendering();
				projectState.NotifyTimelineChanged();
			};
			_editCommandManager.CommandRedone += (sender, command) =>
			{
				playbackState.RequestReRendering();
				projectState.NotifyTimelineChanged();
			};

            NextFrame = ReactiveCommand.Create(() =>
            {
                playbackState.Seek(Frame + 1);
            });
            PreviousFrame = ReactiveCommand.Create(() =>
            {
                playbackState.Seek(Frame - 1);
            });
			Play = ReactiveCommand.Create(PlayMethod);
			Pause = ReactiveCommand.Create(PauseMethod);
			PlayPauseToggle = ReactiveCommand.Create(() =>
			{
				if (IsPlaying)
				{
					Pause.Execute(null);
				}
				else
				{
					Play.Execute(null);
				}
			});

			playbackState.PlaybackFrameChanged += () =>
			{
				Frame = playbackState.CurrentFrame;
			};
			playbackState.PlaybackStarted += () =>
			{
				IsPlaying = true;
			};
			playbackState.PlaybackPaused += () =>
			{
				IsPlaying = false;
			};
			playbackState.PlaybackSeeked += () =>
			{
				Frame = playbackState.CurrentFrame;
			};

            NotifyProjectChanged();
		}

		/// <summary>
		/// プロジェクトに変更が加わったらこれを呼び出す
		/// </summary>
		public void NotifyProjectChanged()
		{
			//再生されてなければ再描写する
			if(playbackState.IsPlaying == false) playbackState.RequestReRendering();

			SliderMaximum = TargetTimeline.EndFrame;

            ProjectChanged?.Invoke(this, EventArgs.Empty);
        }

		private void PlayMethod()
		{
			playbackState.Play();
		}

		private void PauseMethod()
		{
			playbackState.Pause();
		}

	}
}
