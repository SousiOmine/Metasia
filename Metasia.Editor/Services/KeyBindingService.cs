using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Input;
using Metasia.Editor.Models.KeyBinding;

namespace Metasia.Editor.Services
{
    public class KeyBindingService : IKeyBindingService
    {
        private List<KeyBindingDefinition> _keyBindings { get; } = new List<KeyBindingDefinition>();

        private Dictionary<string, ICommand> _commands { get; } = new Dictionary<string, ICommand>();

        public void ApplyKeyBindings(Window target)
        {
            // コマンドの中からidの一致するキーバインドがあるやつのショートカットを登録していく
        }

        public void RegisterCommand(string commandId, ICommand command)
        {
            _commands.Add(commandId, command);
        }
    }
}