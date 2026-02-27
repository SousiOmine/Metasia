using Metasia.Core.Objects;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using System;

namespace Metasia.Editor.ViewModels.Timeline;

public class ClipViewModelFactory : IClipViewModelFactory
{
    private readonly IEditCommandManager editCommandManager;
    private readonly ITimelineViewState timelineViewState;
    private readonly IClipColorProvider clipColorProvider;
    public ClipViewModelFactory(IEditCommandManager editCommandManager, ITimelineViewState timelineViewState, IClipColorProvider clipColorProvider)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(timelineViewState);
        ArgumentNullException.ThrowIfNull(clipColorProvider);
        this.editCommandManager = editCommandManager;
        this.timelineViewState = timelineViewState;
        this.clipColorProvider = clipColorProvider;
    }

    public ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline)
    {
        ArgumentNullException.ThrowIfNull(targetObject);
        ArgumentNullException.ThrowIfNull(parentTimeline);
        return new ClipViewModel(targetObject, parentTimeline, editCommandManager, timelineViewState, clipColorProvider);
    }
}
