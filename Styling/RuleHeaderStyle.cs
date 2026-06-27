using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    [SourceFile]
    public sealed class RuleHeaderStyle : IHeaderStyle
    {
        public RuleHeaderStyle(
            TextJustification? justification = null,
            TextColor? foreground = null,
            RuleBorder? border = null)
        {
            Justification = justification;
            Foreground = foreground;
            Border = border;
        }

        internal RuleHeaderStyle(List<JsonProperty> properties, JsonSerializerOptions options)
        {
            TextJustification? justification = null;
            TextColor? foreground = null;
            RuleBorder? border = null;

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
                        : prop.Value.Deserialize<TextJustification>(options);
                }
                else if (propNameLower == foregroundPropertyName)
                {
                    foreground = prop.Value.Deserialize<TextColor?>(options);
                }
                else if (propNameLower == borderPropertyName)
                {
                    border = prop.Value.ValueKind == JsonValueKind.Null
                        ? null
                        : prop.Value.Deserialize<RuleBorder?>(options);
                }
                else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
                {
                    throw new JsonException($"Unrecognized property on {nameof(RuleHeaderStyle)}: '{prop.Name}'.");
                }
            }
            Justification = justification;
            Foreground = foreground;
            Border = border;
        }

        internal void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Justification), Justification);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Foreground), Foreground);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Border), Border);
        }

        public TextJustification? Justification { get; }

        public TextColor? Foreground { get; }

        public RuleBorder? Border { get; }

        /// <summary>
        /// <c>Rule</c> does not support a background color.
        /// </summary>
        TextColor? IHeaderStyle.Background => null;

        /// <summary>
        /// <c>Rule</c> does not support text decoration (bold, italic, etc.) on its title.
        /// </summary>
        TextDecoration IHeaderStyle.Decoration => TextDecoration.None;

        public override bool Equals(object? obj)
            => obj is RuleHeaderStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground)
                && Border == other.Border;

        public override int GetHashCode() => HashCode.Combine(
            Justification,
            Foreground,
            Border);
    }
}
