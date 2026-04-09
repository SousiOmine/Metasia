using Metasia.Editor.Models.States;

namespace Metasia.Editor.Services;

/// <summary>
/// プロジェクト単位のタイムライン表示状態を永続化するためのリポジトリです。
/// </summary>
public interface IProjectTimelineViewStateRepository
{
    ProjectTimelineViewStateSnapshot? Load(string projectFilePath);

    void Save(ProjectTimelineViewStateSnapshot snapshot);
}
