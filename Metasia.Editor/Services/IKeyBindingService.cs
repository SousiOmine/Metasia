using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;

namespace Metasia.Editor.Services
{
    public interface IKeyBindingService
    {
        /// <summary>
        /// MainWindowとかから呼び出してショートカットキーを適用
        /// </summary>
        /// <param name="target"></param>
        void ApplyKeyBindings(Window target);

        /// <summary>
        /// コマンドを登録 各ViewModelから呼び出してコマンドを登録すると想定
        /// </summary>
        /// <param name="commandId"></param>
        /// <param name="command"></param>
        void RegisterCommand(string commandId, ICommand command);

        /// <summary>
        /// 指定されたアクションに対応する修飾キーを取得
        /// </summary>
        /// <param name="actionId">アクションID</param>
        /// <returns>対応する修飾キー、設定がない場合はnull</returns>
        KeyModifiers? GetModifierForAction(string actionId);

        /// <summary>
        /// 指定された修飾キーが現在押されているかを確認
        /// </summary>
        /// <param name="modifier">確認する修飾キー</param>
        /// <param name="currentModifiers">現在の修飾キーの状態</param>
        /// <returns>押されている場合はtrue</returns>
        bool IsModifierKeyPressed(KeyModifiers modifier, KeyModifiers currentModifiers);
    }
}
