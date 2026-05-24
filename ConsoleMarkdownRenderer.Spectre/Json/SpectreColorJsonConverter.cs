using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Json
{
    /// <summary>
    /// JSON converter for Spectre.Console <see cref="Color"/>. Serializes named colors as
    /// <c>{ "named": "Yellow" }</c> and RGB colors as <c>{ "r": …, "g": …, "b": … }</c>.
    /// The default color is serialized as <c>null</c>.
    /// </summary>
    internal sealed class SpectreColorJsonConverter : JsonConverter<Color?>
    {
        private static readonly Dictionary<string, Color> s_namedColors =
            typeof(Color)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(Color))
                .ToDictionary(p => p.Name, p => (Color)p.GetValue(null)!, StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<Color, string> s_colorNames =
            s_namedColors
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

        /// <summary>
        /// Returns the name of a <see cref="Color"/> for JSON serialization, or
        /// <see langword="null"/> if not found in the named-color lookup.
        /// </summary>
        internal static string? GetColorName(Color color)
            => s_colorNames.TryGetValue(color, out var name) ? name : null;

        /// <summary>
        /// Returns a <see cref="Color"/> by its name, or <see langword="null"/> if not found.
        /// </summary>
        internal static Color? GetColorByName(string name)
            => s_namedColors.TryGetValue(name, out var color) ? color : null;

        public override Color? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.None && !reader.Read())
            {
                throw new JsonException("Expected a color token.");
            }

            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            using var doc = JsonDocument.ParseValue(ref reader);
            var element = doc.RootElement;

            string? namedValue = null;
            byte? r = null, g = null, b = null;

            foreach (var prop in element.EnumerateObject())
            {
                switch (prop.Name.ToLowerInvariant())
                {
                    case "named":
                        namedValue = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString();
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

            if (r.HasValue || g.HasValue || b.HasValue)
            {
                return new Color(r ?? 0, g ?? 0, b ?? 0);
            }

            if (!string.IsNullOrEmpty(namedValue))
            {
                var found = GetColorByName(namedValue);
                if (found.HasValue)
                {
                    return found.Value;
                }
            }

            return Color.Default;
        }

        public override void Write(Utf8JsonWriter writer, Color? value, JsonSerializerOptions options)
        {
            if (value is null || value.Value == Color.Default)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            var name = GetColorName(value.Value);
            if (name is not null)
            {
                writer.WriteString(SpectreJsonWriteHelpers.ConvertName("Named", options), name);
            }
            else
            {
                writer.WriteNumber(SpectreJsonWriteHelpers.ConvertName("R", options), value.Value.R);
                writer.WriteNumber(SpectreJsonWriteHelpers.ConvertName("G", options), value.Value.G);
                writer.WriteNumber(SpectreJsonWriteHelpers.ConvertName("B", options), value.Value.B);
            }
            writer.WriteEndObject();
        }
    }
}
