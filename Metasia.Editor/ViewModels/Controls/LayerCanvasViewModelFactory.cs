using Metasia.Core.Objects;

namespace Metasia.Editor.ViewModels.Controls;

public class LayerCanvasViewModelFactory : ILayerCanvasViewModelFactory
{
    private readonly IClipViewModelFactory _clipViewModelFactory;

    public LayerCanvasViewModelFactory(IClipViewModelFactory clipViewModelFactory)
    {
        _clipViewModelFactory = clipViewModelFactory;
    }

    public LayerCanvasViewModel Create(TimelineViewModel parentTimelineViewModel, PlayerViewModel playerViewModel, LayerObject targetLayerObject)
    {
        return new LayerCanvasViewModel(parentTimelineViewModel, playerViewModel, targetLayerObject, _clipViewModelFactory);
    }

}
