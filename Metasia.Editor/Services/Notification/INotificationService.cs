using System;
using System.Collections.Generic;

namespace Metasia.Editor.Services.Notification;

public interface INotificationService
{
    IReadOnlyList<NotificationItem> Notifications { get; }

    event EventHandler<NotificationItem>? NewNotification;
    event EventHandler<NotificationItem>? NotificationRemoved;

    void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Info, Action? onClick = null);
    void ShowInfo(string title, string message, Action? onClick = null);
    void ShowSuccess(string title, string message, Action? onClick = null);
    void ShowWarning(string title, string message, Action? onClick = null);
    void ShowError(string title, string message, Action? onClick = null);
    void Remove(NotificationItem notification);
    void Clear();
}