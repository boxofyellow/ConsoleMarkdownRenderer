using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Polymorphic JSON converter for <see cref="IHeaderStyle"/>. Switches on a
    /// <c>"kind"</c> discriminator (<c>"text"</c> or <c>"figlet"</c>) to pick between
    /// <see cref="TextStyle"/> and <see cref="FigletTextStyle"/>. When no discriminator is
    /// present the entry is read as a <see cref="TextStyle"/> for backwards compatibility.
    /// </summary>
    internal sealed class HeaderStyleJsonConverter : JsonConverter<IHeaderStyle>
    {
        public override IHeaderStyle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            string? kind = null;
            foreach (var prop in root.EnumerateObject())
            {
                if (string.Equals(prop.Name, "kind", StringComparison.OrdinalIgnoreCase))
                {
                    kind = prop.Value.GetString();
                    break;
                }
            }

            return (kind?.ToLowerInvariant()) switch
            {
                "figlet" => ReadFiglet(root, options),
                _ => ReadTextStyle(root, options),
            };
        }

        public override void Write(Utf8JsonWriter writer, IHeaderStyle value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case FigletTextStyle figlet:
                    writer.WriteStartObject();
                    writer.WriteString("Kind", "figlet");
                    writer.WritePropertyName("Justification");
                    JsonSerializer.Serialize(writer, figlet.Justification, options);
                    writer.WritePropertyName("Foreground");
                    JsonSerializer.Serialize(writer, figlet.Foreground, options);
                    writer.WriteString("FontPath", figlet.FontPath);
                    writer.WriteEndObject();
                    break;
                case TextStyle text:
                    writer.WriteStartObject();
                    writer.WriteString("Kind", "text");
                    writer.WritePropertyName("Decoration");
                    JsonSerializer.Serialize(writer, text.Decoration, options);
                    writer.WritePropertyName("Foreground");
                    JsonSerializer.Serialize(writer, text.Foreground, options);
                    writer.WritePropertyName("Background");
                    JsonSerializer.Serialize(writer, text.Background, options);
                    writer.WriteEndObject();
                    break;
                default:
                    throw new JsonException($"Unsupported {nameof(IHeaderStyle)} implementation: {value?.GetType().FullName}.");
            }
        }

        private static FigletTextStyle ReadFiglet(JsonElement root, JsonSerializerOptions options)
        {
            TextJustification? justification = null;
            TextColor? foreground = null;
            string? fontPath = null;

            foreach (var prop in root.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "justification":
                        if (prop.Value.ValueKind != JsonValueKind.Null)
                        {
                            justification = prop.Value.Deserialize<TextJustification>(options);
                        }
                        break;
                    case "foreground":
                        foreground = prop.Value.Deserialize<TextColor>(options);
                        break;
                    case "fontpath":
                        fontPath = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString();
                        break;
                }
            }

            return FigletTextStyle.Create(justification, foreground, fontPath);
        }

        private static TextStyle ReadTextStyle(JsonElement root, JsonSerializerOptions options)
        {
            var decoration = TextDecoration.None;
            TextColor? foreground = null;
            TextColor? background = null;

            foreach (var prop in root.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "decoration":
                        if (prop.Value.ValueKind != JsonValueKind.Null)
                        {
                            decoration = prop.Value.Deserialize<TextDecoration>(options);
                        }
                        break;
                    case "foreground":
                        foreground = prop.Value.Deserialize<TextColor>(options);
                        break;
                    case "background":
                        background = prop.Value.Deserialize<TextColor>(options);
                        break;
                }
            }

            return new TextStyle(decoration, foreground, background);
        }
    }
}
