using Metasia.Editor.ViewModels.Timeline;

namespace Metasia.Editor.Models.DragDropData
{
    /// <summary>
    /// ドロップ操作の情報を抽象化したモデル
    /// </summary>
    public class ClipsDropTargetContext
    {

        public ClipViewModel ReferenceClipVM { get; set; }

        /// <summary>
        /// ドラッグイベント呼び出し元のクリップを掴むマウスのクリップから見た相対フレーム
        /// </summary>
        public int DraggingFrameOffsetX { get; set; }

        /// <summary>
        /// レイヤーキャンバスにおけるドロップ位置のフレーム
        /// </summary>
        public int DropPositionFrame { get; set; }

        /// <summary>
        /// ドロップ可能かどうか
        /// </summary>
        public bool CanDrop { get; set; }

        public ClipsDropTargetContext(ClipsMoveDragData dragData, int dropPositionFrame, bool canDrop)
        {
            ReferenceClipVM = dragData.ReferencedClipVM;
            DraggingFrameOffsetX = dragData.DraggingFrameOffsetX;
            DropPositionFrame = dropPositionFrame;
            CanDrop = canDrop;
        }

        public ClipsDropTargetContext(ClipViewModel clipVM, int draggingFrameOffsetX, int dropPositionFrame, bool canDrop)
        {
            ReferenceClipVM = clipVM;
            DraggingFrameOffsetX = draggingFrameOffsetX;
            DropPositionFrame = dropPositionFrame;
            CanDrop = canDrop;
        }
    }
}