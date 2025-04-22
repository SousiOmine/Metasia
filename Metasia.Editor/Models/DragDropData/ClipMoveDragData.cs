using Metasia.Editor.ViewModels.Controls;

namespace Metasia.Editor.Models.DragDropData;

public class ClipMoveDragData
{
    public ClipViewModel ClipVM { get; }
    public double DraggingOffsetX { get; }
    
    public ClipMoveDragData(ClipViewModel clipVm, double draggingOffsetX)
    {
        DraggingOffsetX = draggingOffsetX;
        ClipVM = clipVm;
    }
}