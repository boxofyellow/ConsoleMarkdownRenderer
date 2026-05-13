namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Capability interface implemented by every style that can be assigned to a Markdown
    /// heading level via <see cref="DisplayOptions.Headers"/> or
    /// <see cref="DisplayOptions.Header"/>.
    /// </summary>
    /// <remarks>
    /// Two implementations ship with this library:
    /// <list type="bullet">
    /// <item><see cref="TextStyle"/> – renders the heading as inline styled markup.</item>
    /// <item><see cref="FigletTextStyle"/> – renders the heading as large ASCII art using
    /// Spectre.Console's <c>FigletText</c> widget.</item>
    /// </list>
    /// Some members are only meaningful for a subset of implementations; in that case the
    /// implementation explicitly returns a hard-coded value (for example
    /// <see cref="TextStyle.Justification"/> is always <see langword="null"/> and
    /// <see cref="FigletTextStyle.Background"/> is always <see langword="null"/>).
    /// </remarks>
    public interface IHeaderStyle
    {
        /// <summary>
        /// The horizontal justification to use when rendering the heading. Only honored by
        /// implementations that support justification (currently <see cref="FigletTextStyle"/>);
        /// for plain <see cref="TextStyle"/> this is always <see langword="null"/>.
        /// </summary>
        TextJustification? Justification { get; }

        /// <summary>
        /// The foreground color used when rendering the heading.
        /// </summary>
        TextColor? Foreground { get; }

        /// <summary>
        /// The background color used when rendering the heading. Only honored by
        /// implementations that support a background color (currently <see cref="TextStyle"/>);
        /// for <see cref="FigletTextStyle"/> this is always <see langword="null"/>.
        /// </summary>
        TextColor? Background { get; }

        /// <summary>
        /// Text decoration (bold, italic, etc.) used when rendering the heading. Only honored
        /// by implementations that support text decoration (currently <see cref="TextStyle"/>);
        /// for <see cref="FigletTextStyle"/> this is always <see cref="TextDecoration.None"/>.
        /// </summary>
        TextDecoration Decoration { get; }

        /// <summary>
        /// Optional path to a custom FIGlet font file (<c>.flf</c>) loaded via
        /// <see cref="Spectre.Console.FigletFont.Load(string)"/>. Only honored by
        /// implementations that support custom FIGlet fonts (currently
        /// <see cref="FigletTextStyle"/>); for <see cref="TextStyle"/> this is always
        /// <see langword="null"/>.
        /// </summary>
        string? FontPath { get; }
    }
}
