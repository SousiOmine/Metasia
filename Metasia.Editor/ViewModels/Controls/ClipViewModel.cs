using Avalonia;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Metasia.Editor.ViewModels.Controls
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
            set 
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

        /// <summary>
        /// ドラッグ開始時の始端あるいは終端のフレーム
        /// </summary>
        private int _initialDragFrame = 0;

        private TimelineViewModel parentTimeline;

        public ICommand RemoveClipCommand { get; }

        public ClipViewModel(ClipObject targetObject, TimelineViewModel parentTimeline)
        {
            TargetObject = targetObject;
            this.parentTimeline = parentTimeline;
            IsSelecting = false;

            // 削除コマンドの初期化
            RemoveClipCommand = ReactiveCommand.Create(() => parentTimeline.ClipRemove(TargetObject));
        }

        /// <summary>
        /// クリップのサイズを再計算する
        /// </summary>
        public void RecalculateSize()
        {
            Width = (TargetObject.EndFrame - TargetObject.StartFrame + 1) * Frame_Per_DIP;
            StartFrame = TargetObject.StartFrame * Frame_Per_DIP;
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
            _initialDragFrame = (handleName == "StartHandle") ? TargetObject.StartFrame : TargetObject.EndFrame;

            Console.WriteLine(pointerPositionXOnCanvas);
        }

        public void UpdateDrag(double pointerPositionXOnCanvas)
        {
            //ドラッグ中になにかするならここに書く
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

            double deltaX = pointerPositionXOnCanvas - _dragStartX;
            double frameDelta = deltaX / Frame_Per_DIP;
            int frameChange = (int)Math.Round(frameDelta);

            int newStartFrame = TargetObject.StartFrame;
            int newEndFrame = TargetObject.EndFrame;

            if (_dragHandleName == "StartHandle")
            {
                newStartFrame = _initialDragFrame + frameChange;
                // 終端を超えないように、かつ長さが1未満にならないように制限
                newStartFrame = Math.Min(newStartFrame, TargetObject.EndFrame - 1);
                newStartFrame = Math.Max(newStartFrame, 0);
            }
            else if (_dragHandleName == "EndHandle")
            {
                newEndFrame = _initialDragFrame + frameChange;
                // 始端を下回らないように、かつ長さが1未満にならないように制限
                newEndFrame = Math.Max(newEndFrame, TargetObject.StartFrame + 1);
            }

            // 希望のフレームのままリサイズできるならばリサイズ実行
            if (parentTimeline.CanResizeClip(TargetObject, newStartFrame, newEndFrame))
            {
                // フレームが変化していればコマンドを実行
                if (newStartFrame != TargetObject.StartFrame || newEndFrame != TargetObject.EndFrame)
                {
                    IEditCommand command = new ClipResizeCommand(
                        TargetObject,
                        TargetObject.StartFrame, newStartFrame,
                        TargetObject.EndFrame, newEndFrame
                    );
                    parentTimeline.RunEditCommand(command);

                    RecalculateSize();
                }
            }
            else
            {
                // ドラッグしたそのままのフレームでは重複でリサイズできない場合、重複しないぎりぎりまで詰める
            }

            _isDragging = false;
            _dragHandleName = string.Empty;

            Console.WriteLine(pointerPositionXOnCanvas);
        }
    }
}
