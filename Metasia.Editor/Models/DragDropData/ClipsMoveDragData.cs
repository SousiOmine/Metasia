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

    /// <summary>
    /// Initializes a new instance of the <see cref="ClipsMoveDragData"/> class with the specified clip view model, horizontal drag offset, and frame-to-DIP ratio at the start of the drag operation.
    /// </summary>
    /// <param name="clipVM">The clip view model that initiated the drag event.</param>
    /// <param name="draggingClipsOffsetX">The horizontal mouse offset from the left edge of the clip at the start of the drag.</param>
    /// <param name="framePerDIP_AtDragStart">The frame-to-device-independent-pixel ratio at the moment the drag started.</param>
    public ClipsMoveDragData(ClipViewModel clipVM, double draggingClipsOffsetX, double framePerDIP_AtDragStart)
    {
        ReferencedClipVM = clipVM;
        DraggingClipOffsetX = draggingClipsOffsetX;
        FramePerDIP_AtDragStart = framePerDIP_AtDragStart;
    }
}