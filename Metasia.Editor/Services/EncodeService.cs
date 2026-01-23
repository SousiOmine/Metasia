using System;
using System.Collections.Generic;
using Metasia.Core.Encode;
using Metasia.Editor.Models.Media;

namespace Metasia.Editor.Services;

public class EncodeService : IEncodeService
{
    public IReadOnlyList<IEditorEncoder> Encoders
    {
        get
        {
            return _encoders.AsReadOnly();
        }
    }

    public event EventHandler<EventArgs> QueueUpdated = delegate { };

    public int ConcurrentEncodeCount { get; set; } = 1;

    private List<IEditorEncoder> _encoders = new();

    public void QueueEncode(IEditorEncoder encoder, string outputPath)
    {
        encoder.SetOutputPath(outputPath);
        _encoders.Add(encoder);
        QueueUpdated?.Invoke(this, EventArgs.Empty);
        if (encoder.Status == IEncoder.EncoderState.Waiting)
        {
            encoder.Start();
        }
    }

    public void Cancel(IEditorEncoder encoder)
    {
        encoder.CancelRequest();
    }

    public void Delete(IEditorEncoder encoder)
    {
        _encoders.Remove(encoder);
        if (encoder.Status == IEncoder.EncoderState.Waiting || encoder.Status == IEncoder.EncoderState.Encoding)
        {
            Cancel(encoder);
        }
        _encoders.Remove(encoder);
        QueueUpdated?.Invoke(this, EventArgs.Empty);
    }
    
    public void ClearQueue()
    {
        foreach (var encoder in _encoders)
        {
            if (encoder.Status == IEncoder.EncoderState.Waiting)
            {
                Cancel(encoder);
            }
        }
        _encoders.Clear();
        QueueUpdated?.Invoke(this, EventArgs.Empty);
    }
}