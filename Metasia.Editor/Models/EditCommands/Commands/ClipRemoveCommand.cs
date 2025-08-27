using Metasia.Core.Objects;
using System;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class ClipRemoveCommand : IEditCommand
    {
        public string Description => "クリップの削除";

        private readonly ClipObject _targetObject;
        private readonly LayerObject _ownerLayer;
        private int _objectIndex; // Undo時に元の位置に戻すためにインデックスを保持

        public ClipRemoveCommand(ClipObject targetObject, LayerObject ownerLayer)
        {
            _targetObject = targetObject;
            _ownerLayer = ownerLayer;
            _objectIndex = ownerLayer.Objects.IndexOf(targetObject);
        }

        public void Execute()
        {
            // オブジェクトをレイヤーから削除
            if (_ownerLayer.Objects.Contains(_targetObject))
            {
                _ownerLayer.Objects.Remove(_targetObject);
            }
        }

        public void Undo()
        {
            // オブジェクトを元の位置に戻す
            if (_objectIndex >= 0 && _objectIndex <= _ownerLayer.Objects.Count)
            {
                _ownerLayer.Objects.Insert(_objectIndex, _targetObject);
            }
            else
            {
                // インデックスが無効な場合は末尾に追加
                _ownerLayer.Objects.Add(_targetObject);
            }
        }
    }
}
