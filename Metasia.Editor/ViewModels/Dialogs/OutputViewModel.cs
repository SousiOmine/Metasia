using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

    public ObservableCollection<EncoderQueueItemViewModel> OutputHistory { get; } = [];

    public ICommand SelectOutputPathCommand { get; }
    public ICommand OutputCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? CancelAction { get; set; }

    private int _selectedEncoderIndex = 0;

    private readonly List<EncoderInfo> _encoders = [];
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

        _encodeService.QueueUpdated += (_, _) => QueueUpdated();

        UIReflesh();
        QueueUpdated();
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
        List<EncoderInfo> encoderList = [];
        foreach (var plugin in _pluginService.MediaOutputPlugins)
        {
            var factory = new PluginEncoderFactory(plugin);
            encoderList.Add(new EncoderInfo(
                factory.Name,
                plugin.PluginIdentifier,
                factory.SupportedExtensions,
                factory
            ));
        }
        var sequentialImagesFactory = new SequentialImagesEncoderFactory();
        encoderList.Add(new EncoderInfo(
            sequentialImagesFactory.Name,
            "標準",
            sequentialImagesFactory.SupportedExtensions,
            sequentialImagesFactory
        ));

        OutputMethodList.Clear();
        foreach (var encoderInfo in encoderList)
        {
            OutputMethodList.Add(encoderInfo.DisplayName);
            _encoders.Add(encoderInfo);
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
        if (SelectedEncoderIndex >= _encoders.Count) return;
        var allowExtensions = _encoders[SelectedEncoderIndex].SupportedExtensions;
        var result = await _fileDialogService.SaveFileDialogAsync("出力先を選択", allowExtensions);
        if (result is null) return;

        OutputPath = result.Path?.LocalPath ?? "";
    }

    private void QueueUpdated()
    {
        foreach (var item in OutputHistory)
        {
            item.Dispose();
        }

        OutputHistory.Clear();

        foreach (var item in _encodeService.Encoders)
        {
            OutputHistory.Add(new EncoderQueueItemViewModel(item));
        }
    }

    private void OutputExecute()
    {
        if (_encoders is null || _encoders.Count == 0)
        {
            Console.Error.WriteLine("Error: No encoders available");
            return;
        }

        if (SelectedEncoderIndex < 0 || SelectedEncoderIndex >= _encoders.Count)
        {
            Console.Error.WriteLine($"Error: SelectedEncoderIndex {SelectedEncoderIndex} is out of bounds (0-{_encoders.Count - 1})");
            return;
        }

        if (_projectState.CurrentProject is null)
        {
            Console.Error.WriteLine("Error: No current project loaded");
            return;
        }

        var encoderInfo = _encoders[SelectedEncoderIndex];
        var encoder = encoderInfo.Factory.CreateEncoder();
        var project = _projectState.CurrentProject.CreateMetasiaProject();

        if (project.Timelines is null || project.Timelines.Count == 0)
        {
            Console.Error.WriteLine("Error: Project has no timelines");
            return;
        }

        if (SelectedTimelineIndex < 0 || SelectedTimelineIndex >= project.Timelines.Count)
        {
            Console.Error.WriteLine($"Error: SelectedTimelineIndex {SelectedTimelineIndex} is out of bounds (0-{project.Timelines.Count - 1})");
            return;
        }

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

public class EncoderQueueItemViewModel : ViewModelBase
{
    public string QueueText 
    {
        get => _queueText;
        set => this.RaiseAndSetIfChanged(ref _queueText, value);
    }

    public double Progress
    {
        get => _progress;
        set => this.RaiseAndSetIfChanged(ref _progress, value);
    }

    private string _queueText = string.Empty;
    private double _progress = 0;

    private readonly IEditorEncoder _encoder;
    private readonly EventHandler<EventArgs> _onStatusChanged;

    public EncoderQueueItemViewModel(IEditorEncoder encoder)
    {
        _encoder = encoder;
        _onStatusChanged = (sender, e) => UpdateProgress();
        _encoder.StatusChanged += _onStatusChanged;
        UpdateProgress();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _encoder.StatusChanged -= _onStatusChanged;
        }
        base.Dispose(disposing);
    }

    private void UpdateProgress()
    {
        Progress = _encoder.ProgressRate;
        var outputFilename = Path.GetFileName(_encoder.OutputPath);
        QueueText = _encoder.Name + " " + _encoder.Status.ToString() + " " + outputFilename;
    }

}
