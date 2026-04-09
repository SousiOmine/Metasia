namespace Metasia.Editor.Abstractions.States;

public interface ITimelineViewStateStore
{
    /// <summary>
    /// 指定したタイムラインのViewStateを取得
    /// 存在しない場合は新規作成して返す
    /// </summary>
    ITimelineViewState GetViewState(string timelineId);

    /// <summary>
    /// 指定したタイムラインのViewStateが存在するか確認
    /// </summary>
    bool Contains(string timelineId);

    /// <summary>
    /// 指定したタイムラインのViewStateを削除
    /// </summary>
    void Remove(string timelineId);

    /// <summary>
    /// すべてのタイムラインのViewStateを削除
    /// </summary>
    void Clear();
}
