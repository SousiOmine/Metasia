using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Metasia.Core.Project;
using Metasia.Editor.Models.ProjectGenerate;
using Metasia.Editor.Services;
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
    public ReactiveCommand<Unit, Unit> BrowseFileCommand { get; }

    public ObservableCollection<ProjectTemplateInfo> AvailableTemplates { get; } = new();
    public ObservableCollection<string> FramerateOptions { get; } = new();
    public ObservableCollection<string> ResolutionOptions { get; } = new();

    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set => this.RaiseAndSetIfChanged(ref _filePath, value);
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

    private readonly IFileDialogService _fileDialogService;

    public NewProjectViewModel(IFileDialogService fileDialogService)
    {
        _fileDialogService = fileDialogService;
        LoadTemplates();
        LoadOptions();

        var canExecuteOk = this.WhenAnyValue(
            x => x.FilePath,
            filePath => !string.IsNullOrWhiteSpace(filePath));

        OkCommand = ReactiveCommand.Create(() =>
        {
            try
            {
                // ファイルパスの検証
                if (string.IsNullOrWhiteSpace(FilePath))
                {
                    throw new ArgumentException("ファイルパスが指定されていません。");
                }

                // ファイル名の検証
                var fileName = Path.GetFileNameWithoutExtension(FilePath);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    throw new ArgumentException("ファイル名が指定されていません。");
                }
                var invalidChars = Path.GetInvalidFileNameChars();
                if (fileName.Any(c => invalidChars.Contains(c)))
                {
                    throw new ArgumentException($"ファイル名に無効な文字が含まれています: {fileName}");
                }

                var framerate = GetFramerateFromIndex(SelectedFramerateIndex);
                var size = GetResolutionFromIndex(SelectedResolutionIndex);
                var projectInfo = new ProjectInfo(framerate, size, 44100, 2);

                // ファイルパスの拡張子を確認・修正
                var projectFilePath = FilePath;
                if (!projectFilePath.EndsWith(".mtpj", StringComparison.OrdinalIgnoreCase))
                {
                    projectFilePath += ".mtpj";
                }

                // 親ディレクトリが存在しない場合は作成
                var parentDirectory = Path.GetDirectoryName(projectFilePath);
                if (!string.IsNullOrEmpty(parentDirectory) && !Directory.Exists(parentDirectory))
                {
                    Directory.CreateDirectory(parentDirectory);
                }

                MetasiaProject? selectedTemplate = SelectedTemplate?.TemplateFactory(projectInfo);

                return (Result: true, ProjectPath: projectFilePath, ProjectInfo: projectInfo, SelectedTemplate: selectedTemplate);
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

        BrowseFileCommand = ReactiveCommand.CreateFromTask(BrowseFileAsync);
    }

    private async Task BrowseFileAsync()
    {
        var file = await _fileDialogService.SaveFileDialogAsync(
            "新規プロジェクトを保存",
            new[] { "mtpj" },
            "mtpj");
        if (file != null)
        {
            FilePath = file.Path.LocalPath;
        }
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
