using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Timeline;

public interface IClipViewModelFactory
{
    ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline);
}
