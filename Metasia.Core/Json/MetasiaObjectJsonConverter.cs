using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Metasia.Core.Objects;
using System.Diagnostics;

public class MetasiaObjectJsonConverter : JsonConverter<MetasiaObject>
{
    // MetasiaObjectの派生クラスの型情報を保持する辞書
    // キー: クラス名, 値: 型情報
    private static readonly Dictionary<string, Type> _typeMap;

    static MetasiaObjectJsonConverter()
    {
        // アプリケーション起動時に、MetasiaObjectの全ての具象派生クラスを自動的に検出
        // 抽象クラスは除外し、MetasiaObjectを継承したクラスのみを対象とする
        _typeMap = typeof(MetasiaObject).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(MetasiaObject)))
            .ToDictionary(t => t.Name, t => t);
    }

    public override MetasiaObject? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // JSONがオブジェクトで始まることを確認
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException();

        // JSONドキュメントをパース
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;

        // 型情報を示す$typeプロパティの存在確認
        if (!root.TryGetProperty("$type", out var typeProperty))
        {
            Debug.WriteLine("Missing $type property in JSON");
            throw new JsonException("Missing $type property");
        }

        // 型名を取得し、登録済みの型と照合
        var typeDiscriminator = typeProperty.GetString();

        if (string.IsNullOrEmpty(typeDiscriminator) || !_typeMap.TryGetValue(typeDiscriminator, out var type))
        {
            Debug.WriteLine($"Unknown type discriminator: {typeDiscriminator}");
            throw new JsonException($"Unknown type discriminator: {typeDiscriminator}");
        }

        // シリアライズ設定を構成
        // - 大文字小文字を区別しない
        // - フィールドを含める
        // - 列挙型を文字列として扱う
        // - 2バイト文字をエスケープしない
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            Converters = { new JsonStringEnumConverter(), this },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        try
        {
            var instance = (MetasiaObject?)JsonSerializer.Deserialize(root.GetRawText(), type, serializerOptions);
            if (instance == null)
            {
                Debug.WriteLine($"Failed to deserialize {typeDiscriminator} - result was null");
            }
            return instance;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error deserializing {typeDiscriminator}: {ex.Message}");
            throw;
        }
    }

    public override void Write(Utf8JsonWriter writer, MetasiaObject value, JsonSerializerOptions options)
    {
        // シリアライズ設定を構成
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            Converters = { new JsonStringEnumConverter(), this },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        // オブジェクトをJSON文字列に変換
        var json = JsonSerializer.Serialize(value, value.GetType(), serializerOptions);

        // JSON文字列をパースして再構築
        // $typeプロパティを追加しつつ、その他のプロパティを書き込む
        using var jsonDoc = JsonDocument.Parse(json);
        writer.WriteStartObject();
        writer.WriteString("$type", value.GetType().Name);
        
        // $type以外のすべてのプロパティを書き込む
        foreach (var element in jsonDoc.RootElement.EnumerateObject())
        {
            if (element.Name != "$type")
            {
                element.WriteTo(writer);
            }
        }
        
        writer.WriteEndObject();
    }
} 