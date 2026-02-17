using System.Linq;
using Avalonia.Input;
using Metasia.Core.Media;
using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;
using Metasia.Editor.Models.EditCommands.Commands;
using Metasia.Editor.Models.States;

namespace Metasia.Editor.Models.DragDrop.Handlers;

/// <summary>
/// プロジェクト内ファイルのドラッグアンドドロップを処理するハンドラ
/// </summary>
public class ProjectFileDropHandler : IDropHandler
{
    private const int DefaultClipLength = 150;

    public const string ProjectFileFormat = "ProjectFile";

    private readonly IProjectState _projectState;

    public int Priority => 40;

    public ProjectFileDropHandler(IProjectState projectState)
    {
        _projectState = projectState;
    }

    public bool CanHandle(IDataObject data, DropTargetContext context)
    {
        return data.Get(ProjectFileFormat) is ProjectFileDropData;
    }

    public DropPreviewResult HandleDragOver(IDataObject data, DropTargetContext context)
    {
        var dropData = data.Get(ProjectFileFormat) as ProjectFileDropData;
        if (dropData == null) return DropPreviewResult.None;

        if (!IsMediaPathValid(dropData.MediaPath)) return DropPreviewResult.None;

        return DropPreviewResult.Copy();
    }

    public IEditCommand? HandleDrop(IDataObject data, DropTargetContext context)
    {
        var dropData = data.Get(ProjectFileFormat) as ProjectFileDropData;
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

/// <summary>
/// プロジェクト内ファイルのドラッグデータ
/// </summary>
public class ProjectFileDropData
{
    public MediaPath MediaPath { get; }

    public ProjectFileDropData(MediaPath mediaPath)
    {
        MediaPath = mediaPath;
    }
}