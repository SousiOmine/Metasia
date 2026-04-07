using Metasia.Core.Coordinate;
using Metasia.Core.Coordinate.InterpolationLogic;
using Metasia.Editor.Abstractions.EditCommands;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class InterpolationLogicChangeCommand : IEditCommand
{
    public string Description { get; } = "移動ロジックを変更";

    private readonly InterpolationLogicBase _newLogic;
    private readonly InterpolationLogicBase _oldLogic;
    private readonly CoordPoint _targetPoint;

    public InterpolationLogicChangeCommand(InterpolationLogicBase newLogic, CoordPoint targetPoint)
    {
        _newLogic = newLogic;
        _targetPoint = targetPoint;
        _oldLogic = targetPoint.InterpolationLogic.HardCopy();
    }

    public void Execute()
    {
        _targetPoint.InterpolationLogic = _newLogic;
    }

    public void Undo()
    {
        _targetPoint.InterpolationLogic = _oldLogic;
    }
}