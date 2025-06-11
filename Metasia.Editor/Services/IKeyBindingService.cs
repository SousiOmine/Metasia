using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
namespace Metasia.Editor.Services
{
    public interface IKeyBindingService
    {
    /// <summary>
    /// MainWindowとかから呼び出してショートカットキーを適用
    /// </summary>
    /// <param name="target"></param>
    public void ApplyKeyBindings(Window target);

    /// <summary>
    /// コマンドを登録 各ViewModelから呼び出してコマンドを登録すると想定
    /// </summary>
    /// <param name="commandId"></param>
    /// <param name="command"></param>
    public void RegisterCommand(string commandId, ICommand command);
    }
}
