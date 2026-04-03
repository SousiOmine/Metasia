using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using ReactiveUI;
using System;
using System.Windows.Input;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.ViewModels
{
    public class TimelineParentViewModel : ViewModelBase
    {
        public TimelineViewModel? CurrentTimelineViewModel
        {
            get { return _timelineViewModel; }
            set { this.RaiseAndSetIfChanged(ref _timelineViewModel, value); }
        }

        public bool IsTimelineShow
        {
            get { return _isTimelineShow; }
            set { this.RaiseAndSetIfChanged(ref _isTimelineShow, value); }
        }

        public ICommand? CopyCommand { get; set; }
        public ICommand? PasteCommand { get; set; }
        public ICommand? CutCommand { get; set; }

        private TimelineViewModel? _timelineViewModel;

        private bool _isTimelineShow = false;
        private readonly IProjectState _projectState;
        private readonly ITimelineViewModelFactory _timelineViewModelFactory;

        public TimelineParentViewModel(ITimelineViewModelFactory timelineViewModelFactory, IProjectState projectState)
        {
            ArgumentNullException.ThrowIfNull(timelineViewModelFactory);

            CopyCommand = ReactiveCommand.Create(Copy);
            PasteCommand = ReactiveCommand.Create(Paste);
            CutCommand = ReactiveCommand.Create(Cut);

            _timelineViewModelFactory = timelineViewModelFactory;
            _projectState = projectState;
            _projectState.ProjectLoaded += OnProjectLoaded;
            _projectState.ProjectClosed += OnProjectClosed;
            _projectState.CurrentTimelineChanged += OnCurrentTimelineChanged;

            if (_projectState.CurrentProject is not null && _projectState.CurrentTimeline is not null)
            {
                SetCurrentTimeline(_projectState.CurrentTimeline);
            }
        }

        public void Copy()
        {
            CurrentTimelineViewModel?.CopySelectedClips();
        }

        public void Paste()
        {
            CurrentTimelineViewModel?.PasteClips();
        }

        public void Cut()
        {
            CurrentTimelineViewModel?.CutSelectedClips();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _projectState.ProjectLoaded -= OnProjectLoaded;
                _projectState.ProjectClosed -= OnProjectClosed;
                _projectState.CurrentTimelineChanged -= OnCurrentTimelineChanged;
                CurrentTimelineViewModel?.Dispose();
                CurrentTimelineViewModel = null;
            }

            base.Dispose(disposing);
        }

        private void OnProjectLoaded()
        {
            if (_projectState.CurrentTimeline is not null)
            {
                SetCurrentTimeline(_projectState.CurrentTimeline);
            }
            else
            {
                OnProjectClosed();
            }
        }

        private void OnCurrentTimelineChanged()
        {
            if (_projectState.CurrentTimeline is null)
            {
                return;
            }

            SetCurrentTimeline(_projectState.CurrentTimeline);
        }

        private void OnProjectClosed()
        {
            CurrentTimelineViewModel?.Dispose();
            CurrentTimelineViewModel = null;
            IsTimelineShow = false;
        }

        private void SetCurrentTimeline(Metasia.Core.Objects.TimelineObject timeline)
        {
            CurrentTimelineViewModel?.Dispose();
            CurrentTimelineViewModel = _timelineViewModelFactory.Create(timeline);
            IsTimelineShow = true;
        }
    }
}
