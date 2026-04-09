using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Input;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.ViewModels.Timeline;

namespace Metasia.Editor.Models.DragDrop.Handlers;

/// <summary>
/// タイムライン内のクリップ移動を処理するハンドラ
/// </summary>
public class ClipsMoveDropHandler : IDropHandler
{
    private readonly ISelectionState _selectionState;
    private readonly IEditCommandManager _editCommandManager;
    private readonly ITimelineViewStateStore _timelineViewStateStore;
    private IEditCommand? _lastPreviewCommand;

    public int Priority => 10;

    public ClipsMoveDropHandler(
        ISelectionState selectionState,
        IEditCommandManager editCommandManager,
        ITimelineViewStateStore timelineViewStateStore)
    {
        _selectionState = selectionState;
        _editCommandManager = editCommandManager;
        _timelineViewStateStore = timelineViewStateStore;
    }

    public bool CanHandle(IDataTransfer data, DropTargetContext context)
    {
        var id = data.TryGetValue(DragDropFormats.ClipsMove);
        return id != null && DragDropFormats.PeekData<ClipsMoveDragData>(id) != null;
    }

    public DropPreviewResult HandleDragOver(IDataTransfer data, DropTargetContext context)
    {
        var id = data.TryGetValue(DragDropFormats.ClipsMove);
        var dragData = DragDropFormats.PeekData<ClipsMoveDragData>(id);
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

        var viewState = _timelineViewStateStore.GetViewState(context.Timeline.Id);

        ClipInteractor.ApplyMoveSnapping(
            dropInfo,
            _selectionState.SelectedClips,
            context.Timeline,
            viewState.Frame_Per_DIP);

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

    public IEditCommand? HandleDrop(IDataTransfer data, DropTargetContext context)
    {
        var id = data.TryGetValue(DragDropFormats.ClipsMove);
        var dragData = DragDropFormats.RetrieveData<ClipsMoveDragData>(id);
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

        var viewState = _timelineViewStateStore.GetViewState(context.Timeline.Id);

        ClipInteractor.ApplyMoveSnapping(
            dropInfo,
            _selectionState.SelectedClips,
            context.Timeline,
            viewState.Frame_Per_DIP);

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