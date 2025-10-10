using System.Collections.Generic;
using Metasia.Editor.Models.KeyBinding;

namespace Metasia.Editor.Services.KeyBinding
{
    /// <summary>
    /// プラットフォーム別のデフォルトキーバインディング設定を提供するインターフェース
    /// </summary>
    public interface IDefaultKeyBindingProvider
    {
        /// <summary>
        /// デフォルトのキーバインディング定義を取得
        /// </summary>
        List<KeyBindingDefinition> GetDefaultKeyBindings();

        /// <summary>
        /// デフォルトの修飾キー定義を取得
        /// </summary>
        List<ModifierKeyDefinition> GetDefaultModifierKeys();
    }
}