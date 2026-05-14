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
    /// <see cref="FigletTextStyle.Background"/> is always <see langword="null"/>).
    /// </remarks>
    public interface IHeaderStyle
    {
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
    }
}
