using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[SpectreSourceFile]
public abstract class NamedTypeJsonConverterBase<T> : JsonConverter<T>, IDefaultIdentifier
    where T : notnull
{
    abstract protected IReadOnlyDictionary<string, T> ByType { get; }
    abstract protected IReadOnlyDictionary<string, T> ByName { get; }
    abstract protected IReadOnlyDictionary<T, string> ValueToName { get; }

    public abstract bool? IsDefault(object value);

    internal const string TypeDiscriminator = "$type";
    internal const string NamedDiscriminator = "named";

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var element = doc.RootElement;

        string? typeName = null;
        string? named = null;

        var typeNameProperty = JsonWriteHelpers.ConvertName(TypeDiscriminator, options).ToLowerInvariant();
        var namedProperty = JsonWriteHelpers.ConvertName(NamedDiscriminator, options).ToLowerInvariant();

        foreach (var prop in element.EnumerateObject())
        {
            var propName = prop.Name.ToLowerInvariant();
            if (propName == typeNameProperty)
            {
                typeName = prop.Value.GetString();
            }
            else if (propName == namedProperty)
            {
                named = prop.Value.GetString();
            }
            else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
            {
                throw new JsonException($"Unrecognized property on {nameof(T)}: '{prop.Name}'.");
            }
        }

        if (!string.IsNullOrEmpty(named) && !string.IsNullOrEmpty(typeName))
        {
            throw new JsonException($"Cannot specify both '{TypeDiscriminator}' and '{NamedDiscriminator}' on {nameof(T)} entry.");
        }
        else if (!string.IsNullOrEmpty(named))
        {
            return ByName.GetValueOrDefault(named)
                ?? throw new JsonException($"Unknown {nameof(T)} with '{NamedDiscriminator}': '{named}'. Available names: {string.Join(", ", ByName.Keys)}");
        }
        else if (!string.IsNullOrEmpty(typeName))
        {
            return ByType.GetValueOrDefault(typeName)
                ?? throw new JsonException($"Unknown {nameof(T)} '{TypeDiscriminator}': '{typeName}'. Available types: {string.Join(", ", ByType.Keys)}");
        }
        else
        {
            throw new JsonException($"Missing required discriminator '{TypeDiscriminator}' or '{NamedDiscriminator}' on {nameof(T)} entry.");
        }
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (ValueToName.TryGetValue(value, out var name))
        {
            writer.WriteString(JsonWriteHelpers.ConvertName(NamedDiscriminator, options), name);
        }
        else
        {
            writer.WriteString(JsonWriteHelpers.ConvertName(TypeDiscriminator, options), value!.GetType().Name);   
        }
        writer.WriteEndObject();
    }
}