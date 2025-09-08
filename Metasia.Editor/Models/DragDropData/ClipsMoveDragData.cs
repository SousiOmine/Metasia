using System.Collections.Generic;
using Metasia.Editor.ViewModels.Timeline;


namespace Metasia.Editor.Models.DragDropData;

/// <summary>
/// 複数のクリップをドラッグ&ドロップするためのデータ
/// </summary>
public class ClipsMoveDragData
{

    public ClipViewModel ReferencedClipVM { get; }

    /// <summary>
    /// ドラッグイベント呼び出し元のクリップを掴むマウスのクリップから見た相対フレーム
    /// </summary>
    public int DraggingFrameOffsetX { get; }


    public ClipsMoveDragData(ClipViewModel clipVM, int draggingFrameOffsetX)
    {
        ReferencedClipVM = clipVM;
        DraggingFrameOffsetX = draggingFrameOffsetX;
    }
}