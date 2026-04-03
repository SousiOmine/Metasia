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
        ITimelineViewState timelineViewState,
        IPlaybackState playbackState,
        INotificationService notificationService)
    {
        EditCommandManager = editCommandManager;
        SelectionState = selectionState;
        TimelineViewState = timelineViewState;
        PlaybackState = playbackState;
        NotificationService = notificationService;
    }

    public IEditCommandManager EditCommandManager { get; }

    public ISelectionState SelectionState { get; }

    public ITimelineViewState TimelineViewState { get; }

    public IPlaybackState PlaybackState { get; }

    public INotificationService NotificationService { get; }
}
