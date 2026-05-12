namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// A heading style that renders the heading text as large ASCII art via Spectre.Console's
    /// <c>FigletText</c> widget. Implementations of <c>FigletText</c> are intentionally limited
    /// to the small set of options exposed here — <see cref="Justification"/> and
    /// <see cref="Foreground"/> — because <c>FigletText</c> does not support the decoration or
    /// background facilities of <see cref="TextStyle"/>. It is therefore modeled as a peer of
    /// <see cref="TextStyle"/> (both implement <see cref="IHeaderStyle"/>) rather than as a
    /// subclass.
    /// </summary>
    /// <remarks>
    /// Assign an instance to a level in <see cref="DisplayOptions.Headers"/> (or to
    /// <see cref="DisplayOptions.Header"/>) to opt that level in to FIGlet rendering. The
    /// default <see cref="DisplayOptions.Headers"/> list configures <c>#</c>-level headings
    /// (H1) to use a centered <see cref="FigletTextStyle"/>; deeper levels continue to use
    /// the original styled, <c>#</c>-wrapped markup unless explicitly overridden.
    /// </remarks>
    public sealed class FigletTextStyle : IHeaderStyle
    {
        public FigletTextStyle(
            TextJustification? justification = null,
            TextColor? foreground = null)
        {
            Justification = justification;
            Foreground = foreground;
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

        public override bool Equals(object? obj)
            => obj is FigletTextStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground);

        public override int GetHashCode() => HashCode.Combine(Justification, Foreground);
    }
}
