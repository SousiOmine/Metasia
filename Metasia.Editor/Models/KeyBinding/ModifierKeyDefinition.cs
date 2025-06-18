using Avalonia.Input;
using System.Text.Json.Serialization;

namespace Metasia.Editor.Models.KeyBinding
{
    /// <summary>
    /// 修飾キー単体の動作設定
    /// </summary>
    public class ModifierKeyDefinition
    {
        /// <summary>
        /// アクションの識別子（例: "MultiSelectClip", "ConstrainedMove"）
        /// </summary>
        public string ActionId { get; set; } = string.Empty;

        /// <summary>
        /// 使用する修飾キー
        /// </summary>
        public KeyModifiers Modifier { get; set; }

        /// <summary>
        /// アクションの説明
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// 修飾キー設定をJSON形式で保存するためのクラス
    /// </summary>
    public class ModifierKeyDefinitionJson
    {
        [JsonPropertyName("actionId")]
        public string ActionId { get; set; } = string.Empty;

        [JsonPropertyName("modifier")]
        public string Modifier { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
} 