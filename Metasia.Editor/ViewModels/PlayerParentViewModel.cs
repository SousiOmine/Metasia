using System;
using System.IO;
using Metasia.Core.Json;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models;
using ReactiveUI;

namespace Metasia.Editor.ViewModels;

public class PlayerParentViewModel : ViewModelBase
{
    public MetasiaProject? CurrentProject
    {
        get { return currentProject;}
        set
        {
            currentProject = value;
            if(value is not null) LoadProject();
        }
    }

    public event EventHandler? ProjectInstanceChanged;

    public string CurrentProjectFilePath
    {
        get => _currentProjectFilePath;
        set => this.RaiseAndSetIfChanged(ref _currentProjectFilePath, value);
    }

    public ProjectStructureMethod CurrentProjectStructureMethod
    {
        get => currentProjectStructureMethod;
        set => this.RaiseAndSetIfChanged(ref currentProjectStructureMethod, value);
    }

    public PlayerViewModel TargetPlayerViewModel
    {
        get => _targetPlayerViewModel;
        set
        {
            this.RaiseAndSetIfChanged(ref _targetPlayerViewModel, value);
            if (value is not null)
            {
                TargetTimelineName = value.TargetTimeline.Id;
            }
        } 
    }

    public string TargetTimelineName
    {
        get => _targetTimelineName;
        set => this.RaiseAndSetIfChanged(ref _targetTimelineName, value);
    }

    private MetasiaProject? currentProject;
    private ProjectStructureMethod currentProjectStructureMethod;
    private PlayerViewModel? _targetPlayerViewModel;
    private string _targetTimelineName;
    private string _currentProjectFilePath;
    
    public PlayerParentViewModel(MetasiaProject project)
    {
        CurrentProject = project;
        CurrentProjectStructureMethod = ProjectStructureMethod.NONE_SAVED;
    }

    public void LoadProjectFromFilePath(string filePath)
    {
        // ファイルの拡張子を確認
        string extension = Path.GetExtension(filePath);
        switch (extension)
        {
            case ".mtpj":
                CurrentProject = ProjectLoader.LoadProjectFromMTPJ(filePath);
                CurrentProjectStructureMethod = ProjectStructureMethod.MTPJ;
                break;
            default:
                throw new Exception("サポートされていないファイル形式です。");
        }
        CurrentProjectFilePath = filePath;
    }

    public void SaveCurrentProject(string filePath)
    {
        if(CurrentProject is null) return;

        switch(CurrentProjectStructureMethod)
        {
            case ProjectStructureMethod.MTPJ:
                string jsonString = ProjectSerializer.SerializeToMTPJ(CurrentProject);
                File.WriteAllText(filePath, jsonString);
                CurrentProjectFilePath = filePath;
                break;
        }

    }

    private void LoadProject()
    {
        TargetPlayerViewModel = new PlayerViewModel(CurrentProject.Timelines[0], CurrentProject.Info);
        ProjectInstanceChanged?.Invoke(this, EventArgs.Empty);
    }

    public void CreateNewProject(string projectName, string projectPath, ProjectInfo projectInfo)
    {
        // プロジェクトフォルダが存在しない場合は作成
        if (!Directory.Exists(projectPath))
        {
            Directory.CreateDirectory(projectPath);
        }

        // 新しいプロジェクトオブジェクトを作成
        MetasiaProject newProject = new MetasiaProject(projectInfo);
        newProject.LastFrame = 299; // デフォルト5秒（60fpsの場合）

        // ルートタイムラインを作成
        TimelineObject mainTL = new TimelineObject("RootTimeline");
        
        // デフォルトのレイヤーを追加
        LayerObject defaultLayer = new LayerObject("layer1", "レイヤー 1");
        mainTL.Layers.Add(defaultLayer);
        
        // プロジェクトにタイムラインを追加
        newProject.Timelines.Add(mainTL);
        
        // プロジェクトファイルのパスを設定
        string projectFilePath = Path.Combine(projectPath, $"{projectName}.mtpj");
        
        // プロジェクトを現在のプロジェクトとして設定
        CurrentProject = newProject;
        CurrentProjectStructureMethod = ProjectStructureMethod.MTPJ;
        CurrentProjectFilePath = projectFilePath;
        
        // プロジェクトをファイルに保存
        string jsonString = ProjectSerializer.SerializeToMTPJ(newProject);
        File.WriteAllText(projectFilePath, jsonString);
    }
}