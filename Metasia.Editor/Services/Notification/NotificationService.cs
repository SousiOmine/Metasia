using System;
using System.Collections.Generic;

namespace Metasia.Editor.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly List<NotificationItem> _notifications = new();
    private readonly object _notificationsLock = new();

    public IReadOnlyList<NotificationItem> Notifications
    {
        get
        {
            lock (_notificationsLock)
            {
                return _notifications.ToArray();
            }
        }
    }

    public event EventHandler<NotificationItem>? NewNotification;
    public event EventHandler<NotificationItem>? NotificationRemoved;

    public void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Info, Action? onClick = null)
    {
        var notification = new NotificationItem(title, message, severity, onClick);
        lock (_notificationsLock)
        {
            _notifications.Add(notification);
        }
        NewNotification?.Invoke(this, notification);
    }

    public void ShowInfo(string title, string message, Action? onClick = null)
    {
        Show(title, message, NotificationSeverity.Info, onClick);
    }

    public void ShowSuccess(string title, string message, Action? onClick = null)
    {
        Show(title, message, NotificationSeverity.Success, onClick);
    }

    public void ShowWarning(string title, string message, Action? onClick = null)
    {
        Show(title, message, NotificationSeverity.Warning, onClick);
    }

    public void ShowError(string title, string message, Action? onClick = null)
    {
        Show(title, message, NotificationSeverity.Error, onClick);
    }

    public void Remove(NotificationItem notification)
    {
        bool removed;
        lock (_notificationsLock)
        {
            removed = _notifications.Remove(notification);
        }
        if (removed)
        {
            NotificationRemoved?.Invoke(this, notification);
        }
    }

    public void Clear()
    {
        NotificationItem[] notificationsCopy;
        lock (_notificationsLock)
        {
            notificationsCopy = _notifications.ToArray();
            _notifications.Clear();
        }
        foreach (var notification in notificationsCopy)
        {
            NotificationRemoved?.Invoke(this, notification);
        }
    }
}