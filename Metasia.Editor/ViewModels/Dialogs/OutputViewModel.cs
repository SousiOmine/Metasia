using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Metasia.Core.Encode;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Media.Output;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using Metasia.Editor.Services.PluginService;
using ReactiveUI;

namespace Metasia.Editor.ViewModels.Dialogs;

/// <summary>
/// 動画出力ウィンドウのViewModel
/// </summary>
public class OutputViewModel : ViewModelBase
{

    public ObservableCollection<string> OutputMethodList { get; } = [];

    public int SelectedEncoderIndex
    {
        get => _selectedEncoderIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedEncoderIndex, value);
    }

    public ObservableCollection<string> TimelineList { get; } = [];

    public int SelectedTimelineIndex
    {
        get => _selectedTimelineIndex;
        set => this.RaiseAndSetIfChanged(ref _selectedTimelineIndex, value);
    }

    public string OutputPath
    {
        get => _outputPath;
        set => this.RaiseAndSetIfChanged(ref _outputPath, value);
    }

    public ICommand SelectOutputPathCommand { get; }
    public ICommand OutputCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? CancelAction { get; set; }

    private int _selectedEncoderIndex = 0;

    private readonly List<IEditorEncoder> _encoders = [];
    private int _selectedTimelineIndex = 0;
    private string _outputPath = string.Empty;
    private readonly IProjectState _projectState;
    private readonly MediaAccessorRouter _mediaAccessorRouter;
    private readonly IFileDialogService _fileDialogService;
    private readonly IPluginService _pluginService;
    private readonly IEncodeService _encodeService;

    public OutputViewModel(
        IProjectState projectState,
        MediaAccessorRouter mediaAccessorRouter,
        IFileDialogService fileDialogService,
        IPluginService pluginService,
        IEncodeService encodeService
    )
    {
        _projectState = projectState;
        _mediaAccessorRouter = mediaAccessorRouter;
        _fileDialogService = fileDialogService;
        _pluginService = pluginService;
        _encodeService = encodeService;

        CancelCommand = ReactiveCommand.Create(Cancel);
        SelectOutputPathCommand = ReactiveCommand.CreateFromTask(SelectOutputPathExecuteAsync);
        OutputCommand = ReactiveCommand.Create(OutputExecute);

        LoadEncoders();

        _projectState.ProjectLoaded += UIReflesh;
        _projectState.ProjectClosed += UIReflesh;
        _projectState.TimelineChanged += UIReflesh;

        UIReflesh();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _projectState.ProjectLoaded -= UIReflesh;
            _projectState.ProjectClosed -= UIReflesh;
            _projectState.TimelineChanged -= UIReflesh;
        }

        base.Dispose(disposing);
    }

    private void LoadEncoders()
    {
        _encoders.Clear();
        List<(string originName, IEditorEncoder editorEncoder)> encoderList = [];
        foreach (var plugin in _pluginService.MediaOutputPlugins)
        {
            PluginEncoder encoder = new(plugin);
            encoderList.Add((plugin.PluginIdentifier, encoder));
        }
        var sequentialImagesEncoder = new SequentialImagesEncoder();
        encoderList.Add(("標準", sequentialImagesEncoder));

        OutputMethodList.Clear();
        foreach (var (originName, editorEncoder) in encoderList)
        {
            OutputMethodList.Add(editorEncoder.Name + "(" + originName + ")");
            _encoders.Add(editorEncoder);
        }
    }

    private void UIReflesh()
    {
        TimelineList.Clear();
        if (_projectState.CurrentProject is null) return;

        foreach (var timeline in _projectState.CurrentProject.Timelines)
        {
            TimelineList.Add(timeline.Timeline.Id);
        }
    }

    private async Task SelectOutputPathExecuteAsync()
    {
        var allowExtensions = _encoders[SelectedEncoderIndex].SupportedExtensions;
        var result = await _fileDialogService.SaveFileDialogAsync("出力先を選択", allowExtensions);
        if (result is null) return;

        OutputPath = result.Path?.LocalPath ?? "";
    }

    private void OutputExecute()
    {
        var encoder = _encoders[SelectedEncoderIndex];
        var project = _projectState.CurrentProject!.CreateMetasiaProject();
        var timeline = project.Timelines[SelectedTimelineIndex];
        var imageFileAccessor = _mediaAccessorRouter;
        var videoFileAccessor = _mediaAccessorRouter;
        encoder.Initialize(project, timeline, imageFileAccessor, videoFileAccessor, OutputPath);

        _encodeService.QueueEncode(encoder, OutputPath);
    }

    private void Cancel()
    {
        CancelAction?.Invoke();
    }
}

