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

namespace Metasia.Editor.ViewModels.Controls
{
    public class ClipViewModel : ViewModelBase
    {
        public MetasiaObject TargetObject
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
                ChangeFramePerDIP();
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

        private bool _isDragging = false;
        private string _dragHandleName = string.Empty;  // "StartHandle" または "EndHandle"
        private double _dragStartX = 0;
        private int _initialDragFrame = 0; // ドラッグ開始時の始端あるいは終端のフレーム

        private TimelineViewModel parentTimeline;

        public ClipViewModel(MetasiaObject targetObject, TimelineViewModel parentTimeline)
        {
            TargetObject = targetObject;
            this.parentTimeline = parentTimeline;
            IsSelecting = false;
        }

        public void ClipClick()
        {
            parentTimeline.ClipSelect(this);
        }

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

        public void EndDrag(double pointerPositionXOnCanvas)
        {
            if (!_isDragging || string.IsNullOrEmpty(_dragHandleName))
            {
                return;
            }

            double deltaX = pointerPositionXOnCanvas - _dragStartX;
            double frameDelta = deltaX / Frame_Per_DIP;
            int frameChange = (int)Math.Round(frameDelta);

            int finalNewFrame = _initialDragFrame + frameChange;

            if (_dragHandleName == "StartHandle")
            {
                finalNewFrame = Math.Min(finalNewFrame, TargetObject.EndFrame - 1);
            }
            else
            {
                finalNewFrame = Math.Max(finalNewFrame, TargetObject.StartFrame + 1);
            }

            if (finalNewFrame != _initialDragFrame)
            {
                IEditCommand command;
                if (_dragHandleName == "StartHandle")
                {
                    command = new ClipResizeCommand(TargetObject, TargetObject.StartFrame, finalNewFrame, TargetObject.EndFrame, TargetObject.EndFrame);
                }
                else
                {
                    command = new ClipResizeCommand(TargetObject, TargetObject.StartFrame, TargetObject.StartFrame, TargetObject.EndFrame, finalNewFrame);
                }
                parentTimeline.RunEditCommand(command);

                ChangeFramePerDIP();
            }

            _isDragging = false;
            _dragHandleName = string.Empty;

            Console.WriteLine(pointerPositionXOnCanvas);
        }

        private void ChangeFramePerDIP()
        {
            Width = (TargetObject.EndFrame - TargetObject.StartFrame + 1) * Frame_Per_DIP;
            StartFrame = TargetObject.StartFrame * Frame_Per_DIP;
        }
    }
}
