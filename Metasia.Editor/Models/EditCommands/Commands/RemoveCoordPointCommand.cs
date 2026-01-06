using System;
using Metasia.Core.Coordinate;
using Metasia.Core.Objects.Parameters;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class RemoveCoordPointCommand : IEditCommand
{
    public string Description => "CoordPointの削除";

    private readonly MetaNumberParam<double> _targetParam;
    private readonly CoordPoint _coordPoint;

    public RemoveCoordPointCommand(MetaNumberParam<double> targetParam, CoordPoint coordPoint)
    {
        _targetParam = targetParam ?? throw new ArgumentNullException(nameof(targetParam));
        _coordPoint = coordPoint ?? throw new ArgumentNullException(nameof(coordPoint));
    }

    public void Execute()
    {
        _targetParam.RemovePoint(_coordPoint);
    }

    public void Undo()
    {
        _targetParam.AddPoint(_coordPoint);
    }
}
