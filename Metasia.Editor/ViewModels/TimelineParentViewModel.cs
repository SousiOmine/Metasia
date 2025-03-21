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

        private TimelineViewModel? _timelineViewModel;
        
        PlayerParentViewModel _playerParentViewModel;
        public TimelineParentViewModel(PlayerParentViewModel playerParentViewModel)
        {
            _playerParentViewModel = playerParentViewModel;

            _playerParentViewModel.ProjectInstanceChanged += (sender, e) =>
            {
                if (_playerParentViewModel.TargetPlayerViewModel is not null)
                {
                    CurrentTimelineViewModel = new TimelineViewModel(_playerParentViewModel.TargetPlayerViewModel);
                }
            };

            _playerParentViewModel.WhenAnyValue(x => x.TargetPlayerViewModel)
                .Subscribe(playerVM =>
                {
                    if (playerVM is not null)
                    {
                        CurrentTimelineViewModel = new TimelineViewModel(playerVM);
                    }
                });

            if (_playerParentViewModel.TargetPlayerViewModel is not null)
            {
                CurrentTimelineViewModel = new TimelineViewModel(_playerParentViewModel.TargetPlayerViewModel);
            }
        }
    }
}
