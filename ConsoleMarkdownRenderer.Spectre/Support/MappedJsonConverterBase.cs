using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[SpectreSourceFile]
public abstract class MappedJsonConverterBase<T> : JsonConverter<T>
{
    abstract protected IReadOnlyDictionary<string, Action<T, JsonSerializerOptions, JsonElement>> Deserializers { get; }
    abstract protected IReadOnlyList<Action<T, Utf8JsonWriter, JsonSerializerOptions>> Serializers { get; }
    abstract protected Func<T> CreateObject { get; }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var element = doc.RootElement;

        var deserializers = GetDeserializers(options);

        var result = CreateObject();

        foreach (var prop in element.EnumerateObject())
        {
            if (deserializers.TryGetValue(prop.Name, out var deserialize))
            {
                deserialize(result, options, prop.Value);
            }
            else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
            {
                throw new JsonException($"Unrecognized property on {nameof(T)}: '{prop.Name}'.");
            }
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        foreach (var serialize in Serializers)
        {
            serialize(value, writer, options);
        }
        writer.WriteEndObject();
    }

    private IReadOnlyDictionary<string, Action<T, JsonSerializerOptions, JsonElement>> GetDeserializers(JsonSerializerOptions options)
    {
        var namingPolicy = options.PropertyNamingPolicy;
        if (namingPolicy is null)
        {
            return Deserializers;
        }

        if (!_deserializersCache.TryGetValue(namingPolicy, out var deserializers))
        {
            deserializers = Deserializers.ToDictionary(
                kvp => JsonWriteHelpers.ConvertName(kvp.Key, options),
                kvp => kvp.Value,
                StringComparer.OrdinalIgnoreCase);
            _deserializersCache[namingPolicy] = deserializers;
        }
        return deserializers;
    }
    private readonly Dictionary<JsonNamingPolicy, IReadOnlyDictionary<string, Action<T, JsonSerializerOptions, JsonElement>>> _deserializersCache = new();
}