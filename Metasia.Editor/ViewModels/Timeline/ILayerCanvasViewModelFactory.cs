
using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Timeline;

public interface ILayerCanvasViewModelFactory
{
    LayerCanvasViewModel Create(TimelineViewModel parentTimelineViewModel, PlayerViewModel playerViewModel, LayerObject targetLayerObject);
}