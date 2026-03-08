using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Metasia.Core.Encode;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Models.States;
using Metasia.Editor.Plugin;
using Metasia.Editor.Services;
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

    private static OutputViewModel CreateViewModel(FakeMediaOutputPlugin plugin)
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var pluginService = new FakePluginService(plugin);
        return new OutputViewModel(
            new FakeProjectState(),
            router,
            new FakeFileDialogService(),
            pluginService,
            new FakeEncodeService());
    }

    private sealed class FakePluginService : IPluginService
    {
        public List<IEditorPlugin> EditorPlugins { get; } = [];
        public List<IMediaInputPlugin> MediaInputPlugins { get; } = [];
        public List<IMediaOutputPlugin> MediaOutputPlugins { get; } = [];

        public FakePluginService(IMediaOutputPlugin plugin)
        {
            EditorPlugins.Add(plugin);
            MediaOutputPlugins.Add(plugin);
        }

        public Task<IEnumerable<IEditorPlugin>> LoadPluginsAsync()
        {
            return Task.FromResult<IEnumerable<IEditorPlugin>>(EditorPlugins);
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

        public void Initialize()
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
        public MetasiaEditorProject? CurrentProject => null;
        public ProjectInfo? CurrentProjectInfo => null;
        public TimelineObject? CurrentTimeline => null;

        public event Action? ProjectLoaded;
        public event Action? ProjectClosed;
        public event Action? TimelineChanged;

        public Task LoadProjectAsync(MetasiaEditorProject project) => Task.CompletedTask;
        public void CloseProject()
        {
        }

        public void SetCurrentTimeline(TimelineObject timeline)
        {
        }

        public void NotifyTimelineChanged()
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeSettingsService : ISettingsService
    {
        public EditorSettings CurrentSettings { get; private set; } = new();
        public event Action? SettingsChanged;

        public Task LoadAsync() => Task.CompletedTask;
        public Task SaveAsync() => Task.CompletedTask;

        public Task UpdateSettingsAsync(EditorSettings settings)
        {
            CurrentSettings = settings;
            SettingsChanged?.Invoke();
            return Task.CompletedTask;
        }
    }
}
