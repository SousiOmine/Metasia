using System;
using System.IO;
using Metasia.Core.Objects;
using Metasia.Core.Render.Cache;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.FileSystem;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Timeline;
using NUnit.Framework;

namespace Metasia.Editor.Tests.ViewModels;

[TestFixture]
public class TimelineViewModelTests
{
    [Test]
    public void HorizontalScrollPosition_WritesBackToTimelineViewState()
    {
        var timeline = new TimelineObject("RootTimeline");
        var projectState = new ProjectState();
        projectState.LoadProjectAsync(CreateProject(timeline)).GetAwaiter().GetResult();

        var viewState = new TimelineViewState();
        using var viewModel = new TimelineViewModel(
            timeline,
            new DummyLayerButtonViewModelFactory(),
            new DummyLayerCanvasViewModelFactory(),
            new SelectionState(),
            new FakePlaybackState(),
            projectState,
            new EditCommandManager(),
            viewState,
            new FakeClipboardService());

        viewModel.HorizontalScrollPosition = 24;

        Assert.That(viewState.HorizontalScrollPosition, Is.EqualTo(24));
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
