using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Polymorphic JSON converter for <see cref="IHeaderStyle"/>. Switches on a
    /// <c>"$type"</c> discriminator whose value is the simple CLR type name
    /// (<see cref="TextStyle"/> or <see cref="FigletTextStyle"/>) to pick the concrete
    /// implementation. Reading and writing are symmetric: a missing or unknown
    /// discriminator on the read side throws a <see cref="JsonException"/>, just as an
    /// unrecognised concrete type does on the write side.
    /// </summary>
    /// <remarks>
    /// All sub-values (enums, nested <see cref="TextColor"/>) are routed back through
    /// <see cref="JsonSerializer"/> with the caller-supplied <see cref="JsonSerializerOptions"/>,
    /// so the caller's enum-handling policy (e.g. whether
    /// <see cref="JsonStringEnumConverter"/> is registered) is honoured on both sides.
    /// </remarks>
    internal sealed class HeaderStyleJsonConverter : JsonConverter<IHeaderStyle>
    {
        internal const string TypeDiscriminator = "$type";

        public override IHeaderStyle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            string? typeName = null;
            foreach (var prop in root.EnumerateObject())
            {
                if (string.Equals(prop.Name, TypeDiscriminator, StringComparison.OrdinalIgnoreCase))
                {
                    typeName = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString();
                    break;
                }
            }

            if (string.IsNullOrEmpty(typeName))
            {
                throw new JsonException($"Missing required discriminator '{TypeDiscriminator}' on {nameof(IHeaderStyle)} entry.");
            }

            return typeName switch
            {
                nameof(FigletTextStyle) => ReadFiglet(root, options),
                nameof(TextStyle) => ReadTextStyle(root, options),
                _ => throw new JsonException($"Unknown {nameof(IHeaderStyle)} '{TypeDiscriminator}': '{typeName}'."),
            };
        }

        public override void Write(Utf8JsonWriter writer, IHeaderStyle value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case FigletTextStyle figlet:
                    writer.WriteStartObject();
                    writer.WriteString(TypeDiscriminator, nameof(FigletTextStyle));
                    writer.WritePropertyName("justification");
                    JsonSerializer.Serialize(writer, figlet.Justification, options);
                    writer.WritePropertyName("foreground");
                    JsonSerializer.Serialize(writer, figlet.Foreground, options);
                    writer.WriteString("fontPath", figlet.FontPath);
                    writer.WriteEndObject();
                    break;
                case TextStyle text:
                    writer.WriteStartObject();
                    writer.WriteString(TypeDiscriminator, nameof(TextStyle));
                    writer.WritePropertyName("decoration");
                    JsonSerializer.Serialize(writer, text.Decoration, options);
                    writer.WritePropertyName("foreground");
                    JsonSerializer.Serialize(writer, text.Foreground, options);
                    writer.WritePropertyName("background");
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
                        justification = prop.Value.ValueKind == JsonValueKind.Null
                            ? null
                            : prop.Value.Deserialize<TextJustification>(options);
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
                        decoration = prop.Value.Deserialize<TextDecoration>(options);
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
