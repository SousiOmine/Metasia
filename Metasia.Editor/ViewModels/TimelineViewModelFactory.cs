using System;
using Metasia.Editor.ViewModels.Timeline;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.ViewModels.Timeline;

namespace Metasia.Editor.ViewModels;

public class TimelineViewModelFactory : ITimelineViewModelFactory
{
    private readonly ILayerButtonViewModelFactory _layerButtonViewModelFactory;
    private readonly ILayerCanvasViewModelFactory _layerCanvasViewModelFactory;
    private readonly ISelectionState selectionState;
    private readonly IPlaybackState playbackState;
    private readonly IEditCommandManager editCommandManager;
    private readonly IProjectState _projectState;
    private readonly ITimelineViewState _timelineViewState;
    public TimelineViewModelFactory(
        ILayerButtonViewModelFactory layerButtonViewModelFactory,
        ILayerCanvasViewModelFactory layerCanvasViewModelFactory,
        ISelectionState selectionState,
        IPlaybackState playbackState,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        ITimelineViewState timelineViewState)
    {
        ArgumentNullException.ThrowIfNull(layerButtonViewModelFactory);
        ArgumentNullException.ThrowIfNull(layerCanvasViewModelFactory);
        ArgumentNullException.ThrowIfNull(timelineViewState);
        _layerButtonViewModelFactory = layerButtonViewModelFactory;
        _layerCanvasViewModelFactory = layerCanvasViewModelFactory;
        this.selectionState = selectionState;
        this.playbackState = playbackState;
        this.editCommandManager = editCommandManager;
        this._projectState = projectState;
        this._timelineViewState = timelineViewState;
    }
    public TimelineViewModel Create()
    {
        return new TimelineViewModel(_layerButtonViewModelFactory, _layerCanvasViewModelFactory, selectionState, playbackState, _projectState, editCommandManager, _timelineViewState);
    }
}