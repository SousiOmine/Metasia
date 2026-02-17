using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Metasia.Core.Encode;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Services.Notification;

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

    private readonly INotificationService _notificationService;
    private List<IEditorEncoder> _encoders = new();

    public EncodeService(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void QueueEncode(IEditorEncoder encoder)
    {
        _encoders.Add(encoder);
        encoder.StatusChanged += EncoderStatusChanged;
        encoder.EncodeCompleted += OnEncodeCompleted;
        encoder.EncodeFailed += OnEncodeFailed;
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
        UnsubscribeEncoderEvents(encoder);
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
            UnsubscribeEncoderEvents(encoder);
            if (encoder.Status == IEncoder.EncoderState.Waiting)
            {
                Cancel(encoder);
            }
            encoder.Dispose();
        }
        _encoders.Clear();
        QueueUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void UnsubscribeEncoderEvents(IEditorEncoder encoder)
    {
        encoder.StatusChanged -= EncoderStatusChanged;
        encoder.EncodeCompleted -= OnEncodeCompleted;
        encoder.EncodeFailed -= OnEncodeFailed;
    }

    private void EncoderStatusChanged(object? sender, EventArgs e)
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

        if (sender is IEditorEncoder encoder && encoder.Status == IEncoder.EncoderState.Canceled)
        {
            var outputFilename = Path.GetFileName(encoder.OutputPath);
            _notificationService.ShowInfo(
                "エンコードキャンセル",
                $"{outputFilename} のエンコードがキャンセルされました"
            );
        }
    }

    private void OnEncodeCompleted(object? sender, EventArgs e)
    {
        if (sender is IEditorEncoder encoder)
        {
            var outputFilename = Path.GetFileName(encoder.OutputPath);
            var outputPath = encoder.OutputPath;
            _notificationService.ShowSuccess(
                "エンコード完了",
                $"{outputFilename} のエンコードが完了しました",
                () => OpenOutputFolder(outputPath)
            );
        }
    }

    private void OnEncodeFailed(object? sender, EventArgs e)
    {
        if (sender is IEditorEncoder encoder)
        {
            var outputFilename = Path.GetFileName(encoder.OutputPath);
            _notificationService.ShowError(
                "エンコード失敗",
                $"{outputFilename} のエンコードに失敗しました"
            );
        }
    }

    private void OpenOutputFolder(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = directory,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError(
                "フォルダを開けません",
                $"出力フォルダを開く際にエラーが発生しました。\n{ex}"
            );
        }
    }
}
