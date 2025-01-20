using System.Text.Json;
using System.Text.Json.Serialization;
using Metasia.Core.Objects;

public class MetasiaObjectJsonConverter : JsonConverter<MetasiaObject>
{
    private static readonly Dictionary<string, Type> _typeMap;

    static MetasiaObjectJsonConverter()
    {
        // MetasiaObjectの派生クラスを自動的に検出
        _typeMap = typeof(MetasiaObject).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(MetasiaObject)))
            .ToDictionary(t => t.Name, t => t);
    }

    public override MetasiaObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var typeDiscriminator = jsonDoc.RootElement
            .GetProperty("$type")
            .GetString();

        if (string.IsNullOrEmpty(typeDiscriminator) || !_typeMap.TryGetValue(typeDiscriminator, out var type))
            throw new JsonException($"Unknown type discriminator: {typeDiscriminator}");

        return (MetasiaObject?)JsonSerializer.Deserialize(jsonDoc.RootElement.GetRawText(), type, options);
    }

    public override void Write(Utf8JsonWriter writer, MetasiaObject value, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using var jsonDoc = JsonDocument.Parse(json);
        writer.WriteStartObject();
        writer.WriteString("$type", value.GetType().Name);
        
        foreach (var element in jsonDoc.RootElement.EnumerateObject())
        {
            element.WriteTo(writer);
        }
        
        writer.WriteEndObject();
    }
} 