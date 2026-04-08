using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using System.Linq;
using Avalonia.Input;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Editor.Abstractions.EditCommands;
using Metasia.Editor.Models.DragDropData;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Abstractions.States;

namespace Metasia.Editor.Models.DragDrop.Handlers;

public class ProjectFileDropHandler : IDropHandler
{
    private const int DefaultClipLength = 150;

    private readonly IProjectState _projectState;

    public int Priority => 40;

    public ProjectFileDropHandler(IProjectState projectState)
    {
        _projectState = projectState;
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

    public IEditCommand? HandleDrop(IDataTransfer data, DropTargetContext context)
    {
        var id = data.TryGetValue(DragDropFormats.ProjectFile);
        var dropData = DragDropFormats.RetrieveData<ProjectFileDropData>(id);
        if (dropData == null) return null;

        if (!IsMediaPathValid(dropData.MediaPath)) return null;

        ClipObject? clip = CreateClipFromMediaPath(dropData.MediaPath);
        if (clip == null) return null;

        clip.StartFrame = context.TargetFrame;
        clip.EndFrame = clip.StartFrame + DefaultClipLength - 1;

        return new AddClipCommand(context.TargetLayer, clip);
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