using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Controls;

public class ClipViewModelFactory : IClipViewModelFactory
{
    public ClipViewModelFactory()
    {
    }

    public ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline)
    {
        return new ClipViewModel(targetObject, parentTimeline);
    }
}
