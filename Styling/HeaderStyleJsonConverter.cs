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
    /// On the write side this converter honours the caller-supplied
    /// <see cref="JsonSerializerOptions"/>:
    /// <list type="bullet">
    /// <item>property names (other than the literal <c>"$type"</c> discriminator) are
    /// transformed through <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>;</item>
    /// <item>properties whose value matches
    /// <see cref="JsonSerializerOptions.DefaultIgnoreCondition"/>
    /// (<see cref="JsonIgnoreCondition.WhenWritingNull"/> /
    /// <see cref="JsonIgnoreCondition.WhenWritingDefault"/>) are skipped;</item>
    /// <item>all sub-values (enums, nested <see cref="TextColor"/>) flow back through
    /// <see cref="JsonSerializer"/> with the same options, so the caller's enum / nested
    /// converters are honoured.</item>
    /// </list>
    /// On the read side the converter is deliberately lenient: property names are matched
    /// case-insensitively against the literal names regardless of any naming policy, so
    /// JSON written under any policy still round-trips.
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
                    // The $type discriminator is a System.Text.Json convention and is
                    // written literally (not transformed by PropertyNamingPolicy and never
                    // skipped, even under DefaultIgnoreCondition).
                    writer.WriteString(TypeDiscriminator, nameof(FigletTextStyle));
                    JsonWriteHelpers.WriteProperty(writer, options, "justification", figlet.Justification);
                    JsonWriteHelpers.WriteProperty(writer, options, "foreground", figlet.Foreground);
                    JsonWriteHelpers.WriteProperty(writer, options, "fontPath", figlet.FontPath);
                    writer.WriteEndObject();
                    break;
                case TextStyle text:
                    writer.WriteStartObject();
                    writer.WriteString(TypeDiscriminator, nameof(TextStyle));
                    JsonWriteHelpers.WriteProperty(writer, options, "decoration", text.Decoration);
                    JsonWriteHelpers.WriteProperty(writer, options, "foreground", text.Foreground);
                    JsonWriteHelpers.WriteProperty(writer, options, "background", text.Background);
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
