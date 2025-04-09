using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.Models.EditCommands
{
    /// <summary>
    /// 戻る進むの履歴を保存する編集をEditCommandでやるクラス
    /// </summary>
    public class EditCommandManager : IEditCommandManager
    {
        private readonly Stack<IEditCommand> undoStack = new();
        private readonly Stack<IEditCommand> redoStack = new();

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public event EventHandler<IEditCommand> CommandExecuted = delegate { };
        public event EventHandler<IEditCommand> CommandUndone = delegate { };
        public event EventHandler<IEditCommand> CommandRedone = delegate { };

        public void Execute(IEditCommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
            
            CommandExecuted?.Invoke(this, command);
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
                
                CommandUndone?.Invoke(this, command);
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
                
                CommandRedone?.Invoke(this, command);
            }
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }
}
