using System;
using System.Collections.Generic;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands;

/// <summary>
/// MetaEnumParamの値を変更するコマンド
/// </summary>
public class EnumValueChangeCommand : IEditCommand
{
    private readonly string _propertyIdentifier;
    private readonly MetaEnumParam _targetParam;
    private readonly int _oldIndex;
    private readonly int _newIndex;

    public string Description => $"Change {_propertyIdentifier} from {_oldIndex} to {_newIndex}";

    public EnumValueChangeCommand(
        string propertyIdentifier,
        MetaEnumParam targetParam,
        int oldIndex,
        int newIndex)
    {
        ArgumentNullException.ThrowIfNull(propertyIdentifier);
        ArgumentNullException.ThrowIfNull(targetParam);

        if (oldIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(oldIndex), "oldIndex must be non-negative.");

        if (newIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(newIndex), "newIndex must be non-negative.");

        if (oldIndex >= targetParam.Options.Count)
            throw new ArgumentOutOfRangeException(nameof(oldIndex), "oldIndex is out of range.");

        if (newIndex >= targetParam.Options.Count)
            throw new ArgumentOutOfRangeException(nameof(newIndex), "newIndex is out of range.");

        _propertyIdentifier = propertyIdentifier;
        _targetParam = targetParam;
        _oldIndex = oldIndex;
        _newIndex = newIndex;
    }

    public void Execute()
    {
        _targetParam.SelectedIndex = _newIndex;
    }

    public void Undo()
    {
        _targetParam.SelectedIndex = _oldIndex;
    }

    public void Redo()
    {
        Execute();
    }
}
