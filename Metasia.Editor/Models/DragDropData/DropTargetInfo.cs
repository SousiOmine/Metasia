namespace Metasia.Editor.Models.DragDropData
{
    /// <summary>
    /// ドロップ操作の情報を抽象化したモデル
    /// </summary>
    public class DropTargetInfo
    {
        /// <summary>
        /// ドロップされたクリップ
        /// </summary>
        public ClipMoveDragData? DragData { get; set; }
        
        /// <summary>
        /// ドロップ位置のX座標（論理座標）
        /// </summary>
        public double DropPositionX { get; set; }
        
        /// <summary>
        /// ドロップ可能かどうか
        /// </summary>
        public bool CanDrop { get; set; }
    }
} 