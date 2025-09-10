using System;
using System.Threading.Tasks;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models.Projects;
using SkiaSharp;

namespace Metasia.Editor.Models.States;

/// <summary>
/// プロジェクトの状態を管理するクラス
/// </summary>
public class ProjectState : IProjectState
{
    
    private TimelineObject? _currentTimeline;

    /// <summary>
    /// 現在開いているプロジェクト
    /// </summary>
    public MetasiaEditorProject? CurrentProject => _currentProject;

    /// <summary>
    /// 現在開いているプロジェクトの情報
    /// </summary>
    public ProjectInfo? CurrentProjectInfo => _currentProjectInfo;

    /// <summary>
    /// 現在選択されているタイムライン
    /// </summary>
    public TimelineObject? CurrentTimeline => _currentTimeline;

    /// <summary>
    /// プロジェクトが読み込まれたときに発生するイベント
    /// </summary>
    public event Action? ProjectLoaded;

    /// <summary>
    /// プロジェクトが閉じられたときに発生するイベント
    /// </summary>
    public event Action? ProjectClosed;

    /// <summary>
    /// タイムラインが変更されたときに発生するイベント
    /// </summary>
    public event Action? TimelineChanged;

    private MetasiaEditorProject? _currentProject;
    private ProjectInfo? _currentProjectInfo;

    /// <summary>
    /// プロジェクトを非同期で読み込む
    /// </summary>
    /// <param name="project">読み込むプロジェクト</param>
    /// <returns>読み込み完了を待つタスク</returns>
    public async Task LoadProjectAsync(MetasiaEditorProject project)
    {
        _currentProject = project;
        _currentProjectInfo = new ProjectInfo(project.ProjectFile.Framerate, new SKSize(project.ProjectFile.Resolution.Width, project.ProjectFile.Resolution.Height));

        // 重い処理の代わりに仮で100ms待つ
        await Task.Delay(100);

        // デフォルトで最初のタイムラインを選択
        if (project.Timelines.Count > 0)
        {
            _currentTimeline = project.Timelines[0].Timeline;
        }
        else
        {
            _currentTimeline = null;
        }

        ProjectLoaded?.Invoke();
    }

    /// <summary>
    /// 現在開いているプロジェクトを閉じる
    /// </summary>
    public void CloseProject()
    {
        _currentProject = null;
        _currentTimeline = null;
        _currentProjectInfo = null;
        ProjectClosed?.Invoke();
    }

    /// <summary>
    /// 開くタイムラインを設定
    /// </summary>
    /// <param name="timeline">設定するタイムライン</param>
    public void SetCurrentTimeline(TimelineObject timeline)
    {
        if (_currentProject == null)
        {
            throw new InvalidOperationException("プロジェクトが読み込まれていないため、タイムラインを設定できません。");
        }

        _currentTimeline = timeline;

        TimelineChanged?.Invoke();
    }

    public void Dispose()
    {
        
    }
}
