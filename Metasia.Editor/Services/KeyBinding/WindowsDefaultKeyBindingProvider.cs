using System.Collections.Generic;
using Avalonia.Input;
using Metasia.Editor.Models.KeyBinding;

namespace Metasia.Editor.Services.KeyBinding
{
    /// <summary>
    /// Windows用のデフォルトキーバインディング設定
    /// </summary>
    public class WindowsDefaultKeyBindingProvider : DefaultKeyBindingProviderBase
    {
        protected override KeyGesture GetUndoGesture()
        {
            return new KeyGesture(Key.Z, KeyModifiers.Control);
        }

        protected override KeyGesture GetRedoGesture()
        {
            return new KeyGesture(Key.Y, KeyModifiers.Control);
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
            
            // Windows固有のキーバインディングを追加
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
                },
                new KeyBindingDefinition
                {
                    CommandId = "PlayPauseToggle",
                    Gesture = new KeyGesture(Key.Space)
                }
            });

            return keyBindings;
        }

        public override List<ModifierKeyDefinition> GetDefaultModifierKeys()
        {
            return GetCommonModifierKeys();
        }
    }
} 