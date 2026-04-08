using Metasia.Core.Media;

namespace Metasia.Editor.Models.DragDropData;

public class ProjectFileDropData
{
    public MediaPath MediaPath { get; }

    public ProjectFileDropData(MediaPath mediaPath)
    {
        MediaPath = mediaPath;
    }
}