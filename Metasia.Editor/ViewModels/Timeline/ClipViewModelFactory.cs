using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Core.Objects;
using Metasia.Editor.Models;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Services;
using System;

namespace Metasia.Editor.ViewModels.Timeline;

public class ClipViewModelFactory : IClipViewModelFactory
{
    private readonly IEditCommandManager editCommandManager;
    private readonly ITimelineViewStateStore timelineViewStateStore;
    private readonly IClipColorProvider clipColorProvider;
    private readonly ISelectionState selectionState;
    private readonly IProjectState projectState;
    private readonly IFileDialogService fileDialogService;

    public ClipViewModelFactory(
        IEditCommandManager editCommandManager,
        ITimelineViewStateStore timelineViewStateStore,
        IClipColorProvider clipColorProvider,
        ISelectionState selectionState,
        IProjectState projectState,
        IFileDialogService fileDialogService)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(timelineViewStateStore);
        ArgumentNullException.ThrowIfNull(clipColorProvider);
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(fileDialogService);
        this.editCommandManager = editCommandManager;
        this.timelineViewStateStore = timelineViewStateStore;
        this.clipColorProvider = clipColorProvider;
        this.selectionState = selectionState;
        this.projectState = projectState;
        this.fileDialogService = fileDialogService;
    }

    public ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline)
    {
        ArgumentNullException.ThrowIfNull(targetObject);
        ArgumentNullException.ThrowIfNull(parentTimeline);
        var viewState = timelineViewStateStore.GetViewState(parentTimeline.Timeline.Id);
        return new ClipViewModel(targetObject, parentTimeline, editCommandManager, viewState, clipColorProvider, selectionState, projectState, fileDialogService);
    }
}
