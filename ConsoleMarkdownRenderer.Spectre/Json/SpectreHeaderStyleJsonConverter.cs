using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Json
{
    /// <summary>
    /// Polymorphic JSON converter for <see cref="ISpectreHeaderStyle"/>. Uses a
    /// <c>"$type"</c> discriminator whose value is the simple CLR type name
    /// (<c>SpectreFigletHeaderStyle</c>, <c>SpectreRuleHeaderStyle</c>, or
    /// <c>SpectreStyleHeaderStyle</c>) to pick the concrete implementation.
    /// </summary>
    internal sealed class SpectreHeaderStyleJsonConverter : JsonConverter<ISpectreHeaderStyle>
    {
        internal const string TypeDiscriminator = "$type";

        public override ISpectreHeaderStyle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
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
                throw new JsonException($"Missing required discriminator '{TypeDiscriminator}' on {nameof(ISpectreHeaderStyle)} entry.");
            }

            return typeName switch
            {
                nameof(SpectreFigletHeaderStyle) => ReadFiglet(root, options),
                nameof(SpectreRuleHeaderStyle) => ReadRule(root, options),
                nameof(SpectreStyleHeaderStyle) => ReadStyleHeader(root, options),
                _ => throw new JsonException($"Unknown {nameof(ISpectreHeaderStyle)} '{TypeDiscriminator}': '{typeName}'."),
            };
        }

        public override void Write(Utf8JsonWriter writer, ISpectreHeaderStyle value, JsonSerializerOptions options)
        {
            var colorConverter = new SpectreColorJsonConverter();
            var boxBorderConverter = new SpectreBoxBorderJsonConverter();
            var styleConverter = new SpectreStyleJsonConverter();

            switch (value)
            {
                case SpectreFigletHeaderStyle figlet:
                    writer.WriteStartObject();
                    writer.WriteString(TypeDiscriminator, nameof(SpectreFigletHeaderStyle));
                    if (figlet.Justification.HasValue)
                    {
                        writer.WriteString(
                            SpectreJsonWriteHelpers.ConvertName(nameof(figlet.Justification), options),
                            SpectreJsonWriteHelpers.ConvertName(figlet.Justification.Value.ToString(), options));
                    }
                    if (figlet.Foreground.HasValue && figlet.Foreground.Value != Color.Default)
                    {
                        writer.WritePropertyName(SpectreJsonWriteHelpers.ConvertName(nameof(figlet.Foreground), options));
                        colorConverter.Write(writer, figlet.Foreground, options);
                    }
                    // Note: Font (FigletFont) is not serialized — on deserialization the default
                    // Spectre.Console font is used. Use SpectreDisplayOptions programmatically
                    // if a custom font is required.
                    writer.WriteEndObject();
                    break;
                case SpectreRuleHeaderStyle rule:
                    writer.WriteStartObject();
                    writer.WriteString(TypeDiscriminator, nameof(SpectreRuleHeaderStyle));
                    if (rule.Justification.HasValue)
                    {
                        writer.WriteString(
                            SpectreJsonWriteHelpers.ConvertName(nameof(rule.Justification), options),
                            SpectreJsonWriteHelpers.ConvertName(rule.Justification.Value.ToString(), options));
                    }
                    if (rule.Foreground.HasValue && rule.Foreground.Value != Color.Default)
                    {
                        writer.WritePropertyName(SpectreJsonWriteHelpers.ConvertName(nameof(rule.Foreground), options));
                        colorConverter.Write(writer, rule.Foreground, options);
                    }
                    if (rule.Border is not null)
                    {
                        writer.WritePropertyName(SpectreJsonWriteHelpers.ConvertName(nameof(rule.Border), options));
                        boxBorderConverter.Write(writer, rule.Border, options);
                    }
                    writer.WriteEndObject();
                    break;
                case SpectreStyleHeaderStyle styleHeader:
                    writer.WriteStartObject();
                    writer.WriteString(TypeDiscriminator, nameof(SpectreStyleHeaderStyle));
                    writer.WritePropertyName(SpectreJsonWriteHelpers.ConvertName(nameof(styleHeader.Style), options));
                    styleConverter.Write(writer, styleHeader.Style, options);
                    writer.WriteEndObject();
                    break;
                default:
                    throw new JsonException($"Unsupported {nameof(ISpectreHeaderStyle)} implementation: {value?.GetType().FullName}.");
            }
        }

        private static SpectreFigletHeaderStyle ReadFiglet(JsonElement root, JsonSerializerOptions options)
        {
            Justify? justification = null;
            Color? foreground = null;

            var colorConverter = new SpectreColorJsonConverter();

            foreach (var prop in root.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "justification":
                        if (prop.Value.ValueKind == JsonValueKind.String
                            && Enum.TryParse<Justify>(prop.Value.GetString(), ignoreCase: true, out var j))
                        {
                            justification = j;
                        }
                        break;
                    case "foreground":
                        if (prop.Value.ValueKind != JsonValueKind.Null)
                        {
                            var bytes = System.Text.Encoding.UTF8.GetBytes(prop.Value.GetRawText());
                            var rdr = new Utf8JsonReader(bytes);
                            foreground = colorConverter.Read(ref rdr, typeof(Color), options);
                        }
                        break;
                }
            }

            return new SpectreFigletHeaderStyle(font: null, justification: justification, foreground: foreground);
        }

        private static SpectreRuleHeaderStyle ReadRule(JsonElement root, JsonSerializerOptions options)
        {
            Justify? justification = null;
            Color? foreground = null;
            BoxBorder? border = null;

            var colorConverter = new SpectreColorJsonConverter();
            var boxBorderConverter = new SpectreBoxBorderJsonConverter();

            foreach (var prop in root.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "justification":
                        if (prop.Value.ValueKind == JsonValueKind.String
                            && Enum.TryParse<Justify>(prop.Value.GetString(), ignoreCase: true, out var j))
                        {
                            justification = j;
                        }
                        break;
                    case "foreground":
                        if (prop.Value.ValueKind != JsonValueKind.Null)
                        {
                            var bytes = System.Text.Encoding.UTF8.GetBytes(prop.Value.GetRawText());
                            var rdr = new Utf8JsonReader(bytes);
                            foreground = colorConverter.Read(ref rdr, typeof(Color), options);
                        }
                        break;
                    case "border":
                        if (prop.Value.ValueKind != JsonValueKind.Null)
                        {
                            var bytes = System.Text.Encoding.UTF8.GetBytes(prop.Value.GetRawText());
                            var rdr = new Utf8JsonReader(bytes);
                            border = boxBorderConverter.Read(ref rdr, typeof(BoxBorder), options);
                        }
                        break;
                }
            }

            return new SpectreRuleHeaderStyle(justification: justification, foreground: foreground, border: border);
        }

        private static SpectreStyleHeaderStyle ReadStyleHeader(JsonElement root, JsonSerializerOptions options)
        {
            var styleConverter = new SpectreStyleJsonConverter();
            var style = Style.Plain;

            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(prop.Value.GetRawText());
                    var rdr = new Utf8JsonReader(bytes);
                    style = styleConverter.Read(ref rdr, typeof(Style), options);
                    break;
                }
            }

            return new SpectreStyleHeaderStyle(style);
        }
    }
}
