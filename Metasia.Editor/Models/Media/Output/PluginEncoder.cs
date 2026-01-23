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

    private readonly IMediaOutputPlugin _plugin;

    public PluginEncoder(IMediaOutputPlugin plugin)
    {
        _plugin = plugin;

        Name = _plugin.Name;
        SupportedExtensions = _plugin.SupportedExtensions;
        
        _plugin.Encoder.StatusChanged += (sender, e) => OnStatusChanged();
        _plugin.Encoder.EncodeStarted += (sender, e) => EncodeStarted.Invoke(this, e);
        _plugin.Encoder.EncodeCompleted += (sender, e) => EncodeCompleted.Invoke(this, e);
        _plugin.Encoder.EncodeFailed += (sender, e) => EncodeFailed.Invoke(this, e);
    }
    
    public void Initialize(
        MetasiaProject project,
        TimelineObject timeline,
        IImageFileAccessor imageFileAccessor,
        IVideoFileAccessor videoFileAccessor,
        string projectPath)
    {
        _plugin.Encoder.Initialize(project, timeline, imageFileAccessor, videoFileAccessor, projectPath);
    }

    public void CancelRequest()
    {
        _plugin.Encoder.CancelRequest();
    }

    public void Start()
    {
        _plugin.Encoder.Start();
    }

    private void OnStatusChanged()
    {
        ProgressRate = _plugin.Encoder.ProgressRate;
        Status = _plugin.Encoder.Status;
        StatusChanged.Invoke(this, EventArgs.Empty);
    }
    
    public void Dispose()
    {
        _plugin.Encoder.Dispose();
        _plugin.Encoder.StatusChanged -= (sender, e) => OnStatusChanged();
        _plugin.Encoder.EncodeStarted -= (sender, e) => EncodeStarted.Invoke(sender, e);
        _plugin.Encoder.EncodeCompleted -= (sender, e) => EncodeCompleted.Invoke(sender, e);
        _plugin.Encoder.EncodeFailed -= (sender, e) => EncodeFailed.Invoke(sender, e);
    }
}
