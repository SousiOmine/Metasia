using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels;

public interface ITimelineViewModelFactory
{
    TimelineViewModel Create(TimelineObject timeline);
}
