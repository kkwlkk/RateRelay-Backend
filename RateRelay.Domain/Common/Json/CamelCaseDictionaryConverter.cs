using System.Text.Json;
using System.Text.Json.Serialization;

namespace RateRelay.Domain.Common.Json;

public class CamelCaseDictionaryConverter : JsonConverter<Dictionary<string, object>>
{
    public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions? options)
    {
        return JsonSerializer.Deserialize<Dictionary<string, object>>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            var camelCaseKey = JsonNamingPolicy.CamelCase.ConvertName(kvp.Key);
            writer.WritePropertyName(camelCaseKey);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }
}