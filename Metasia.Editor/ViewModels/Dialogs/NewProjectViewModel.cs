using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using Metasia.Core.Project;
using Metasia.Editor.Models.ProjectGenerate;
using ReactiveUI;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace Metasia.Editor.ViewModels.Dialogs;

public class ProjectTemplateInfo
{
    public string Name { get; set; } = string.Empty;
    public Func<ProjectInfo, MetasiaProject?> TemplateFactory { get; set; } = _ => null;
}

public class NewProjectViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, (bool, string, ProjectInfo, MetasiaProject?)> OkCommand { get; }
    public ReactiveCommand<Unit, (bool, string, ProjectInfo, MetasiaProject?)> CancelCommand { get; }
    public ReactiveCommand<Unit, Unit> BrowseFolderCommand { get; }

    public ObservableCollection<ProjectTemplateInfo> AvailableTemplates { get; } = new();
    public ObservableCollection<string> FramerateOptions { get; } = new();
    public ObservableCollection<string> ResolutionOptions { get; } = new();

    private string _projectName = string.Empty;
    public string ProjectName
    {
        get => _projectName;
        set => this.RaiseAndSetIfChanged(ref _projectName, value);
    }

    private string _folderPath = string.Empty;
    public string FolderPath
    {
        get => _folderPath;
        set => this.RaiseAndSetIfChanged(ref _folderPath, value);
    }

    private ProjectTemplateInfo? _selectedTemplate;
    public ProjectTemplateInfo? SelectedTemplate
    {
        get => _selectedTemplate;
        set => this.RaiseAndSetIfChanged(ref _selectedTemplate, value);
    }

    private int _selectedFramerateIndex = 1;
    public int SelectedFramerateIndex
    {
        get => _selectedFramerateIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedFramerateIndex, value);
    }

    private int _selectedResolutionIndex = 1;
    public int SelectedResolutionIndex
    {
        get => _selectedResolutionIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedResolutionIndex, value);
    }

    private string? _selectedFolderPath;
    public string? SelectedFolderPath
    {
        get => _selectedFolderPath;
        set => this.RaiseAndSetIfChanged(ref _selectedFolderPath, value);
    }

    public NewProjectViewModel()
    {
        LoadTemplates();
        LoadOptions();

        var canExecuteOk = this.WhenAnyValue(
            x => x.ProjectName,
            x => x.FolderPath,
            (projectName, folderPath) => 
                !string.IsNullOrWhiteSpace(projectName) && 
                !string.IsNullOrWhiteSpace(folderPath));

        OkCommand = ReactiveCommand.Create(() =>
        {
            try
            {
                // プロジェクト名の検証
                var invalidChars = Path.GetInvalidFileNameChars();
                if (ProjectName.Any(c => invalidChars.Contains(c)))
                {
                    throw new ArgumentException($"プロジェクト名に無効な文字が含まれています: {ProjectName}");
                }
                
                var framerate = GetFramerateFromIndex(SelectedFramerateIndex);
                var size = GetResolutionFromIndex(SelectedResolutionIndex);
                var projectInfo = new ProjectInfo(framerate, size, 44100, 2);
                
                var projectPath = Path.Combine(FolderPath, ProjectName);
                
                // プロジェクトフォルダを作成
                if (!Directory.Exists(projectPath))
                {
                    Directory.CreateDirectory(projectPath);
                }

                MetasiaProject? selectedTemplate = SelectedTemplate?.TemplateFactory(projectInfo);

                return (Result: true, ProjectPath: projectPath, ProjectInfo: projectInfo, SelectedTemplate: selectedTemplate);
            }
            catch (Exception ex)
            {
                // エラーをログに記録
                Console.Error.WriteLine($"プロジェクト作成エラー: {ex.Message}");
                
                // ユーザーが選択した設定を使用
                var framerate = GetFramerateFromIndex(SelectedFramerateIndex);
                var size = GetResolutionFromIndex(SelectedResolutionIndex);
                return (false, string.Empty, new ProjectInfo(framerate, size, 44100, 2), null);
            }
        }, canExecuteOk);
        CancelCommand = ReactiveCommand.Create(() => 
            (Result: false, ProjectPath: string.Empty, ProjectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2), SelectedTemplate: (MetasiaProject?)null));

        BrowseFolderCommand = ReactiveCommand.Create(() => { });
    }

    private void LoadTemplates()
    {
        AvailableTemplates.Clear();
        
        AvailableTemplates.Add(new ProjectTemplateInfo
        {
            Name = "空のプロジェクト",
            TemplateFactory = info => new EmptyProjectTemplate(info).Template
        });

        var kariTemplate = new KariProjectTemplate();
        AvailableTemplates.Add(new ProjectTemplateInfo
        {
            Name = kariTemplate.Name,
            TemplateFactory = _ => new KariProjectTemplate().Template
        });

        SelectedTemplate = AvailableTemplates.FirstOrDefault();
    }

    private void LoadOptions()
    {
        FramerateOptions.Clear();
        FramerateOptions.Add("24 fps");
        FramerateOptions.Add("30 fps");
        FramerateOptions.Add("60 fps");

        ResolutionOptions.Clear();
        ResolutionOptions.Add("HD (1280×720)");
        ResolutionOptions.Add("Full HD (1920×1080)");
        ResolutionOptions.Add("4K (3840×2160)");
    }

    private int GetFramerateFromIndex(int index)
    {
        return index switch
        {
            0 => 24,
            1 => 30,
            2 => 60,
            _ => 30
        };
    }

    private SKSize GetResolutionFromIndex(int index)
    {
        return index switch
        {
            0 => new SKSize(1280, 720),
            1 => new SKSize(1920, 1080),
            2 => new SKSize(3840, 2160),
            _ => new SKSize(1920, 1080)
        };
    }
}