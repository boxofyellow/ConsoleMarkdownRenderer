namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// A heading style that renders the heading text as large ASCII art via Spectre.Console's
    /// <c>FigletText</c> widget. Implementations of <c>FigletText</c> are intentionally limited
    /// to the small set of options exposed here — <see cref="Justification"/>,
    /// <see cref="Foreground"/> and <see cref="FontPath"/> — because <c>FigletText</c> does
    /// not support the decoration or background facilities of <see cref="TextStyle"/>. It is
    /// therefore modeled as a peer of <see cref="TextStyle"/> (both implement
    /// <see cref="IHeaderStyle"/>) rather than as a subclass.
    /// </summary>
    /// <remarks>
    /// Assign an instance to a level in <see cref="DisplayOptions.Headers"/> (or to
    /// <see cref="DisplayOptions.Header"/>) to opt that level in to FIGlet rendering. By
    /// default <see cref="DisplayOptions.Headers"/> configures <c>#</c>-level headings (H1)
    /// to use a <see cref="FigletTextStyle"/>; deeper levels continue to use the styled,
    /// <c>#</c>-wrapped markup unless explicitly overridden.
    /// </remarks>
    public sealed class FigletTextStyle : IHeaderStyle
    {
        public FigletTextStyle(
            TextJustification? justification = null,
            TextColor? foreground = null,
            string? fontPath = null)
        {
            Justification = justification;
            Foreground = foreground;
            FontPath = fontPath;
        }

        /// <summary>
        /// The horizontal justification for the rendered FIGlet text. When <see langword="null"/>,
        /// Spectre.Console's default justification is used.
        /// </summary>
        public TextJustification? Justification { get; }

        /// <summary>
        /// The foreground color forwarded to <c>FigletText.Color</c>. When <see langword="null"/>,
        /// the FIGlet text inherits whatever color Spectre.Console would otherwise use.
        /// </summary>
        public TextColor? Foreground { get; }

        /// <summary>
        /// Optional path to a custom FIGlet font file (<c>.flf</c>). When non-<see langword="null"/>
        /// the font is loaded via <see cref="Spectre.Console.FigletFont.Load(string)"/> and used
        /// for the rendered FIGlet text; otherwise Spectre.Console's built-in default font is used.
        /// </summary>
        public string? FontPath { get; }

        /// <summary>
        /// Always <see langword="null"/>: <c>FigletText</c> does not support a background color.
        /// </summary>
        TextColor? IHeaderStyle.Background => null;

        /// <summary>
        /// Always <see cref="TextDecoration.None"/>: <c>FigletText</c> does not support text
        /// decoration (bold, italic, etc.).
        /// </summary>
        TextDecoration IHeaderStyle.Decoration => TextDecoration.None;

        public override bool Equals(object? obj)
            => obj is FigletTextStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground)
                && string.Equals(FontPath, other.FontPath, StringComparison.Ordinal);

        public override int GetHashCode() => HashCode.Combine(Justification, Foreground, FontPath);
    }
}
