using Metasia.Core.Coordinate;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class CoordPointFrameChangeCommand : IEditCommand
{
    public string Description => "CoordPointのフレームを変更";

    private readonly MetaNumberParam<double> _targetMetaNumberParam;
    private readonly CoordPoint _targetCoordPoint;
    private readonly int _beforeFrame;
    private readonly int _frame;

    public CoordPointFrameChangeCommand(MetaNumberParam<double> targetMetaNumberParam, CoordPoint targetCoordPoint, int beforeFrame, int frame)
    {
        _targetMetaNumberParam = targetMetaNumberParam;
        _targetCoordPoint = targetCoordPoint;
        _beforeFrame = beforeFrame;
        _frame = frame;
    }

    public void Execute()
    {
        _targetCoordPoint.Frame = _frame;
        _targetMetaNumberParam.UpdatePoint(_targetCoordPoint);
    }

    public void Undo()
    {
        _targetCoordPoint.Frame = _beforeFrame;
        _targetMetaNumberParam.UpdatePoint(_targetCoordPoint);
    }
}