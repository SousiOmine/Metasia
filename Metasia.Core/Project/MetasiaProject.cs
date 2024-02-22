using Metasia.Core.Objects;

namespace Metasia.Core.Project
{
    /// <summary>
    /// Metasiaのプロジェクトに含まれるタイムラインオブジェクトを格納するクラス
    /// </summary>
    public class MetasiaProject
    {
        public ProjectInfo Info;

        public List<ListObject> Timelines = new List<ListObject>();

        public MetasiaProject(ProjectInfo info)
        {
            Info = info;
        }
    }
}
