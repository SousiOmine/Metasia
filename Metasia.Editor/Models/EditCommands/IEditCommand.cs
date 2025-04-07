using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metasia.Editor.Models.EditCommands
{
    public interface IEditCommand
    {
        /// <summary>
        /// コマンドが行う編集の説明
        /// </summary>
        string Description { get; }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        void Execute();

        /// <summary>
        /// 巻き戻し処理を実行
        /// </summary>
        void Undo();
    }
}
