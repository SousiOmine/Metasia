using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
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

    /// <summary>
    /// 現在表示中のタイムラインが切り替わった時に発生するイベント
    /// </summary>
    event Action? CurrentTimelineChanged;

    /// <summary>
    /// タイムラインに変更が加えられたことを通知する
    /// </summary>
    void NotifyTimelineChanged();

    /// <summary>
    /// 最後の保存以降に変更が加えられたかどうか
    /// </summary>
    bool IsDirty { get; set; }

    /// <summary>
    /// IsDirtyが変更された時に発生するイベント
    /// </summary>
    event Action? IsDirtyChanged;

    /// <summary>
    /// プロジェクトが保存されたことを通知します。
    /// IsDirtyが変化していなくても常にIsDirtyChangedを発火します。
    /// </summary>
    void MarkProjectSaved();
}
