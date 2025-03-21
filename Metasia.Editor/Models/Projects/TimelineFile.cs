using Metasia.Core.Objects;
using Metasia.Editor.Models.FileSystem;

namespace Metasia.Editor.Models.Projects;

public class TimelineFile
{
    public FileEntity TimelineFilePath { get; set; }

    public TimelineObject Timeline { get; set; }

    public TimelineFile(FileEntity timelineFilePath, TimelineObject timeline)
    {
        TimelineFilePath = timelineFilePath;
        Timeline = timeline;
    }
}
