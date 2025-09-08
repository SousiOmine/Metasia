using System;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Timeline;

public class LayerButtonViewModelFactory : ILayerButtonViewModelFactory
{
    public LayerButtonViewModelFactory()
    {
    }
    public LayerButtonViewModel Create(TimelineViewModel parentTimelineViewModel, LayerObject targetLayerObject)
    {
        ArgumentNullException.ThrowIfNull(parentTimelineViewModel);
        ArgumentNullException.ThrowIfNull(targetLayerObject);
        return new LayerButtonViewModel(parentTimelineViewModel, targetLayerObject);
    }
}