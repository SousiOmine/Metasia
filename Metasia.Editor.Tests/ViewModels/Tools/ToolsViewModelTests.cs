using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render.Cache;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Plugin;
using Metasia.Editor.Services;
using Metasia.Editor.Services.PluginService;
using Metasia.Editor.ViewModels;
using SkiaSharp;

namespace Metasia.Editor.Tests.ViewModels.Tools;

[TestFixture]
public class ToolsViewModelTests
{
    [Test]
    public void Constructor_IncludesBuiltinProjectPanelFirst()
    {
        using var viewModel = CreateToolsViewModel(new FakePluginService());

        Assert.That(viewModel.Panels.Select(x => x.Id).First(), Is.EqualTo("builtin.project"));
    }

    [Test]
    public void ReSelectingPanel_ReusesCreatedControl()
    {
        var pluginService = new FakePluginService();
        var factoryCallCount = 0;
        pluginService.Panels.Add(new LeftPanePanelDefinition(
            "sample.panel",
            "Sample",
            () =>
            {
                factoryCallCount++;
                return new TextBlock { Text = "Plugin Panel" };
            }));

        using var viewModel = CreateToolsViewModel(pluginService);
        var pluginPanel = viewModel.Panels.Single(x => x.Id == "sample.panel");

        viewModel.SelectedPanel = pluginPanel;
        var firstContent = pluginPanel.Content;

        viewModel.SelectedPanel = viewModel.Panels[0];
        viewModel.SelectedPanel = pluginPanel;
        var secondContent = pluginPanel.Content;

        Assert.Multiple(() =>
        {
            Assert.That(factoryCallCount, Is.EqualTo(1));
            Assert.That(secondContent, Is.SameAs(firstContent));
        });
    }

    private static ToolsViewModel CreateToolsViewModel(FakePluginService pluginService)
    {
        var selectionState = new SelectionState();
        var projectState = new ProjectState();
        var editCommandManager = new EditCommandManager();
        var playbackState = new FakePlaybackState();
        var playerFactory = new FakePlayerViewModelFactory(selectionState, playbackState, projectState, editCommandManager);

        var playerParent = new PlayerParentViewModel(
            new FakeKeyBindingService(),
            playerFactory,
            projectState,
            editCommandManager,
            selectionState);

        var project = CreateProject(new TimelineObject("RootTimeline"));
        projectState.LoadProjectAsync(project).GetAwaiter().GetResult();

        return new ToolsViewModel(playerParent, projectState, selectionState, editCommandManager, pluginService);
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
        public List<LeftPanePanelDefinition> Panels { get; } = [];

        public Task<IEnumerable<IEditorPlugin>> LoadPluginsAsync()
        {
            return Task.FromResult<IEnumerable<IEditorPlugin>>(EditorPlugins);
        }

        public IEnumerable<LeftPanePanelDefinition> GetLeftPanePanels()
        {
            return Panels;
        }

        public IEnumerable<IPluginSettingsProvider> GetSettingsProviders()
        {
            return [];
        }
    }

    private sealed class FakePlayerViewModelFactory : IPlayerViewModelFactory
    {
        private readonly ISelectionState _selectionState;
        private readonly IPlaybackState _playbackState;
        private readonly IProjectState _projectState;
        private readonly IEditCommandManager _editCommandManager;

        public FakePlayerViewModelFactory(
            ISelectionState selectionState,
            IPlaybackState playbackState,
            IProjectState projectState,
            IEditCommandManager editCommandManager)
        {
            _selectionState = selectionState;
            _playbackState = playbackState;
            _projectState = projectState;
            _editCommandManager = editCommandManager;
        }

        public PlayerViewModel Create(TimelineObject timeline, ProjectInfo projectInfo)
        {
            return new PlayerViewModel(timeline, projectInfo, _selectionState, _playbackState, _projectState, _editCommandManager);
        }
    }

    private sealed class FakePlaybackState : IPlaybackState
    {
        public int CurrentFrame { get; private set; }
        public bool IsPlaying { get; private set; }
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

    private sealed class FakeKeyBindingService : IKeyBindingService
    {
        public void ApplyKeyBindings(Window target)
        {
        }

        public void RegisterCommand(string commandId, System.Windows.Input.ICommand command)
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
}
