using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.Abstractions.Hosting;

public interface IEditorHostContext
{
    IEditCommandManager EditCommandManager { get; }

    ISelectionState SelectionState { get; }

    ITimelineViewStateStore TimelineViewStateStore { get; }

    IPlaybackState PlaybackState { get; }

    INotificationService NotificationService { get; }
}
