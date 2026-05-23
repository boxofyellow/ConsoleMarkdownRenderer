using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Internal extension methods to convert TextStyle abstractions to Spectre.Console types.
    /// This is the only place where the Styling types interact with Spectre.Console.
    /// </summary>
    internal static class TextStyleExtensions
    {
        private static readonly Dictionary<TextDecoration, Decoration> s_decorationMap
            = BuildMap<TextDecoration, Decoration>(d => d != TextDecoration.None);

        private static readonly Dictionary<NamedColor, Color> s_colorMap
            = BuildMap<NamedColor, Color>();

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

        private static readonly Dictionary<TextTableBorder, TableBorder> s_tableBorderMap
            = BuildMap<TextTableBorder, TableBorder>();

        /// <summary>
        /// Converts a <see cref="TextTableBorder"/> to its Spectre.Console <see cref="TableBorder"/> counterpart.
        /// </summary>
        internal static TableBorder ToSpectreTableBorder(this TextTableBorder border)
            => s_tableBorderMap[border];

        private static readonly Dictionary<TextJustification, Justify> s_justifyMap
            = BuildMap<TextJustification, Justify>();

        /// <summary>
        /// Converts a <see cref="TextJustification"/> to its Spectre.Console <see cref="Justify"/> counterpart.
        /// </summary>
        internal static Justify ToSpectreJustify(this TextJustification justification)
            => s_justifyMap[justification];

        private static readonly Dictionary<RuleBorder, BoxBorder> s_boxBorderMap
            = BuildMap<RuleBorder, BoxBorder>();

        /// <summary>
        /// Converts a <see cref="RuleBorder"/> to its Spectre.Console <see cref="BoxBorder"/> counterpart.
        /// </summary>
        internal static BoxBorder ToSpectreBoxBorder(this RuleBorder border)
            => s_boxBorderMap[border];

        /// <summary>
        /// Builds a dictionary that maps every value of the source enum <typeparamref name="TFrom"/> to a
        /// corresponding value of <typeparamref name="TTo"/> with the same name. If <typeparamref name="TTo"/>
        /// is itself an enum the value is resolved via <see cref="Enum.Parse(Type, string)"/>; otherwise it is
        /// resolved by looking up a public static property on <typeparamref name="TTo"/> with the matching name.
        /// </summary>
        /// <typeparam name="TFrom">The enum type used as the dictionary key.</typeparam>
        /// <typeparam name="TTo">
        /// The target type used as the dictionary value. Must either be an enum or expose public static
        /// properties named the same as each (non-filtered) value of <typeparamref name="TFrom"/>.
        /// </typeparam>
        /// <param name="filter">
        /// Optional predicate used to exclude values from <typeparamref name="TFrom"/> (for example, a flags
        /// enum's <c>None</c> sentinel that has no counterpart on <typeparamref name="TTo"/>).
        /// </param>
        private static Dictionary<TFrom, TTo> BuildMap<TFrom, TTo>(Func<TFrom, bool>? filter = null)
            where TFrom : struct, Enum
        {
            IEnumerable<TFrom> values = Enum.GetValues<TFrom>();
            if (filter != null)
            {
                values = values.Where(filter);
            }

            return values.ToDictionary(v => v, v => ResolveValue<TTo>(v.ToString()));
        }

        private static TTo ResolveValue<TTo>(string name)
        {
            if (typeof(TTo).IsEnum)
            {
                return (TTo)Enum.Parse(typeof(TTo), name);
            }

            return (TTo)typeof(TTo).GetProperty(name)!.GetValue(null)!;
        }
    }
}
