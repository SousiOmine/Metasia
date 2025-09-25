using Metasia.Core.Objects;
using System;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class AddClipCommand : IEditCommand
    {
        public string Description => "クリップの追加";

        private readonly ClipObject _targetObject;
        private readonly LayerObject _ownerLayer;

        public AddClipCommand(LayerObject ownerLayer, ClipObject targetObject)
        {
            _ownerLayer = ownerLayer ?? throw new ArgumentNullException(nameof(ownerLayer));
            _targetObject = targetObject ?? throw new ArgumentNullException(nameof(targetObject));
        }

        public void Execute()
        {
            // オブジェクトをレイヤーに追加
            if (!_ownerLayer.Objects.Contains(_targetObject))
            {
                _ownerLayer.Objects.Add(_targetObject);
            }
        }

        public void Undo()
        {
            // オブジェクトをレイヤーから削除
            if (_ownerLayer.Objects.Contains(_targetObject))
            {
                _ownerLayer.Objects.Remove(_targetObject);
            }
        }
    }
}