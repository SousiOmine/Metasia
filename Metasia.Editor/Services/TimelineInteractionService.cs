using Metasia.Core.Objects;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;


using Metasia.Editor.ViewModels;
using Metasia.Editor.ViewModels.Controls;



using System;
using System.Collections.Generic;
using System.Linq;

namespace Metasia.Editor.Services
{
    /// <summary>
    /// ドラッグハンドルの名前を定義する
    /// </summary>
    public static class DragHandleNames
    {
        public const string StartHandle = "StartHandle";
        public const string EndHandle = "EndHandle";
    }

    /// <summary>
    /// タイムライン上のインタラクションを専門に扱うサービス
    /// </summary>
    public class TimelineInteractionService : ITimelineInteractionService
    {
        private readonly ITimelineContext _timelineContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimelineInteractionService"/> with the specified timeline context.
        /// </summary>
        public TimelineInteractionService(ITimelineContext timelineContext)
        {
            _timelineContext = timelineContext;
        }

        /// <summary>
        /// クリップのリサイズ操作を処理する
        /// <summary>
        /// Initiates a resize operation on the specified clip using the given drag handle and pointer position.
        /// </summary>
        /// <param name="clipViewModel">The clip to be resized.</param>
        /// <param name="handleName">The name of the drag handle being used (e.g., start or end handle).</param>
        /// <param name="pointerPositionXOnCanvas">The X position of the pointer on the canvas at the start of the resize.</param>
        public void ResizeClip(ClipViewModel clipViewModel, string handleName, double pointerPositionXOnCanvas)
        {
            // リサイズ操作のビジネスロジックをここに集約
            clipViewModel.StartDrag(handleName, pointerPositionXOnCanvas);
        }

        /// <summary>
        /// クリップのドラッグ操作を開始する
        /// <summary>
        /// Initiates a drag operation on a clip, setting the drag state, handle name, initial pointer position, and initial frame based on the selected handle.
        /// </summary>
        /// <param name="clipViewModel">The clip view model to begin dragging.</param>
        /// <param name="handleName">The name of the handle being dragged (e.g., start or end).</param>
        /// <param name="pointerPositionXOnCanvas">The X position of the pointer on the canvas at the start of the drag.</param>
        public void StartClipDrag(ClipViewModel clipViewModel, string handleName, double pointerPositionXOnCanvas)
        {
            // ドラッグ開始時のビジネスロジックをここに集約
            clipViewModel.IsDragging = true;
            clipViewModel.DragHandleName = handleName;
            clipViewModel.DragStartX = pointerPositionXOnCanvas;
            clipViewModel.InitialDragFrame = (handleName == DragHandleNames.StartHandle) ? clipViewModel.TargetObject.StartFrame : clipViewModel.TargetObject.EndFrame;
        }

        /// <summary>
        /// クリップのドラッグ操作を更新する
        /// <summary>
        /// Placeholder for updating the state of a clip during a drag operation.
        /// </summary>
        public void UpdateClipDrag(ClipViewModel clipViewModel, double pointerPositionXOnCanvas)
        {
            // ドラッグ中のビジネスロジックをここに集約
            // 現在の実装では特に何もしないが、将来的な拡張のためにメソッドを残す
        }

        /// <summary>
        /// クリップのドラッグ操作を終了する
        /// <summary>
        /// Finalizes a clip drag-resize operation, calculates the new start or end frame based on pointer movement, enforces valid frame constraints, and applies the resize if allowed.
        /// </summary>
        /// <param name="clipViewModel">The clip view model being resized.</param>
        /// <param name="pointerPositionXOnCanvas">The X position of the pointer on the canvas at the end of the drag.</param>
        public void EndClipDrag(ClipViewModel clipViewModel, double pointerPositionXOnCanvas)
        {
            // ドラッグ終了時のビジネスロジックをここに集約
            if (!clipViewModel.IsDragging || string.IsNullOrEmpty(clipViewModel.DragHandleName))
            {
                return;
            }

            double deltaX = pointerPositionXOnCanvas - clipViewModel.DragStartX;
            double frameDelta = deltaX / clipViewModel.Frame_Per_DIP;
            int frameChange = (int)Math.Round(frameDelta);

            int newStartFrame = clipViewModel.TargetObject.StartFrame;
            int newEndFrame = clipViewModel.TargetObject.EndFrame;

            if (clipViewModel.DragHandleName == DragHandleNames.StartHandle)
            {
                newStartFrame = clipViewModel.InitialDragFrame + frameChange;
                // 終端を超えないように、かつ長さが1未満にならないように制限
                newStartFrame = Math.Min(newStartFrame, clipViewModel.TargetObject.EndFrame - 1);
                newStartFrame = Math.Max(newStartFrame, 0);
            }
            else if (clipViewModel.DragHandleName == DragHandleNames.EndHandle)
            {
                newEndFrame = clipViewModel.InitialDragFrame + frameChange;
                // 始端を下回らないように、かつ長さが1未満にならないように制限
                newEndFrame = Math.Max(newEndFrame, clipViewModel.TargetObject.StartFrame + 1);
            }

            // 希望のフレームのままリサイズできるならばリサイズ実行
            if (CanResizeClip(clipViewModel.TargetObject, newStartFrame, newEndFrame))
            {
                // フレームが変化していればコマンドを実行
                if (newStartFrame != clipViewModel.TargetObject.StartFrame || newEndFrame != clipViewModel.TargetObject.EndFrame)
                {
                    IEditCommand command = new ClipResizeCommand(
                        clipViewModel.TargetObject,
                        clipViewModel.TargetObject.StartFrame, newStartFrame,
                        clipViewModel.TargetObject.EndFrame, newEndFrame
                    );
                    _timelineViewModel.RunEditCommand(command);

                    clipViewModel.RecalculateSize();
                }
            }
            else
            {
                // ドラッグしたそのままのフレームでは重複でリサイズできない場合、重複しないぎりぎりまで詰める
            }

            clipViewModel.IsDragging = false;
            clipViewModel.DragHandleName = string.Empty;
        }

        /// <summary>
        /// クリップの移動操作を処理する
        /// <summary>
        /// Moves a clip to a new layer and frame position based on drag-and-drop information, if the move is valid.
        /// </summary>
        public void MoveClips(ClipsDropTargetInfo dropInfo)
        {
            if (dropInfo == null || dropInfo.DragData == null || dropInfo.DragData.ReferencedClipVM == null)
                return;

            var clipVM = dropInfo.DragData.ReferencedClipVM;
            var timelineVM = _timelineContext;

            // 移動先のフレームを計算
            int moveFrame = CalculateMoveFrame(dropInfo.DropPositionX, clipVM, dropInfo.DragData.FramePerDIP_AtDragStart);

            // 移動先のレイヤーを取得
            var sourceLayer = FindOwnerLayer(clipVM.TargetObject);
            var targetLayer = timelineVM.Timeline.Layers.FirstOrDefault(l => l == _timelineViewModel.TargetLayer);

            // 移動可能かを確認
            if (sourceLayer != null && targetLayer != null &&
                targetLayer.CanPlaceObjectAt(clipVM.TargetObject, clipVM.TargetObject.StartFrame + moveFrame, clipVM.TargetObject.EndFrame + moveFrame))
            {
                // 移動コマンドを生成して実行
                var moveInfos = new List<ClipMoveInfo>
                {
                    new ClipMoveInfo(
                        clipVM.TargetObject,
                        sourceLayer,
                        targetLayer,
                        clipVM.TargetObject.StartFrame, clipVM.TargetObject.EndFrame,
                        clipVM.TargetObject.StartFrame + moveFrame, clipVM.TargetObject.EndFrame + moveFrame)
                };

                var command = new MoveClipsCommand(moveInfos);
                timelineVM.RunEditCommand(command);
            }
        }

        /// <summary>
        /// クリップの選択状態を更新する
        /// <summary>
        /// Updates the selection state of a clip, supporting both single and multi-select modes.
        /// </summary>
        /// <param name="clipViewModel">The clip to select or deselect.</param>
        /// <param name="isMultiSelect">
        /// If true, toggles the selection of the specified clip without affecting other selections; 
        /// if false, clears existing selections and selects only the specified clip.
        /// </param>
        public void SelectClip(ClipViewModel clipViewModel, bool isMultiSelect = false)
        {
            if (isMultiSelect)
            {
                // 複数選択モード：既に選択されている場合は選択解除、そうでなければ追加
                if (_timelineViewModel.SelectClip.Contains(clipViewModel))
                {
                    _timelineViewModel.SelectClip.Remove(clipViewModel);
                }
                else
                {
                    _timelineViewModel.SelectClip.Add(clipViewModel);
                }
            }
            else
            {
                // 単一選択モード：既存の選択をクリアして新しいクリップを選択
                _timelineViewModel.SelectClip.Clear();
                _timelineViewModel.SelectClip.Add(clipViewModel);
            }
        }

        /// <summary>
        /// クリップのリサイズ可能かを確認する
        /// <summary>
        /// Determines whether the specified clip can be resized to the given start and end frames based on its owner layer's placement rules.
        /// </summary>
        /// <param name="clipObject">The clip object to check for resize eligibility.</param>
        /// <param name="newStartFrame">The proposed new start frame for the clip.</param>
        /// <param name="newEndFrame">The proposed new end frame for the clip.</param>
        /// <returns>True if the clip can be resized to the specified frame range; otherwise, false.</returns>
        public bool CanResizeClip(MetasiaObject clipObject, int newStartFrame, int newEndFrame)
        {
            LayerObject? ownerLayer = FindOwnerLayer(clipObject);

            if (ownerLayer is not null)
            {
                return ownerLayer.CanPlaceObjectAt(clipObject, newStartFrame, newEndFrame);
            }
            return false;
        }

        /// <summary>
        /// 移動先のフレームを計算する
        /// <summary>
        /// Calculates the frame offset for moving a clip based on the drop position and frame-per-DIP ratio.
        /// </summary>
        /// <param name="dropPositionX">The X position on the canvas where the clip is dropped.</param>
        /// <param name="clipVM">The view model of the clip being moved.</param>
        /// <param name="framePerDIP">The number of frames represented by one device-independent pixel.</param>
        /// <returns>The integer frame offset to apply for the move operation.</returns>
        private int CalculateMoveFrame(double dropPositionX, ClipViewModel clipVM, double framePerDIP)
        {
            // 移動先のフレームを計算するロジック
            double frameDelta = (dropPositionX - clipVM.DraggingClipOffsetX) / framePerDIP;
            return (int)Math.Round(frameDelta);
        }

        /// <summary>
        /// クリップが所属するレイヤーを取得する
        /// <summary>
        /// Searches all layers in the timeline and returns the layer that contains the specified object, or null if not found.
        /// </summary>
        /// <param name="targetObject">The object to locate within the timeline layers.</param>
        /// <returns>The layer containing the object, or null if no such layer exists.</returns>
        private LayerObject? FindOwnerLayer(MetasiaObject targetObject)
        {
            foreach (var layer in _timelineViewModel.Timeline.Layers)
            {
                if (layer.Objects.Any(x => x.Id == targetObject.Id))
                {
                    return layer;
                }
            }
            return null;
        }
    }
}

