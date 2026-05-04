using Avalonia.Input;
using Avalonia.Platform.Storage;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Abstractions.Notification;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.Media;
using Metasia.Editor.Models.Settings;
using Metasia.Editor.Models.States;
using Metasia.Editor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Metasia.Editor.Models.DragDrop.Handlers;

public class ExternalFileDropHandler : IDropHandler
{
    private const int DefaultClipLength = 150;

    private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".webm", ".wmv", ".flv" };
    private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg", ".flac", ".aac", ".m4a", ".wma" };
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".tiff", ".svg" };

    private readonly IProjectState _projectState;
    private readonly ISettingsService _settingsService;
    private readonly INotificationService _notificationService;
    private readonly MediaAccessorRouter _mediaAccessorRouter;

    public int Priority => 50;

    public ExternalFileDropHandler(
        IProjectState projectState,
        ISettingsService settingsService,
        INotificationService notificationService,
        MediaAccessorRouter mediaAccessorRouter)
    {
        _projectState = projectState;
        _settingsService = settingsService;
        _notificationService = notificationService;
        _mediaAccessorRouter = mediaAccessorRouter;
    }

    public bool CanHandle(IDataTransfer data, DropTargetContext context)
    {
        if (!data.Contains(DataFormat.File)) return false;

        var files = data.TryGetFiles();
        return files != null && files.Any(IsSupportedFile);
    }

    public DropPreviewResult HandleDragOver(IDataTransfer data, DropTargetContext context)
    {
        if (!CanHandle(data, context)) return DropPreviewResult.None;

        var files = data.TryGetFiles();
        if (files == null || !files.Any(IsSupportedFile)) return DropPreviewResult.None;

        return DropPreviewResult.Copy();
    }

    public async Task<IEditCommand?> HandleDropAsync(IDataTransfer data, DropTargetContext context)
    {
        var files = data.TryGetFiles();
        if (files == null) return null;

        var supportedFiles = files.Where(IsSupportedFile).ToList();
        if (supportedFiles.Count == 0) return null;

        var projectDir = _projectState.CurrentProject?.ProjectPath.Path;
        if (string.IsNullOrEmpty(projectDir)) return null;

        var commands = new List<IEditCommand>();
        int currentStartFrame = context.TargetFrame;
        foreach (var file in supportedFiles)
        {
            var clip = await CreateClipFromFileAsync(file, currentStartFrame, projectDir);
            if (clip != null)
            {
                commands.Add(new AddClipCommand(context.TargetLayer, clip));
                currentStartFrame = clip.EndFrame + 1;
            }
        }

        if (commands.Count == 0) return null;
        if (commands.Count == 1) return commands[0];

        return new CompositeEditCommand(commands, "複数ファイルの追加");
    }

    private bool IsSupportedFile(IStorageItem item)
    {
        if (item is not IStorageFile file) return false;
        var ext = Path.GetExtension(file.Name)?.ToLowerInvariant();
        return VideoExtensions.Contains(ext) || AudioExtensions.Contains(ext) || ImageExtensions.Contains(ext);
    }

    private async Task<ClipObject?> CreateClipFromFileAsync(IStorageItem item, int startFrame, string projectDir)
    {
        if (item is not IStorageFile file) return null;

        var ext = Path.GetExtension(file.Name)?.ToLowerInvariant();
        string filePath = file.Path.LocalPath;
        var fileName = file.Name;

        ClipObject? clip = null;

        if (VideoExtensions.Contains(ext))
        {
            clip = CreateVideoObject(filePath, fileName, projectDir);
        }
        else if (AudioExtensions.Contains(ext))
        {
            clip = CreateAudioObject(filePath, fileName, projectDir);
        }
        else if (ImageExtensions.Contains(ext))
        {
            clip = CreateImageObject(filePath, fileName, projectDir);
        }

        if (clip == null) return null;

        clip.StartFrame = startFrame;

        if (clip is VideoObject or AudioObject)
        {
            var mediaInfo = await _mediaAccessorRouter.GetMediaInfoAsync(filePath);
            if (TryCalculateMediaFrameCount(mediaInfo, out int frameCount))
            {
                clip.EndFrame = clip.StartFrame + frameCount - 1;
            }
            else
            {
                clip.EndFrame = clip.StartFrame + DefaultClipLength - 1;
            }
        }
        else
        {
            clip.EndFrame = clip.StartFrame + DefaultClipLength - 1;
        }

        return clip;
    }

    private bool TryCalculateMediaFrameCount(MediaInfoResult? mediaInfo, out int frameCount)
    {
        frameCount = 0;
        if (mediaInfo?.IsSuccessful != true || mediaInfo.Duration <= TimeSpan.Zero)
        {
            return false;
        }

        int projectFps = _projectState.CurrentProjectInfo?.Framerate ?? 60;
        frameCount = (int)Math.Ceiling(mediaInfo.Duration.TotalSeconds * projectFps);
        return frameCount > 0;
    }

    private VideoObject? CreateVideoObject(string filePath, string fileName, string projectDir)
    {
        var clip = new VideoObject
        {
            VideoPath = CreateMediaPath(filePath, fileName, projectDir, MediaType.Video)
        };
        return clip;
    }

    private AudioObject? CreateAudioObject(string filePath, string fileName, string projectDir)
    {
        var clip = new AudioObject
        {
            AudioPath = CreateMediaPath(filePath, fileName, projectDir, MediaType.Audio)
        };
        return clip;
    }

    private ImageObject? CreateImageObject(string filePath, string fileName, string projectDir)
    {
        var clip = new ImageObject
        {
            ImagePath = CreateMediaPath(filePath, fileName, projectDir, MediaType.Image)
        };
        return clip;
    }

    private MediaPath CreateMediaPath(string filePath, string fileName, string projectDir, MediaType mediaType)
    {
        var directory = Path.GetDirectoryName(filePath) ?? string.Empty;

        bool saveAsRelative = _settingsService.CurrentSettings.General.MediaPathStyle == MediaPathStyle.Relative;
        if (saveAsRelative && _projectState.CurrentProject?.ProjectFilePath == null)
        {
            _notificationService.ShowWarning(
                "プロジェクト未保存",
                "プロジェクトが保存されていないため、メディアパスを絶対パスで保存しました。保存後にもう一度ドロップすると相対パスで保存されます。");
            saveAsRelative = false;
        }

        return MediaPath.CreateFromPath(directory, fileName, projectDir, saveAsRelative);
    }
}
