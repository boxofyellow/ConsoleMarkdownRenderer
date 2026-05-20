using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// JSON converter for <see cref="TextColor"/>. Reads
    /// <c>{ "IsRgb": ..., "Named": ..., "R": ..., "G": ..., "B": ... }</c> and constructs a
    /// <see cref="TextColor"/> via the public factory methods, preserving
    /// <see cref="TextColor"/>'s private-constructor invariant. Writes the same shape.
    /// </summary>
    /// <remarks>
    /// This converter is self-sufficient with respect to the surrounding
    /// <see cref="JsonSerializerOptions"/>: the <see cref="NamedColor"/> enum value is read
    /// and written directly so the converter remains correct even when callers supply a
    /// <see cref="JsonSerializerOptions"/> instance that lacks the conventional
    /// <see cref="JsonStringEnumConverter"/>.
    /// </remarks>
    internal sealed class TextColorJsonConverter : JsonConverter<TextColor>
    {
        public override TextColor? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            return ReadValue(doc.RootElement);
        }

        public override void Write(Utf8JsonWriter writer, TextColor value, JsonSerializerOptions options)
            => WriteValue(writer, value);

        /// <summary>
        /// Reads a <see cref="TextColor"/> from an already-parsed <see cref="JsonElement"/>.
        /// Used by <see cref="HeaderStyleJsonConverter"/> so the same options-independent
        /// parsing logic is shared.
        /// </summary>
        internal static TextColor? ReadValue(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }

            bool isRgb = false;
            NamedColor named = NamedColor.Default;
            byte r = 0, g = 0, b = 0;

            foreach (var prop in element.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "isrgb":
                        isRgb = prop.Value.GetBoolean();
                        break;
                    case "named":
                        named = JsonEnumHelpers.Read<NamedColor>(prop.Value);
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

        /// <summary>
        /// Writes a (possibly null) <see cref="TextColor"/>. Shared with
        /// <see cref="HeaderStyleJsonConverter"/> so nested colours are emitted with the
        /// same shape regardless of how the surrounding object is being written.
        /// </summary>
        internal static void WriteValue(Utf8JsonWriter writer, TextColor? value)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteBoolean("IsRgb", value.IsRgb);
            writer.WriteString("Named", value.Named.ToString());
            writer.WriteNumber("R", value.R);
            writer.WriteNumber("G", value.G);
            writer.WriteNumber("B", value.B);
            writer.WriteEndObject();
        }
    }
}
