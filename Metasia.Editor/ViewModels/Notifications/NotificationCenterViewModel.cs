using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Metasia.Editor.Abstractions.Notification;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Notifications;

public sealed class NotificationCenterViewModel : ViewModelBase
{
    private readonly INotificationService _notificationService;
    private readonly TimeSpan _toastDuration;
    private readonly int _maxActiveToasts;
    private readonly Dictionary<Guid, NotificationEntryViewModel> _historyIndex = [];
    private readonly Dictionary<Guid, CancellationTokenSource> _dismissTokens = [];

    public ObservableCollection<NotificationEntryViewModel> History { get; } = [];
    public ObservableCollection<NotificationEntryViewModel> ActiveToasts { get; } = [];
    public bool HasHistory => History.Count > 0;
    public bool IsHistoryEmpty => History.Count == 0;

    public NotificationCenterViewModel(
        INotificationService notificationService,
        TimeSpan? toastDuration = null,
        int maxActiveToasts = 4)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _toastDuration = toastDuration ?? TimeSpan.FromSeconds(5);
        _maxActiveToasts = Math.Max(1, maxActiveToasts);

        _notificationService.NewNotification += OnNewNotification;
        _notificationService.NotificationRemoved += OnNotificationRemoved;
        History.CollectionChanged += OnHistoryCollectionChanged;

        // 起動直後に既存通知があれば履歴へ取り込み、最新分だけトーストも復元する。
        var existingNotifications = _notificationService.Notifications
            .OrderByDescending(x => x.Timestamp)
            .ToArray();
        foreach (var notification in existingNotifications)
        {
            var entry = GetOrCreateEntry(notification);
            History.Add(entry);
        }

        foreach (var notification in existingNotifications.Take(_maxActiveToasts))
        {
            ShowToast(notification);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _notificationService.NewNotification -= OnNewNotification;
            _notificationService.NotificationRemoved -= OnNotificationRemoved;
            History.CollectionChanged -= OnHistoryCollectionChanged;

            foreach (var cancellationTokenSource in _dismissTokens.Values)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
            }

            _dismissTokens.Clear();
            _historyIndex.Clear();
            ActiveToasts.Clear();
            History.Clear();
        }

        base.Dispose(disposing);
    }

    private void OnHistoryCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        this.RaisePropertyChanged(nameof(HasHistory));
        this.RaisePropertyChanged(nameof(IsHistoryEmpty));
    }

    private void OnNewNotification(object? sender, NotificationItem item)
    {
        RunOnUiThread(() =>
        {
            var entry = GetOrCreateEntry(item);
            if (!History.Contains(entry))
            {
                History.Insert(0, entry);
            }

            ShowToast(item);
        });
    }

    private void OnNotificationRemoved(object? sender, NotificationItem item)
    {
        RunOnUiThread(() => RemoveToast(item.Id));
    }

    private NotificationEntryViewModel GetOrCreateEntry(NotificationItem item)
    {
        if (_historyIndex.TryGetValue(item.Id, out var existing))
        {
            return existing;
        }

        var entry = new NotificationEntryViewModel(item, () => OnToastActivated(item.Id));
        _historyIndex[item.Id] = entry;
        return entry;
    }

    private void ShowToast(NotificationItem item)
    {
        var entry = GetOrCreateEntry(item);

        RemoveToast(item.Id);
        ActiveToasts.Insert(0, entry);
        StartDismissTimer(item.Id);

        while (ActiveToasts.Count > _maxActiveToasts)
        {
            var lastToast = ActiveToasts[^1];
            RemoveToast(lastToast.Id);
        }
    }

    private void OnToastActivated(Guid notificationId)
    {
        RemoveToast(notificationId);
    }

    private void StartDismissTimer(Guid notificationId)
    {
        if (_dismissTokens.TryGetValue(notificationId, out var existing))
        {
            existing.Cancel();
            existing.Dispose();
        }

        var cancellationTokenSource = new CancellationTokenSource();
        _dismissTokens[notificationId] = cancellationTokenSource;
        _ = DismissToastAsync(notificationId, cancellationTokenSource);
    }

    private async Task DismissToastAsync(Guid notificationId, CancellationTokenSource cancellationTokenSource)
    {
        try
        {
            await Task.Delay(_toastDuration, cancellationTokenSource.Token);
            RunOnUiThread(() => RemoveToast(notificationId, keepCancellationRegistration: false));
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            if (_dismissTokens.TryGetValue(notificationId, out var current) && ReferenceEquals(current, cancellationTokenSource))
            {
                _dismissTokens.Remove(notificationId);
                cancellationTokenSource.Dispose();
            }
        }
    }

    private void RemoveToast(Guid notificationId, bool keepCancellationRegistration = false)
    {
        var toast = ActiveToasts.FirstOrDefault(x => x.Id == notificationId);
        if (toast is not null)
        {
            ActiveToasts.Remove(toast);
        }

        if (!keepCancellationRegistration &&
            _dismissTokens.TryGetValue(notificationId, out var cancellationTokenSource))
        {
            _dismissTokens.Remove(notificationId);
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }

    private static void RunOnUiThread(Action action)
    {
        if (Application.Current is null || Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Post(action);
    }
}
