using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling
{
    [SpectreSourceFile]
    public sealed class SpectreRuleHeaderStyle : ISpectreHeaderStyle
    {
        public SpectreRuleHeaderStyle(
            Justify? justification = null,
            Color? foreground = null,
            BoxBorder? border = null)
        {
            Justification = justification;
            Foreground = foreground;
            Border = border;
        }

        internal SpectreRuleHeaderStyle(List<JsonProperty> properties, JsonSerializerOptions options)
        {
            Justify? justification = null;
            Color? foreground = null;
            BoxBorder? border = null;

            var justificationPropertyName = JsonWriteHelpers.ConvertName(nameof(Justification), options).ToLowerInvariant();
            var foregroundPropertyName = JsonWriteHelpers.ConvertName(nameof(Foreground), options).ToLowerInvariant();
            var borderPropertyName = JsonWriteHelpers.ConvertName(nameof(Border), options).ToLowerInvariant();

            foreach (var prop in properties)
            {
                var propNameLower = prop.Name.ToLowerInvariant();
                if (propNameLower == justificationPropertyName)
                {
                    justification = prop.Value.ValueKind == JsonValueKind.Null
                        ? null
                        : prop.Value.Deserialize<Justify>(options);
                }
                else if (propNameLower == foregroundPropertyName)
                {
                    foreground = prop.Value.Deserialize<Color?>(options);
                }
                else if (propNameLower == borderPropertyName)
                {
                    border = prop.Value.ValueKind == JsonValueKind.Null
                        ? null
                        : prop.Value.Deserialize<BoxBorder?>(options);
                }
                else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
                {
                    throw new JsonException($"Unrecognized property on {nameof(SpectreRuleHeaderStyle)}: '{prop.Name}'.");
                }
            }
            Justification = justification;
            Foreground = foreground;
            Border = border;
        }

        public Justify? Justification { get; }

        public Color? Foreground { get; }

        public BoxBorder? Border { get; }

        Color? ISpectreHeaderStyle.Background => null;

        Decoration ISpectreHeaderStyle.Decoration => Decoration.None;

        public override bool Equals(object? obj)
            => obj is SpectreRuleHeaderStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground)
                && Border == other.Border;

        public override int GetHashCode() => HashCode.Combine(Justification, Foreground, Border);

        internal void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Justification), Justification);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Foreground), Foreground);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Border), Border);
        }
    }
}
