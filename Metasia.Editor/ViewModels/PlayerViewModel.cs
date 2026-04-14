using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DynamicData;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Services;
using ReactiveUI;

namespace Metasia.Editor.ViewModels
{
    public class PlayerViewModel : ViewModelBase
    {
        private bool _isPlaying;
        private int frame;
        private int sliderMaximum = 100;
        private int sliderMinimum;
        private bool _isUpdatingFrameFromPlayback;

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
        public string CurrentTime
        {
            get
            {
                if (TargetProjectInfo is null || TargetProjectInfo.Framerate <= 0) return "00:00:00.00";
                double totalSeconds = (double)Frame / TargetProjectInfo.Framerate;
                var ts = TimeSpan.FromSeconds(totalSeconds);
                return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{(ts.Milliseconds / 10):D2}";
            }
        }

        public int Frame
        {
            get => frame;
            set
            {
                if (frame != value)
                {
                    this.RaiseAndSetIfChanged(ref frame, value);
                    this.RaisePropertyChanged(nameof(CurrentTime));

                    if (!_isUpdatingFrameFromPlayback)
                    {
                        playbackState.Seek(value);
                        playbackState.RequestReRendering();
                    }
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

        public int SliderSelectionStart
        {
            get => _sliderSelectionStart;
            set => this.RaiseAndSetIfChanged(ref _sliderSelectionStart, value);
        }

        public int SliderSelectionEnd
        {
            get => _sliderSelectionEnd;
            set => this.RaiseAndSetIfChanged(ref _sliderSelectionEnd, value);
        }

        public ObservableCollection<ClipObject> SelectingObjects { get; } = new();

        public Action? PlayStart;

        public ICommand NextFrame { get; }
        public ICommand PreviousFrame { get; }
        public ICommand Play { get; }
        public ICommand Pause { get; }
        public ICommand PlayPauseToggle { get; }

        public ICommand SetSelectionStart { get; }
        public ICommand SetSelectionEnd { get; }


        private int _sliderSelectionStart;
        private int _sliderSelectionEnd;

        private readonly IPlaybackState playbackState;
        private readonly IProjectState projectState;
        private readonly IEditCommandManager _editCommandManager;
        private readonly ISelectionState selectionState;

        public string ProjectPath => projectState.CurrentProject?.ProjectPath?.Path ?? string.Empty;

        public IReadOnlyDictionary<string, TimelineObject> AvailableTimelines
        {
            get
            {
                Dictionary<string, TimelineObject> result = new(StringComparer.OrdinalIgnoreCase);

                foreach (var timeline in projectState.CurrentProject?.Timelines ?? [])
                {
                    if (timeline is null || string.IsNullOrWhiteSpace(timeline.Id))
                    {
                        continue;
                    }

                    result[timeline.Id] = timeline;
                }

                if (!string.IsNullOrWhiteSpace(TargetTimeline.Id))
                {
                    result[TargetTimeline.Id] = TargetTimeline;
                }

                return result;
            }
        }

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

            selectionState.SelectionChanged += OnSelectionChanged;

            _editCommandManager.CommandExecuted += OnCommandExecuted;
            _editCommandManager.CommandPreviewExecuted += OnCommandPreviewExecuted;
            _editCommandManager.CommandUndone += OnCommandUndone;
            _editCommandManager.CommandRedone += OnCommandRedone;

            NextFrame = ReactiveCommand.Create(() => playbackState.Seek(Frame + 1));
            PreviousFrame = ReactiveCommand.Create(() => playbackState.Seek(Frame - 1));
            Play = ReactiveCommand.Create(PlayMethod);
            Pause = ReactiveCommand.Create(PauseMethod);
            SetSelectionStart = ReactiveCommand.Create(SetSelectionStartMethod);
            SetSelectionEnd = ReactiveCommand.Create(SetSelectionEndMethod);
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
            playbackState.PlaybackStarted += OnPlaybackStarted;
            playbackState.PlaybackPaused += OnPlaybackPaused;
            playbackState.PlaybackSeeked += OnPlaybackFrameChanged;
            projectState.TimelineChanged += UpdateSlider;

            OnPlaybackFrameChanged();
            NotifyProjectChanged();
        }

        public void NotifyProjectChanged()
        {
            if (!playbackState.IsPlaying)
            {
                playbackState.RequestReRendering();
            }

            UpdateSlider();
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

        public void PauseAndSeekToFrame(int frame)
        {
            playbackState.Pause();
            playbackState.Seek(frame);
        }

        private void SetSelectionStartMethod()
        {
            var command = new TimelineSelectionRangeChangeCommand(TargetTimeline, Frame, TargetTimeline.SelectionEnd);
            _editCommandManager.Execute(command);
        }

        private void SetSelectionEndMethod()
        {
            var command = new TimelineSelectionRangeChangeCommand(TargetTimeline, TargetTimeline.SelectionStart, Frame);
            _editCommandManager.Execute(command);
        }

        private void UpdateSlider()
        {
            SliderSelectionStart = TargetTimeline.SelectionStart;
            SliderSelectionEnd = TargetTimeline.SelectionEnd;
            SliderMaximum = Math.Max(TargetTimeline.SelectionEnd, TargetTimeline.GetLastFrameOfClips());
        }

        private void OnPlaybackFrameChanged()
        {
            _isUpdatingFrameFromPlayback = true;
            Frame = playbackState.CurrentFrame;
            _isUpdatingFrameFromPlayback = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                selectionState.SelectionChanged -= OnSelectionChanged;
                playbackState.PlaybackFrameChanged -= OnPlaybackFrameChanged;
                playbackState.PlaybackStarted -= OnPlaybackStarted;
                playbackState.PlaybackPaused -= OnPlaybackPaused;
                playbackState.PlaybackSeeked -= OnPlaybackFrameChanged;
                projectState.TimelineChanged -= UpdateSlider;
                _editCommandManager.CommandExecuted -= OnCommandExecuted;
                _editCommandManager.CommandPreviewExecuted -= OnCommandPreviewExecuted;
                _editCommandManager.CommandUndone -= OnCommandUndone;
                _editCommandManager.CommandRedone -= OnCommandRedone;
            }

            base.Dispose(disposing);
        }

        private void OnSelectionChanged()
        {
            SelectingObjects.Clear();
            SelectingObjects.AddRange(selectionState.SelectedClips);
        }

        private void OnCommandExecuted(object? sender, IEditCommand e)
        {
            playbackState.RequestReRendering();
            projectState.NotifyTimelineChanged();
        }

        private void OnCommandPreviewExecuted(object? sender, IEditCommand e)
        {
            playbackState.RequestReRendering();
        }

        private void OnCommandUndone(object? sender, IEditCommand e)
        {
            playbackState.RequestReRendering();
            projectState.NotifyTimelineChanged();
        }

        private void OnCommandRedone(object? sender, IEditCommand e)
        {
            playbackState.RequestReRendering();
            projectState.NotifyTimelineChanged();
        }

        private void OnPlaybackStarted()
        {
            IsPlaying = true;
        }

        private void OnPlaybackPaused()
        {
            IsPlaying = false;
        }
    }
}
