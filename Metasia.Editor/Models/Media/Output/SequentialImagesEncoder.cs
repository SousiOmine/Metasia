using System;
using Metasia.Core.Encode;

namespace Metasia.Editor.Models.Media.Output;

public class SequentialImagesEncoder : EncoderBase, IEditorEncoder
{
    public string Name { get; } = "連番画像出力";
    public string[] SupportedExtensions { get; } = new[] { ".png", ".bmp", ".jpg" };
    public override double ProgressRate { get; }
    
    public override event EventHandler<EventArgs> StatusChanged = delegate {};
    public override event EventHandler<EventArgs> EncodeStarted = delegate {};
    public override event EventHandler<EventArgs> EncodeCompleted = delegate {};
    public override event EventHandler<EventArgs> EncodeFailed = delegate {};
    
    public override void CancelRequest()
    {
        
    }
    
    public override void Start()
    {
        
    }
}
