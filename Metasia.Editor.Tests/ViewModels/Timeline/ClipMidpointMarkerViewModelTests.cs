using Avalonia.Media;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Timeline;
using Moq;
using SkiaSharp;

namespace Metasia.Editor.Tests.ViewModels.Timeline;

[TestFixture]
public class ClipMidpointMarkerViewModelTests
{
    [Test]
    public void RefreshMidpointMarkers_SortsMarkersAndUpdatesPositionOnZoomChange()
    {
        var clip = new ShapeObject
        {
            StartFrame = 0,
            EndFrame = 100
        };
        clip.X.IsMovable = true;
        clip.Y.IsMovable = true;
        clip.X.AddPoint(new CoordPoint { Id = "b", Frame = 10, Value = 1 });
        clip.Y.AddPoint(new CoordPoint { Id = "a", Frame = 10, Value = 2 });

        var timelineViewState = new TestTimelineViewState { Frame_Per_DIP = 2 };
        var clipViewModel = CreateClipViewModel(clip, timelineViewState, out _);

        var markerIds = clipViewModel.MidpointMarkers.Select(x => x.MarkerId).ToList();
        var expectedOrder = markerIds.OrderBy(x => x, StringComparer.Ordinal).ToList();

        Assert.That(markerIds, Is.EqualTo(expectedOrder));
        Assert.That(clipViewModel.MidpointMarkers[0].Left, Is.EqualTo(16d).Within(0.001));

        timelineViewState.Frame_Per_DIP = 4;

        Assert.That(clipViewModel.MidpointMarkers[0].Left, Is.EqualTo(36d).Within(0.001));
    }

    [Test]
    public void MarkerDrag_PreviewsAndCommitsFrameChange()
    {
        var clip = new ShapeObject
        {
            StartFrame = 0,
            EndFrame = 100
        };
        clip.X.IsMovable = true;
        var point = new CoordPoint { Id = "point", Frame = 10, Value = 1 };
        clip.X.AddPoint(point);

        var timelineViewState = new TestTimelineViewState { Frame_Per_DIP = 2 };
        var clipViewModel = CreateClipViewModel(clip, timelineViewState, out var editCommandManager);
        var marker = clipViewModel.MidpointMarkers.Single();

        marker.StartDrag(20);
        marker.UpdateDrag(30);

        Assert.That(point.Frame, Is.EqualTo(15));

        marker.EndDrag(30);

        Assert.That(point.Frame, Is.EqualTo(15));
        Assert.That(editCommandManager.CanUndo, Is.True);

        editCommandManager.Undo();

        Assert.That(point.Frame, Is.EqualTo(10));
    }

    [Test]
    public void MarkerDrag_MovesAllMidpointsAtSameFrame()
    {
        var clip = new ShapeObject
        {
            StartFrame = 0,
            EndFrame = 100
        };
        clip.X.IsMovable = true;
        clip.Y.IsMovable = true;
        var pointX = new CoordPoint { Id = "point-x", Frame = 10, Value = 1 };
        var pointY = new CoordPoint { Id = "point-y", Frame = 10, Value = 2 };
        clip.X.AddPoint(pointX);
        clip.Y.AddPoint(pointY);

        var timelineViewState = new TestTimelineViewState { Frame_Per_DIP = 2 };
        var clipViewModel = CreateClipViewModel(clip, timelineViewState, out var editCommandManager);
        var marker = clipViewModel.MidpointMarkers.First();

        marker.StartDrag(20);
        marker.UpdateDrag(30);

        Assert.That(pointX.Frame, Is.EqualTo(15));
        Assert.That(pointY.Frame, Is.EqualTo(15));

        marker.EndDrag(30);
        editCommandManager.Undo();

        Assert.That(pointX.Frame, Is.EqualTo(10));
        Assert.That(pointY.Frame, Is.EqualTo(10));
    }

    [Test]
    public void MarkerDrag_DoesNotMoveFrameZeroPointWithoutDragDistance()
    {
        var clip = new ShapeObject
        {
            StartFrame = 0,
            EndFrame = 100
        };
        clip.X.IsMovable = true;
        var point = new CoordPoint { Id = "point", Frame = 0, Value = 1 };
        clip.X.AddPoint(point);

        var timelineViewState = new TestTimelineViewState { Frame_Per_DIP = 2 };
        var clipViewModel = CreateClipViewModel(clip, timelineViewState, out var editCommandManager);
        var marker = clipViewModel.MidpointMarkers.Single();

        marker.StartDrag(0);
        marker.EndDrag(0);

        Assert.That(point.Frame, Is.EqualTo(0));
        Assert.That(editCommandManager.CanUndo, Is.False);
    }

    [Test]
    public void RefreshMidpointMarkers_HidesMarkersWhenParamIsNotMovable()
    {
        var clip = new ShapeObject
        {
            StartFrame = 0,
            EndFrame = 100
        };
        clip.X.IsMovable = true;
        clip.X.AddPoint(new CoordPoint { Id = "point", Frame = 10, Value = 1 });
        clip.X.IsMovable = false;

        var timelineViewState = new TestTimelineViewState { Frame_Per_DIP = 2 };
        var clipViewModel = CreateClipViewModel(clip, timelineViewState, out _);

        Assert.That(clipViewModel.MidpointMarkers, Is.Empty);
    }

    private static ClipViewModel CreateClipViewModel(ClipObject clip, TestTimelineViewState timelineViewState, out EditCommandManager editCommandManager)
    {
        var timeline = new TimelineObject();

        var projectStateMock = new Mock<IProjectState>();
        projectStateMock.SetupGet(x => x.CurrentTimeline).Returns(timeline);
        projectStateMock.SetupGet(x => x.CurrentProjectInfo).Returns(new ProjectInfo(60, new SKSize(1920, 1080), 48000, 2));

        var selectionStateMock = new Mock<ISelectionState>();
        selectionStateMock.SetupGet(x => x.SelectedClips).Returns(Array.Empty<ClipObject>());

        var playbackStateMock = new Mock<IPlaybackState>();
        playbackStateMock.SetupGet(x => x.CurrentFrame).Returns(0);
        playbackStateMock.SetupGet(x => x.IsPlaying).Returns(false);
        playbackStateMock.SetupGet(x => x.SamplingRate).Returns(48000);
        playbackStateMock.SetupGet(x => x.AudioChannels).Returns(2);

        var layerButtonFactoryMock = new Mock<ILayerButtonViewModelFactory>();
        var layerCanvasFactoryMock = new Mock<ILayerCanvasViewModelFactory>();
        var clipboardServiceMock = new Mock<IClipboardService>();
        var clipColorProviderMock = new Mock<IClipColorProvider>();
        clipColorProviderMock.Setup(x => x.GetBrush(It.IsAny<ClipObject>())).Returns(Brushes.Blue);
        var fileDialogServiceMock = new Mock<IFileDialogService>();

        editCommandManager = new EditCommandManager();

        var timelineViewModel = new TimelineViewModel(
            layerButtonFactoryMock.Object,
            layerCanvasFactoryMock.Object,
            selectionStateMock.Object,
            playbackStateMock.Object,
            projectStateMock.Object,
            editCommandManager,
            timelineViewState,
            clipboardServiceMock.Object);

        return new ClipViewModel(
            clip,
            timelineViewModel,
            editCommandManager,
            timelineViewState,
            clipColorProviderMock.Object,
            selectionStateMock.Object,
            projectStateMock.Object,
            fileDialogServiceMock.Object);
    }

    private sealed class TestTimelineViewState : ITimelineViewState
    {
        public double Frame_Per_DIP
        {
            get => _framePerDip;
            set
            {
                _framePerDip = value;
                Frame_Per_DIP_Changed?.Invoke();
            }
        }

        public event Action? Frame_Per_DIP_Changed;

        public void Dispose()
        {
        }

        private double _framePerDip;
    }
}
