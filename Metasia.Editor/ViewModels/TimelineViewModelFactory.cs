using Metasia.Editor.ViewModels.Controls;

namespace Metasia.Editor.ViewModels;

public class TimelineViewModelFactory : ITimelineViewModelFactory
{
    private ILayerButtonViewModelFactory _layerButtonViewModelFactory;
    private ILayerCanvasViewModelFactory _layerCanvasViewModelFactory;
    public TimelineViewModelFactory(ILayerButtonViewModelFactory layerButtonViewModelFactory, ILayerCanvasViewModelFactory layerCanvasViewModelFactory)
    {
        _layerButtonViewModelFactory = layerButtonViewModelFactory;
        _layerCanvasViewModelFactory = layerCanvasViewModelFactory;
    }
    public TimelineViewModel Create(PlayerViewModel playerViewModel)
    {
        return new TimelineViewModel(playerViewModel, _layerButtonViewModelFactory, _layerCanvasViewModelFactory);
    }
}