using System.Collections.Generic;
using Avalonia.Input;
using Metasia.Editor.Models.KeyBinding;

namespace Metasia.Editor.Services.KeyBinding
{
    /// <summary>
    /// デフォルトキーバインディングプロバイダーの基底クラス
    /// プラットフォーム共通の設定を提供
    /// </summary>
    public abstract class DefaultKeyBindingProviderBase : IDefaultKeyBindingProvider
    {
        /// <summary>
        /// プラットフォーム共通のキーバインディングを取得
        /// </summary>
        protected virtual List<KeyBindingDefinition> GetCommonKeyBindings()
        {
            return new List<KeyBindingDefinition>
            {
                new KeyBindingDefinition
                {
                    CommandId = "OverrideSaveEditingProject",
                    Gesture = GetSaveGesture()
                },
                new KeyBindingDefinition
                {
                    CommandId = "LoadEditingProject",
                    Gesture = GetOpenGesture()
                },
                new KeyBindingDefinition
                {
                    CommandId = "CreateNewProject",
                    Gesture = GetNewGesture()
                }
            };
        }

        /// <summary>
        /// プラットフォーム共通の修飾キー定義を取得
        /// </summary>
        protected virtual List<ModifierKeyDefinition> GetCommonModifierKeys()
        {
            return new List<ModifierKeyDefinition>
            {
                new ModifierKeyDefinition
                {
                    ActionId = "MultiSelectClip",
                    Modifier = GetMultiSelectModifier(),
                    Description = "クリップの複数選択"
                },
                new ModifierKeyDefinition
                {
                    ActionId = "ConstrainedMove",
                    Modifier = KeyModifiers.Shift,
                    Description = "水平/垂直方向への移動制限"
                },
                new ModifierKeyDefinition
                {
                    ActionId = "DuplicateOnDrag",
                    Modifier = KeyModifiers.Alt,
                    Description = "ドラッグ時に複製"
                }
            };
        }

        // プラットフォーム固有のジェスチャーを取得する抽象メソッド
        protected abstract KeyGesture GetUndoGesture();
        protected abstract KeyGesture GetRedoGesture();
        protected abstract KeyGesture GetSaveGesture();
        protected abstract KeyGesture GetOpenGesture();
        protected abstract KeyGesture GetNewGesture();
        protected abstract KeyModifiers GetMultiSelectModifier();

        // 実装クラスで具体的な設定を組み立てる
        public abstract List<KeyBindingDefinition> GetDefaultKeyBindings();
        public abstract List<ModifierKeyDefinition> GetDefaultModifierKeys();
    }
}
