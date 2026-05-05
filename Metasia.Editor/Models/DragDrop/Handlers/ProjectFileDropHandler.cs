using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Core.Objects.Clips;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Abstractions.States;
using Metasia.Editor.Models.Media;

namespace Metasia.Editor.Models.DragDrop.Handlers;

public class ProjectFileDropHandler : IDropHandler
{
    private const int DefaultClipLength = 150;

    private readonly IProjectState _projectState;
    private readonly MediaAccessorRouter _mediaAccessorRouter;

    public int Priority => 40;

    public ProjectFileDropHandler(IProjectState projectState, MediaAccessorRouter mediaAccessorRouter)
    {
        _projectState = projectState;
        _mediaAccessorRouter = mediaAccessorRouter;
    }

    public bool CanHandle(IDataTransfer data, DropTargetContext context)
    {
        var id = data.TryGetValue(DragDropFormats.ProjectFile);
        return id != null && DragDropFormats.PeekData<ProjectFileDropData>(id) != null;
    }

    public DropPreviewResult HandleDragOver(IDataTransfer data, DropTargetContext context)
    {
        var id = data.TryGetValue(DragDropFormats.ProjectFile);
        var dropData = DragDropFormats.PeekData<ProjectFileDropData>(id);
        if (dropData == null) return DropPreviewResult.None;

        if (!IsMediaPathValid(dropData.MediaPath)) return DropPreviewResult.None;

        return DropPreviewResult.Copy();
    }

    public async Task<IEditCommand?> HandleDropAsync(IDataTransfer data, DropTargetContext context)
    {
        var id = data.TryGetValue(DragDropFormats.ProjectFile);
        var dropData = DragDropFormats.RetrieveData<ProjectFileDropData>(id);
        if (dropData == null) return null;

        if (!IsMediaPathValid(dropData.MediaPath)) return null;

        ClipObject? clip = CreateClipFromMediaPath(dropData.MediaPath);
        if (clip == null) return null;

        clip.StartFrame = context.TargetFrame;

        if (clip is VideoObject or AudioObject)
        {
            string fullPath = MediaPath.GetFullPath(dropData.MediaPath, _projectState.CurrentProject?.ProjectPath.Path);
            var mediaInfo = await _mediaAccessorRouter.GetMediaInfoAsync(fullPath);
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

        return new AddClipCommand(context.TargetLayer, clip);
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

    private bool IsMediaPathValid(MediaPath? mediaPath)
    {
        return mediaPath != null && !string.IsNullOrEmpty(mediaPath.FileName);
    }

    private ClipObject? CreateClipFromMediaPath(MediaPath mediaPath)
    {
        if (mediaPath.Types == null || mediaPath.Types.Length == 0) return null;

        var type = mediaPath.Types[0];

        return type switch
        {
            MediaType.Video => new VideoObject { VideoPath = mediaPath },
            MediaType.Audio => new AudioObject { AudioPath = mediaPath },
            MediaType.Image => new ImageObject { ImagePath = mediaPath },
            _ => null
        };
    }
}
