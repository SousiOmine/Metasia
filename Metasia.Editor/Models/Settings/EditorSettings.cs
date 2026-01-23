using System.Text.Json.Serialization;

namespace Metasia.Editor.Models.Settings
{
    public class EditorSettings
    {
        [JsonPropertyName("version")]
        public string Version { get; set; } = "1.0";

        [JsonPropertyName("general")]
        public GeneralSettings General { get; set; } = new();

        [JsonPropertyName("editor")]
        public EditorBehaviorSettings Editor { get; set; } = new();
    }
}
