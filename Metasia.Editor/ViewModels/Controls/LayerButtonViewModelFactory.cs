using System;
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Controls;

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