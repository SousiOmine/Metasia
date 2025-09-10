using ReactiveUI;
using System;
using Metasia.Editor.Models.States;

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

        private TimelineViewModel? _timelineViewModel;

        private bool _isTimelineShow = false;
        private IProjectState _projectState;

        public TimelineParentViewModel(ITimelineViewModelFactory timelineViewModelFactory, IProjectState projectState)
        {
            ArgumentNullException.ThrowIfNull(timelineViewModelFactory);
            _projectState = projectState;
            _projectState.ProjectLoaded += () =>
            {
                if (_projectState.CurrentProject is not null)
                {
                    CurrentTimelineViewModel = timelineViewModelFactory.Create();
                    IsTimelineShow = true;
                }
                else
                {
                    IsTimelineShow = false;
                }
            };

            if (_projectState.CurrentProject is not null)
            {
                CurrentTimelineViewModel = timelineViewModelFactory.Create();
                IsTimelineShow = true;
            }
        }
    }
}
