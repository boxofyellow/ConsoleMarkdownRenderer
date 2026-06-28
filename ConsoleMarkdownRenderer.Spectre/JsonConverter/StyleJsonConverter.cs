using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;

[SpectreSourceFile]
internal sealed class StyleJsonConverter : JsonConverter<Style>, IDefaultIdentifier
{
    public override Style Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var element = doc.RootElement;

        Color foreground = Color.Default;
        Color background = Color.Default;
        Decoration decoration = Decoration.None;

        var foregroundPropertyName = JsonWriteHelpers.ConvertName(nameof(Style.Foreground), options).ToLowerInvariant();
        var backgroundPropertyName = JsonWriteHelpers.ConvertName(nameof(Style.Background), options).ToLowerInvariant();
        var decorationPropertyName = JsonWriteHelpers.ConvertName(nameof(Style.Decoration), options).ToLowerInvariant();

        foreach (var prop in element.EnumerateObject())
        {
            var propName = prop.Name.ToLowerInvariant();
            if (propName == foregroundPropertyName)
            {
                foreground = prop.Value.Deserialize<Color>(options);
            }
            else if (propName == backgroundPropertyName)
            {
                background = prop.Value.Deserialize<Color>(options);
            }
            else if (propName == decorationPropertyName)
            {
                decoration = prop.Value.Deserialize<Decoration>(options);
            }
            else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
            {
                throw new JsonException($"Unrecognized property on Style: '{prop.Name}'.");
            }
        }

        return new Style(foreground, background, decoration);
    }

    public override void Write(Utf8JsonWriter writer, Style value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        JsonWriteHelpers.WriteProperty(writer, options, nameof(value.Foreground), value.Foreground);
        JsonWriteHelpers.WriteProperty(writer, options, nameof(value.Background), value.Background);
        JsonWriteHelpers.WriteProperty(writer, options, nameof(value.Decoration), value.Decoration);
        writer.WriteEndObject();
    }

    public bool? IsDefault(object value) 
        => value switch
        {
            Style style => style.Foreground.IsDefault() 
                && style.Background.IsDefault()
                && style.Decoration == Decoration.None,
            _ => null
        };
}
