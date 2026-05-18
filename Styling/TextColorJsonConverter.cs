using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// JSON converter for <see cref="TextColor"/>. Reads <c>{ "isRgb": ..., "named": ..., "r": ..., "g": ..., "b": ... }</c>
    /// and constructs a <see cref="TextColor"/> via the public factory methods, preserving
    /// <see cref="TextColor"/>'s private-constructor invariant. Writes the same shape.
    /// </summary>
    internal sealed class TextColorJsonConverter : JsonConverter<TextColor>
    {
        public override TextColor? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            bool isRgb = false;
            NamedColor named = NamedColor.Default;
            byte r = 0, g = 0, b = 0;

            foreach (var prop in root.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "isrgb":
                        isRgb = prop.Value.GetBoolean();
                        break;
                    case "named":
                        named = ReadNamed(prop.Value, options);
                        break;
                    case "r":
                        r = prop.Value.GetByte();
                        break;
                    case "g":
                        g = prop.Value.GetByte();
                        break;
                    case "b":
                        b = prop.Value.GetByte();
                        break;
                }
            }

            return isRgb ? TextColor.FromRgb(r, g, b) : TextColor.FromNamed(named);
        }

        public override void Write(Utf8JsonWriter writer, TextColor value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteBoolean("IsRgb", value.IsRgb);
            writer.WritePropertyName("Named");
            JsonSerializer.Serialize(writer, value.Named, options);
            writer.WriteNumber("R", value.R);
            writer.WriteNumber("G", value.G);
            writer.WriteNumber("B", value.B);
            writer.WriteEndObject();
        }

        private static NamedColor ReadNamed(JsonElement element, JsonSerializerOptions options)
        {
            if (element.ValueKind == JsonValueKind.String)
            {
                // Honor any JsonStringEnumConverter (case-insensitive) registered on options.
                var name = element.GetString();
                if (Enum.TryParse<NamedColor>(name, ignoreCase: true, out var parsed))
                {
                    return parsed;
                }
                throw new JsonException($"Unknown NamedColor value '{name}'.");
            }
            if (element.ValueKind == JsonValueKind.Number)
            {
                return (NamedColor)element.GetInt32();
            }
            throw new JsonException($"Unexpected token {element.ValueKind} for NamedColor.");
        }
    }
}
