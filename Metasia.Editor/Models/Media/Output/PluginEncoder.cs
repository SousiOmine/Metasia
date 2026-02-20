using System;
using Metasia.Core.Encode;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;
using Metasia.Editor.Plugin;

namespace Metasia.Editor.Models.Media.Output;

public class PluginEncoder : IEditorEncoder, IDisposable
{
    public string Name { get; private set; }
    public string[] SupportedExtensions { get; private set; }
    public double ProgressRate { get; private set; }

    public IEncoder.EncoderState Status { get; private set; }

    public event EventHandler<EventArgs> StatusChanged = delegate { };
    public event EventHandler<EventArgs> EncodeStarted = delegate { };
    public event EventHandler<EventArgs> EncodeCompleted = delegate { };
    public event EventHandler<EventArgs> EncodeFailed = delegate { };

    public string OutputPath { get; private set; } = string.Empty;

    private readonly IMediaOutputPlugin _plugin;
    private readonly EncoderBase _encoder;
    private readonly EventHandler<EventArgs> _onStatusChanged;
    private readonly EventHandler<EventArgs> _onEncodeStarted;
    private readonly EventHandler<EventArgs> _onEncodeCompleted;
    private readonly EventHandler<EventArgs> _onEncodeFailed;

    public PluginEncoder(IMediaOutputPlugin plugin)
    {
        _plugin = plugin;
        _encoder = _plugin.CreateEncoderInstance();

        Name = _plugin.Name;
        SupportedExtensions = _plugin.SupportedExtensions;

        _onStatusChanged = (sender, e) => OnStatusChanged();
        _onEncodeStarted = (sender, e) => EncodeStarted.Invoke(this, e);
        _onEncodeCompleted = (sender, e) => EncodeCompleted.Invoke(this, e);
        _onEncodeFailed = (sender, e) => EncodeFailed.Invoke(this, e);

        _encoder.StatusChanged += _onStatusChanged;
        _encoder.EncodeStarted += _onEncodeStarted;
        _encoder.EncodeCompleted += _onEncodeCompleted;
        _encoder.EncodeFailed += _onEncodeFailed;
    }

    public void Initialize(
        MetasiaProject project,
        TimelineObject timeline,
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

    private void OnStatusChanged()
    {
        ProgressRate = _encoder.ProgressRate;
        Status = _encoder.Status;
        StatusChanged.Invoke(this, EventArgs.Empty);
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
}
