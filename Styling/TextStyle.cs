using System;
using System.Collections.Generic;

namespace ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Represents a text style with decoration, foreground, and background color.
    /// This is the public abstraction that replaces direct use of Spectre.Console.Style in DisplayOptions.
    /// </summary>
    public sealed class TextStyle
    {
        public TextStyle(TextDecoration decoration = TextDecoration.None, TextColor? foreground = null, TextColor? background = null)
        {
            Decoration = decoration;
            Foreground = foreground;
            Background = background;
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

        public override int GetHashCode()
            => HashCode.Combine(Decoration, Foreground, Background);

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
                else if (TryParseColor(part, out var color))
                {
                    if (isBackground)
                        background = color;
                    else
                        foreground = color;
                }
            }

            return new TextStyle(decoration, foreground, background);
        }

        private static bool TryParseDecoration(string value, out TextDecoration decoration)
        {
            decoration = value switch
            {
                "bold" => TextDecoration.Bold,
                "dim" => TextDecoration.Dim,
                "italic" => TextDecoration.Italic,
                "underline" => TextDecoration.Underline,
                "slowblink" => TextDecoration.SlowBlink,
                "rapidblink" => TextDecoration.RapidBlink,
                "invert" => TextDecoration.Invert,
                "conceal" => TextDecoration.Conceal,
                "strikethrough" => TextDecoration.Strikethrough,
                _ => TextDecoration.None,
            };
            return decoration != TextDecoration.None;
        }

        private static bool TryParseColor(string value, out TextColor? color)
        {
            color = value switch
            {
                "black" => TextColor.Black,
                "red" => TextColor.Red,
                "green" => TextColor.Green,
                "yellow" => TextColor.Yellow,
                "blue" => TextColor.Blue,
                "purple" => TextColor.Purple,
                "default" => TextColor.Default,
                _ => null,
            };
            return color != null;
        }
    }
}
