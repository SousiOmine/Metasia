using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using Metasia.Editor.ViewModels.Timeline;

namespace Metasia.Editor.Models.DragDrop.Handlers;

/// <summary>
/// タイムライン内のクリップ移動を処理するハンドラ
/// </summary>
public class ClipsMoveDropHandler : IDropHandler
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly ITimelineViewState _timelineViewState;
    private IEditCommand? _lastPreviewCommand;

    public int Priority => 10;

    public ClipsMoveDropHandler(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        ITimelineViewState timelineViewState)
    {
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _timelineViewState = timelineViewState;
    }

    public bool CanHandle(IDataObject data, DropTargetContext context)
    {
        return data.Get(DragDropFormats.ClipsMove) is ClipsMoveDragData;
    }

    public DropPreviewResult HandleDragOver(IDataObject data, DropTargetContext context)
    {
        var dragData = data.Get(DragDropFormats.ClipsMove) as ClipsMoveDragData;
        if (dragData is null || _selectionState.SelectedClips.Count() == 0)
        {
            return DropPreviewResult.None;
        }

        _editCommandManager.CancelPreview();

        var dropInfo = CreateDropTargetContext(dragData, context);
        if (!dropInfo.CanDrop)
        {
            return DropPreviewResult.None;
        }

        ClipInteractor.ApplyMoveSnapping(
            dropInfo,
            _selectionState.SelectedClips,
            context.Timeline,
            _timelineViewState.Frame_Per_DIP);

        var command = ClipInteractor.CreateMoveClipsCommand(
            dropInfo,
            context.Timeline,
            context.TargetLayer,
            _selectionState.SelectedClips);

        if (command is null)
        {
            return DropPreviewResult.None;
        }

        _lastPreviewCommand = command;
        _editCommandManager.PreviewExecute(command);

        return DropPreviewResult.Move(command);
    }

    public IEditCommand? HandleDrop(IDataObject data, DropTargetContext context)
    {
        var dragData = data.Get(DragDropFormats.ClipsMove) as ClipsMoveDragData;
        if (dragData is null || _selectionState.SelectedClips.Count() == 0)
        {
            return null;
        }

        _editCommandManager.CancelPreview();

        var dropInfo = CreateDropTargetContext(dragData, context);
        if (!dropInfo.CanDrop)
        {
            return null;
        }

        ClipInteractor.ApplyMoveSnapping(
            dropInfo,
            _selectionState.SelectedClips,
            context.Timeline,
            _timelineViewState.Frame_Per_DIP);

        return ClipInteractor.CreateMoveClipsCommand(
            dropInfo,
            context.Timeline,
            context.TargetLayer,
            _selectionState.SelectedClips);
    }

    private ClipsDropTargetContext CreateDropTargetContext(ClipsMoveDragData dragData, DropTargetContext context)
    {
        return new ClipsDropTargetContext(
            dragData.ReferencedClipVM,
            dragData.DraggingFrameOffsetX,
            context.TargetFrame,
            canDrop: true);
    }
}