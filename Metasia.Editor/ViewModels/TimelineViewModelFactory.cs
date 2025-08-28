namespace Metasia.Editor.ViewModels;

public class TimelineViewModelFactory : ITimelineViewModelFactory
{
    public TimelineViewModelFactory()
    {
    }
    public TimelineViewModel Create(PlayerViewModel playerViewModel)
    {
        return new TimelineViewModel(playerViewModel);
    }
}