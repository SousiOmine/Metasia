using Metasia.Editor.ViewModels.Factory;
using ReactiveUI;
using System;

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

        PlayerParentViewModel _playerParentViewModel;
        public TimelineParentViewModel(PlayerParentViewModel playerParentViewModel, ITimelineViewModelFactory timelineViewModelFactory)
        {
            _playerParentViewModel = playerParentViewModel;

            _playerParentViewModel.ProjectInstanceChanged += (sender, e) =>
            {
                if (_playerParentViewModel.TargetPlayerViewModel is not null)
                {
                    CurrentTimelineViewModel = timelineViewModelFactory.Create(_playerParentViewModel.TargetPlayerViewModel);
                    IsTimelineShow = true;
                }
                else
                {
                    IsTimelineShow = false;
                }
            };

            if (_playerParentViewModel.TargetPlayerViewModel is not null)
            {
                CurrentTimelineViewModel = timelineViewModelFactory.Create(_playerParentViewModel.TargetPlayerViewModel);
            }
        }
    }
}
