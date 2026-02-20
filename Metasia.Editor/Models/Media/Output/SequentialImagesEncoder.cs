using System;
using System.Threading;
using System.Threading.Tasks;
using Metasia.Core.Encode;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Project;

namespace Metasia.Editor.Models.Media.Output;

public class SequentialImagesEncoder : EncoderBase, IEditorEncoder
{
    public string Name { get; } = "連番画像出力";
    public string[] SupportedExtensions { get; } = ["*.png", "*.bmp", "*.jpg", "*.jpeg"];
    public override double ProgressRate { get; protected set; }

    public override event EventHandler<EventArgs> StatusChanged = delegate { };
    public override event EventHandler<EventArgs> EncodeStarted = delegate { };
    public override event EventHandler<EventArgs> EncodeCompleted = delegate { };
    public override event EventHandler<EventArgs> EncodeFailed = delegate { };

    public string OutputPath { get; private set; } = string.Empty;

    private string _outputFileFolder = string.Empty;
    private string _outputFileName = string.Empty;
    private string _outputFileExtension = string.Empty;

    private CancellationTokenSource _cts = new();
    private Task? _encodingTask;

    public override void Initialize(
        MetasiaProject project,
        TimelineObject timeline,
        IImageFileAccessor imageFileAccessor,
        IVideoFileAccessor videoFileAccessor,
        IAudioFileAccessor audioFileAccessor,
        string projectPath,
        string outputPath)
    {
        base.Initialize(project, timeline, imageFileAccessor, videoFileAccessor, audioFileAccessor, projectPath, outputPath);
        OutputPath = outputPath;
        _outputFileFolder = System.IO.Path.GetDirectoryName(outputPath) ?? string.Empty;
        _outputFileName = System.IO.Path.GetFileNameWithoutExtension(outputPath);
        _outputFileExtension = System.IO.Path.GetExtension(outputPath);
    }

    public override void CancelRequest()
    {
        _cts.Cancel();
        _cts.Dispose();
        Status = IEncoder.EncoderState.Canceled;
        StatusChanged?.Invoke(this, EventArgs.Empty);
    }

    public override void Start()
    {
        if (Status != IEncoder.EncoderState.Waiting)
        {
            throw new InvalidOperationException("エンコーダーが待機状態ではありません");
        }
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        Status = IEncoder.EncoderState.Encoding;
        StatusChanged?.Invoke(this, EventArgs.Empty);
        EncodeStarted?.Invoke(this, EventArgs.Empty);
        _encodingTask = Task.Run(() => OutputFramesAsync(_cts.Token));
        _encodingTask.ContinueWith(t =>
        {
            if (t.Exception is not null)
            {
                System.Diagnostics.Debug.WriteLine($"Encoding failed: {t.Exception}");
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    private async Task OutputFramesAsync(CancellationToken ct)
    {
        try
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

            if (!ct.IsCancellationRequested)
            {
                Status = IEncoder.EncoderState.Completed;
                StatusChanged?.Invoke(this, EventArgs.Empty);
                EncodeCompleted?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Status = IEncoder.EncoderState.Canceled;
                StatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (OperationCanceledException)
        {
            Status = IEncoder.EncoderState.Canceled;
            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Status = IEncoder.EncoderState.Failed;
            StatusChanged?.Invoke(this, EventArgs.Empty);
            EncodeFailed?.Invoke(this, EventArgs.Empty);
        }
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts?.Dispose();
        }
        base.Dispose(disposing);
    }
}
