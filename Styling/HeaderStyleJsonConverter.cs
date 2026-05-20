using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Polymorphic JSON converter for <see cref="IHeaderStyle"/>. Switches on a <c>"$type"</c>
    /// discriminator whose value is the simple CLR type name (<c>"TextStyle"</c> or
    /// <c>"FigletTextStyle"</c>) to pick between <see cref="TextStyle"/> and
    /// <see cref="FigletTextStyle"/>. When no discriminator is present the entry is read as a
    /// <see cref="TextStyle"/> for backwards compatibility.
    /// </summary>
    /// <remarks>
    /// This converter does not assume anything about the surrounding
    /// <see cref="JsonSerializerOptions"/>: enum-valued sub-properties (e.g.
    /// <see cref="TextDecoration"/>, <see cref="TextJustification"/>) are read and written
    /// directly so the converter remains correct even when callers supply a
    /// <see cref="JsonSerializerOptions"/> instance that lacks the conventional
    /// <see cref="JsonStringEnumConverter"/>.
    /// </remarks>
    internal sealed class HeaderStyleJsonConverter : JsonConverter<IHeaderStyle>
    {
        internal const string TypeDiscriminator = "$type";
        internal const string TextStyleTypeName = nameof(TextStyle);
        internal const string FigletTextStyleTypeName = nameof(FigletTextStyle);

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
                    typeName = prop.Value.GetString();
                    break;
                }
            }

            return typeName switch
            {
                FigletTextStyleTypeName => ReadFiglet(root, options),
                _ => ReadTextStyle(root, options),
            };
        }

        public override void Write(Utf8JsonWriter writer, IHeaderStyle value, JsonSerializerOptions options)
        {
            switch (value)
            {
                case FigletTextStyle figlet:
                    writer.WriteStartObject();
                    writer.WriteString(TypeDiscriminator, FigletTextStyleTypeName);
                    if (figlet.Justification is { } justification)
                    {
                        writer.WriteString("Justification", justification.ToString());
                    }
                    else
                    {
                        writer.WriteNull("Justification");
                    }
                    writer.WritePropertyName("Foreground");
                    TextColorJsonConverter.WriteValue(writer, figlet.Foreground);
                    writer.WriteString("FontPath", figlet.FontPath);
                    writer.WriteEndObject();
                    break;
                case TextStyle text:
                    writer.WriteStartObject();
                    writer.WriteString(TypeDiscriminator, TextStyleTypeName);
                    writer.WriteString("Decoration", text.Decoration.ToString());
                    writer.WritePropertyName("Foreground");
                    TextColorJsonConverter.WriteValue(writer, text.Foreground);
                    writer.WritePropertyName("Background");
                    TextColorJsonConverter.WriteValue(writer, text.Background);
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
                        justification = JsonEnumHelpers.ReadNullable<TextJustification>(prop.Value);
                        break;
                    case "foreground":
                        foreground = TextColorJsonConverter.ReadValue(prop.Value);
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
                        decoration = JsonEnumHelpers.Read<TextDecoration>(prop.Value);
                        break;
                    case "foreground":
                        foreground = TextColorJsonConverter.ReadValue(prop.Value);
                        break;
                    case "background":
                        background = TextColorJsonConverter.ReadValue(prop.Value);
                        break;
                }
            }

            return new TextStyle(decoration, foreground, background);
        }
    }
}
