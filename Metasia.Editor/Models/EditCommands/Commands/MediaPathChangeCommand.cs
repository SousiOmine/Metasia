using System;
using Metasia.Core.Media;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class MediaPathChangeCommand : IEditCommand
{
    public string Description { get; } = "MediaPathChangeCommand";

    private readonly MediaPath _targetMediaPath;
    private readonly MediaPath _oldPath;
    private readonly MediaPath _newPath;

    public MediaPathChangeCommand(MediaPath targetMediaPath, MediaPath oldPath, MediaPath newPath)
    {
        ArgumentNullException.ThrowIfNull(targetMediaPath);
        ArgumentNullException.ThrowIfNull(oldPath);
        ArgumentNullException.ThrowIfNull(newPath);
        _targetMediaPath = targetMediaPath;
        _oldPath = oldPath;
        _newPath = newPath;
    }

    public MediaPathChangeCommand(MediaPath targetMediaPath, MediaPath newPath)
    {
        ArgumentNullException.ThrowIfNull(targetMediaPath);
        ArgumentNullException.ThrowIfNull(newPath);
        _targetMediaPath = targetMediaPath;
        _newPath = newPath;
        _oldPath = new MediaPath
        {
            FileName = _targetMediaPath.FileName,
            Directory = _targetMediaPath.Directory,
            PathType = _targetMediaPath.PathType
        };
    }

    public void Execute()
    {
        _targetMediaPath.FileName = _newPath.FileName;
        _targetMediaPath.Directory = _newPath.Directory;
        _targetMediaPath.PathType = _newPath.PathType;
    }

    public void Undo()
    {
        _targetMediaPath.FileName = _oldPath.FileName;
        _targetMediaPath.Directory = _oldPath.Directory;
        _targetMediaPath.PathType = _oldPath.PathType;
    }
}
