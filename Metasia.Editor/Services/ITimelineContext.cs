

using Metasia.Core.Objects;
using Metasia.Editor.Models.EditCommands;

namespace Metasia.Editor.Services
{
    /// <summary>
    /// タイムラインコンテキストを定義するインターフェース
    /// </summary>
    public interface ITimelineContext
    {
        /// <summary>
        /// タイムラインを取得する
        /// </summary>
        Timeline Timeline { get; }

        /// <summary>
        /// ターゲットレイヤーを取得する
        /// </summary>
        LayerObject TargetLayer { get; }

        /// <summary>
        /// 編集コマンドを実行する
        /// </summary>
        void RunEditCommand(IEditCommand command);
    }
}

