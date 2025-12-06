using Avalonia;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
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

        private TimelineViewModel parentTimeline;
        private readonly IEditCommandManager editCommandManager;
        private readonly ITimelineViewState _timelineViewState;
        public ICommand RemoveClipCommand { get; }
        public ICommand SplitClipCommand { get; }

        public ClipViewModel(ClipObject targetObject, TimelineViewModel parentTimeline, IEditCommandManager editCommandManager, ITimelineViewState timelineViewState)
        {
            TargetObject = targetObject;
            this.parentTimeline = parentTimeline;
            IsSelecting = false;
            this.editCommandManager = editCommandManager;
            this._timelineViewState = timelineViewState;
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
        }

        public void UpdateDrag(double pointerPositionXOnCanvas)
        {
            if (!_isDragging) return;

            CalculateNewFrames(pointerPositionXOnCanvas, out int newStart, out int newEnd);

            if (parentTimeline.CanResizeClip(TargetObject, newStart, newEnd))
            {
                var command = new ClipResizeCommand(
                    TargetObject,
                    _originalStartFrame, newStart,
                    _originalEndFrame, newEnd
                );
                editCommandManager.PreviewExecute(command);
            }
            else
            {
                editCommandManager.CancelPreview();
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

            CalculateNewFrames(pointerPositionXOnCanvas, out int newStart, out int newEnd);

            // 希望のフレームのままリサイズできるならばリサイズ実行
            if (parentTimeline.CanResizeClip(TargetObject, newStart, newEnd))
            {
                // フレームが変化していればコマンドを実行
                if (newStart != _originalStartFrame || newEnd != _originalEndFrame)
                {
                    IEditCommand command = new ClipResizeCommand(
                        TargetObject,
                        _originalStartFrame, newStart,
                        _originalEndFrame, newEnd
                    );
                    editCommandManager.Execute(command);

                    RecalculateSize();
                }
                else
                {
                    // 変化なしの場合はプレビューをキャンセル
                    editCommandManager.CancelPreview();
                }
            }
            else
            {
                // ドラッグしたそのままのフレームでは重複でリサイズできない場合
                // プレビューをキャンセルして元に戻す
                editCommandManager.CancelPreview();

                // キャンセルした状態をUIに反映
                RecalculateSize();

                // TODO: ここで「重複しないぎりぎりまで詰める」処理を追加可能
            }

            _isDragging = false;
            _dragHandleName = string.Empty;
        }

        private void CalculateNewFrames(double pointerPositionXOnCanvas, out int newStart, out int newEnd)
        {
            double deltaX = pointerPositionXOnCanvas - _dragStartX;
            double frameDelta = deltaX / _timelineViewState.Frame_Per_DIP;
            int frameChange = (int)Math.Round(frameDelta);

            newStart = _originalStartFrame;
            newEnd = _originalEndFrame;

            if (_dragHandleName == "StartHandle")
            {
                newStart = _originalStartFrame + frameChange;
                // 終端を超えないように、かつ長さが1未満にならないように制限
                newStart = Math.Min(newStart, _originalEndFrame - 1);
                newStart = Math.Max(newStart, 0);
            }
            else if (_dragHandleName == "EndHandle")
            {
                newEnd = _originalEndFrame + frameChange;
                // 始端を下回らないように、かつ長さが1未満にならないように制限
                newEnd = Math.Max(newEnd, _originalStartFrame + 1);
            }
        }
    }
}
