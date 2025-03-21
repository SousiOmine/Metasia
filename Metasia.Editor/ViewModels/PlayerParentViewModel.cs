using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CsToml;
using Metasia.Core.Json;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Models;
using Metasia.Editor.Models.Projects;
using ReactiveUI;
using SkiaSharp;

namespace Metasia.Editor.ViewModels;

public class PlayerParentViewModel : ViewModelBase
{
    public MetasiaProject? CurrentProject
    {
        get { return currentProject;}
        set
        {
            currentProject = value;
            ProjectInstanceChanged?.Invoke(this, EventArgs.Empty);
            if (value is not null)
            {
                TargetPlayerViewModel = new PlayerViewModel(CurrentProject.Timelines[0], CurrentProject.Info);
            }
        }
    }

    public MetasiaEditorProject? CurrentEditorProject
    {
        get => currentEditorProject;
        set
        {
            currentEditorProject = value;
        }
    }

    public event EventHandler? ProjectInstanceChanged;

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
    private MetasiaEditorProject? currentEditorProject;

    private List<PlayerViewModel> _playerViewModels = new();
    
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
    }

    public void SaveCurrentProject(string filePath)
    {
        if(CurrentProject is null) return;

        switch(CurrentProjectStructureMethod)
        {
            case ProjectStructureMethod.MTPJ:
                string jsonString = ProjectSerializer.SerializeToMTPJ(CurrentProject);
                File.WriteAllText(filePath, jsonString);
                break;
        }

    }

    public void LoadProject(MetasiaEditorProject editorProject)
    {
        CurrentEditorProject = editorProject;

        _playerViewModels.Clear();

        ProjectInfo projectInfo = new ProjectInfo()
        {
            Framerate = editorProject.ProjectFile.Framerate,
            Size = new SKSize(editorProject.ProjectFile.Resolution.Width, editorProject.ProjectFile.Resolution.Height),
        };

        foreach (TimelineFile timeline in editorProject.Timelines)
        {
            _playerViewModels.Add(new PlayerViewModel(timeline.Timeline, projectInfo));
        }

        TargetPlayerViewModel = _playerViewModels[0];
        ProjectInstanceChanged?.Invoke(this, EventArgs.Empty);
    }
}