
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
        /// <param name="pointerPositionXOnCanvas">ポインタの位置</param>
        void ResizeClip(ClipViewModel clipViewModel, string handleName, double pointerPositionXOnCanvas);

        /// <summary>
        /// クリップのドラッグ操作を開始する
        /// </summary>
        /// <param name="clipViewModel">ドラッグ対象のクリップ</param>
        /// <param name="handleName">ドラッグハンドル名 ("StartHandle" または "EndHandle")</param>
        /// <param name="pointerPositionXOnCanvas">ポインタの初期位置</param>
        void StartClipDrag(ClipViewModel clipViewModel, string handleName, double pointerPositionXOnCanvas);

        /// <summary>
        /// クリップのドラッグ操作を更新する
        /// </summary>
        /// <param name="clipViewModel">ドラッグ対象のクリップ</param>
        /// <param name="pointerPositionXOnCanvas">ポインタの現在位置</param>
        void UpdateClipDrag(ClipViewModel clipViewModel, double pointerPositionXOnCanvas);

        /// <summary>
        /// クリップのドラッグ操作を終了する
        /// </summary>
        /// <param name="clipViewModel">ドラッグ対象のクリップ</param>
        /// <param name="pointerPositionXOnCanvas">ポインタの最終位置</param>
        void EndClipDrag(ClipViewModel clipViewModel, double pointerPositionXOnCanvas);

        /// <summary>
        /// クリップの移動操作を処理する
        /// </summary>
        /// <param name="dropInfo">ドロップ情報</param>
        void MoveClips(ClipsDropTargetInfo dropInfo);

        /// <summary>
        /// クリップの選択状態を更新する
        /// </summary>
        /// <param name="clipViewModel">選択対象のクリップ</param>
        /// <param name="isMultiSelect">複数選択モードかどうか</param>
        void SelectClip(ClipViewModel clipViewModel, bool isMultiSelect = false);

        /// <summary>
        /// クリップのリサイズ可能かを確認する
        /// </summary>
        /// <param name="clipObject">リサイズ対象のクリップ</param>
        /// <param name="newStartFrame">新しい開始フレーム</param>
        /// <param name="newEndFrame">新しい終了フレーム</param>
        /// <returns>リサイズ可能かどうか</returns>
        bool CanResizeClip(MetasiaObject clipObject, int newStartFrame, int newEndFrame);
    }
}
