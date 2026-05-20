using System.Text.Json;
using System.Text.Json.Serialization;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// JSON converter for <see cref="TextColor"/>. Constructs values via the public factory
    /// methods (<see cref="TextColor.FromRgb(byte, byte, byte)"/> /
    /// <see cref="TextColor.FromNamed(NamedColor)"/>) so <see cref="TextColor"/> keeps its
    /// private-constructor invariant.
    /// </summary>
    /// <remarks>
    /// Emits a compact shape that does not duplicate information: RGB values are written as
    /// <c>{ "r": …, "g": …, "b": … }</c> and named colours as <c>{ "named": "…" }</c>. The
    /// <c>IsRgb</c> flag is never emitted; the reader infers it from which fields are present
    /// (any of <c>r</c>/<c>g</c>/<c>b</c> implies RGB).
    ///
    /// All sub-values (currently the <see cref="NamedColor"/> enum) are written and read via
    /// the caller-supplied <see cref="JsonSerializerOptions"/>, so the caller's enum-handling
    /// policy (e.g. whether <see cref="JsonStringEnumConverter"/> is registered) is honoured
    /// on both sides.
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
            var element = doc.RootElement;

            NamedColor? named = null;
            byte? r = null, g = null, b = null;

            foreach (var prop in element.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "named":
                        named = prop.Value.ValueKind == JsonValueKind.Null
                            ? null
                            : prop.Value.Deserialize<NamedColor>(options);
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

            // The presence of any RGB field implies an RGB colour; missing channels default
            // to 0. Otherwise fall back to the (possibly default) named colour.
            if (r.HasValue || g.HasValue || b.HasValue)
            {
                return TextColor.FromRgb(r ?? 0, g ?? 0, b ?? 0);
            }

            return TextColor.FromNamed(named ?? NamedColor.Default);
        }

        public override void Write(Utf8JsonWriter writer, TextColor value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            if (value.IsRgb)
            {
                writer.WriteNumber("r", value.R);
                writer.WriteNumber("g", value.G);
                writer.WriteNumber("b", value.B);
            }
            else
            {
                writer.WritePropertyName("named");
                JsonSerializer.Serialize(writer, value.Named, options);
            }
            writer.WriteEndObject();
        }
    }
}
