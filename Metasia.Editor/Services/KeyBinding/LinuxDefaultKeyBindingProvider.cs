using System.Collections.Generic;
using Avalonia.Input;
using Metasia.Editor.Models.KeyBinding;

namespace Metasia.Editor.Services.KeyBinding
{
    /// <summary>
    /// Linux用のデフォルトキーバインディング設定
    /// LinuxではControlキーを主に使用し、一部のキーバインディングが異なる
    /// </summary>
    public class LinuxDefaultKeyBindingProvider : DefaultKeyBindingProviderBase
    {
        protected override KeyGesture GetUndoGesture()
        {
            return new KeyGesture(Key.Z, KeyModifiers.Control);
        }

        protected override KeyGesture GetRedoGesture()
        {
            // LinuxではCtrl+Shift+Zが一般的
            return new KeyGesture(Key.Z, KeyModifiers.Control | KeyModifiers.Shift);
        }

        protected override KeyGesture GetSaveGesture()
        {
            return new KeyGesture(Key.S, KeyModifiers.Control);
        }

        protected override KeyGesture GetOpenGesture()
        {
            return new KeyGesture(Key.O, KeyModifiers.Control);
        }

        protected override KeyGesture GetNewGesture()
        {
            return new KeyGesture(Key.N, KeyModifiers.Control);
        }

        protected override KeyModifiers GetMultiSelectModifier()
        {
            return KeyModifiers.Control;
        }

        public override List<KeyBindingDefinition> GetDefaultKeyBindings()
        {
            var keyBindings = GetCommonKeyBindings();
            
            // Linux固有のキーバインディングを追加
            keyBindings.AddRange(new List<KeyBindingDefinition>
            {
                new KeyBindingDefinition
                {
                    CommandId = "Undo",
                    Gesture = GetUndoGesture()
                },
                new KeyBindingDefinition
                {
                    CommandId = "Redo",
                    Gesture = GetRedoGesture()
                }
            });

            return keyBindings;
        }

        public override List<ModifierKeyDefinition> GetDefaultModifierKeys()
        {
            var modifierKeys = GetCommonModifierKeys();
            
            // Linux固有の修飾キー設定があれば追加
            // 例：特定のLinuxデスクトップ環境向けの設定
            
            return modifierKeys;
        }
    }
} 