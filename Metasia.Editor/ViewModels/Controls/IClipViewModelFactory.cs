using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Controls;

public interface IClipViewModelFactory
{
    ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline);
}
