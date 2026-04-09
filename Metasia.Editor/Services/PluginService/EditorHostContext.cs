using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.Hosting;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.Services.PluginService;

internal sealed class EditorHostContext : IEditorHostContext
{
    public EditorHostContext(
        IEditCommandManager editCommandManager,
        ISelectionState selectionState,
        ITimelineViewStateStore timelineViewStateStore,
        IPlaybackState playbackState,
        INotificationService notificationService)
    {
        EditCommandManager = editCommandManager;
        SelectionState = selectionState;
        TimelineViewStateStore = timelineViewStateStore;
        PlaybackState = playbackState;
        NotificationService = notificationService;
    }

    public IEditCommandManager EditCommandManager { get; }

    public ISelectionState SelectionState { get; }

    public ITimelineViewStateStore TimelineViewStateStore { get; }

    public IPlaybackState PlaybackState { get; }

    public INotificationService NotificationService { get; }
}
