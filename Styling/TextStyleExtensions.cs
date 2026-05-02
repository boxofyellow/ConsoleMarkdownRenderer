using System.Collections.Generic;
using Spectre.Console;

namespace ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Internal extension methods to convert TextStyle abstractions to Spectre.Console types.
    /// This is the only place where the Styling types interact with Spectre.Console.
    /// </summary>
    internal static class TextStyleExtensions
    {
        private static readonly Dictionary<TextDecoration, Decoration> s_decorationMap = new()
        {
            { TextDecoration.Bold, Decoration.Bold },
            { TextDecoration.Dim, Decoration.Dim },
            { TextDecoration.Italic, Decoration.Italic },
            { TextDecoration.Underline, Decoration.Underline },
            { TextDecoration.SlowBlink, Decoration.SlowBlink },
            { TextDecoration.RapidBlink, Decoration.RapidBlink },
            { TextDecoration.Invert, Decoration.Invert },
            { TextDecoration.Conceal, Decoration.Conceal },
            { TextDecoration.Strikethrough, Decoration.Strikethrough },
        };

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
            var result = Decoration.None;

            foreach (var (textDec, spectreDec) in s_decorationMap)
            {
                if (decoration.HasFlag(textDec))
                {
                    result |= spectreDec;
                }
            }

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
