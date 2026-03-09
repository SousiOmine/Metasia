using System.Text.Json.Serialization;

namespace Metasia.Editor.Models.Settings
{
    public class MainWindowLayoutSettings
    {
        [JsonPropertyName("isMaximized")]
        public bool IsMaximized { get; set; }

        [JsonPropertyName("normalWidth")]
        public double? NormalWidth { get; set; }

        [JsonPropertyName("normalHeight")]
        public double? NormalHeight { get; set; }

        [JsonPropertyName("leftPaneRatio")]
        public double LeftPaneRatio { get; set; } = 1d / 6d;

        [JsonPropertyName("centerPaneRatio")]
        public double CenterPaneRatio { get; set; } = 3d / 6d;

        [JsonPropertyName("rightPaneRatio")]
        public double RightPaneRatio { get; set; } = 2d / 6d;

        [JsonPropertyName("topPaneRatio")]
        public double TopPaneRatio { get; set; } = 0.5d;
    }
}
