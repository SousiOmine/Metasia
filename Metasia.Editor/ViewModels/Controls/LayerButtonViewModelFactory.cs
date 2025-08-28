using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Controls;

public class LayerButtonViewModelFactory : ILayerButtonViewModelFactory
{
    public LayerButtonViewModelFactory()
    {
    }
    public LayerButtonViewModel Create(TimelineViewModel parentTimelineViewModel, LayerObject targetLayerObject)
    {
        return new LayerButtonViewModel(parentTimelineViewModel, targetLayerObject);
    }
}