using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Metasia.Core.Encode;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Abstractions.Hosting;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Plugin;
using Metasia.Editor.Services;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.Services.PluginService;
using Metasia.Editor.ViewModels.Dialogs;

namespace Metasia.Editor.Tests.ViewModels.Dialogs;

[TestFixture]
public class OutputViewModelTests
{
    [Test]
    public void Constructor_CreatesPluginSessionView()
    {
        var plugin = new FakeMediaOutputPlugin("Plugin Encoder");
        using var viewModel = CreateViewModel(plugin);

        Assert.That(viewModel.OutputMethodList, Has.Count.EqualTo(2));
        Assert.That(viewModel.SelectedOutputSession, Is.Not.Null);
        Assert.That(viewModel.HasSelectedPluginSettings, Is.True);
        Assert.That(plugin.SessionCreateCount, Is.EqualTo(1));
        Assert.That(plugin.LastSession?.IsDisposed, Is.False);
    }

    [Test]
    public void ChangingEncoder_DisposesPreviousSession()
    {
        var plugin = new FakeMediaOutputPlugin("Plugin Encoder");
        using var viewModel = CreateViewModel(plugin);

        var firstSession = plugin.LastSession;
        viewModel.SelectedEncoderIndex = 1;

        Assert.That(firstSession, Is.Not.Null);
        Assert.That(firstSession!.IsDisposed, Is.True);
        Assert.That(viewModel.SelectedOutputSession, Is.Null);
        Assert.That(viewModel.HasSelectedPluginSettings, Is.False);
    }

    [Test]
    public void Dispose_DisposesActiveSession()
    {
        var plugin = new FakeMediaOutputPlugin("Plugin Encoder");
        var viewModel = CreateViewModel(plugin);
        var session = plugin.LastSession;

        viewModel.Dispose();

        Assert.That(session, Is.Not.Null);
        Assert.That(session!.IsDisposed, Is.True);
    }

    [Test]
    public void OutputExecute_WithoutProject_ShowsErrorNotification()
    {
        var plugin = new FakeMediaOutputPlugin("Plugin Encoder");
        var notificationService = new FakeNotificationService();
        using var viewModel = CreateViewModel(plugin, notificationService: notificationService);

        viewModel.OutputCommand.Execute(null);

        Assert.That(notificationService.Notifications, Has.Count.EqualTo(1));
        Assert.That(notificationService.Notifications[0].Title, Is.EqualTo("出力失敗"));
        Assert.That(notificationService.Notifications[0].Message, Does.Contain("プロジェクト"));
    }

    [Test]
    public void OutputExecute_WithoutOutputPath_ShowsErrorNotification()
    {
        var plugin = new FakeMediaOutputPlugin("Plugin Encoder");
        var notificationService = new FakeNotificationService();
        var projectState = new FakeProjectState(CreateProject(new TimelineObject("RootTimeline")));
        using var viewModel = CreateViewModel(plugin, projectState, notificationService);

        viewModel.OutputCommand.Execute(null);

        Assert.That(notificationService.Notifications, Has.Count.EqualTo(1));
        Assert.That(notificationService.Notifications[0].Title, Is.EqualTo("出力失敗"));
        Assert.That(notificationService.Notifications[0].Message, Does.Contain("出力先"));
    }

    private static OutputViewModel CreateViewModel(
        FakeMediaOutputPlugin plugin,
        FakeProjectState? projectState = null,
        FakeNotificationService? notificationService = null)
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var pluginService = new FakePluginService(plugin);
        projectState ??= new FakeProjectState();
        notificationService ??= new FakeNotificationService();
        return new OutputViewModel(
            projectState,
            router,
            new FakeFileDialogService(),
            pluginService,
            new FakeEncodeService(),
            notificationService,
            new NullRenderSurfaceFactory());
    }

    private static MetasiaEditorProject CreateProject(params TimelineObject[] timelines)
    {
        var project = new MetasiaEditorProject(
            new DirectoryEntity(Path.GetTempPath()),
            new MetasiaProjectFile
            {
                Framerate = 60,
                Resolution = new VideoResolution { Width = 1920, Height = 1080 }
            });

        foreach (var timeline in timelines)
        {
            project.Timelines.Add(timeline);
        }

        return project;
    }

    private sealed class FakePluginService : IPluginService
    {
        public List<IEditorPlugin> EditorPlugins { get; } = [];
        public List<IMediaInputPlugin> MediaInputPlugins { get; } = [];
        public List<IMediaOutputPlugin> MediaOutputPlugins { get; } = [];
        public IReadOnlyList<PluginTypeInfo> PluginClipTypes { get; } = [];
        public IReadOnlyList<PluginTypeInfo> PluginVisualEffectTypes { get; } = [];
        public IReadOnlyList<PluginTypeInfo> PluginAudioEffectTypes { get; } = [];

        public FakePluginService(IMediaOutputPlugin plugin)
        {
            EditorPlugins.Add(plugin);
            MediaOutputPlugins.Add(plugin);
        }

        public Task<IEnumerable<IEditorPlugin>> LoadPluginsAsync()
        {
            return Task.FromResult<IEnumerable<IEditorPlugin>>(EditorPlugins);
        }

        public IEnumerable<LeftPanePanelDefinition> GetLeftPanePanels()
        {
            return [];
        }

        public IEnumerable<IPluginSettingsProvider> GetSettingsProviders()
        {
            foreach (var plugin in EditorPlugins)
            {
                if (plugin is IPluginSettingsProvider settingsProvider)
                {
                    yield return settingsProvider;
                }
            }
        }
    }

    private sealed class FakeMediaOutputPlugin : IMediaOutputPlugin
    {
        public string PluginIdentifier { get; } = "test.output.plugin";
        public string PluginVersion { get; } = "1.0.0";
        public string PluginName { get; }
        public string Name => PluginName;
        public string[] SupportedExtensions { get; } = ["*.mp4"];
        public IEnumerable<IEditorPlugin.SupportEnvironment> SupportedEnvironments { get; } =
        [
            IEditorPlugin.SupportEnvironment.Windows_x64
        ];

        public int SessionCreateCount { get; private set; }
        public FakeMediaOutputSession? LastSession { get; private set; }

        public FakeMediaOutputPlugin(string name)
        {
            PluginName = name;
        }

        public IMediaOutputSession CreateSession()
        {
            SessionCreateCount++;
            LastSession = new FakeMediaOutputSession(Name, SupportedExtensions);
            return LastSession;
        }

        public void Initialize(IEditorHostContext hostContext)
        {
        }
    }

    private sealed class FakeMediaOutputSession : IMediaOutputSession
    {
        public string Name { get; }
        public string[] SupportedExtensions { get; }
        public Control? SettingsView { get; } = new TextBlock { Text = "settings" };
        public bool IsDisposed { get; private set; }

        public FakeMediaOutputSession(string name, string[] supportedExtensions)
        {
            Name = name;
            SupportedExtensions = supportedExtensions;
        }

        public EncoderBase CreateEncoderInstance()
        {
            return new FakeEncoder();
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private sealed class FakeEncoder : EncoderBase
    {
        public override double ProgressRate { get; protected set; }
        public override event EventHandler<EventArgs> StatusChanged = delegate { };
        public override event EventHandler<EventArgs> EncodeStarted = delegate { };
        public override event EventHandler<EventArgs> EncodeCompleted = delegate { };
        public override event EventHandler<EventArgs> EncodeFailed = delegate { };

        public override void Start()
        {
        }

        public override void CancelRequest()
        {
        }
    }

    private sealed class FakeEncodeService : IEncodeService
    {
        public IReadOnlyList<IEditorEncoder> Encoders { get; } = [];
        public event EventHandler<EventArgs> QueueUpdated = delegate { };

        public void QueueEncode(IEditorEncoder encoder)
        {
        }

        public void Cancel(IEditorEncoder encoder)
        {
        }

        public void Delete(IEditorEncoder encoder)
        {
        }

        public void ClearQueue()
        {
        }
    }

    private sealed class FakeFileDialogService : IFileDialogService
    {
        public Task<IStorageFile?> OpenFileDialogAsync() => Task.FromResult<IStorageFile?>(null);
        public Task<IStorageFile?> OpenFileDialogAsync(string title, string[] patterns) => Task.FromResult<IStorageFile?>(null);
        public Task<IStorageFile?> SaveFileDialogAsync(string title, string[] extensions, string defaultExtension = "") => Task.FromResult<IStorageFile?>(null);
        public Task<IStorageFolder?> OpenFolderDialogAsync() => Task.FromResult<IStorageFolder?>(null);
    }

    private sealed class FakeProjectState : IProjectState
    {
        public MetasiaEditorProject? CurrentProject { get; private set; }
        public ProjectInfo? CurrentProjectInfo => null;
        public TimelineObject? CurrentTimeline { get; private set; }
        public bool IsDirty { get; set; }

        public FakeProjectState(MetasiaEditorProject? currentProject = null)
        {
            CurrentProject = currentProject;
            CurrentTimeline = currentProject?.Timelines.FirstOrDefault();
        }

        public event Action? ProjectLoaded;
        public event Action? ProjectClosed;
        public event Action? TimelineChanged;
        public event Action? CurrentTimelineChanged;
        public event Action? IsDirtyChanged;

        public Task LoadProjectAsync(MetasiaEditorProject project)
        {
            CurrentProject = project;
            CurrentTimeline = project.Timelines.FirstOrDefault();
            ProjectLoaded?.Invoke();
            return Task.CompletedTask;
        }
        public void CloseProject()
        {
        }

        public void SetCurrentTimeline(TimelineObject timeline)
        {
            CurrentTimelineChanged?.Invoke();
        }

        public void NotifyTimelineChanged()
        {
        }

        public void Dispose()
        {
        }
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
            if (Notifications.Remove(notification))
            {
                NotificationRemoved?.Invoke(this, notification);
            }
        }

        public void Clear()
        {
            foreach (var notification in Notifications.ToArray())
            {
                NotificationRemoved?.Invoke(this, notification);
            }

            Notifications.Clear();
        }
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public EditorSettings CurrentSettings { get; private set; } = new();
        public event Action? SettingsChanged;

        public Task LoadAsync() => Task.CompletedTask;
        public Task SaveAsync() => Task.CompletedTask;

        public void UpdateSettings(EditorSettings settings)
        {
            CurrentSettings = settings;
            SettingsChanged?.Invoke();
        }

        public Task UpdateSettingsAsync(EditorSettings settings)
        {
            UpdateSettings(settings);
            return Task.CompletedTask;
        }

        public void UpdateSettingsSilent(EditorSettings settings)
        {
            CurrentSettings = settings;
        }

        public void NotifySettingsChanged()
        {
            SettingsChanged?.Invoke();
        }
    }
}
