using Metasia.Core.Objects;
using System;

namespace Metasia.Editor.ViewModels.Controls;

public class ClipViewModelFactory : IClipViewModelFactory
{
    public ClipViewModelFactory()
    {
    }

    public ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline)
    {
        ArgumentNullException.ThrowIfNull(targetObject);
        ArgumentNullException.ThrowIfNull(parentTimeline);
        return new ClipViewModel(targetObject, parentTimeline);
    }
}
