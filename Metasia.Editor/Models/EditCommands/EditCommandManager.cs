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
        public bool CanUndo => undoStack.Count > 0;

        public bool CanRedo => redoStack.Count > 0;


        private Stack<IEditCommand> undoStack = new();
        private Stack<IEditCommand> redoStack = new();

        public void Execute(IEditCommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear();  //redoは新しいコマンド実行時にクリアしちゃう
        }

        public void Undo()
        {
            if (undoStack.Count > 0)
            {
                var command = undoStack.Pop();
                command.Undo();
                redoStack.Push(command);
            }
        }

        public void Redo()
        {
            if (redoStack.Count > 0)
            {
                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
            }
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }
}
