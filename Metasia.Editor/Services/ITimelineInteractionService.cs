using Metasia.Core.Objects;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.ViewModels.Controls;
using System.Collections.Generic;

namespace Metasia.Editor.Services
{
    /// <summary>
    /// タイムライン上のインタラクションを専門に扱うサービスインターフェース
    /// </summary>
    public interface ITimelineInteractionService
    {
        /// <summary>
        /// クリップのリサイズ操作を処理する
        /// </summary>
        /// <param name="clipViewModel">リサイズ対象のクリップ</param>
        /// <param name="handleName">リサイズハンドル名 ("StartHandle" または "EndHandle")</param>
        /// <summary>
/// Resizes the specified clip using the given handle and pointer X position on the canvas.
/// </summary>
/// <param name="clipViewModel">The clip to be resized.</param>
/// <param name="handleName">The name of the resize handle ("StartHandle" or "EndHandle").</param>
/// <param name="pointerPositionXOnCanvas">The X position of the pointer on the canvas.</param>
        void ResizeClip(ClipViewModel clipViewModel, string handleName, double pointerPositionXOnCanvas);

        /// <summary>
        /// クリップのドラッグ操作を開始する
        /// </summary>
        /// <param name="clipViewModel">ドラッグ対象のクリップ</param>
        /// <param name="handleName">ドラッグハンドル名 ("StartHandle" または "EndHandle")</param>
        /// <summary>
/// Initiates a drag operation for the specified clip using the given handle and pointer position.
/// </summary>
/// <param name="clipViewModel">The clip to begin dragging.</param>
/// <param name="handleName">The name of the handle used to start the drag operation.</param>
/// <param name="pointerPositionXOnCanvas">The initial X position of the pointer on the canvas.</param>
        void StartClipDrag(ClipViewModel clipViewModel, string handleName, double pointerPositionXOnCanvas);

        /// <summary>
        /// クリップのドラッグ操作を更新する
        /// </summary>
        /// <param name="clipViewModel">ドラッグ対象のクリップ</param>
        /// <summary>
/// Updates the position of a clip during a drag operation based on the current pointer X position on the canvas.
/// </summary>
/// <param name="clipViewModel">The clip being dragged.</param>
/// <param name="pointerPositionXOnCanvas">The current X position of the pointer on the canvas.</param>
        void UpdateClipDrag(ClipViewModel clipViewModel, double pointerPositionXOnCanvas);

        /// <summary>
        /// クリップのドラッグ操作を終了する
        /// </summary>
        /// <param name="clipViewModel">ドラッグ対象のクリップ</param>
        /// <summary>
/// Completes the drag operation for a clip at the specified pointer X position on the canvas.
/// </summary>
/// <param name="clipViewModel">The clip being dragged.</param>
/// <param name="pointerPositionXOnCanvas">The final X position of the pointer on the canvas.</param>
        void EndClipDrag(ClipViewModel clipViewModel, double pointerPositionXOnCanvas);

        /// <summary>
        /// クリップの移動操作を処理する
        /// </summary>
        /// <summary>
/// Moves clips to a new location on the timeline based on the provided drop target information.
/// </summary>
/// <param name="dropInfo">Information describing the drop target and the clips to be moved.</param>
        void MoveClips(ClipsDropTargetInfo dropInfo);

        /// <summary>
        /// クリップの選択状態を更新する
        /// </summary>
        /// <param name="clipViewModel">選択対象のクリップ</param>
        /// <summary>
/// Updates the selection state of a clip, optionally enabling multi-selection mode.
/// </summary>
/// <param name="isMultiSelect">If true, enables multi-selection mode when selecting the clip.</param>
        void SelectClip(ClipViewModel clipViewModel, bool isMultiSelect = false);

        /// <summary>
        /// クリップのリサイズ可能かを確認する
        /// </summary>
        /// <param name="clipObject">リサイズ対象のクリップ</param>
        /// <param name="newStartFrame">新しい開始フレーム</param>
        /// <param name="newEndFrame">新しい終了フレーム</param>
        /// <summary>
/// Determines whether the specified clip can be resized to the given start and end frames.
/// </summary>
/// <param name="clipObject">The clip object to evaluate for resizing.</param>
/// <param name="newStartFrame">The proposed new start frame for the clip.</param>
/// <param name="newEndFrame">The proposed new end frame for the clip.</param>
/// <returns>True if the clip can be resized to the specified frames; otherwise, false.</returns>
        bool CanResizeClip(MetasiaObject clipObject, int newStartFrame, int newEndFrame);
    }
}
