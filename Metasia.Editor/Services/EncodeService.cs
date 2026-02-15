using System;
using System.Collections.Generic;
using System.Linq;
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

    public int ConcurrentEncodeCount { get; private set; } = 1;

    private List<IEditorEncoder> _encoders = new();

    public void QueueEncode(IEditorEncoder encoder)
    {
        _encoders.Add(encoder);
        encoder.StatusChanged += encoderStatusChanged;
        QueueUpdated?.Invoke(this, EventArgs.Empty);

        int encodingCount = _encoders.Count(e => e.Status == IEncoder.EncoderState.Encoding);
        if (encoder.Status == IEncoder.EncoderState.Waiting && encodingCount < ConcurrentEncodeCount)
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
        encoder.StatusChanged -= encoderStatusChanged;
        if (encoder.Status == IEncoder.EncoderState.Waiting || encoder.Status == IEncoder.EncoderState.Encoding)
        {
            Cancel(encoder);
        }
        encoder.Dispose();
        QueueUpdated?.Invoke(this, EventArgs.Empty);
    }

    public void ClearQueue()
    {
        foreach (var encoder in _encoders)
        {
            encoder.StatusChanged -= encoderStatusChanged;
            if (encoder.Status == IEncoder.EncoderState.Waiting)
            {
                Cancel(encoder);
            }
            encoder.Dispose();
        }
        _encoders.Clear();
        QueueUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void encoderStatusChanged(object? sender, EventArgs e)
    {
        int encodingCount = _encoders.Count(enc => enc.Status == IEncoder.EncoderState.Encoding);
        if (encodingCount < ConcurrentEncodeCount)
        {
            var nextEncoder = _encoders.FirstOrDefault(enc => enc.Status == IEncoder.EncoderState.Waiting);
            if (nextEncoder is not null)
            {
                nextEncoder.Start();
            }
        }
    }
}
