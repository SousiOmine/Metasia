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
            return _runningEncoders.AsReadOnly();
        }
    }

    public int ConcurrentEncodeCount { get; set; } = 1;

    private List<IEditorEncoder> _runningEncoders = new();

    public void QueueEncode(IEditorEncoder encoder, string outputPath)
    {
        encoder.SetOutputPath(outputPath);
        _runningEncoders.Add(encoder);
        if (encoder.Status == IEncoder.EncoderState.Waiting)
        {
            encoder.Start();
        }
    }

    public void Cancel(IEditorEncoder encoder)
    {
        _runningEncoders.Remove(encoder);
        encoder.CancelRequest();
    }

    public void Delete(IEditorEncoder encoder)
    {
        _runningEncoders.Remove(encoder);
        if (encoder.Status == IEncoder.EncoderState.Waiting || encoder.Status == IEncoder.EncoderState.Encoding)
        {
            Cancel(encoder);
        }
        _runningEncoders.Remove(encoder);
    }
    
    public void ClearQueue()
    {
        foreach (var encoder in _runningEncoders)
        {
            if (encoder.Status == IEncoder.EncoderState.Waiting)
            {
                Cancel(encoder);
            }
        }
        _runningEncoders.Clear();
    }
}