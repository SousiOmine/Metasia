using System;
using System.Threading;
using System.Threading.Tasks;
using Metasia.Core.Encode;

namespace Metasia.Editor.Models.Media.Output;

public class SequentialImagesEncoder : EncoderBase, IEditorEncoder
{
    public string Name { get; } = "連番画像出力";
    public string[] SupportedExtensions { get; } = ["*.png", "*.bmp", "*.jpg", "*.jpeg"];
    public override double ProgressRate { get; protected set; }
    
    public override event EventHandler<EventArgs> StatusChanged = delegate {};
    public override event EventHandler<EventArgs> EncodeStarted = delegate {};
    public override event EventHandler<EventArgs> EncodeCompleted = delegate {};
    public override event EventHandler<EventArgs> EncodeFailed = delegate {};
    
    public string OutputPath { get; private set; } = string.Empty;

    private string _outputFileFolder = string.Empty;
    private string _outputFileName = string.Empty;
    private string _outputFileExtension = string.Empty;

    private CancellationTokenSource _cts = new();
    
    public void SetOutputPath(string outputPath)
    {
        if (Status != IEncoder.EncoderState.Waiting)
        {
            throw new InvalidOperationException("エンコーダーが待機状態ではありません");
        }
        OutputPath = outputPath;
        _outputFileFolder = System.IO.Path.GetDirectoryName(outputPath) ?? string.Empty;
        _outputFileName = System.IO.Path.GetFileNameWithoutExtension(outputPath);
        _outputFileExtension = System.IO.Path.GetExtension(outputPath);
    }
    
    public override void CancelRequest()
    {
        _cts.Cancel();
        Status = IEncoder.EncoderState.Canceled;
        StatusChanged?.Invoke(this, EventArgs.Empty);
    }
    
    public override void Start()
    {
        Status = IEncoder.EncoderState.Encoding;
        StatusChanged?.Invoke(this, EventArgs.Empty);
        Task.Run(() => OutputFramesAsync(_cts.Token));
    }

    private async Task OutputFramesAsync(CancellationToken ct)
    {
        int index = 0;
        await foreach (var frame in GetFramesAsync(0, FrameCount - 1, ct))
        {
            using var data = frame.Encode(GetSKEncodedImageFormat(_outputFileExtension), 90);
            using var stream = System.IO.File.Create(System.IO.Path.Combine(_outputFileFolder, $"{_outputFileName}_{index}{_outputFileExtension}"));
            data.SaveTo(stream);
            index++;

            ProgressRate = (double)index / FrameCount;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }

        Status = IEncoder.EncoderState.Completed;
        StatusChanged?.Invoke(this, EventArgs.Empty);
    }
    
    private static SkiaSharp.SKEncodedImageFormat GetSKEncodedImageFormat(string extension)
    {
        return extension.ToLower() switch
        {
            ".png" => SkiaSharp.SKEncodedImageFormat.Png,
            ".bmp" => SkiaSharp.SKEncodedImageFormat.Bmp,
            ".jpg" or ".jpeg" => SkiaSharp.SKEncodedImageFormat.Jpeg,
            _ => SkiaSharp.SKEncodedImageFormat.Png
        };
    }
}
