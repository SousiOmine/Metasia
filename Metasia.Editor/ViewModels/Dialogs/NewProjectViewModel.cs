using Metasia.Core.Project;
using Metasia.Editor.Models.ProjectGenerate;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;

namespace Metasia.Editor.ViewModels.Dialogs;

public class ProjectTemplateInfo
{
    public string Name { get; set; } = string.Empty;
    public Func<ProjectInfo, MetasiaProject?> TemplateFactory { get; set; } = _ => null;
}

public class NewProjectViewModel : ViewModelBase
{
    public ReactiveCommand<Unit, (bool, ProjectInfo, MetasiaProject?)> OkCommand { get; }
    public ReactiveCommand<Unit, (bool, ProjectInfo, MetasiaProject?)> CancelCommand { get; }

    public ObservableCollection<ProjectTemplateInfo> AvailableTemplates { get; } = new();
    public ObservableCollection<string> FramerateOptions { get; } = new();
    public ObservableCollection<string> ResolutionOptions { get; } = new();

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

    public NewProjectViewModel()
    {
        LoadTemplates();
        LoadOptions();

        OkCommand = ReactiveCommand.Create(() =>
        {
            var framerate = GetFramerateFromIndex(SelectedFramerateIndex);
            var size = GetResolutionFromIndex(SelectedResolutionIndex);
            var projectInfo = new ProjectInfo(framerate, size, 44100, 2);

            MetasiaProject? selectedTemplate = SelectedTemplate?.TemplateFactory(projectInfo);

            return (Result: true, ProjectInfo: projectInfo, SelectedTemplate: selectedTemplate);
        });

        CancelCommand = ReactiveCommand.Create(() =>
            (Result: false, ProjectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2), SelectedTemplate: (MetasiaProject?)null));
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
