using Spectre.Console;

namespace ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Internal extension methods to convert TextStyle abstractions to Spectre.Console types.
    /// This is the only place where the Styling types interact with Spectre.Console.
    /// </summary>
    internal static class TextStyleExtensions
    {
        /// <summary>
        /// Converts a TextStyle to a Spectre.Console Style.
        /// </summary>
        internal static Style ToSpectreStyle(this TextStyle textStyle)
        {
            var decoration = ToSpectreDecoration(textStyle.Decoration);
            var foreground = textStyle.Foreground != null ? ToSpectreColor(textStyle.Foreground) : Color.Default;
            var background = textStyle.Background != null ? ToSpectreColor(textStyle.Background) : Color.Default;
            return new Style(foreground: foreground, background: background, decoration: decoration);
        }

        private static Decoration ToSpectreDecoration(TextDecoration decoration)
        {
            var result = Spectre.Console.Decoration.None;

            if (decoration.HasFlag(TextDecoration.Bold)) result |= Spectre.Console.Decoration.Bold;
            if (decoration.HasFlag(TextDecoration.Dim)) result |= Spectre.Console.Decoration.Dim;
            if (decoration.HasFlag(TextDecoration.Italic)) result |= Spectre.Console.Decoration.Italic;
            if (decoration.HasFlag(TextDecoration.Underline)) result |= Spectre.Console.Decoration.Underline;
            if (decoration.HasFlag(TextDecoration.SlowBlink)) result |= Spectre.Console.Decoration.SlowBlink;
            if (decoration.HasFlag(TextDecoration.RapidBlink)) result |= Spectre.Console.Decoration.RapidBlink;
            if (decoration.HasFlag(TextDecoration.Invert)) result |= Spectre.Console.Decoration.Invert;
            if (decoration.HasFlag(TextDecoration.Strikethrough)) result |= Spectre.Console.Decoration.Strikethrough;

            return result;
        }

        private static Color ToSpectreColor(TextColor textColor)
        {
            if (textColor.IsRgb)
            {
                return new Color(textColor.R, textColor.G, textColor.B);
            }

            return textColor.Named switch
            {
                NamedColor.Black => Color.Black,
                NamedColor.Red => Color.Red,
                NamedColor.Green => Color.Green,
                NamedColor.Yellow => Color.Yellow,
                NamedColor.Blue => Color.Blue,
                NamedColor.Purple => Color.Purple,
                NamedColor.Default => Color.Default,
                _ => Color.Default,
            };
        }
    }
}
