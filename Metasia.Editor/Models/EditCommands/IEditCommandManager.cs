using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.Models.EditCommands
{
    public interface IEditCommandManager
    {
        /// <summary>
        /// Undoできるかどうか
        /// </summary>
        public bool CanUndo { get; }

        /// <summary>
        /// Redoできるかどうか
        /// </summary>
        public bool CanRedo { get; }

        /// <summary>
        /// コマンドが実行されたときに発火するイベント
        /// </summary>
        event EventHandler<IEditCommand> CommandExecuted;

        /// <summary>
        /// プレビューに反映するためのコマンドが実行されたときに発火するイベント
        /// </summary>
        event EventHandler<IEditCommand> CommandPreviewExecuted;

        /// <summary>
        /// Undoが実行されたときに発火するイベント
        /// </summary>
        event EventHandler<IEditCommand> CommandUndone;

        /// <summary>
        /// Redoが実行されたときに発火するイベント
        /// </summary>
        event EventHandler<IEditCommand> CommandRedone;

        /// <summary>
        /// コマンドを実行する
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        public void Execute(IEditCommand command);

        /// <summary>
        /// プレビューに反映するためのコマンドを実行する
        /// </summary>
        /// <param name="command">実行するコマンド</param>
        public void PreviewExecute(IEditCommand command);

        /// <summary>
        /// Undoを実行する
        /// </summary>
        public void Undo();
        

        /// <summary>
        /// Undoしたコマンドを再実行(Redo)する
        /// </summary>
        public void Redo();

        /// <summary>
        /// 履歴をクリアする
        /// </summary>
        public void Clear();
    }
}
