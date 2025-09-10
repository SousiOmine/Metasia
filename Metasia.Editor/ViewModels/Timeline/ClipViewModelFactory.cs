using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using System;

namespace Metasia.Editor.ViewModels.Timeline;

public class ClipViewModelFactory : IClipViewModelFactory
{
    private readonly IEditCommandManager editCommandManager;
    private readonly ITimelineViewState timelineViewState;
    public ClipViewModelFactory(IEditCommandManager editCommandManager, ITimelineViewState timelineViewState)
    {
        ArgumentNullException.ThrowIfNull(editCommandManager);
        ArgumentNullException.ThrowIfNull(timelineViewState);
        this.editCommandManager = editCommandManager;
        this.timelineViewState = timelineViewState;
    }

    public ClipViewModel Create(ClipObject targetObject, TimelineViewModel parentTimeline)
    {
        ArgumentNullException.ThrowIfNull(targetObject);
        ArgumentNullException.ThrowIfNull(parentTimeline);
        return new ClipViewModel(targetObject, parentTimeline, editCommandManager, timelineViewState);
    }
}
