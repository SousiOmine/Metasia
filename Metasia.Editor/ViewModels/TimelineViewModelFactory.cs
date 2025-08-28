using System;
using Metasia.Editor.ViewModels.Controls;

namespace Metasia.Editor.ViewModels;

public class TimelineViewModelFactory : ITimelineViewModelFactory
{
    private readonly ILayerButtonViewModelFactory _layerButtonViewModelFactory;
    private readonly ILayerCanvasViewModelFactory _layerCanvasViewModelFactory;
    public TimelineViewModelFactory(ILayerButtonViewModelFactory layerButtonViewModelFactory, ILayerCanvasViewModelFactory layerCanvasViewModelFactory)
    {
        ArgumentNullException.ThrowIfNull(layerButtonViewModelFactory);
        ArgumentNullException.ThrowIfNull(layerCanvasViewModelFactory);
        _layerButtonViewModelFactory = layerButtonViewModelFactory;
        _layerCanvasViewModelFactory = layerCanvasViewModelFactory;
    }
    public TimelineViewModel Create(PlayerViewModel playerViewModel)
    {
        ArgumentNullException.ThrowIfNull(playerViewModel);
        return new TimelineViewModel(playerViewModel, _layerButtonViewModelFactory, _layerCanvasViewModelFactory);
    }
}