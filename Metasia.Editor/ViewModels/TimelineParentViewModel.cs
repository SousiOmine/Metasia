using ReactiveUI;

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
            
            CurrentTimelineViewModel = new TimelineViewModel(_playerParentViewModel.TargetPlayerViewModel);
        }
    }
}
