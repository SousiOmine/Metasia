using System;
using System.IO;
using System.Linq;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.Models.DragDrop.Handlers;

/// <summary>
/// 外部ファイルのドラッグアンドドロップを処理するハンドラ
/// </summary>
public class ExternalFileDropHandler : IDropHandler
{
    private const int DefaultClipLength = 150;

    private static readonly string[] VideoExtensions = { ".mp4", ".avi", ".mov", ".mkv", ".webm", ".wmv", ".flv" };
    private static readonly string[] AudioExtensions = { ".mp3", ".wav", ".ogg", ".flac", ".aac", ".m4a", ".wma" };
    private static readonly string[] ImageExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".webp", ".tiff", ".svg" };

    private readonly IProjectState _projectState;

    public int Priority => 50;

    public ExternalFileDropHandler(IProjectState projectState)
    {
        _projectState = projectState;
    }

    public bool CanHandle(IDataObject data, DropTargetContext context)
    {
        if (!data.Contains(DataFormats.Files)) return false;

        var files = data.GetFiles();
        return files != null && files.Any(IsSupportedFile);
    }

    public DropPreviewResult HandleDragOver(IDataObject data, DropTargetContext context)
    {
        if (!CanHandle(data, context)) return DropPreviewResult.None;

        var files = data.GetFiles();
        if (files == null || !files.Any(IsSupportedFile)) return DropPreviewResult.None;

        return DropPreviewResult.Copy();
    }

    public IEditCommand? HandleDrop(IDataObject data, DropTargetContext context)
    {
        var files = data.GetFiles();
        if (files == null) return null;

        var supportedFiles = files.Where(IsSupportedFile).ToList();
        if (supportedFiles.Count == 0) return null;

        var projectDir = _projectState.CurrentProject?.ProjectPath.Path;
        if (string.IsNullOrEmpty(projectDir)) return null;

        var commands = supportedFiles
            .Select((file, index) => CreateClipFromFile(file, context, projectDir, index))
            .Where(cmd => cmd != null)
            .Cast<IEditCommand>()
            .ToList();

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

    private AddClipCommand? CreateClipFromFile(IStorageItem item, DropTargetContext context, string projectDir, int index)
    {
        if (item is not IStorageFile file) return null;

        var ext = Path.GetExtension(file.Name)?.ToLowerInvariant();
        string filePath;
        try
        {
            filePath = file.Path.LocalPath;
        }
        catch (InvalidOperationException)
        {
            filePath = file.Path?.ToString() ?? string.Empty;
        }
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

        clip.StartFrame = context.TargetFrame + (index * DefaultClipLength);
        clip.EndFrame = clip.StartFrame + DefaultClipLength - 1;

        return new AddClipCommand(context.TargetLayer, clip);
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
        try
        {
            var relativeDir = Path.GetRelativePath(projectDir, directory);
            return MediaPath.CreateFromPath(relativeDir, fileName);
        }
        catch (Exception)
        {
            return new MediaPath(new[] { mediaType })
            {
                FileName = fileName,
                Directory = directory
            };
        }
    }
}