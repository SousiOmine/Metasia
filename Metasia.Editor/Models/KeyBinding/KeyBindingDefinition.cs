using Metasia.Editor.Services.Notification;
using Metasia.Editor.Models.States;
using Metasia.Editor.Models.EditCommands;
using Avalonia.Input;

namespace Metasia.Editor.Models.KeyBinding
{
    /// <summary>
    /// コマンドとキーボードショートカットのペアを格納するクラス
    /// </summary>
    public class KeyBindingDefinition
    {
        public string CommandId { get; set; } = string.Empty;
        public KeyGesture? Gesture { get; set; }
    }
}