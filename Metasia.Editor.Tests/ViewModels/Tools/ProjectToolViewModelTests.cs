using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Core.Project;
using Metasia.Core.Render.Cache;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models.Tools.ProjectTool;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Tools;
using NUnit.Framework;
using SkiaSharp;

namespace Metasia.Editor.Tests.ViewModels.Tools;

[TestFixture]
public class ProjectToolViewModelTests
{
    [Test]
    public void CreateTimelineCommand_AddsUniqueTimelineIdsWith100LayersAndActivatesLatest()
    {
        var selectionState = new SelectionState();
        var projectState = new ProjectState();
        var editCommandManager = new EditCommandManager(projectState);
        var playbackState = new FakePlaybackState();
        var timelineViewStateStore = new TimelineViewStateStore();
        var playerFactory = new FakePlayerViewModelFactory(selectionState, playbackState, projectState, editCommandManager);

        using var playerParent = new PlayerParentViewModel(
            new FakeKeyBindingService(),
            playerFactory,
            projectState,
            playbackState,
            timelineViewStateStore,
            editCommandManager,
            selectionState);
        using var viewModel = new ProjectToolViewModel(playerParent, projectState, selectionState, editCommandManager);

        var project = CreateProject(new TimelineObject("RootTimeline"));
        projectState.LoadProjectAsync(project).GetAwaiter().GetResult();

        viewModel.CreateTimelineCommand.Execute(null);
        viewModel.CreateTimelineCommand.Execute(null);

        var createdTimelines = project.Timelines.Where(x => x.Id.StartsWith("Timeline", StringComparison.Ordinal)).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(createdTimelines.Select(x => x.Id), Is.EqualTo(new[] { "Timeline1", "Timeline2" }));
            Assert.That(createdTimelines.All(x => x.Layers.Count == 100), Is.True);
            Assert.That(createdTimelines[0].Layers[0].Id, Is.EqualTo("layer1"));
            Assert.That(createdTimelines[0].Layers[0].Name, Is.EqualTo("Layer 1"));
            Assert.That(createdTimelines[1].Layers[^1].Id, Is.EqualTo("layer100"));
            Assert.That(createdTimelines[1].Layers[^1].Name, Is.EqualTo("Layer 100"));
            Assert.That(projectState.CurrentTimeline, Is.SameAs(createdTimelines[1]));
            Assert.That(playerParent.TargetPlayerViewModel?.TargetTimeline, Is.SameAs(createdTimelines[1]));
        });
    }

    [Test]
    public void SelectingClipNodeFromAnotherTimeline_SwitchesTimelineAndSelectsClip()
    {
        var selectionState = new SelectionState();
        var projectState = new ProjectState();
        var editCommandManager = new EditCommandManager(projectState);
        var playbackState = new FakePlaybackState();
        var timelineViewStateStore = new TimelineViewStateStore();
        var playerFactory = new FakePlayerViewModelFactory(selectionState, playbackState, projectState, editCommandManager);

        using var playerParent = new PlayerParentViewModel(
            new FakeKeyBindingService(),
            playerFactory,
            projectState,
            playbackState,
            timelineViewStateStore,
            editCommandManager,
            selectionState);
        using var viewModel = new ProjectToolViewModel(playerParent, projectState, selectionState, editCommandManager);

        var rootTimeline = new TimelineObject("RootTimeline");
        var secondTimeline = new TimelineObject("Timeline2");
        var layer = new LayerObject("layer1", "Layer 1");
        var clip = new ImageObject
        {
            Id = "clip1",
            StartFrame = 0,
            EndFrame = 30
        };
        layer.Objects.Add(clip);
        secondTimeline.Layers.Add(layer);

        var project = CreateProject(rootTimeline, secondTimeline);
        projectState.LoadProjectAsync(project).GetAwaiter().GetResult();

        var clipNode = FindNode(viewModel.Nodes, clip);
        Assert.That(clipNode, Is.Not.Null);

        viewModel.SelectedNode = clipNode;

        Assert.Multiple(() =>
        {
            Assert.That(projectState.CurrentTimeline, Is.SameAs(secondTimeline));
            Assert.That(playerParent.TargetPlayerViewModel?.TargetTimeline, Is.SameAs(secondTimeline));
            Assert.That(selectionState.SelectedClips, Has.Count.EqualTo(1));
            Assert.That(selectionState.SelectedClips.Single(), Is.SameAs(clip));
        });
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

    private static ProjectObjectTreeNode? FindNode<T>(System.Collections.Generic.IEnumerable<ProjectObjectTreeNode> nodes, T target)
    {
        foreach (var node in nodes)
        {
            if (ReferenceEquals(node.SourceObject, target))
            {
                return node;
            }

            if (node.SubNodes is null)
            {
                continue;
            }

            var child = FindNode(node.SubNodes, target);
            if (child is not null)
            {
                return child;
            }
        }

        return null;
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
