using Metasia.Core.Project;
using Metasia.Editor.Models;
using Metasia.Editor.Models.ProjectGenerate;
using ReactiveUI;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;

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
    public ObservableCollection<VideoPreset> Presets { get; } = new(VideoPreset.DefaultPresets);

    private ProjectTemplateInfo? _selectedTemplate;
    public ProjectTemplateInfo? SelectedTemplate
    {
        get => _selectedTemplate;
        set => this.RaiseAndSetIfChanged(ref _selectedTemplate, value);
    }

    private VideoPreset? _selectedPreset;
    public VideoPreset? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedPreset, value);
            if (value is not null && !value.IsCustom)
            {
                _width = value.Width;
                _height = value.Height;
                _frameRate = value.FrameRate;
                this.RaisePropertyChanged(nameof(Width));
                this.RaisePropertyChanged(nameof(Height));
                this.RaisePropertyChanged(nameof(FrameRate));
            }
        }
    }

    private int _width = 1920;
    public int Width
    {
        get => _width;
        set
        {
            var clamped = Math.Max(1, value);
            this.RaiseAndSetIfChanged(ref _width, clamped);
            UpdateSelectedPresetFromValues();
        }
    }

    private int _height = 1080;
    public int Height
    {
        get => _height;
        set
        {
            var clamped = Math.Max(1, value);
            this.RaiseAndSetIfChanged(ref _height, clamped);
            UpdateSelectedPresetFromValues();
        }
    }

    private int _frameRate = 30;
    public int FrameRate
    {
        get => _frameRate;
        set
        {
            var clamped = Math.Max(1, value);
            this.RaiseAndSetIfChanged(ref _frameRate, clamped);
            UpdateSelectedPresetFromValues();
        }
    }

    public NewProjectViewModel()
    {
        LoadTemplates();

        SelectedPreset = VideoPreset.FindMatch(Width, Height, FrameRate) ?? VideoPreset.Custom;

        OkCommand = ReactiveCommand.Create(() =>
        {
            var framerate = FrameRate;
            var size = new SKSize(Width, Height);
            var projectInfo = new ProjectInfo(framerate, size, 44100, 2);

            MetasiaProject? selectedTemplate = SelectedTemplate?.TemplateFactory(projectInfo);

            return (Result: true, ProjectInfo: projectInfo, SelectedTemplate: selectedTemplate);
        });

        CancelCommand = ReactiveCommand.Create(() =>
            (Result: false, ProjectInfo: new ProjectInfo(30, new SKSize(1920, 1080), 44100, 2), SelectedTemplate: (MetasiaProject?)null));
    }

    private void UpdateSelectedPresetFromValues()
    {
        var match = VideoPreset.FindMatch(_width, _height, _frameRate);
        var newPreset = match ?? VideoPreset.Custom;
        if (!ReferenceEquals(_selectedPreset, newPreset))
        {
            _selectedPreset = newPreset;
            this.RaisePropertyChanged(nameof(SelectedPreset));
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

        SelectedTemplate = AvailableTemplates.FirstOrDefault();
    }
}
