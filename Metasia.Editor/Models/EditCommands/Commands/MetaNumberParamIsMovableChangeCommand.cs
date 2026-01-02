using Metasia.Core.Objects.Parameters;
using System.Numerics;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class MetaNumberParamIsMovableChangeCommand<T> : IEditCommand where T : struct, INumber<T>
{
    public string Description => "移動の有効無効変更";

    private readonly MetaNumberParam<T> _targetParam;
    private readonly bool _afterIsMovable;
    private readonly bool _beforeIsMovable;

    /// <summary>
    /// MetaNumberParamのIsMovableを変更するコマンド
    /// </summary>
    /// <param name="targetParam">対象のMetaNumberParam</param>
    /// <param name="isMovable">変更後のIsMovable値</param>
    public MetaNumberParamIsMovableChangeCommand(MetaNumberParam<T> targetParam, bool isMovable)
    {
        _targetParam = targetParam;
        _afterIsMovable = isMovable;
        _beforeIsMovable = targetParam.IsMovable;
    }

    public void Execute()
    {
        _targetParam.IsMovable = _afterIsMovable;
    }

    public void Undo()
    {
        _targetParam.IsMovable = _beforeIsMovable;
    }
}
