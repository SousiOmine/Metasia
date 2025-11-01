using System.Collections.Generic;
using Avalonia.Input;
using Metasia.Editor.Models.KeyBinding;

namespace Metasia.Editor.Services.KeyBinding
{
    /// <summary>
    /// Mac用のデフォルトキーバインディング設定
    /// MacではCommandキー（Meta）を主に使用
    /// </summary>
    public class MacDefaultKeyBindingProvider : DefaultKeyBindingProviderBase
    {
        protected override KeyGesture GetUndoGesture()
        {
            return new KeyGesture(Key.Z, KeyModifiers.Meta);
        }

        protected override KeyGesture GetRedoGesture()
        {
            // MacではCmd+Shift+Zが一般的
            return new KeyGesture(Key.Z, KeyModifiers.Meta | KeyModifiers.Shift);
        }

        protected override KeyGesture GetSaveGesture()
        {
            return new KeyGesture(Key.S, KeyModifiers.Meta);
        }

        protected override KeyGesture GetOpenGesture()
        {
            return new KeyGesture(Key.O, KeyModifiers.Meta);
        }

        protected override KeyGesture GetNewGesture()
        {
            return new KeyGesture(Key.N, KeyModifiers.Meta);
        }

        protected override KeyModifiers GetMultiSelectModifier()
        {
            return KeyModifiers.Meta;  // MacではCommandキーで複数選択
        }

        protected override KeyModifiers GetTimelineZoomModifier()
        {
            return KeyModifiers.Meta;  // MacではCommandキーでズーム
        }

        public override List<KeyBindingDefinition> GetDefaultKeyBindings()
        {
            var keyBindings = GetCommonKeyBindings();

            // Mac固有のキーバインディングを追加
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
            var modifierKeys = GetCommonModifierKeys();

            // Mac固有の修飾キー設定があれば追加
            // 例：Optionキー（Alt）の動作をMac風にカスタマイズ

            return modifierKeys;
        }
    }
}