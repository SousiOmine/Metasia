using Metasia.Core.Objects;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using System;

namespace Metasia.Editor.ViewModels.Timeline;

public class ClipViewModelFactory : IClipViewModelFactory
{
    private readonly IEditCommandManager editCommandManager;
    private readonly ITimelineViewState timelineViewState;
    private readonly IClipColorProvider clipColorProvider;
    private readonly ISelectionState selectionState;
    private readonly IProjectState projectState;
    private readonly IFileDialogService fileDialogService;

    public ClipViewModelFactory(
        IEditCommandManager editCommandManager,
        ITimelineViewState timelineViewState,
        IClipColorProvider clipColorProvider,
        ISelectionState selectionState,
        IProjectState projectState,
        IFileDialogService fileDialogService)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(timelineViewState);
        ArgumentNullException.ThrowIfNull(clipColorProvider);
        ArgumentNullException.ThrowIfNull(selectionState);
        ArgumentNullException.ThrowIfNull(projectState);
        ArgumentNullException.ThrowIfNull(fileDialogService);
        this.editCommandManager = editCommandManager;
        this.timelineViewState = timelineViewState;
        this.clipColorProvider = clipColorProvider;
        this.selectionState = selectionState;
        this.projectState = projectState;
        this.fileDialogService = fileDialogService;
    }

    public ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline)
    {
        ArgumentNullException.ThrowIfNull(targetObject);
        ArgumentNullException.ThrowIfNull(parentTimeline);
        return new ClipViewModel(targetObject, parentTimeline, editCommandManager, timelineViewState, clipColorProvider, selectionState, projectState, fileDialogService);
    }
}
