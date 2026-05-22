using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
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

        private static readonly Dictionary<NamedColor, Color> s_colorMap = Enum.GetValues(typeof(NamedColor))
            .Cast<NamedColor>()
            .ToDictionary(c => c, c => (Color)typeof(Color).GetProperty(c.ToString())!.GetValue(null)!);

        /// <summary>
        /// Converts an <see cref="IHeaderStyle"/> (which all of our styling types implement,
        /// including <see cref="TextStyle"/>) to a Spectre.Console <see cref="Style"/>.
        /// </summary>
        internal static Style ToSpectreStyle(this IHeaderStyle headerStyle)
        {
            var decoration = ToSpectreDecoration(headerStyle.Decoration);
            var foreground = headerStyle.Foreground != null ? headerStyle.Foreground.ToSpectreColor() : Color.Default;
            var background = headerStyle.Background != null ? headerStyle.Background.ToSpectreColor() : Color.Default;
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

        internal static Color ToSpectreColor(this TextColor textColor)
        {
            if (textColor.IsRgb)
            {
                return new Color(textColor.R, textColor.G, textColor.B);
            }

            return s_colorMap[textColor.Named];
        }

        private static readonly Dictionary<TextTableBorder, TableBorder> s_tableBorderMap = Enum.GetValues(typeof(TextTableBorder))
            .Cast<TextTableBorder>()
            .ToDictionary(b => b, b => (TableBorder)typeof(TableBorder).GetProperty(b.ToString())!.GetValue(null)!);

        /// <summary>
        /// Converts a <see cref="TextTableBorder"/> to its Spectre.Console <see cref="TableBorder"/> counterpart.
        /// </summary>
        internal static TableBorder ToSpectreTableBorder(this TextTableBorder border)
            => s_tableBorderMap[border];

        private static readonly Dictionary<TextJustification, Justify> s_justifyMap = new()
        {
            { TextJustification.Left, Justify.Left },
            { TextJustification.Right, Justify.Right },
            { TextJustification.Center, Justify.Center },
        };

        /// <summary>
        /// Converts a <see cref="TextJustification"/> to its Spectre.Console <see cref="Justify"/> counterpart.
        /// </summary>
        internal static Justify ToSpectreJustify(this TextJustification justification)
            => s_justifyMap[justification];

        private static readonly Dictionary<RuleBorder, BoxBorder> s_boxBorderMap = new()
        {
            { RuleBorder.None,    BoxBorder.None    },
            { RuleBorder.Ascii,   BoxBorder.Ascii   },
            { RuleBorder.Square,  BoxBorder.Square  },
            { RuleBorder.Rounded, BoxBorder.Rounded },
            { RuleBorder.Heavy,   BoxBorder.Heavy   },
            { RuleBorder.Double,  BoxBorder.Double  },
        };

        /// <summary>
        /// Converts a <see cref="RuleBorder"/> to its Spectre.Console <see cref="BoxBorder"/> counterpart.
        /// </summary>
        internal static BoxBorder ToSpectreBoxBorder(this RuleBorder border)
            => s_boxBorderMap[border];
    }
}
