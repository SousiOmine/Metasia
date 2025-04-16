// Metasia.Editor/Models/EditCommands/Commands/MoveClipCommand.cs
using Metasia.Core.Objects;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class MoveClipCommand : IEditCommand
    {
        public string Description => "クリップの移動";

        private readonly MetasiaObject _targetObject;
        private readonly LayerObject _sourceLayer;
        private readonly LayerObject _targetLayer;
        private readonly int _oldStartFrame;
        private readonly int _oldEndFrame;
        private readonly int _newStartFrame;
        private readonly int _newEndFrame;

        public MoveClipCommand(
            MetasiaObject targetObject,
            LayerObject sourceLayer,
            LayerObject targetLayer,
            int oldStartFrame, int oldEndFrame,
            int newStartFrame, int newEndFrame)
        {
            _targetObject = targetObject;
            _sourceLayer = sourceLayer;
            _targetLayer = targetLayer;
            _oldStartFrame = oldStartFrame;
            _oldEndFrame = oldEndFrame;
            _newStartFrame = newStartFrame;
            _newEndFrame = newEndFrame;
        }

        public void Execute()
        {
            // Execute は Redo 時にも呼ばれる
            // まずソースから削除 (ソースとターゲットが同じ場合は削除しない)
            if (_sourceLayer != _targetLayer)
            {
                 _sourceLayer.Objects.Remove(_targetObject);
            }

            // フレームを更新
            _targetObject.StartFrame = _newStartFrame;
            _targetObject.EndFrame = _newEndFrame;

            // ターゲットに追加 (ソースとターゲットが同じ場合は既に入っているので追加しない)
             if (_sourceLayer != _targetLayer)
            {
                // ターゲットレイヤーに既に追加されていないか確認（念のため）
                if (!_targetLayer.Objects.Contains(_targetObject))
                {
                    _targetLayer.Objects.Add(_targetObject);
                }
            }
        }

        public void Undo()
        {
             // まずターゲットから削除 (ソースとターゲットが同じ場合は削除しない)
             if (_sourceLayer != _targetLayer)
            {
                _targetLayer.Objects.Remove(_targetObject);
            }

            // フレームを元に戻す
            _targetObject.StartFrame = _oldStartFrame;
            _targetObject.EndFrame = _oldEndFrame;

            // ソースに追加 (ソースとターゲットが同じ場合は既に入っているので追加しない)
             if (_sourceLayer != _targetLayer)
            {
                 // ソースレイヤーに既に追加されていないか確認（念のため）
                if (!_sourceLayer.Objects.Contains(_targetObject))
                {
                    _sourceLayer.Objects.Add(_targetObject);
                }
            }
        }
    }
}