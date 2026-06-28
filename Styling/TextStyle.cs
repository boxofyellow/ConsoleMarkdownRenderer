using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling;

/// <summary>
/// Represents a text style with decoration, foreground, and background color.
/// This is the public abstraction that replaces direct use of Spectre.Console.Style in DisplayOptions.
/// </summary>
[SourceFile]
public sealed class TextStyle : IHeaderStyle
{
    public TextStyle(TextDecoration decoration = TextDecoration.None, TextColor? foreground = null, TextColor? background = null)
    {
        Decoration = decoration;
        Foreground = foreground;
        Background = background;
    }

    internal TextStyle(List<JsonProperty> properties, JsonSerializerOptions options)
    {
        var decoration = TextDecoration.None;
        TextColor? foreground = null;
        TextColor? background = null;

        var decorationPropertyName = JsonWriteHelpers.ConvertName(nameof(Decoration), options).ToLowerInvariant();
        var foregroundPropertyName = JsonWriteHelpers.ConvertName(nameof(Foreground), options).ToLowerInvariant();
        var backgroundPropertyName = JsonWriteHelpers.ConvertName(nameof(Background), options).ToLowerInvariant();

        foreach (var prop in properties)
        {
            var propNameLower = prop.Name.ToLowerInvariant();
            if (propNameLower == decorationPropertyName)
            {
                decoration = prop.Value.Deserialize<TextDecoration>(options);
            }
            else if (propNameLower == foregroundPropertyName)
            {
                foreground = prop.Value.Deserialize<TextColor?>(options);
            }
            else if (propNameLower == backgroundPropertyName)
            {
                background = prop.Value.Deserialize<TextColor?>(options);
            }
            else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
            {
                throw new JsonException($"Unrecognized property on {nameof(TextStyle)}: '{prop.Name}'.");
            }
        }

        Decoration = decoration;
        Foreground = foreground;
        Background = background;
    }

    internal void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
    {
        JsonWriteHelpers.WriteProperty(writer, options, nameof(Decoration), Decoration);
        JsonWriteHelpers.WriteProperty(writer, options, nameof(Foreground), Foreground);
        JsonWriteHelpers.WriteProperty(writer, options, nameof(Background), Background);   
    }

    public TextDecoration Decoration { get; }
    public TextColor? Foreground { get; }
    public TextColor? Background { get; }

    /// <summary>
    /// A plain style with no decoration or colors.
    /// </summary>
    public static TextStyle Plain { get; } = new();

    public override bool Equals(object? obj)
    {
        if (obj is not TextStyle other)
        {
            return false;
        }
        return Decoration == other.Decoration
            && Equals(Foreground, other.Foreground)
            && Equals(Background, other.Background);
    }

    public override int GetHashCode() => HashCode.Combine(
        Decoration,
        Foreground,
        Background);

    public override string ToString()
    {
        var parts = new List<string>();
        if (Decoration != TextDecoration.None)
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

    /// <summary>
    /// Implicit conversion from string to TextStyle, allowing markup-style strings like "red on purple".
    /// </summary>
    public static implicit operator TextStyle(string markup) => FromMarkup(markup);

    /// <summary>
    /// Parses a simple Spectre-style markup string (e.g. "bold", "red on blue", "bold italic red on green").
    /// Supports: decoration names, color names, "on" separator for background.
    /// </summary>
    public static TextStyle FromMarkup(string markup)
    {
        var decoration = TextDecoration.None;
        TextColor? foreground = null;
        TextColor? background = null;

        var parts = markup.Split(' ');
        bool isBackground = false;

        foreach (var part in parts)
        {
            if (part == "on")
            {
                isBackground = true;
                continue;
            }

            if (TryParseDecoration(part, out var dec))
            {
                decoration |= dec;
            }
            else if (TextColor.TryParseColor(part, out var color))
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
                var colorName = part.Substring(3);
                if (TextColor.TryParseColor(colorName, out var fgColor))
                {
                    foreground = fgColor;
                }
            }
            else if (part.StartsWith("bg:", StringComparison.OrdinalIgnoreCase))
            {
                var colorName = part.Substring(3);
                if (TextColor.TryParseColor(colorName, out var bgColor))
                {
                    background = bgColor;
                }
            }
        }

        return new TextStyle(decoration, foreground, background);
    }

    private static readonly Dictionary<string, TextDecoration> s_decorationNames = Enum.GetValues<TextDecoration>()
        .Where(d => d != TextDecoration.None)
        .ToDictionary(d => d.ToString().ToLowerInvariant(), d => d, StringComparer.OrdinalIgnoreCase);

    private static bool TryParseDecoration(string value, out TextDecoration decoration) 
        => s_decorationNames.TryGetValue(value, out decoration);
}
