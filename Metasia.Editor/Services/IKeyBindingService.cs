using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;

namespace Metasia.Editor.Services
{
    public interface IKeyBindingService
    {
        void ApplyKeyBindings(Window target);
        void RegisterCommand(string commandId, ICommand command);

        /// <summary>
        /// 指定されたコマンドIDのコマンドを登録解除する
        /// </summary>
        /// <param name="commandId">解除するコマンドのID</param>
        /// <returns>コマンドが正常に解除された場合はtrue</returns>
        bool UnregisterCommand(string commandId);

        /// <summary>
        /// すべての登録されたコマンドをクリアする
        /// </summary>
        void ClearCommands();

        /// <summary>
        /// 現在のターゲットウィンドウのキーバインディングを更新する
        /// </summary>
        void RefreshKeyBindings();

        KeyModifiers? GetModifierForAction(string actionId);
        bool IsModifierKeyPressed(KeyModifiers modifier, KeyModifiers currentModifiers);
        void SaveKeyBindings();
    }
}
