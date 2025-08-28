namespace Metasia.Editor.ViewModels;

public interface ITimelineViewModelFactory
{
    TimelineViewModel Create(PlayerViewModel playerViewModel);
}
