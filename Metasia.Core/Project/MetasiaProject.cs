using Metasia.Core.Objects;

namespace Metasia.Core.Project
{
    /// <summary>
    /// Metasiaのプロジェクトに含まれるタイムラインオブジェクトを格納するクラス
    /// </summary>
    public class MetasiaProject
    {
        public ProjectInfo Info;

        public string RootTimelineId = "RootTimeline";

        public List<TimelineObject> Timelines = new();

		public MetasiaProject(ProjectInfo info)
        {
            Info = info;
        }
    }
}
