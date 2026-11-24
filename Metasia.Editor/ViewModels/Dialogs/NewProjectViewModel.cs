using Metasia.Core.Project;
using Metasia.Core.Sounds;
using Metasia.Editor.Models;
using Metasia.Editor.Models.ProjectGenerate;
using ReactiveUI;
using RxVoid = ReactiveUI.Primitives.RxVoid;
using SkiaSharp;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Metasia.Editor.ViewModels.Dialogs;

public class ProjectTemplateInfo
{
    public string Name { get; set; } = string.Empty;
    public Func<ProjectInfo, MetasiaProject?> TemplateFactory { get; set; } = _ => null;
}

/// <summary>
/// 新規プロジェクトで選べる音声チャンネルの選択肢
/// </summary>
public class AudioChannelOption
{
    public string Name { get; }
    public AudioChannelLayout Layout { get; }
    public int ChannelCount => (int)Layout;

    public AudioChannelOption(string name, AudioChannelLayout layout)
    {
        Name = name;
        Layout = layout;
    }
}

public class NewProjectViewModel : ViewModelBase
{
    public ReactiveCommand<RxVoid, (bool, ProjectInfo, MetasiaProject?)> OkCommand { get; }
    public ReactiveCommand<RxVoid, (bool, ProjectInfo, MetasiaProject?)> CancelCommand { get; }

    public ObservableCollection<ProjectTemplateInfo> AvailableTemplates { get; } = new();
    public ObservableCollection<VideoPreset> Presets { get; } = new(VideoPreset.DefaultPresets);

    /// <summary>
    /// 音声チャンネルの選択肢。プロジェクトファイルの並び順が決まっているため1、2、6chに限定する
    /// </summary>
    public ObservableCollection<AudioChannelOption> AudioChannelOptions { get; } = new(new[]
    {
        new AudioChannelOption("モノラル 1ch", AudioChannelLayout.Mono),
        new AudioChannelOption("ステレオ 2ch", AudioChannelLayout.Stereo),
        new AudioChannelOption("5.1サラウンド 6ch", AudioChannelLayout.Surround5_1),
    });

    private AudioChannelOption? _selectedAudioChannelOption;
    public AudioChannelOption? SelectedAudioChannelOption
    {
        get => _selectedAudioChannelOption;
        set => this.RaiseAndSetIfChanged(ref _selectedAudioChannelOption, value);
    }

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
        SelectedAudioChannelOption = AudioChannelOptions.First(option => option.Layout == AudioChannelLayout.Stereo);

        OkCommand = ReactiveCommand.Create(() =>
        {
            var framerate = FrameRate;
            var size = new SKSize(Width, Height);
            var audioChannelLayout = SelectedAudioChannelOption?.Layout ?? AudioChannelLayout.Stereo;
            var projectInfo = new ProjectInfo(framerate, size, 44100, (int)audioChannelLayout);

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
