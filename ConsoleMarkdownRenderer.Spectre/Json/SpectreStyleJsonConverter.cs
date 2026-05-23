using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Json
{
    /// <summary>
    /// JSON converter for Spectre.Console <see cref="Style"/>. Serializes as
    /// <c>{ "foreground"?: …, "background"?: …, "decoration"?: "Bold,Italic" }</c>.
    /// A <see cref="Style.Plain"/> value round-trips as an empty object <c>{}</c>.
    /// </summary>
    internal sealed class SpectreStyleJsonConverter : JsonConverter<Style>
    {
        public override Style Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return Style.Plain;
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            var element = doc.RootElement;

            Color foreground = Color.Default;
            Color background = Color.Default;
            Decoration decoration = Decoration.None;

            foreach (var prop in element.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "foreground":
                        if (prop.Value.ValueKind != JsonValueKind.Null)
                        {
                            var colorConverter = new SpectreColorJsonConverter();
                            var bytes = System.Text.Encoding.UTF8.GetBytes(prop.Value.GetRawText());
                            var colorReader = new Utf8JsonReader(bytes);
                            var parsed = colorConverter.Read(ref colorReader, typeof(Color), options);
                            if (parsed.HasValue)
                            {
                                foreground = parsed.Value;
                            }
                        }
                        break;
                    case "background":
                        if (prop.Value.ValueKind != JsonValueKind.Null)
                        {
                            var colorConverter = new SpectreColorJsonConverter();
                            var bytes = System.Text.Encoding.UTF8.GetBytes(prop.Value.GetRawText());
                            var colorReader = new Utf8JsonReader(bytes);
                            var parsed = colorConverter.Read(ref colorReader, typeof(Color), options);
                            if (parsed.HasValue)
                            {
                                background = parsed.Value;
                            }
                        }
                        break;
                    case "decoration":
                        if (prop.Value.ValueKind == JsonValueKind.String)
                        {
                            decoration = ParseDecoration(prop.Value.GetString());
                        }
                        break;
                }
            }

            return new Style(foreground, background, decoration);
        }


        public override void Write(Utf8JsonWriter writer, Style value, JsonSerializerOptions options)
        {
            var colorConverter = new SpectreColorJsonConverter();

            writer.WriteStartObject();
            if (value.Foreground != Color.Default)
            {
                writer.WritePropertyName(SpectreJsonWriteHelpers.ConvertName("Foreground", options));
                colorConverter.Write(writer, value.Foreground, options);
            }
            if (value.Background != Color.Default)
            {
                writer.WritePropertyName(SpectreJsonWriteHelpers.ConvertName("Background", options));
                colorConverter.Write(writer, value.Background, options);
            }
            if (value.Decoration != Decoration.None)
            {
                writer.WriteString(
                    SpectreJsonWriteHelpers.ConvertName("Decoration", options),
                    FormatDecoration(value.Decoration));
            }
            writer.WriteEndObject();
        }

        private static string FormatDecoration(Decoration decoration)
        {
            var parts = new List<string>();
            foreach (Decoration flag in Enum.GetValues<Decoration>())
            {
                if (flag != Decoration.None && decoration.HasFlag(flag))
                {
                    parts.Add(flag.ToString());
                }
            }
            return string.Join(",", parts);
        }

        private static Decoration ParseDecoration(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Decoration.None;
            }

            var result = Decoration.None;
            foreach (var part in value.Split(','))
            {
                var trimmed = part.Trim();
                if (Enum.TryParse<Decoration>(trimmed, ignoreCase: true, out var flag))
                {
                    result |= flag;
                }
            }
            return result;
        }
    }
}
