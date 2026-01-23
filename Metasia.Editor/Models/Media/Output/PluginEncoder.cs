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

    public PluginEncoder(IMediaOutputPlugin plugin)
    {
        _plugin = plugin;
        _encoder = _plugin.CreateEncoderInstance();

        Name = _plugin.Name;
        SupportedExtensions = _plugin.SupportedExtensions;
        
        _encoder.StatusChanged += (sender, e) => OnStatusChanged();
        _encoder.EncodeStarted += (sender, e) => EncodeStarted.Invoke(this, e);
        _encoder.EncodeCompleted += (sender, e) => EncodeCompleted.Invoke(this, e);
        _encoder.EncodeFailed += (sender, e) => EncodeFailed.Invoke(this, e);
    }
    
    public void Initialize(
        MetasiaProject project,
        TimelineObject timeline,
        IImageFileAccessor imageFileAccessor,
        IVideoFileAccessor videoFileAccessor,
        string projectPath)
    {
        _encoder.Initialize(project, timeline, imageFileAccessor, videoFileAccessor, projectPath);
    }

    public void SetOutputPath(string outputPath)
    {
        ArgumentException.ThrowIfNullOrEmpty(outputPath);
        OutputPath = outputPath;
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
        _encoder.Dispose();
        _encoder.StatusChanged -= (sender, e) => OnStatusChanged();
        _encoder.EncodeStarted -= (sender, e) => EncodeStarted.Invoke(sender, e);
        _encoder.EncodeCompleted -= (sender, e) => EncodeCompleted.Invoke(sender, e);
        _encoder.EncodeFailed -= (sender, e) => EncodeFailed.Invoke(sender, e);
    }
}
