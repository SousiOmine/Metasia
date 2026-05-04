using System.Reflection;
using Avalonia.Controls;
using Metasia.Core.Attributes;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Render.Cache;
using Metasia.Core.Xml;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.Hosting;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Models.States;
using Metasia.Editor.Plugin;
using Metasia.Editor.Services.Notification;
using Metasia.Editor.Services.PluginService;
using EditorPluginService = Metasia.Editor.Services.PluginService.PluginService;

namespace Metasia.Editor.Tests.Services.PluginService;

[TestFixture]
public class PluginServiceTests
{
    [Test]
    public void RegisterMediaInputPlugins_DoesNotDuplicateStdInput()
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var service = CreateService(router, new TypeRegistry());

        service.MediaInputPlugins.Add(new FakeMediaInputPlugin("plugin.a", "Plugin A"));

        InvokePrivateRegisterMediaInputPlugins(service);

        var infos = router.GetRegisteredAccessorInfos();
        Assert.That(infos.Count(x => x.Id == MediaAccessorRouter.StdInputAccessorId), Is.EqualTo(1));
        Assert.That(infos.Count(x => x.Id == "plugin.a"), Is.EqualTo(1));
    }

    [Test]
    public void GetLeftPanePanels_IgnoresPluginsWithoutPanelProvider()
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var service = CreateService(router, new TypeRegistry());

        service.EditorPlugins.Add(new FakeEditorPlugin("plugin.a", "Plugin A"));

        var panels = service.GetLeftPanePanels().ToList();

        Assert.That(panels, Is.Empty);
    }

    [Test]
    public void GetLeftPanePanels_ReturnsNormalizedIdsInPluginOrder()
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var service = CreateService(router, new TypeRegistry());

        service.EditorPlugins.Add(new FakePanelPlugin("plugin.a", "Plugin A", "alpha", "beta"));
        service.EditorPlugins.Add(new FakePanelPlugin("plugin.b", "Plugin B", "main"));

        var panels = service.GetLeftPanePanels().ToList();

        Assert.That(panels.Select(x => x.Id), Is.EqualTo(new[]
        {
            "plugin.a:alpha",
            "plugin.a:beta",
            "plugin.b:main"
        }));
    }

    [Test]
    public void GetLeftPanePanels_IgnoresExceptionsThrownDuringLazyEnumeration()
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var service = CreateService(router, new TypeRegistry());

        service.EditorPlugins.Add(new ThrowingPanelPlugin("plugin.a", "Plugin A"));
        service.EditorPlugins.Add(new FakePanelPlugin("plugin.b", "Plugin B", "main"));

        var panels = service.GetLeftPanePanels().ToList();

        Assert.That(panels.Select(x => x.Id), Is.EqualTo(new[]
        {
            "plugin.a:first",
            "plugin.b:main"
        }));
    }

    [Test]
    public void RegisterPluginTypes_SkipsClipTypesWhoseNameConflictsWithRegisteredType()
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var registry = new TypeRegistry();
        registry.Register("metasia/core", nameof(Metasia.Core.Objects.Text), typeof(Metasia.Core.Objects.Text));
        var service = CreateService(router, registry);

        service.EditorPlugins.Add(new ConflictingClipPlugin());

        InvokePrivateRegisterPluginTypes(service);

        Assert.That(service.PluginClipTypes, Is.Empty);
        Assert.That(registry.GetTypeId(typeof(Metasia.Core.Objects.Text)), Is.EqualTo("metasia/core:Text"));
        Assert.That(registry.GetTypeIdByTypeName(nameof(Metasia.Core.Objects.Text)), Is.EqualTo("metasia/core:Text"));
    }

    [Test]
    public void RegisterPluginTypes_SkipsClipTypesWithoutPublicParameterlessConstructor()
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var service = CreateService(router, new TypeRegistry());

        service.EditorPlugins.Add(new NoDefaultConstructorClipPlugin());

        InvokePrivateRegisterPluginTypes(service);

        Assert.That(service.PluginClipTypes, Is.Empty);
    }

    [Test]
    public async Task LoadPluginsAsync_PassesHostContextToPluginInitialize()
    {
        var settingsService = new FakeSettingsService();
        var router = new MediaAccessorRouter(settingsService);
        var plugin = new CapturingEditorPlugin("plugin.a", "Plugin A");
        var service = CreateService(router, new TypeRegistry(), () => Task.FromResult<IEnumerable<IEditorPlugin>>([plugin]));

        await service.LoadPluginsAsync();

        Assert.That(plugin.HostContext, Is.Not.Null);
        Assert.That(plugin.HostContext!.EditCommandManager, Is.Not.Null);
        Assert.That(plugin.HostContext.SelectionState, Is.Not.Null);
        Assert.That(plugin.HostContext.TimelineViewStateStore, Is.Not.Null);
        Assert.That(plugin.HostContext.PlaybackState, Is.Not.Null);
        Assert.That(plugin.HostContext.NotificationService, Is.Not.Null);

        var command = new TestEditCommand();
        plugin.HostContext.EditCommandManager.Execute(command);
        Assert.That(command.ExecuteCallCount, Is.EqualTo(1));
    }

    private static EditorPluginService CreateService(
        MediaAccessorRouter router,
        TypeRegistry registry,
        Func<Task<IEnumerable<IEditorPlugin>>>? pluginLoader = null)
    {
        var projectState = new ProjectState();
        return new EditorPluginService(
            router,
            registry,
            new EditCommandManager(projectState),
            new SelectionState(),
            new TimelineViewStateStore(),
            new FakePlaybackState(),
            new NotificationService(),
            pluginLoader);
    }

    private static void InvokePrivateRegisterMediaInputPlugins(EditorPluginService service)
    {
        var method = typeof(EditorPluginService).GetMethod("RegisterMediaInputPlugins", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        method!.Invoke(service, null);
    }

    private static void InvokePrivateRegisterPluginTypes(EditorPluginService service)
    {
        var method = typeof(EditorPluginService).GetMethod("RegisterPluginTypes", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(method, Is.Not.Null);
        method!.Invoke(service, null);
    }

    private sealed class FakeSettingsService : Metasia.Editor.Services.ISettingsService
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

    private class FakeEditorPlugin : IEditorPlugin
    {
        public string PluginIdentifier { get; }
        public string PluginVersion { get; } = "1.0.0";
        public string PluginName { get; }

        public IEnumerable<IEditorPlugin.SupportEnvironment> SupportedEnvironments { get; } =
        [
            IEditorPlugin.SupportEnvironment.Windows_x64
        ];

        public FakeEditorPlugin(string id, string name)
        {
            PluginIdentifier = id;
            PluginName = name;
        }

        public virtual void Initialize(IEditorHostContext hostContext)
        {
        }
    }

    private sealed class CapturingEditorPlugin : FakeEditorPlugin
    {
        public CapturingEditorPlugin(string id, string name)
            : base(id, name)
        {
        }

        public IEditorHostContext? HostContext { get; private set; }

        public override void Initialize(IEditorHostContext hostContext)
        {
            HostContext = hostContext;
        }
    }

    private sealed class FakePanelPlugin : FakeEditorPlugin, ILeftPanePanelProvider
    {
        private readonly string[] _panelIds;

        public FakePanelPlugin(string id, string name, params string[] panelIds)
            : base(id, name)
        {
            _panelIds = panelIds;
        }

        public IEnumerable<LeftPanePanelDefinition> GetLeftPanePanels()
        {
            foreach (var panelId in _panelIds)
            {
                yield return new LeftPanePanelDefinition(panelId, panelId, static () => new TextBlock());
            }
        }
    }

    private sealed class ThrowingPanelPlugin : FakeEditorPlugin, ILeftPanePanelProvider
    {
        public ThrowingPanelPlugin(string id, string name)
            : base(id, name)
        {
        }

        public IEnumerable<LeftPanePanelDefinition> GetLeftPanePanels()
        {
            yield return new LeftPanePanelDefinition("first", "First", static () => new TextBlock());

            throw new InvalidOperationException("boom");
        }
    }

    private sealed class FakeMediaInputPlugin : IMediaInputPlugin
    {
        public string PluginIdentifier { get; }
        public string PluginVersion { get; } = "1.0.0";
        public string PluginName { get; }

        public IEnumerable<IEditorPlugin.SupportEnvironment> SupportedEnvironments { get; } =
        [
            IEditorPlugin.SupportEnvironment.Windows_x64
        ];

        public int ImageCallCount { get; private set; }

        public FakeMediaInputPlugin(string id, string name)
        {
            PluginIdentifier = id;
            PluginName = name;
        }

        public void Initialize(IEditorHostContext hostContext)
        {
        }

        public Task<ImageFileAccessorResult> GetImageAsync(string path)
        {
            ImageCallCount++;
            return Task.FromResult(new ImageFileAccessorResult { IsSuccessful = true });
        }

        public Task<VideoFileAccessorResult> GetImageAsync(string path, TimeSpan time)
        {
            return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = true });
        }

        public Task<VideoFileAccessorResult> GetImageAsync(string path, int frame)
        {
            return Task.FromResult(new VideoFileAccessorResult { IsSuccessful = true });
        }

        public Task<AudioFileAccessorResult> GetAudioAsync(string path, TimeSpan? startTime = null, TimeSpan? duration = null)
        {
            return Task.FromResult(new AudioFileAccessorResult { IsSuccessful = false, Chunk = null });
        }

        public Task<AudioSampleResult> GetAudioBySampleAsync(string path, long startSample, long sampleCount, int sampleRate)
        {
            return Task.FromResult(new AudioSampleResult { IsSuccessful = false, Chunk = null });
        }

        public Task<VideoMediaInfoResult?> GetVideoMediaInfoAsync(string path)
        {
            return Task.FromResult<VideoMediaInfoResult?>(null);
        }

        public Task<AudioMediaInfoResult?> GetAudioMediaInfoAsync(string path)
        {
            return Task.FromResult<AudioMediaInfoResult?>(null);
        }
    }

    private sealed class ConflictingClipPlugin : FakeEditorPlugin, IClipTypeProvider
    {
        public ConflictingClipPlugin()
            : base("plugin.conflict", "Conflict Plugin")
        {
        }

        public IEnumerable<Type> GetClipTypes()
        {
            yield return typeof(Text);
        }
    }

    private sealed class NoDefaultConstructorClipPlugin : FakeEditorPlugin, IClipTypeProvider
    {
        public NoDefaultConstructorClipPlugin()
            : base("plugin.no-default", "No Default Plugin")
        {
        }

        public IEnumerable<Type> GetClipTypes()
        {
            yield return typeof(ClipWithoutPublicParameterlessConstructor);
        }
    }

    private sealed class TestEditCommand : IEditCommand
    {
        public string Description => "test";
        public int ExecuteCallCount { get; private set; }

        public void Execute()
        {
            ExecuteCallCount++;
        }

        public void Undo()
        {
        }
    }

    [ClipTypeIdentifier("PluginText")]
    private sealed class Text : ClipObject
    {
        public Text()
        {
        }

        public Text(string id)
            : base(id)
        {
        }
    }

    [ClipTypeIdentifier("Ctorless")]
    private sealed class ClipWithoutPublicParameterlessConstructor : ClipObject
    {
        public ClipWithoutPublicParameterlessConstructor(string id)
            : base(id)
        {
        }
    }

    private sealed class FakePlaybackState : IPlaybackState
    {
        public int CurrentFrame { get; private set; }
        public bool IsPlaying { get; private set; }
        public int SamplingRate { get; } = 44100;
        public int AudioChannels { get; } = 2;
        public IRenderImageCache? ImageCache => null;
        public event Action? PlaybackStarted;
        public event Action? PlaybackPaused;
        public event Action? PlaybackSeeked;
        public event Action? PlaybackFrameChanged;
        public event Action? ReRenderingRequested;

        public void Play()
        {
            IsPlaying = true;
            PlaybackStarted?.Invoke();
        }

        public void Pause()
        {
            IsPlaying = false;
            PlaybackPaused?.Invoke();
        }

        public void Seek(int frame)
        {
            CurrentFrame = frame;
            PlaybackSeeked?.Invoke();
            PlaybackFrameChanged?.Invoke();
        }

        public void RequestReRendering()
        {
            ReRenderingRequested?.Invoke();
        }

        public void Dispose()
        {
        }
    }
}
