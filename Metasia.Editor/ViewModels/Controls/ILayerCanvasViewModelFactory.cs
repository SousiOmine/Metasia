
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Controls;

public interface ILayerCanvasViewModelFactory
{
    LayerCanvasViewModel Create(TimelineViewModel parentTimelineViewModel, PlayerViewModel playerViewModel, LayerObject targetLayerObject);
}