using Avalonia;
using Avalonia.Media;
using Metasia.Core.Objects;
using Metasia.Editor.Models;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Interactor;
using Metasia.Editor.Models.States;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Metasia.Editor.ViewModels.Timeline
{
    public class ClipViewModel : ViewModelBase
    {
        public ClipObject TargetObject
        {
            get;
            set;
        }

        public bool IsSelecting
        {
            get => isSelecting;
            set => this.RaiseAndSetIfChanged(ref isSelecting, value);
        }
        public double Width
        {
            get => width;
            set => this.RaiseAndSetIfChanged(ref width, value);
        }

        public double Frame_Per_DIP
        {
            get => _frame_per_DIP;
            private set
            {
                this.RaiseAndSetIfChanged(ref _frame_per_DIP, value);
                RecalculateSize();
            }
        }

        public double StartFrame
        {
            get => startFrame;
            set => this.RaiseAndSetIfChanged(ref startFrame, value);
        }

        private double width;
        private double _frame_per_DIP;
        private double startFrame;
        private bool isSelecting;

        /// <summary>
        /// ドラッグ中かどうかを示すフラグ
        /// </summary>
        private bool _isDragging = false;

        /// <summary>
        /// ドラッグされているのが始端か終端かを示す
        /// </summary>
        private string _dragHandleName = string.Empty;  // "StartHandle" または "EndHandle"

        /// <summary>
        /// ドラッグ開始時のポインタ位置
        /// </summary>
        private double _dragStartX = 0;

        private int _originalStartFrame;
        private int _originalEndFrame;

        /// <summary>
        /// ロールエディット時の隣接クリップ（null なら通常リサイズ）
        /// </summary>
        private ClipObject? _adjacentClip;
        private int _adjacentOriginalStartFrame;
        private int _adjacentOriginalEndFrame;

        private TimelineViewModel parentTimeline;
        private readonly IEditCommandManager editCommandManager;
        private readonly ITimelineViewState _timelineViewState;
        private readonly IClipColorProvider _clipColorProvider;
        public ICommand RemoveClipCommand { get; }
        public ICommand SplitClipCommand { get; }

        public IBrush ClipColor => _clipColorProvider.GetBrush(TargetObject);

        public ClipViewModel(ClipObject targetObject, TimelineViewModel parentTimeline, IEditCommandManager editCommandManager, ITimelineViewState timelineViewState, IClipColorProvider clipColorProvider)
        {
            TargetObject = targetObject;
            this.parentTimeline = parentTimeline;
            IsSelecting = false;
            this.editCommandManager = editCommandManager;
            this._timelineViewState = timelineViewState;
            this._clipColorProvider = clipColorProvider;
            // 削除コマンドの初期化
            RemoveClipCommand = ReactiveCommand.Create(() => parentTimeline.ClipRemove(TargetObject));

            // 分割コマンドの初期化
            SplitClipCommand = ReactiveCommand.Create(() => parentTimeline.SplitSelectedClips());

            _timelineViewState.Frame_Per_DIP_Changed += () =>
            {
                Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;
            };
            Frame_Per_DIP = _timelineViewState.Frame_Per_DIP;
        }

        /// <summary>
        /// クリップのサイズを再計算する
        /// </summary>
        public void RecalculateSize()
        {
            Width = (TargetObject.EndFrame - TargetObject.StartFrame + 1) * _timelineViewState.Frame_Per_DIP;
            StartFrame = TargetObject.StartFrame * _timelineViewState.Frame_Per_DIP;
        }

        public void ClipClick(bool isMultiSelect, int targetFrame = -1)
        {
            // まず通常の選択処理を実行
            parentTimeline.ClipSelect(TargetObject, isMultiSelect);

            if (targetFrame >= 0)
            {
                // 次にプレビュー位置を移動
                parentTimeline.SeekFrame(targetFrame);
            }
        }

        /// <summary>
        /// ドラッグ開始時の処理
        /// </summary>
        /// <param name="handleName">StartHandle あるいは EndHandle</param>
        /// <param name="pointerPositionXOnCanvas">ポインタの初期位置</param>
        public void StartDrag(string handleName, double pointerPositionXOnCanvas)
        {
            _isDragging = true;
            _dragHandleName = handleName;
            _dragStartX = pointerPositionXOnCanvas;

            _originalStartFrame = TargetObject.StartFrame;
            _originalEndFrame = TargetObject.EndFrame;

            // 隣接クリップの検出（ロールエディット判定）
            var ownerLayer = ClipInteractor.FindOwnerLayer(parentTimeline.Timeline, TargetObject);
            _adjacentClip = ownerLayer != null
                ? ClipInteractor.FindAdjacentClip(TargetObject, handleName, ownerLayer)
                : null;
            if (_adjacentClip != null)
            {
                _adjacentOriginalStartFrame = _adjacentClip.StartFrame;
                _adjacentOriginalEndFrame = _adjacentClip.EndFrame;
            }
        }

        public void UpdateDrag(double pointerPositionXOnCanvas)
        {
            if (!_isDragging) return;

            double deltaPixels = pointerPositionXOnCanvas - _dragStartX;
            var ownerLayer = ClipInteractor.FindOwnerLayer(parentTimeline.Timeline, TargetObject);

            if (_adjacentClip != null && ownerLayer is not null)
            {
                // ロールエディットモード
                var (newStart, newEnd, adjNewStart, adjNewEnd) = ClipInteractor.ComputeRollEditFrames(
                    TargetObject, _adjacentClip, _dragHandleName,
                    _originalStartFrame, _originalEndFrame,
                    _adjacentOriginalStartFrame, _adjacentOriginalEndFrame,
                    deltaPixels, _timelineViewState.Frame_Per_DIP);

                if (ClipInteractor.CanRollEdit(TargetObject, newStart, newEnd, _adjacentClip, adjNewStart, adjNewEnd, ownerLayer))
                {
                    var command = ClipInteractor.CreateRollEditCommand(
                        TargetObject,
                        _originalStartFrame, newStart,
                        _originalEndFrame, newEnd,
                        _adjacentClip,
                        _adjacentOriginalStartFrame, adjNewStart,
                        _adjacentOriginalEndFrame, adjNewEnd);
                    editCommandManager.PreviewExecute(command);
                }
                else
                {
                    editCommandManager.CancelPreview();
                }
            }
            else
            {
                // 通常リサイズモード
                var (newStart, newEnd) = ClipInteractor.ApplyResizeSnapping(
                    TargetObject,
                    _dragHandleName,
                    _originalStartFrame,
                    _originalEndFrame,
                    deltaPixels,
                    parentTimeline.Timeline,
                    _timelineViewState.Frame_Per_DIP);

                if (ownerLayer is not null && ClipInteractor.CanResize(TargetObject, newStart, newEnd, ownerLayer))
                {
                    var command = ClipInteractor.CreateResizeCommand(
                        TargetObject,
                        _originalStartFrame, newStart,
                        _originalEndFrame, newEnd);
                    editCommandManager.PreviewExecute(command);
                }
                else
                {
                    editCommandManager.CancelPreview();
                }
            }
        }

        /// <summary>
        /// ドラッグ終了時の処理
        /// </summary>
        /// <param name="pointerPositionXOnCanvas">ポインタの最後の位置</param>
        public void EndDrag(double pointerPositionXOnCanvas)
        {
            if (!_isDragging || string.IsNullOrEmpty(_dragHandleName))
            {
                return;
            }

            double deltaPixels = pointerPositionXOnCanvas - _dragStartX;
            var ownerLayer = ClipInteractor.FindOwnerLayer(parentTimeline.Timeline, TargetObject);

            if (_adjacentClip != null && ownerLayer is not null)
            {
                // ロールエディットモード
                var (newStart, newEnd, adjNewStart, adjNewEnd) = ClipInteractor.ComputeRollEditFrames(
                    TargetObject, _adjacentClip, _dragHandleName,
                    _originalStartFrame, _originalEndFrame,
                    _adjacentOriginalStartFrame, _adjacentOriginalEndFrame,
                    deltaPixels, _timelineViewState.Frame_Per_DIP);

                bool canRollEdit = ClipInteractor.CanRollEdit(
                    TargetObject, newStart, newEnd,
                    _adjacentClip, adjNewStart, adjNewEnd, ownerLayer);

                if (canRollEdit)
                {
                    bool changed = newStart != _originalStartFrame || newEnd != _originalEndFrame
                                || adjNewStart != _adjacentOriginalStartFrame || adjNewEnd != _adjacentOriginalEndFrame;
                    if (changed)
                    {
                        var command = ClipInteractor.CreateRollEditCommand(
                            TargetObject,
                            _originalStartFrame, newStart,
                            _originalEndFrame, newEnd,
                            _adjacentClip,
                            _adjacentOriginalStartFrame, adjNewStart,
                            _adjacentOriginalEndFrame, adjNewEnd);
                        editCommandManager.Execute(command);

                        RecalculateSize();
                        // 隣接クリップのVMもサイズ再計算が必要
                        parentTimeline.RecalculateClipSize(_adjacentClip);
                    }
                    else
                    {
                        editCommandManager.CancelPreview();
                    }
                }
                else
                {
                    editCommandManager.CancelPreview();
                    RecalculateSize();
                    parentTimeline.RecalculateClipSize(_adjacentClip);
                }
            }
            else
            {
                // 通常リサイズモード
                var (newStart, newEnd) = ClipInteractor.ApplyResizeSnapping(
                    TargetObject,
                    _dragHandleName,
                    _originalStartFrame,
                    _originalEndFrame,
                    deltaPixels,
                    parentTimeline.Timeline,
                    _timelineViewState.Frame_Per_DIP);

                bool canResize = ownerLayer is not null &&
                                ClipInteractor.CanResize(TargetObject, newStart, newEnd, ownerLayer);

                if (canResize)
                {
                    if (newStart != _originalStartFrame || newEnd != _originalEndFrame)
                    {
                        var command = ClipInteractor.CreateResizeCommand(
                            TargetObject,
                            _originalStartFrame, newStart,
                            _originalEndFrame, newEnd);
                        editCommandManager.Execute(command);

                        RecalculateSize();
                    }
                    else
                    {
                        editCommandManager.CancelPreview();
                    }
                }
                else
                {
                    editCommandManager.CancelPreview();
                    RecalculateSize();
                }
            }

            _isDragging = false;
            _dragHandleName = string.Empty;
            _adjacentClip = null;
        }
    }
}

