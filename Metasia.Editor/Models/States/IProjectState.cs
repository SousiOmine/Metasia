using System;
using System.Threading.Tasks;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.Projects;


namespace Metasia.Editor.Models.States;

public interface IProjectState : IDisposable
{
    /// <summary>
    /// 現在のプロジェクト
    /// </summary>
    MetasiaEditorProject? CurrentProject { get; }

    /// <summary>
    /// 現在のプロジェクト情報
    /// </summary>
    ProjectInfo? CurrentProjectInfo { get; }

    /// <summary>
    /// 現在のタイムライン
    /// </summary>
    TimelineObject? CurrentTimeline { get; }

    /// <summary>
    /// プロジェクトを読み込む
    /// </summary>
    Task LoadProjectAsync(MetasiaEditorProject project);

    /// <summary>
    /// プロジェクトを閉じる
    /// </summary>
    void CloseProject();

    /// <summary>
    /// 現在のタイムラインを設定する
    /// </summary>
    void SetCurrentTimeline(TimelineObject timeline);

    /// <summary>
    /// プロジェクトが読み込まれた時に発生するイベント
    /// </summary>
    event Action? ProjectLoaded;

    /// <summary>
    /// プロジェクトが閉じられた時に発生するイベント
    /// </summary>
    event Action? ProjectClosed;

    /// <summary>
    /// タイムラインが変更された時に発生するイベント
    /// </summary>
    event Action? TimelineChanged;
}