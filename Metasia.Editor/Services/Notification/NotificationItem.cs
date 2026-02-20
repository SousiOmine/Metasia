using System;

namespace Metasia.Editor.Services.Notification;

public class NotificationItem
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; }
    public string Message { get; }
    public NotificationSeverity Severity { get; }
    public DateTime Timestamp { get; } = DateTime.Now;
    public Action? OnClick { get; }

    public NotificationItem(string title, string message, NotificationSeverity severity, Action? onClick = null)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Severity = severity;
        OnClick = onClick;
    }
}