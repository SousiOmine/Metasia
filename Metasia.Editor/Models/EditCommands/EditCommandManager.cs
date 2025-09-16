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

        private IEditCommand? lastPreviewCommand = null;

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public event EventHandler<IEditCommand> CommandExecuted = delegate { };
        public event EventHandler<IEditCommand> CommandPreviewExecuted = delegate { };
        public event EventHandler<IEditCommand> CommandUndone = delegate { };
        public event EventHandler<IEditCommand> CommandRedone = delegate { };

        public void Execute(IEditCommand command)
        {
            PreviewUndo();

            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();
            
            CommandExecuted?.Invoke(this, command);

            Console.WriteLine("Execute: " + command.Description);
        }

        public void PreviewExecute(IEditCommand command)
        {
            PreviewUndo();
            command.Execute();
            lastPreviewCommand = command;
            CommandPreviewExecuted?.Invoke(this, command);

            Console.WriteLine("PreviewExecute: " + command.Description);
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                PreviewUndo();
                var command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
                
                CommandUndone?.Invoke(this, command);

                
            }
            Console.WriteLine("Undo");
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                PreviewUndo();
                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
                
                CommandRedone?.Invoke(this, command);

                
            }
            Console.WriteLine("Redo");
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            lastPreviewCommand = null;
        }

        private void PreviewUndo()
        {
            lastPreviewCommand?.Undo();
            lastPreviewCommand = null;
        }
    }
}
