// Metasia.Editor/Models/EditCommands/Commands/ClipResizeCommand.cs
using Metasia.Core.Objects;
using System;

namespace Metasia.Editor.Models.EditCommands.Commands
{
    public class ClipResizeCommand : IEditCommand
    {
        public string Description { get; }

        private readonly ClipObject _targetObject;
        private readonly int _oldStartFrame;
        private readonly int _oldEndFrame;
        private readonly int _newStartFrame;
        private readonly int _newEndFrame;


        public ClipResizeCommand(ClipObject targetObject, int oldStartFrame, int newStartFrame, int oldEndFrame, int newEndFrame)
        {
            _targetObject = targetObject;
            _oldStartFrame = oldStartFrame;
            _oldEndFrame = oldEndFrame;
            _newStartFrame = newStartFrame;
            _newEndFrame = newEndFrame;
        }

        public void Execute()
        {
            _targetObject.StartFrame = _newStartFrame;
            _targetObject.EndFrame = _newEndFrame;
        }

        public void Undo()
        {
            _targetObject.StartFrame = _oldStartFrame;
            _targetObject.EndFrame = _oldEndFrame;
        }
    }
}