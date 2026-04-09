using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Core.Render.Cache;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using NUnit.Framework;

namespace Metasia.Editor.Tests.ViewModels;

[TestFixture]
public class PlayerParentViewModelTests
{
    [Test]
    public void SwitchToTimeline_ReplacesPlayerWithoutLeakingPlaybackSubscriptions()
    {
        var selectionState = new SelectionState();
        var projectState = new ProjectState();
        var editCommandManager = new EditCommandManager();
        var playbackState = new CountingPlaybackState();
        var timelineViewStateStore = new TimelineViewStateStore();
        var playerFactory = new FakePlayerViewModelFactory(selectionState, playbackState, projectState, editCommandManager);

        using var viewModel = new PlayerParentViewModel(
            new FakeKeyBindingService(),
            playerFactory,
            projectState,
            playbackState,
            timelineViewStateStore,
            editCommandManager,
            selectionState);

        var rootTimeline = new TimelineObject("RootTimeline");
        var secondTimeline = new TimelineObject("Timeline2");
        var project = CreateProject(rootTimeline, secondTimeline);

        projectState.LoadProjectAsync(project).GetAwaiter().GetResult();
        var initialSubscriptionCount = playbackState.TotalSubscriptionCount;
        var initialSeekCount = playbackState.SeekCallCount;

        viewModel.SwitchToTimeline(secondTimeline);

        Assert.Multiple(() =>
        {
            Assert.That(projectState.CurrentTimeline, Is.SameAs(secondTimeline));
            Assert.That(viewModel.TargetPlayerViewModel?.TargetTimeline, Is.SameAs(secondTimeline));
            Assert.That(playbackState.PauseCallCount, Is.EqualTo(1));
            Assert.That(playbackState.SeekCallCount, Is.EqualTo(initialSeekCount + 1));
            Assert.That(playbackState.TotalSubscriptionCount, Is.EqualTo(initialSubscriptionCount));
        });
    }

    [Test]
    public void SwitchToTimeline_RestoresSavedFrameWithoutOverwritingPreviousTimelineState()
    {
        var selectionState = new SelectionState();
        var projectState = new ProjectState();
        var editCommandManager = new EditCommandManager();
        var playbackState = new CountingPlaybackState();
        var timelineViewStateStore = new TimelineViewStateStore();
        var playerFactory = new FakePlayerViewModelFactory(selectionState, playbackState, projectState, editCommandManager);

        using var viewModel = new PlayerParentViewModel(
            new FakeKeyBindingService(),
            playerFactory,
            projectState,
            playbackState,
            timelineViewStateStore,
            editCommandManager,
            selectionState);

        var rootTimeline = new TimelineObject("RootTimeline");
        var secondTimeline = new TimelineObject("Timeline2");
        var project = CreateProject(rootTimeline, secondTimeline);

        projectState.LoadProjectAsync(project).GetAwaiter().GetResult();

        timelineViewStateStore.GetViewState(rootTimeline.Id).LastPreviewFrame = 42;
        timelineViewStateStore.GetViewState(secondTimeline.Id).LastPreviewFrame = 24;

        viewModel.SwitchToTimeline(secondTimeline);

        Assert.Multiple(() =>
        {
            Assert.That(timelineViewStateStore.GetViewState(rootTimeline.Id).LastPreviewFrame, Is.EqualTo(42));
            Assert.That(playbackState.CurrentFrame, Is.EqualTo(24));
            Assert.That(viewModel.TargetPlayerViewModel?.Frame, Is.EqualTo(24));
            Assert.That(viewModel.TargetPlayerViewModel?.TargetTimeline, Is.SameAs(secondTimeline));
        });
    }

    [Test]
    public void Dispose_ReleasesPlaybackSubscriptions()
    {
        var selectionState = new SelectionState();
        var projectState = new ProjectState();
        var editCommandManager = new EditCommandManager();
        var playbackState = new CountingPlaybackState();
        var timelineViewStateStore = new TimelineViewStateStore();
        var playerFactory = new FakePlayerViewModelFactory(selectionState, playbackState, projectState, editCommandManager);

        var viewModel = new PlayerParentViewModel(
            new FakeKeyBindingService(),
            playerFactory,
            projectState,
            playbackState,
            timelineViewStateStore,
            editCommandManager,
            selectionState);

        var project = CreateProject(new TimelineObject("RootTimeline"));
        projectState.LoadProjectAsync(project).GetAwaiter().GetResult();

        viewModel.Dispose();

        Assert.That(playbackState.TotalSubscriptionCount, Is.EqualTo(0));
    }

    [Test]
    public void LoadProjectAsync_RestoresRootTimelineFrameAndKeepsOtherTimelineStateForLaterSwitch()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDirectory);
        try
        {
            var projectFilePath = Path.Combine(tempDirectory, "sample.mtpj");

            var initialSelectionState = new SelectionState();
            var initialProjectState = new ProjectState();
            var initialEditCommandManager = new EditCommandManager();
            var initialPlaybackState = new CountingPlaybackState();
            var repository = new ProjectTimelineViewStateRepository(tempDirectory);
            var initialTimelineViewStateStore = new PersistentTimelineViewStateStore(
                new TimelineViewStateStore(),
                repository,
                initialProjectState,
                TimeSpan.Zero);
            var initialPlayerFactory = new FakePlayerViewModelFactory(
                initialSelectionState,
                initialPlaybackState,
                initialProjectState,
                initialEditCommandManager);

            using (var initialViewModel = new PlayerParentViewModel(
                new FakeKeyBindingService(),
                initialPlayerFactory,
                initialProjectState,
                initialPlaybackState,
                initialTimelineViewStateStore,
                initialEditCommandManager,
                initialSelectionState))
            {
                var initialRootTimeline = new TimelineObject("RootTimeline");
                var initialSecondTimeline = new TimelineObject("Timeline2");
                initialProjectState.LoadProjectAsync(CreateProject(projectFilePath, initialRootTimeline, initialSecondTimeline))
                    .GetAwaiter()
                    .GetResult();

                initialTimelineViewStateStore.GetViewState(initialRootTimeline.Id).LastPreviewFrame = 42;
                initialTimelineViewStateStore.GetViewState(initialRootTimeline.Id).Frame_Per_DIP = 5.0;
                initialTimelineViewStateStore.GetViewState(initialRootTimeline.Id).HorizontalScrollPosition = 12;
                initialTimelineViewStateStore.GetViewState(initialSecondTimeline.Id).LastPreviewFrame = 24;
                initialTimelineViewStateStore.GetViewState(initialSecondTimeline.Id).Frame_Per_DIP = 2.0;
                initialTimelineViewStateStore.GetViewState(initialSecondTimeline.Id).HorizontalScrollPosition = 7;
            }

            var selectionState = new SelectionState();
            var projectState = new ProjectState();
            var editCommandManager = new EditCommandManager();
            var playbackState = new CountingPlaybackState();
            var timelineViewStateStore = new PersistentTimelineViewStateStore(
                new TimelineViewStateStore(),
                repository,
                projectState,
                TimeSpan.Zero);
            var playerFactory = new FakePlayerViewModelFactory(selectionState, playbackState, projectState, editCommandManager);

            using var viewModel = new PlayerParentViewModel(
                new FakeKeyBindingService(),
                playerFactory,
                projectState,
                playbackState,
                timelineViewStateStore,
                editCommandManager,
                selectionState);

            var reloadedRootTimeline = new TimelineObject("RootTimeline");
            var reloadedSecondTimeline = new TimelineObject("Timeline2");
            projectState.LoadProjectAsync(CreateProject(projectFilePath, reloadedRootTimeline, reloadedSecondTimeline))
                .GetAwaiter()
                .GetResult();

            Assert.Multiple(() =>
            {
                Assert.That(projectState.CurrentTimeline, Is.SameAs(reloadedRootTimeline));
                Assert.That(viewModel.TargetPlayerViewModel?.TargetTimeline, Is.SameAs(reloadedRootTimeline));
                Assert.That(playbackState.CurrentFrame, Is.EqualTo(42));
                Assert.That(timelineViewStateStore.GetViewState(reloadedRootTimeline.Id).Frame_Per_DIP, Is.EqualTo(5.0));
                Assert.That(timelineViewStateStore.GetViewState(reloadedRootTimeline.Id).HorizontalScrollPosition, Is.EqualTo(12));
            });

            viewModel.SwitchToTimeline(reloadedSecondTimeline);

            Assert.Multiple(() =>
            {
                Assert.That(projectState.CurrentTimeline, Is.SameAs(reloadedSecondTimeline));
                Assert.That(playbackState.CurrentFrame, Is.EqualTo(24));
                Assert.That(timelineViewStateStore.GetViewState(reloadedSecondTimeline.Id).Frame_Per_DIP, Is.EqualTo(2.0));
                Assert.That(timelineViewStateStore.GetViewState(reloadedSecondTimeline.Id).HorizontalScrollPosition, Is.EqualTo(7));
            });
        }
        finally
        {
            Directory.Delete(tempDirectory, true);
        }
    }

    private static MetasiaEditorProject CreateProject(params TimelineObject[] timelines)
    {
        return CreateProject(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mtpj"), timelines);
    }

    private static MetasiaEditorProject CreateProject(string projectFilePath, params TimelineObject[] timelines)
    {
        var projectDirectory = Path.GetDirectoryName(projectFilePath)!;

        var project = new MetasiaEditorProject(
            new DirectoryEntity(projectDirectory),
            new MetasiaProjectFile
            {
                RootTimelineId = "RootTimeline",
                Framerate = 60,
                Resolution = new VideoResolution { Width = 1920, Height = 1080 }
            })
        {
            ProjectFilePath = projectFilePath
        };

        foreach (var timeline in timelines)
        {
            project.Timelines.Add(timeline);
        }

        return project;
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

    private sealed class CountingPlaybackState : IPlaybackState
    {
        private Action? _playbackStarted;
        private Action? _playbackPaused;
        private Action? _playbackSeeked;
        private Action? _playbackFrameChanged;
        private Action? _reRenderingRequested;

        public int CurrentFrame { get; private set; }
        public bool IsPlaying { get; private set; }
        public int SamplingRate => 44100;
        public int AudioChannels => 2;
        public IRenderImageCache? ImageCache => null;
        public int PauseCallCount { get; private set; }
        public int SeekCallCount { get; private set; }
        public int TotalSubscriptionCount =>
            PlaybackStartedSubscriberCount +
            PlaybackPausedSubscriberCount +
            PlaybackSeekedSubscriberCount +
            PlaybackFrameChangedSubscriberCount +
            ReRenderingRequestedSubscriberCount;

        public int PlaybackStartedSubscriberCount { get; private set; }
        public int PlaybackPausedSubscriberCount { get; private set; }
        public int PlaybackSeekedSubscriberCount { get; private set; }
        public int PlaybackFrameChangedSubscriberCount { get; private set; }
        public int ReRenderingRequestedSubscriberCount { get; private set; }

        public event Action? PlaybackStarted
        {
            add
            {
                _playbackStarted += value;
                PlaybackStartedSubscriberCount++;
            }
            remove
            {
                _playbackStarted -= value;
                PlaybackStartedSubscriberCount--;
            }
        }

        public event Action? PlaybackPaused
        {
            add
            {
                _playbackPaused += value;
                PlaybackPausedSubscriberCount++;
            }
            remove
            {
                _playbackPaused -= value;
                PlaybackPausedSubscriberCount--;
            }
        }

        public event Action? PlaybackSeeked
        {
            add
            {
                _playbackSeeked += value;
                PlaybackSeekedSubscriberCount++;
            }
            remove
            {
                _playbackSeeked -= value;
                PlaybackSeekedSubscriberCount--;
            }
        }

        public event Action? PlaybackFrameChanged
        {
            add
            {
                _playbackFrameChanged += value;
                PlaybackFrameChangedSubscriberCount++;
            }
            remove
            {
                _playbackFrameChanged -= value;
                PlaybackFrameChangedSubscriberCount--;
            }
        }

        public event Action? ReRenderingRequested
        {
            add
            {
                _reRenderingRequested += value;
                ReRenderingRequestedSubscriberCount++;
            }
            remove
            {
                _reRenderingRequested -= value;
                ReRenderingRequestedSubscriberCount--;
            }
        }

        public void Play()
        {
            IsPlaying = true;
            _playbackStarted?.Invoke();
        }

        public void Pause()
        {
            PauseCallCount++;
            IsPlaying = false;
            _playbackPaused?.Invoke();
        }

        public void Seek(int frame)
        {
            SeekCallCount++;
            CurrentFrame = frame;
            _playbackSeeked?.Invoke();
            _playbackFrameChanged?.Invoke();
        }

        public void RequestReRendering()
        {
            _reRenderingRequested?.Invoke();
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
