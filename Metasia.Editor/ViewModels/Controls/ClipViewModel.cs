using Avalonia;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;

using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Services;

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

        // Drag state is now handled by the TimelineInteractionService

        /// <summary>
        /// ドラッグ中かどうかを示すフラグ
        /// </summary>
        public bool IsDragging { get; set; }

        /// <summary>
        /// ドラッグされているのが始端か終端かを示す
        /// </summary>
        public string DragHandleName { get; set; }

        /// <summary>
        /// ドラッグ開始時のポインタ位置
        /// </summary>
        public double DragStartX { get; set; }

        /// <summary>
        /// ドラッグ開始時の始端あるいは終端のフレーム
        /// </summary>
        public int InitialDragFrame { get; set; }


        private TimelineViewModel parentTimeline;
        private ITimelineInteractionService timelineInteractionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClipViewModel"/> class for a given clip object, associating it with a parent timeline and an interaction service.
        /// </summary>
        /// <param name="targetObject">The underlying data object representing the clip.</param>
        /// <param name="parentTimeline">The parent timeline view model containing this clip.</param>
        /// <param name="interactionService">The service responsible for handling timeline interactions such as selection and dragging.</param>
        public ClipViewModel(MetasiaObject targetObject, TimelineViewModel parentTimeline, ITimelineInteractionService interactionService)
        {
            TargetObject = targetObject;
            this.parentTimeline = parentTimeline;
            this.timelineInteractionService = interactionService;
            IsSelecting = false;
        }

        /// <summary>
        /// クリップのサイズを再計算する
        /// </summary>
        public void RecalculateSize()
        {
            Width = (TargetObject.EndFrame - TargetObject.StartFrame + 1) * Frame_Per_DIP;
            StartFrame = TargetObject.StartFrame * Frame_Per_DIP;
        }

        /// <summary>
        /// Selects this clip, optionally enabling multi-selection.
        /// </summary>
        /// <param name="isMultiSelect">If true, adds this clip to the current selection; otherwise, selects only this clip.</param>
        public void ClipClick(bool isMultiSelect = false)
        {
            timelineInteractionService.SelectClip(this, isMultiSelect);
        }

        /// <summary>
        /// ドラッグ開始時の処理
        /// </summary>
        /// <param name="handleName">StartHandle あるいは EndHandle</param>
        /// <summary>
        /// Initiates a drag operation for the clip using the specified handle and pointer position.
        /// </summary>
        /// <param name="handleName">The name of the handle being dragged (e.g., start or end).</param>
        /// <param name="pointerPositionXOnCanvas">The initial X position of the pointer on the canvas.</param>
        public void StartDrag(string handleName, double pointerPositionXOnCanvas)
        {
            timelineInteractionService.StartClipDrag(this, handleName, pointerPositionXOnCanvas);
        }

        /// <summary>
        /// Updates the clip's position during a drag operation based on the current pointer X position.
        /// </summary>
        /// <param name="pointerPositionXOnCanvas">The current X coordinate of the pointer on the canvas.</param>
        public void UpdateDrag(double pointerPositionXOnCanvas)
        {
            timelineInteractionService.UpdateClipDrag(this, pointerPositionXOnCanvas);
        }

        /// <summary>
        /// ドラッグ終了時の処理
        /// </summary>
        /// <summary>
        /// Finalizes the drag operation for this clip at the specified pointer position.
        /// </summary>
        /// <param name="pointerPositionXOnCanvas">The X position of the pointer on the canvas when the drag ends.</param>
        public void EndDrag(double pointerPositionXOnCanvas)
        {
            timelineInteractionService.EndClipDrag(this, pointerPositionXOnCanvas);
        }
    }
}
