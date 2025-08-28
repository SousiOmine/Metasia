using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Controls;

public interface ILayerButtonViewModelFactory
{
    LayerButtonViewModel Create(TimelineViewModel parentTimelineViewModel, LayerObject targetLayerObject);
}
