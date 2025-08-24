using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Metasia.Core.Project;

namespace Metasia.Core.Json
{
    public class ProjectSerializer
    {
        /// <summary>
        /// MetasiaProjectをJSON文字列(.mtpj)にシリアライズする。
        /// </summary>
        /// <param name="project">シリアライズするMetasiaProject</param>
        /// <returns>JSON文字列</returns>
        public static string SerializeToMTPJ(MetasiaProject project)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
                Converters = { new MetasiaObjectJsonConverter() },
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return JsonSerializer.Serialize(project, options);
        }

        /// <summary>
        /// JSON文字列(.mtpj)をMetasiaProjectにデシリアライズする。
        /// </summary>
        /// <param name="json">デシリアライズするJSON文字列</param>
        /// <returns>デシリアライズしたMetasiaProject</returns>
        public static MetasiaProject DeserializeFromMTPJ(string json)
        {
            var options = new JsonSerializerOptions
            {
                IncludeFields = true,
                Converters = { new MetasiaObjectJsonConverter() }
            };
            var project = JsonSerializer.Deserialize<MetasiaProject>(json, options)
                ?? throw new JsonException("Failed to deserialize MetasiaProject");
            return project;
        }
    }
}