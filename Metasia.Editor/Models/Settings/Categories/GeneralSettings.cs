using System.Text.Json.Serialization;

namespace Metasia.Editor.Models.Settings
{
    public class GeneralSettings
    {
        /// <summary>
        /// エディタの言語 en, ja
        /// </summary>
        [JsonPropertyName("language")]
        public string Language { get; set; } = "ja";

        /// <summary>
        /// UIのテーマカラー auto, dark, light
        /// </summary>
        [JsonPropertyName("theme")]
        public string Theme { get; set; } = "dark";

        [JsonPropertyName("autoSave")]
        public bool AutoSave { get; set; } = true;

        [JsonPropertyName("autoSaveInterval")]
        public int AutoSaveInterval { get; set; } = 5;
    }
}
