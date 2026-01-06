using System;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class AddCoordPointCommand : IEditCommand
{
    public string Description => "CoordPointの追加";

    private readonly MetaNumberParam<double> _targetParam;
    private readonly CoordPoint _coordPoint;

    public AddCoordPointCommand(MetaNumberParam<double> targetParam, CoordPoint coordPoint)
    {
        _targetParam = targetParam ?? throw new ArgumentNullException(nameof(targetParam));
        _coordPoint = coordPoint ?? throw new ArgumentNullException(nameof(coordPoint));
    }

    public void Execute()
    {
        _targetParam.AddPoint(_coordPoint);
    }

    public void Undo()
    {
        _targetParam.RemovePoint(_coordPoint);
    }
}
