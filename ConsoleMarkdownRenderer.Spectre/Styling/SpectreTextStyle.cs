using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling
{
    [SpectreSourceFile]
    public sealed class SpectreTextStyle : ISpectreHeaderStyle
    {
        public Decoration Decoration { get; }
        public Color? Foreground { get; }
        public Color? Background { get; }

        public SpectreTextStyle(Decoration decoration = Decoration.None, Color? foreground = null, Color? background = null)
        {
            Decoration = decoration;
            Foreground = foreground;
            Background = background;
        }

        internal SpectreTextStyle(List<JsonProperty> properties, JsonSerializerOptions options)
        {
            var decoration = Decoration.None;
            Color? foreground = null;
            Color? background = null;

            var decorationPropertyName = JsonWriteHelpers.ConvertName(nameof(Decoration), options).ToLowerInvariant();
            var foregroundPropertyName = JsonWriteHelpers.ConvertName(nameof(Foreground), options).ToLowerInvariant();
            var backgroundPropertyName = JsonWriteHelpers.ConvertName(nameof(Background), options).ToLowerInvariant();

            foreach (var prop in properties)
            {
                var propNameLower = prop.Name.ToLowerInvariant();
                if (propNameLower == decorationPropertyName)
                {
                    decoration = prop.Value.Deserialize<Decoration>(options);
                }
                else if (propNameLower == foregroundPropertyName)
                {
                    foreground = prop.Value.Deserialize<Color?>(options);
                }
                else if (propNameLower == backgroundPropertyName)
                {
                    background = prop.Value.Deserialize<Color?>(options);
                }
                else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
                {
                    throw new JsonException($"Unrecognized property on {nameof(SpectreTextStyle)}: '{prop.Name}'.");
                }
            }

            Decoration = decoration;
            Foreground = foreground;
            Background = background;
        }

        public override bool Equals(object? obj)
        {
            if (obj is not SpectreTextStyle other)
            {
                return false;
            }
            return Decoration == other.Decoration
                && Equals(Foreground, other.Foreground)
                && Equals(Background, other.Background);
        }

        public override int GetHashCode()
            => HashCode.Combine(Decoration, Foreground, Background);

        public override string ToString()
        {
            var parts = new List<string>();
            if (Decoration != Decoration.None)
            {
                parts.Add(Decoration.ToString());
            }
            if (Foreground != null)
            {
                parts.Add($"fg:{Foreground}");
            }
            if (Background != null)
            {
                parts.Add($"bg:{Background}");
            }
            return parts.Count > 0 ? string.Join(" ", parts) : "plain";
        }

        public static implicit operator SpectreTextStyle(string markup) => FromMarkup(markup);

        public static SpectreTextStyle FromMarkup(string markup)
        {
            var decoration = Decoration.None;
            Color? foreground = null;
            Color? background = null;

            var parts = markup.Split(' ');
            bool isBackground = false;

            foreach (var part in parts)
            {
                if (part == "on")
                {
                    isBackground = true;
                    continue;
                }

                if (Mappings.DecorationByName.TryGetValue(part, out var dec))
                {
                    decoration |= dec;
                }
                else if (Utilities.TryParseColor(part, out var color))
                {
                    if (isBackground)
                    {
                        background = color;
                    }
                    else
                    {
                        foreground = color;
                    }
                }
                else if (part.StartsWith("fg:", StringComparison.OrdinalIgnoreCase))
                {
                    var colorValue = part.Substring(3);
                    if (Utilities.TryParseColor(colorValue, out var fgColor))
                    {
                        foreground = fgColor;
                    }
                }
                else if (part.StartsWith("bg:", StringComparison.OrdinalIgnoreCase))
                {
                    var colorValue = part.Substring(3);
                    if (Utilities.TryParseColor(colorValue, out var bgColor))
                    {
                        background = bgColor;
                    }
                }
            }

            return new SpectreTextStyle(decoration, foreground, background);
        }

        internal void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Decoration), Decoration);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Foreground), Foreground);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Background), Background);   
        }
    }
}

