using System.Text.Json;
using Metasia.Core.Objects;
using Metasia.Core.Project;

namespace Metasia.Core.Json
{
    public class TimelineSerializer
    {
        /// <summary>
        /// タイムラインをJSON形式で保存
        /// </summary>
        public static string SerializeTimeline(TimelineObject timeline)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
                Converters = { new MetasiaObjectJsonConverter() }
            };
            return JsonSerializer.Serialize(timeline, options);
        }
        
        /// <summary>
        /// JSONからタイムラインを読み込み
        /// </summary>
        public static TimelineObject DeserializeTimeline(string json)
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                Converters = { new MetasiaObjectJsonConverter() }
            };
            var timeline = JsonSerializer.Deserialize<TimelineObject>(json, options)
                ?? throw new JsonException("タイムラインのデシリアライズに失敗しました");
            return timeline;
        }
    }
}