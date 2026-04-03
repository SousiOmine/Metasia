using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using Metasia.Core.Objects;
using Metasia.Editor.ViewModels.Timeline;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Services;

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
    private readonly IClipboardService _clipboardService;
    public TimelineViewModelFactory(
        ILayerButtonViewModelFactory layerButtonViewModelFactory,
        ILayerCanvasViewModelFactory layerCanvasViewModelFactory,
        ISelectionState selectionState,
        IPlaybackState playbackState,
        IEditCommandManager editCommandManager,
        IProjectState projectState,
        ITimelineViewState timelineViewState,
        IClipboardService clipboardService)
    {
        ArgumentNullException.ThrowIfNull(layerButtonViewModelFactory);
        ArgumentNullException.ThrowIfNull(layerCanvasViewModelFactory);
        ArgumentNullException.ThrowIfNull(timelineViewState);
        ArgumentNullException.ThrowIfNull(clipboardService);
        _layerButtonViewModelFactory = layerButtonViewModelFactory;
        _layerCanvasViewModelFactory = layerCanvasViewModelFactory;
        this.selectionState = selectionState;
        this.playbackState = playbackState;
        this.editCommandManager = editCommandManager;
        _projectState = projectState;
        _timelineViewState = timelineViewState;
        _clipboardService = clipboardService;
    }
    public TimelineViewModel Create(TimelineObject timeline)
    {
        ArgumentNullException.ThrowIfNull(timeline);
        return new TimelineViewModel(timeline, _layerButtonViewModelFactory, _layerCanvasViewModelFactory, selectionState, playbackState, _projectState, editCommandManager, _timelineViewState, _clipboardService);
    }
}
