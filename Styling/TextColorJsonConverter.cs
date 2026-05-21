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
    /// On the write side this converter honours the caller-supplied
    /// <see cref="JsonSerializerOptions"/>: property names flow through
    /// <see cref="JsonSerializerOptions.PropertyNamingPolicy"/>, the named-colour property
    /// is suppressed when its value matches
    /// <see cref="JsonSerializerOptions.DefaultIgnoreCondition"/>
    /// (<see cref="JsonIgnoreCondition.WhenWritingDefault"/> drops
    /// <see cref="NamedColor.Default"/>), and the <see cref="NamedColor"/> enum is serialized
    /// through <see cref="JsonSerializer"/> with the caller's options. The three RGB
    /// channels are emitted as a unit (not subject to ignore conditions individually) since
    /// they represent a single logical value.
    ///
    /// On the read side the converter is deliberately lenient: property names are matched
    /// case-insensitively against the literal names regardless of any naming policy, so
    /// JSON written under any policy still round-trips.
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
                // RGB channels are a single logical triple, so all three are emitted as a
                // unit (not subject to DefaultIgnoreCondition individually). The property
                // names still flow through PropertyNamingPolicy.
                writer.WriteNumber(JsonWriteHelpers.ConvertName(nameof(value.R), options), value.R);
                writer.WriteNumber(JsonWriteHelpers.ConvertName(nameof(value.G), options), value.G);
                writer.WriteNumber(JsonWriteHelpers.ConvertName(nameof(value.B), options), value.B);
            }
            else
            {
                JsonWriteHelpers.WriteProperty(writer, options, nameof(value.Named), value.Named);
            }
            writer.WriteEndObject();
        }
    }
}
