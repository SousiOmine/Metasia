using Metasia.Core.Objects;
using Metasia.Editor.Models.FileSystem;

namespace Metasia.Editor.Models.Projects;

/// <summary>
/// タイムラインを記録する.mttlファイルの情報を扱う
/// </summary>
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
