using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Metasia.Core.Encode;
using Metasia.Core.Media;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Media.Output;
using Metasia.Editor.Models.Projects;
using Metasia.Editor.Models.States;
using Metasia.Editor.Plugin;
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
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedEncoderIndex, value);
            UpdateSelectedEncoderSession();
        }
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

    public bool HasSelectedPluginSettings => _selectedOutputSession?.SettingsView is not null;

    public ICommand SelectOutputPathCommand { get; }
    public ICommand OutputCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? CancelAction { get; set; }

    private int _selectedEncoderIndex = 0;

    private readonly List<OutputEncoderEntry> _encoders = [];
    private int _selectedTimelineIndex = 0;
    private string _outputPath = string.Empty;
    private readonly IProjectState _projectState;
    private readonly MediaAccessorRouter _mediaAccessorRouter;
    private readonly IFileDialogService _fileDialogService;
    private readonly IPluginService _pluginService;
    private readonly IEncodeService _encodeService;
    private IMediaOutputSession? _selectedOutputSession;

    public IMediaOutputSession? SelectedOutputSession
    {
        get => _selectedOutputSession;
        private set
        {
            this.RaiseAndSetIfChanged(ref _selectedOutputSession, value);
            this.RaisePropertyChanged(nameof(HasSelectedPluginSettings));
        }
    }

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
            SelectedOutputSession?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void LoadEncoders()
    {
        DisposeSelectedSession();
        _encoders.Clear();
        foreach (var plugin in _pluginService.MediaOutputPlugins)
        {
            _encoders.Add(new OutputEncoderEntry(
                plugin.Name,
                plugin.PluginIdentifier,
                plugin.SupportedExtensions,
                plugin));
        }
        var sequentialImagesFactory = new SequentialImagesEncoderFactory();
        _encoders.Add(new OutputEncoderEntry(
            sequentialImagesFactory.Name,
            "標準",
            sequentialImagesFactory.SupportedExtensions,
            sequentialImagesFactory));

        OutputMethodList.Clear();
        foreach (var encoderInfo in _encoders)
        {
            OutputMethodList.Add(encoderInfo.DisplayName);
        }

        if (_encoders.Count == 0)
        {
            SelectedEncoderIndex = -1;
            return;
        }

        if (SelectedEncoderIndex < 0 || SelectedEncoderIndex >= _encoders.Count)
        {
            _selectedEncoderIndex = 0;
        }

        UpdateSelectedEncoderSession();
    }

    private void UIReflesh()
    {
        TimelineList.Clear();
        if (_projectState.CurrentProject is null) return;

        foreach (var timeline in _projectState.CurrentProject.Timelines)
        {
            TimelineList.Add(timeline.Id);
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
        var encoder = encoderInfo.CreateEncoder(SelectedOutputSession);
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
        var projectPath = GetProjectPath();
        encoder.Initialize(project, timeline, imageFileAccessor, videoFileAccessor, videoFileAccessor as IAudioFileAccessor, projectPath, OutputPath);

        _encodeService.QueueEncode(encoder);
    }

    private string GetProjectPath()
    {
        var project = _projectState.CurrentProject;
        if (project?.ProjectFilePath is not null)
        {
            return Path.GetDirectoryName(project.ProjectFilePath) ?? Directory.GetCurrentDirectory();
        }
        if (!string.IsNullOrEmpty(OutputPath))
        {
            return Path.GetDirectoryName(OutputPath) ?? Directory.GetCurrentDirectory();
        }
        return Directory.GetCurrentDirectory();
    }

    private void Cancel()
    {
        CancelAction?.Invoke();
    }

    private void UpdateSelectedEncoderSession()
    {
        DisposeSelectedSession();

        if (SelectedEncoderIndex < 0 || SelectedEncoderIndex >= _encoders.Count)
        {
            SelectedOutputSession = null;
            return;
        }

        SelectedOutputSession = _encoders[SelectedEncoderIndex].CreateSession();
    }

    private void DisposeSelectedSession()
    {
        SelectedOutputSession?.Dispose();
        SelectedOutputSession = null;
    }
}

internal sealed class OutputEncoderEntry
{
    public string Name { get; }
    public string OriginName { get; }
    public string[] SupportedExtensions { get; }

    public OutputEncoderEntry(string name, string originName, string[] supportedExtensions, IMediaOutputPlugin plugin)
    {
        Name = name;
        OriginName = originName;
        SupportedExtensions = supportedExtensions;
        _plugin = plugin;
    }

    public OutputEncoderEntry(string name, string originName, string[] supportedExtensions, IEditorEncoderFactory factory)
    {
        Name = name;
        OriginName = originName;
        SupportedExtensions = supportedExtensions;
        _factory = factory;
    }

    public string DisplayName => $"{Name} ({OriginName})";

    private readonly IMediaOutputPlugin? _plugin;
    private readonly IEditorEncoderFactory? _factory;

    public IMediaOutputSession? CreateSession()
    {
        return _plugin?.CreateSession();
    }

    public IEditorEncoder CreateEncoder(IMediaOutputSession? session)
    {
        if (_factory is not null)
        {
            return _factory.CreateEncoder();
        }

        if (session is null)
        {
            throw new InvalidOperationException("出力セッションが初期化されていません。");
        }

        return new SessionPluginEncoder(session.Name, session.SupportedExtensions, session.CreateEncoderInstance());
    }
}

internal sealed class SessionPluginEncoder : IEditorEncoder, IDisposable
{
    public string Name { get; private set; }
    public string[] SupportedExtensions { get; private set; }
    public double ProgressRate { get; private set; }
    public IEncoder.EncoderState Status { get; private set; }
    public string OutputPath { get; private set; } = string.Empty;

    public event EventHandler<EventArgs> StatusChanged = delegate { };
    public event EventHandler<EventArgs> EncodeStarted = delegate { };
    public event EventHandler<EventArgs> EncodeCompleted = delegate { };
    public event EventHandler<EventArgs> EncodeFailed = delegate { };

    private readonly EncoderBase _encoder;
    private readonly EventHandler<EventArgs> _onStatusChanged;
    private readonly EventHandler<EventArgs> _onEncodeStarted;
    private readonly EventHandler<EventArgs> _onEncodeCompleted;
    private readonly EventHandler<EventArgs> _onEncodeFailed;

    public SessionPluginEncoder(string name, string[] supportedExtensions, EncoderBase encoder)
    {
        _encoder = encoder;

        Name = name;
        SupportedExtensions = supportedExtensions;

        _onStatusChanged = (_, _) => OnStatusChanged();
        _onEncodeStarted = (_, e) => EncodeStarted.Invoke(this, e);
        _onEncodeCompleted = (_, e) => EncodeCompleted.Invoke(this, e);
        _onEncodeFailed = (_, e) => EncodeFailed.Invoke(this, e);

        _encoder.StatusChanged += _onStatusChanged;
        _encoder.EncodeStarted += _onEncodeStarted;
        _encoder.EncodeCompleted += _onEncodeCompleted;
        _encoder.EncodeFailed += _onEncodeFailed;
    }

    public void Initialize(
        Metasia.Core.Project.MetasiaProject project,
        Metasia.Core.Objects.TimelineObject timeline,
        IImageFileAccessor imageFileAccessor,
        IVideoFileAccessor videoFileAccessor,
        IAudioFileAccessor audioFileAccessor,
        string projectPath,
        string outputPath)
    {
        OutputPath = outputPath;
        _encoder.Initialize(project, timeline, imageFileAccessor, videoFileAccessor, audioFileAccessor, projectPath, outputPath);
    }

    public void CancelRequest()
    {
        _encoder.CancelRequest();
    }

    public void Start()
    {
        _encoder.Start();
    }

    public void Dispose()
    {
        _encoder.StatusChanged -= _onStatusChanged;
        _encoder.EncodeStarted -= _onEncodeStarted;
        _encoder.EncodeCompleted -= _onEncodeCompleted;
        _encoder.EncodeFailed -= _onEncodeFailed;
        _encoder.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnStatusChanged()
    {
        ProgressRate = _encoder.ProgressRate;
        Status = _encoder.Status;
        StatusChanged.Invoke(this, EventArgs.Empty);
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
