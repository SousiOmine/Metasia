using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Metasia.Editor.Models.KeyBinding
{
    /// <summary>
    /// キーバインディング設定を格納するクラス
    /// </summary>
    public class KeyBindingSettings
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.1";

        [JsonPropertyName("keyBindings")]
        public List<KeyBindingDefinitionJson> KeyBindings { get; set; } = new List<KeyBindingDefinitionJson>();

        [JsonPropertyName("modifierKeys")]
        public List<ModifierKeyDefinitionJson> ModifierKeys { get; set; } = new List<ModifierKeyDefinitionJson>();
    }

    /// <summary>
    /// JSON形式でのキーバインディング定義
    /// </summary>
    public class KeyBindingDefinitionJson
    {
        [JsonPropertyName("commandId")]
        public string CommandId { get; set; } = string.Empty;

        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("modifiers")]
        public List<string> Modifiers { get; set; } = new List<string>();
    }
} 