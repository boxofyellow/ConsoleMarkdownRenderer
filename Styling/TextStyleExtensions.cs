using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Internal extension methods to convert TextStyle abstractions to Spectre.Console types,
    /// and back again. This is the only place where the Styling types interact with Spectre.Console.
    /// </summary>
    internal static class TextStyleExtensions
    {
        private static readonly Dictionary<TextDecoration, Decoration> s_decorationMap
            = BuildMap<TextDecoration, Decoration>(d => d != TextDecoration.None);

        private static readonly Dictionary<NamedColor, Color> s_colorMap
            = BuildMap<NamedColor, Color>();

        private static readonly Dictionary<Color, NamedColor> s_reverseColorMap
            = s_colorMap
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

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

        private static readonly Dictionary<TableBorder, TextTableBorder> s_reverseTableBorderMap
            = s_tableBorderMap
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

        /// <summary>
        /// Converts a <see cref="TextTableBorder"/> to its Spectre.Console <see cref="TableBorder"/> counterpart.
        /// </summary>
        internal static TableBorder ToSpectreTableBorder(this TextTableBorder border)
            => s_tableBorderMap[border];

        private static readonly Dictionary<TextJustification, Justify> s_justifyMap
            = BuildMap<TextJustification, Justify>();

        private static readonly Dictionary<Justify, TextJustification> s_reverseJustifyMap
            = s_justifyMap
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

        /// <summary>
        /// Converts a <see cref="TextJustification"/> to its Spectre.Console <see cref="Justify"/> counterpart.
        /// </summary>
        internal static Justify ToSpectreJustify(this TextJustification justification)
            => s_justifyMap[justification];

        private static readonly Dictionary<RuleBorder, BoxBorder> s_boxBorderMap
            = BuildMap<RuleBorder, BoxBorder>();

        private static readonly Dictionary<BoxBorder, RuleBorder> s_reverseBoxBorderMap
            = s_boxBorderMap
                .GroupBy(kvp => kvp.Value)
                .ToDictionary(g => g.Key, g => g.First().Key);

        /// <summary>
        /// Converts a <see cref="RuleBorder"/> to its Spectre.Console <see cref="BoxBorder"/> counterpart.
        /// </summary>
        internal static BoxBorder ToSpectreBoxBorder(this RuleBorder border)
            => s_boxBorderMap[border];

        // -----------------------------------------------------------------------
        // Reverse conversions: Spectre.Console → Text* types
        // -----------------------------------------------------------------------

        /// <summary>
        /// Converts a Spectre.Console <see cref="Style"/> back to a <see cref="TextStyle"/>.
        /// </summary>
        internal static TextStyle FromSpectreStyle(Style style)
        {
            var decoration = FromSpectreDecoration(style.Decoration);
            var foreground = style.Foreground == Color.Default ? null : FromSpectreColor(style.Foreground);
            var background = style.Background == Color.Default ? null : FromSpectreColor(style.Background);
            return new TextStyle(decoration, foreground, background);
        }

        /// <summary>
        /// Converts a Spectre.Console <see cref="Color"/> back to a <see cref="TextColor"/>.
        /// </summary>
        internal static TextColor? FromSpectreColor(Color color)
        {
            if (color == Color.Default)
            {
                return null;
            }
            if (s_reverseColorMap.TryGetValue(color, out var namedColor))
            {
                return TextColor.FromNamed(namedColor);
            }
            return TextColor.FromRgb(color.R, color.G, color.B);
        }

        private static TextDecoration FromSpectreDecoration(Decoration decoration)
        {
            var result = TextDecoration.None;
            foreach (var (textDec, spectreDec) in s_decorationMap)
            {
                if (decoration.HasFlag(spectreDec))
                {
                    result |= textDec;
                }
            }
            return result;
        }

        /// <summary>
        /// Converts a Spectre.Console <see cref="TableBorder"/> back to a <see cref="TextTableBorder"/>.
        /// </summary>
        internal static TextTableBorder FromSpectreTableBorder(TableBorder border)
            => s_reverseTableBorderMap.TryGetValue(border, out var result) ? result : TextTableBorder.Square;

        /// <summary>
        /// Converts a nullable Spectre.Console <see cref="Justify"/> back to a nullable <see cref="TextJustification"/>.
        /// </summary>
        internal static TextJustification? FromSpectreJustify(Justify? justify)
            => justify.HasValue && s_reverseJustifyMap.TryGetValue(justify.Value, out var result) ? result : null;

        /// <summary>
        /// Converts a nullable Spectre.Console <see cref="BoxBorder"/> back to a nullable <see cref="RuleBorder"/>.
        /// </summary>
        internal static RuleBorder? FromSpectreBoxBorder(BoxBorder? border)
            => border is not null && s_reverseBoxBorderMap.TryGetValue(border, out var result) ? result : null;

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
