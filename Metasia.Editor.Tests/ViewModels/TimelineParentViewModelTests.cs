using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.IO;
using Metasia.Core.Objects;
using Metasia.Core.Render.Cache;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Timeline;
using NUnit.Framework;

namespace Metasia.Editor.Tests.ViewModels;

[TestFixture]
public class TimelineParentViewModelTests
{
    [Test]
    public void CurrentTimelineChanged_ReplacesTimelineViewModel()
    {
        var projectState = new ProjectState();
        var selectionState = new SelectionState();
        var editCommandManager = new EditCommandManager();
        var playbackState = new FakePlaybackState();
        var factory = new TrackingTimelineViewModelFactory(projectState, selectionState, playbackState, editCommandManager);

        using var viewModel = new TimelineParentViewModel(factory, projectState);

        var rootTimeline = new TimelineObject("RootTimeline");
        var secondTimeline = new TimelineObject("Timeline2");
        var project = CreateProject(rootTimeline, secondTimeline);
        projectState.LoadProjectAsync(project).GetAwaiter().GetResult();

        var firstViewModel = (TrackingTimelineViewModel)viewModel.CurrentTimelineViewModel!;

        projectState.SetCurrentTimeline(secondTimeline);

        Assert.Multiple(() =>
        {
            Assert.That(firstViewModel.IsDisposed, Is.True);
            Assert.That(viewModel.CurrentTimelineViewModel, Is.Not.SameAs(firstViewModel));
            Assert.That(viewModel.CurrentTimelineViewModel?.Timeline, Is.SameAs(secondTimeline));
            Assert.That(viewModel.IsTimelineShow, Is.True);
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

    private sealed class TrackingTimelineViewModelFactory : ITimelineViewModelFactory
    {
        private readonly IProjectState _projectState;
        private readonly ISelectionState _selectionState;
        private readonly IPlaybackState _playbackState;
        private readonly IEditCommandManager _editCommandManager;
        private readonly ITimelineViewState _timelineViewState = new TimelineViewState();
        private readonly IClipboardService _clipboardService = new FakeClipboardService();
        private readonly ILayerButtonViewModelFactory _layerButtonFactory = new DummyLayerButtonViewModelFactory();
        private readonly ILayerCanvasViewModelFactory _layerCanvasFactory = new DummyLayerCanvasViewModelFactory();

        public TrackingTimelineViewModelFactory(
            IProjectState projectState,
            ISelectionState selectionState,
            IPlaybackState playbackState,
            IEditCommandManager editCommandManager)
        {
            _projectState = projectState;
            _selectionState = selectionState;
            _playbackState = playbackState;
            _editCommandManager = editCommandManager;
        }

        public TimelineViewModel Create(TimelineObject timeline)
        {
            return new TrackingTimelineViewModel(
                timeline,
                _layerButtonFactory,
                _layerCanvasFactory,
                _selectionState,
                _playbackState,
                _projectState,
                _editCommandManager,
                _timelineViewState,
                _clipboardService);
        }
    }

    private sealed class TrackingTimelineViewModel : TimelineViewModel
    {
        public bool IsDisposed { get; private set; }

        public TrackingTimelineViewModel(
            TimelineObject timeline,
            ILayerButtonViewModelFactory layerButtonViewModelFactory,
            ILayerCanvasViewModelFactory layerCanvasViewModelFactory,
            ISelectionState selectionState,
            IPlaybackState playbackState,
            IProjectState projectState,
            IEditCommandManager editCommandManager,
            ITimelineViewState timelineViewState,
            IClipboardService clipboardService)
            : base(
                timeline,
                layerButtonViewModelFactory,
                layerCanvasViewModelFactory,
                selectionState,
                playbackState,
                projectState,
                editCommandManager,
                timelineViewState,
                clipboardService)
        {
        }

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }

    private sealed class DummyLayerButtonViewModelFactory : ILayerButtonViewModelFactory
    {
        public LayerButtonViewModel Create(LayerObject targetLayerObject)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class DummyLayerCanvasViewModelFactory : ILayerCanvasViewModelFactory
    {
        public LayerCanvasViewModel Create(TimelineViewModel parentTimelineViewModel, LayerObject targetLayerObject)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeClipboardService : IClipboardService
    {
        public bool HasClips => false;

        public void StoreClips(string clipTemplateXml)
        {
        }

        public string? GetStoredClips()
        {
            return null;
        }

        public void Clear()
        {
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
}
