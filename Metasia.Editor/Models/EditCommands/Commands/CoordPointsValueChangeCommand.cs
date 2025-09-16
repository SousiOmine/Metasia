using System;
using System.Collections.Generic;
using System.Linq;
using Metasia.Core.Coordinate;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class CoordPointsValueChangeCommand : IEditCommand
{
    public record CoordPointValueChangeInfo(MetaNumberParam<double> targetMetaNumberParam, CoordPoint targetCoordPoint, double valueDifference);

    public string Description => "CoordPointsの値を変更";

    private readonly IEnumerable<CoordPointValueChangeInfo> _changeInfos;

    public CoordPointsValueChangeCommand(IEnumerable<CoordPointValueChangeInfo> changeInfos)
    {
        if(!changeInfos.Any())
        {
            throw new ArgumentException("changeInfos is empty");
        }
        _changeInfos = changeInfos;
    }

    public void Execute()
    {
        foreach(var changeInfo in _changeInfos)
        {
            var newCoordPoint = new CoordPoint(){
                Id = changeInfo.targetCoordPoint.Id,
                Frame = changeInfo.targetCoordPoint.Frame,
                Value = changeInfo.valueDifference + changeInfo.targetCoordPoint.Value,
                InterpolationLogic = changeInfo.targetCoordPoint.InterpolationLogic.HardCopy()
            };
            changeInfo.targetMetaNumberParam.UpdatePoint(newCoordPoint);
        }
    }

    public void Undo()
    {
        foreach(var changeInfo in _changeInfos)
        {
            var newCoordPoint = new CoordPoint(){
                Id = changeInfo.targetCoordPoint.Id,
                Frame = changeInfo.targetCoordPoint.Frame,
                Value = changeInfo.targetCoordPoint.Value - changeInfo.valueDifference,
                InterpolationLogic = changeInfo.targetCoordPoint.InterpolationLogic.HardCopy()
            };
            changeInfo.targetMetaNumberParam.UpdatePoint(newCoordPoint);
        }
    }
}