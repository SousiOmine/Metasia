using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Timeline;

public class ClipMidpointMarkerViewModel : ViewModelBase
{
    public const double MarkerSize = 8d;

    public string MarkerId { get; }
    internal MetaNumberParam<double> TargetParam => _targetParam;
    internal CoordPoint TargetCoordPoint => _targetCoordPoint;
    internal int CurrentFrame => _targetCoordPoint.Frame;

    public double Left
    {
        get => _left;
        private set => this.RaiseAndSetIfChanged(ref _left, value);
    }

    public double Top => 11d;

    public double Width => MarkerSize;

    public double Height => MarkerSize;

    public ClipMidpointMarkerViewModel(
        string markerId,
        ClipViewModel ownerClipViewModel,
        MetaNumberParam<double> targetParam,
        CoordPoint targetCoordPoint,
        IEditCommandManager editCommandManager,
        double framePerDip)
    {
        MarkerId = markerId ?? throw new ArgumentNullException(nameof(markerId));
        _ownerClipViewModel = ownerClipViewModel ?? throw new ArgumentNullException(nameof(ownerClipViewModel));
        _targetParam = targetParam ?? throw new ArgumentNullException(nameof(targetParam));
        _targetCoordPoint = targetCoordPoint ?? throw new ArgumentNullException(nameof(targetCoordPoint));
        _editCommandManager = editCommandManager ?? throw new ArgumentNullException(nameof(editCommandManager));
        if (framePerDip <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(framePerDip), framePerDip, "framePerDip must be positive.");
        }
        _framePerDip = framePerDip;

        RefreshPosition();
    }

    public void Refresh(MetaNumberParam<double> targetParam, CoordPoint targetCoordPoint, double framePerDip)
    {
        _targetParam = targetParam ?? throw new ArgumentNullException(nameof(targetParam));
        _targetCoordPoint = targetCoordPoint ?? throw new ArgumentNullException(nameof(targetCoordPoint));
        if (framePerDip <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(framePerDip), framePerDip, "framePerDip must be positive.");
        }
        _framePerDip = framePerDip;
        RefreshPosition();
    }

    public void StartDrag(double pointerPositionX)
    {
        _ownerClipViewModel.SelectOnly();
        _isDragging = true;
        _dragStartPointerX = pointerPositionX;
        _originalFrame = _targetCoordPoint.Frame;
        _lastPreviewFrame = _originalFrame;
        _dragTargets = _ownerClipViewModel.CaptureMidpointGroupAtFrame(_originalFrame);
    }

    public void UpdateDrag(double pointerPositionX)
    {
        if (!_isDragging)
        {
            return;
        }

        int targetFrame = CalculateTargetFrame(pointerPositionX);
        if (targetFrame == _lastPreviewFrame)
        {
            return;
        }

        if (targetFrame == _originalFrame)
        {
            _editCommandManager.CancelPreview();
            _lastPreviewFrame = _originalFrame;
            _ownerClipViewModel.RefreshMidpointMarkers();
            return;
        }

        _editCommandManager.PreviewExecute(CreateFrameChangeCommand(targetFrame));
        _lastPreviewFrame = targetFrame;
    }

    public void EndDrag(double pointerPositionX)
    {
        if (!_isDragging)
        {
            return;
        }

        int targetFrame = CalculateTargetFrame(pointerPositionX);
        if (targetFrame == _originalFrame)
        {
            _editCommandManager.CancelPreview();
            _ownerClipViewModel.RefreshMidpointMarkers();
        }
        else
        {
            _editCommandManager.Execute(CreateFrameChangeCommand(targetFrame));
        }

        _isDragging = false;
        _lastPreviewFrame = _targetCoordPoint.Frame;
        _dragTargets.Clear();
    }

    private int CalculateTargetFrame(double pointerPositionX)
    {
        double deltaPixels = pointerPositionX - _dragStartPointerX;
        int deltaFrame = (int)Math.Round(deltaPixels / _framePerDip, MidpointRounding.AwayFromZero);
        int clipLength = _ownerClipViewModel.TargetObject.EndFrame - _ownerClipViewModel.TargetObject.StartFrame + 1;
        int minFrame = _originalFrame == 0 ? 0 : 1;
        int maxFrame = _originalFrame == clipLength ? clipLength : Math.Max(1, clipLength - 1);
        return Math.Clamp(_originalFrame + deltaFrame, minFrame, maxFrame);
    }

    private void RefreshPosition()
    {
        Left = (_targetCoordPoint.Frame * _framePerDip) - (MarkerSize / 2d);
    }

    private IEditCommand CreateFrameChangeCommand(int targetFrame)
    {
        if (_dragTargets.Count == 0)
        {
            return new CoordPointFrameChangeCommand(_targetParam, _targetCoordPoint, _originalFrame, targetFrame);
        }

        var commands = _dragTargets
            .Select(x => (IEditCommand)new CoordPointFrameChangeCommand(x.TargetParam, x.TargetCoordPoint, x.BeforeFrame, targetFrame))
            .ToList();

        return commands.Count == 1
            ? commands[0]
            : new CompositeCommand(commands, "同一フレーム中間点の移動");
    }

    private readonly ClipViewModel _ownerClipViewModel;
    private readonly IEditCommandManager _editCommandManager;
    private MetaNumberParam<double> _targetParam;
    private CoordPoint _targetCoordPoint;
    private double _framePerDip;
    private double _left;
    private bool _isDragging;
    private double _dragStartPointerX;
    private int _originalFrame;
    private int _lastPreviewFrame;
    private List<ClipViewModel.MidpointDragTarget> _dragTargets = new();
}
