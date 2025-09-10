using System;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.ViewModels.Timeline;

public class LayerCanvasViewModelFactory : ILayerCanvasViewModelFactory
{
    private readonly IClipViewModelFactory _clipViewModelFactory;
    private readonly ISelectionState selectionState;
    private readonly IEditCommandManager editCommandManager;
    private readonly IProjectState projectState;
    private readonly ITimelineViewState timelineViewState;
    public LayerCanvasViewModelFactory(IClipViewModelFactory clipViewModelFactory, ISelectionState selectionState, IEditCommandManager editCommandManager, IProjectState projectState, ITimelineViewState timelineViewState)
    {
        ArgumentNullException.ThrowIfNull(clipViewModelFactory);
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(timelineViewState);
        _clipViewModelFactory = clipViewModelFactory;
        this.selectionState = selectionState;
        this.editCommandManager = editCommandManager;
        this.projectState = projectState;
        this.timelineViewState = timelineViewState;
    }

    public LayerCanvasViewModel Create(TimelineViewModel parentTimelineViewModel, LayerObject targetLayerObject)
    {
        ArgumentNullException.ThrowIfNull(parentTimelineViewModel);
        ArgumentNullException.ThrowIfNull(targetLayerObject);
        return new LayerCanvasViewModel(parentTimelineViewModel, targetLayerObject, _clipViewModelFactory, projectState, selectionState, editCommandManager, timelineViewState);
    }

}
