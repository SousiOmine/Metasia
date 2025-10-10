using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using ReactiveUI;

namespace Metasia.Editor.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        private const int PreviewMarginSeconds = 5;
        private bool _isPlaying;
        private int frame;
        private int sliderMaximum = 100;
        private int sliderMinimum;

        public int AudioVolume { get; set; } = 100;

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
            set
            {
                if (frame != value)
                {
                    this.RaiseAndSetIfChanged(ref frame, value);
                    playbackState.RequestReRendering();
                }
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

        public ObservableCollection<ClipObject> SelectingObjects { get; } = new();

        public Action? PlayStart;

        public ICommand NextFrame { get; }
        public ICommand PreviousFrame { get; }
        public ICommand Play { get; }
        public ICommand Pause { get; }
        public ICommand PlayPauseToggle { get; }

        private readonly IPlaybackState playbackState;
        private readonly IProjectState projectState;
        private readonly IEditCommandManager _editCommandManager;
        private readonly ISelectionState selectionState;

        public PlayerViewModel(
            TimelineObject targetTimeline,
            ProjectInfo projectInfo,
            ISelectionState selectionState,
            IPlaybackState playbackState,
            IProjectState projectState,
            IEditCommandManager editCommandManager)
        {
            TargetTimeline = targetTimeline;
            TargetProjectInfo = projectInfo;
            this.selectionState = selectionState;
            this.playbackState = playbackState;
            this.projectState = projectState;
            _editCommandManager = editCommandManager;

            selectionState.SelectionChanged += () =>
            {
                SelectingObjects.Clear();
                SelectingObjects.AddRange(selectionState.SelectedClips);
            };

            _editCommandManager.CommandExecuted += (_, _) =>
            {
                playbackState.RequestReRendering();
                projectState.NotifyTimelineChanged();
            };
            _editCommandManager.CommandPreviewExecuted += (_, _) => playbackState.RequestReRendering();
            _editCommandManager.CommandUndone += (_, _) =>
            {
                playbackState.RequestReRendering();
                projectState.NotifyTimelineChanged();
            };
            _editCommandManager.CommandRedone += (_, _) =>
            {
                playbackState.RequestReRendering();
                projectState.NotifyTimelineChanged();
            };

            NextFrame = ReactiveCommand.Create(() => playbackState.Seek(Frame + 1));
            PreviousFrame = ReactiveCommand.Create(() => playbackState.Seek(Frame - 1));
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

            playbackState.PlaybackFrameChanged += OnPlaybackFrameChanged;
            playbackState.PlaybackStarted += () => IsPlaying = true;
            playbackState.PlaybackPaused += () => IsPlaying = false;
            playbackState.PlaybackSeeked += OnPlaybackFrameChanged;

            NotifyProjectChanged();
        }

        public void NotifyProjectChanged()
        {
            if (!playbackState.IsPlaying)
            {
                playbackState.RequestReRendering();
            }

            SliderMinimum = TargetTimeline.StartFrame;
            SliderMaximum = TargetTimeline.EndFrame + PreviewMarginFrames;
            EnsureSliderRangeIncludes(Frame);

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

        private void EnsureSliderRangeIncludes(int currentFrame)
        {
            int baseMax = TargetTimeline.EndFrame + PreviewMarginFrames;
            int desiredMax = Math.Max(baseMax, currentFrame + PreviewMarginFrames);
            if (desiredMax > SliderMaximum)
            {
                SliderMaximum = desiredMax;
            }
        }

        private int PreviewMarginFrames => TargetProjectInfo?.Framerate is int fps ? fps * PreviewMarginSeconds : 0;

        private void OnPlaybackFrameChanged()
        {
            Frame = playbackState.CurrentFrame;
        }
    }
}
