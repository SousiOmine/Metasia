using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render.Cache;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.Plugin;
using Metasia.Editor.Services;
using Metasia.Editor.Services.Notification;
using Metasia.Editor.Services.PluginService;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Dialogs;
using Metasia.Editor.ViewModels.Notifications;
using Metasia.Editor.ViewModels.Timeline;
using ReactiveUI;
using System.Reactive;

namespace Metasia.Editor.Tests.ViewModels;

[TestFixture]
public class MenuViewModelTests
{
    [Test]
    public async Task OpenNotifications_InvokesNotificationInteraction()
    {
        var notificationService = new FakeNotificationService();
        using var notificationCenter = new NotificationCenterViewModel(notificationService);
        using var viewModel = CreateViewModel(notificationService, notificationCenter);
        var invoked = false;

        viewModel.OpenNotificationsInteraction.RegisterHandler(interaction =>
        {
            invoked = true;
            Assert.That(interaction.Input, Is.SameAs(notificationCenter));
            interaction.SetOutput(Unit.Default);
        });

        viewModel.OpenNotifications.Execute(null);
        await Task.Delay(50);

        Assert.That(invoked, Is.True);
    }

    [Test]
    public async Task CreateNewProject_WhenInteractionFails_ShowsErrorNotification()
    {
        var notificationService = new FakeNotificationService();
        using var notificationCenter = new NotificationCenterViewModel(notificationService);
        using var viewModel = CreateViewModel(notificationService, notificationCenter);

        viewModel.NewProjectInteraction.RegisterHandler(_ => throw new InvalidOperationException("boom"));

        viewModel.CreateNewProject.Execute(null);
        await Task.Delay(50);

        Assert.That(notificationService.Notifications, Has.Count.EqualTo(1));
        Assert.That(notificationService.Notifications[0].Title, Is.EqualTo("新規プロジェクト作成失敗"));
    }

    private static MenuViewModel CreateViewModel(
        FakeNotificationService notificationService,
        NotificationCenterViewModel notificationCenterViewModel)
    {
        var projectState = new ProjectState();
        var selectionState = new SelectionState();
        var editCommandManager = new EditCommandManager();

        var playerParentViewModel = new PlayerParentViewModel(
            new FakeKeyBindingService(),
            new DummyPlayerViewModelFactory(),
            projectState,
            editCommandManager,
            selectionState);

        var timelineParentViewModel = new TimelineParentViewModel(new DummyTimelineViewModelFactory(), projectState);

        return new MenuViewModel(
            playerParentViewModel,
            timelineParentViewModel,
            new FakeKeyBindingService(),
            new FakeFileDialogService(),
            new FakePlaybackState(),
            projectState,
            editCommandManager,
            new FakeNewProjectViewModelFactory(),
            new DummyOutputViewModelFactory(),
            new FakePluginService(),
            notificationService,
            notificationCenterViewModel);
    }

    private sealed class FakeNotificationService : INotificationService
    {
        public List<NotificationItem> Notifications { get; } = [];
        IReadOnlyList<NotificationItem> INotificationService.Notifications => Notifications;

        public event EventHandler<NotificationItem>? NewNotification;
        public event EventHandler<NotificationItem>? NotificationRemoved;

        public void Show(string title, string message, NotificationSeverity severity = NotificationSeverity.Info, Action? onClick = null)
        {
            var item = new NotificationItem(title, message, severity, onClick);
            Notifications.Add(item);
            NewNotification?.Invoke(this, item);
        }

        public void ShowInfo(string title, string message, Action? onClick = null) => Show(title, message, NotificationSeverity.Info, onClick);
        public void ShowSuccess(string title, string message, Action? onClick = null) => Show(title, message, NotificationSeverity.Success, onClick);
        public void ShowWarning(string title, string message, Action? onClick = null) => Show(title, message, NotificationSeverity.Warning, onClick);
        public void ShowError(string title, string message, Action? onClick = null) => Show(title, message, NotificationSeverity.Error, onClick);

        public void Remove(NotificationItem notification)
        {
            if (Notifications.Remove(notification))
            {
                NotificationRemoved?.Invoke(this, notification);
            }
        }

        public void Clear()
        {
            foreach (var item in Notifications.ToArray())
            {
                NotificationRemoved?.Invoke(this, item);
            }

            Notifications.Clear();
        }
    }

    private sealed class FakePluginService : IPluginService
    {
        public List<IEditorPlugin> EditorPlugins { get; } = [];
        public List<IMediaInputPlugin> MediaInputPlugins { get; } = [];
        public List<IMediaOutputPlugin> MediaOutputPlugins { get; } = [];

        public Task<IEnumerable<IEditorPlugin>> LoadPluginsAsync() => Task.FromResult<IEnumerable<IEditorPlugin>>(EditorPlugins);
        public IEnumerable<LeftPanePanelDefinition> GetLeftPanePanels() => [];
        public IEnumerable<IPluginSettingsProvider> GetSettingsProviders() => [];
    }

    private sealed class FakeNewProjectViewModelFactory : INewProjectViewModelFactory
    {
        public NewProjectViewModel Create()
        {
            return new NewProjectViewModel(new FakeFileDialogService());
        }
    }

    private sealed class DummyOutputViewModelFactory : IOutputViewModelFactory
    {
        public OutputViewModel Create()
        {
            throw new NotSupportedException();
        }
    }

    private sealed class DummyPlayerViewModelFactory : IPlayerViewModelFactory
    {
        public PlayerViewModel Create(TimelineObject timeline, ProjectInfo projectInfo)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class DummyTimelineViewModelFactory : ITimelineViewModelFactory
    {
        public TimelineViewModel Create(TimelineObject timeline)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeFileDialogService : IFileDialogService
    {
        public Task<Avalonia.Platform.Storage.IStorageFile?> OpenFileDialogAsync() => Task.FromResult<Avalonia.Platform.Storage.IStorageFile?>(null);
        public Task<Avalonia.Platform.Storage.IStorageFile?> OpenFileDialogAsync(string title, string[] patterns) => Task.FromResult<Avalonia.Platform.Storage.IStorageFile?>(null);
        public Task<Avalonia.Platform.Storage.IStorageFile?> SaveFileDialogAsync(string title, string[] extensions, string defaultExtension = "") => Task.FromResult<Avalonia.Platform.Storage.IStorageFile?>(null);
        public Task<Avalonia.Platform.Storage.IStorageFolder?> OpenFolderDialogAsync() => Task.FromResult<Avalonia.Platform.Storage.IStorageFolder?>(null);
    }

    private sealed class FakeKeyBindingService : IKeyBindingService
    {
        public void ApplyKeyBindings(Window target)
        {
        }

        public void RegisterCommand(string commandId, ICommand command)
        {
        }

        public bool UnregisterCommand(string commandId)
        {
            return true;
        }

        public void ClearCommands()
        {
        }

        public void RefreshKeyBindings()
        {
        }

        public KeyModifiers? GetModifierForAction(string actionId)
        {
            return null;
        }

        public bool IsModifierKeyPressed(KeyModifiers modifier, KeyModifiers currentModifiers)
        {
            return false;
        }

        public void SaveKeyBindings()
        {
        }
    }

    private sealed class FakePlaybackState : IPlaybackState
    {
        public int CurrentFrame => 0;
        public bool IsPlaying => false;
        public int SamplingRate => 44100;
        public int AudioChannels => 2;
        public IRenderImageCache? ImageCache => null;

        public event Action? PlaybackStarted;
        public event Action? PlaybackPaused;
        public event Action? PlaybackSeeked;
        public event Action? PlaybackFrameChanged;
        public event Action? ReRenderingRequested;

        public void Play()
        {
        }

        public void Pause()
        {
        }

        public void Seek(int frame)
        {
        }

        public void RequestReRendering()
        {
        }

        public void Dispose()
        {
        }
    }
}
