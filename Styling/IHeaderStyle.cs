namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Marker / capability interface implemented by every style that can be assigned to a
    /// Markdown heading level via <see cref="DisplayOptions.Headers"/> or
    /// <see cref="DisplayOptions.Header"/>.
    /// </summary>
    /// <remarks>
    /// Two implementations ship with this library:
    /// <list type="bullet">
    /// <item><see cref="TextStyle"/> – renders the heading as inline styled markup
    /// (the original behavior). <see cref="Justification"/> is always <see langword="null"/>.</item>
    /// <item><see cref="FigletTextStyle"/> – renders the heading as large ASCII art using
    /// Spectre.Console's <c>FigletText</c> widget. <see cref="Justification"/> may be set to
    /// position the FIGlet output.</item>
    /// </list>
    /// </remarks>
    public interface IHeaderStyle
    {
        /// <summary>
        /// The horizontal justification to use when rendering the heading. Only honored by
        /// implementations that support justification (currently <see cref="FigletTextStyle"/>);
        /// for plain <see cref="TextStyle"/> this is always <see langword="null"/>.
        /// </summary>
        TextJustification? Justification { get; }
    }
}
