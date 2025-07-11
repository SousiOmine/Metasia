

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

        public TimelineInteractionService(ITimelineContext timelineContext)
        {
            _timelineContext = timelineContext;
        }

        /// <summary>
        /// クリップのリサイズ操作を処理する
        /// </summary>
        public void ResizeClip(ClipViewModel clipViewModel, string handleName, double pointerPositionXOnCanvas)
        {
            // リサイズ操作のビジネスロジックをここに集約
            clipViewModel.StartDrag(handleName, pointerPositionXOnCanvas);
        }

        /// <summary>
        /// クリップのドラッグ操作を開始する
        /// </summary>
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
        /// </summary>
        public void UpdateClipDrag(ClipViewModel clipViewModel, double pointerPositionXOnCanvas)
        {
            // ドラッグ中のビジネスロジックをここに集約
            // 現在の実装では特に何もしないが、将来的な拡張のためにメソッドを残す
        }

        /// <summary>
        /// クリップのドラッグ操作を終了する
        /// </summary>
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
        /// </summary>
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
        /// </summary>
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
        /// </summary>
        private int CalculateMoveFrame(double dropPositionX, ClipViewModel clipVM, double framePerDIP)
        {
            // 移動先のフレームを計算するロジック
            double frameDelta = (dropPositionX - clipVM.DraggingClipOffsetX) / framePerDIP;
            return (int)Math.Round(frameDelta);
        }

        /// <summary>
        /// クリップが所属するレイヤーを取得する
        /// </summary>
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

