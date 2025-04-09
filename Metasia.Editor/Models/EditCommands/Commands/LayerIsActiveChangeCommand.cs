using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands;

public class LayerIsActiveChangeCommand: IEditCommand
{
    public string Description { get; } = string.Empty;
    
    private LayerObject _targetLayerObject;
    private bool _afterActive;
    private bool _beforeActive;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetLayerObject">有効無効を切り替えたいレイヤー</param>
    /// <param name="isActive">変更後の値</param>
    public LayerIsActiveChangeCommand(LayerObject targetLayerObject, bool isActive)
    {
        _targetLayerObject = targetLayerObject;
        _afterActive = isActive;
        _beforeActive = targetLayerObject.IsActive;
    }
    public void Execute()
    {
        _targetLayerObject.IsActive = _afterActive;
    }

    public void Undo()
    {
        _targetLayerObject.IsActive = _beforeActive;
    }
}