using System.Text.Json.Serialization;

namespace Metasia.Editor.Models.Settings
{
    public class EditorBehaviorSettings
    {
        [JsonPropertyName("snapToGrid")]
        public bool SnapToGrid { get; set; } = true;
    }
}
