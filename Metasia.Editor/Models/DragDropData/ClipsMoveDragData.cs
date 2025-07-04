using System.Collections.Generic;
using Metasia.Editor.ViewModels.Controls;


namespace Metasia.Editor.Models.DragDropData;

/// <summary>
/// 複数のクリップをドラッグ&ドロップするためのデータ
/// </summary>
public class ClipsMoveDragData
{

    public ClipViewModel ReferencedClipVM { get; }

    /// <summary>
    /// ドラッグイベント呼び出し元のクリップを掴むマウスX座標(クリップ左端から見たX座標)
    /// </summary>
    public double DraggingClipOffsetX { get; }

    public double FramePerDIP_AtDragStart { get; }

    public ClipsMoveDragData(ClipViewModel clipVM, double draggingClipsOffsetX, double framePerDIP_AtDragStart)
    {
        ReferencedClipVM = clipVM;
        DraggingClipOffsetX = draggingClipsOffsetX;
        FramePerDIP_AtDragStart = framePerDIP_AtDragStart;
    }
}