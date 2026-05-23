using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    /// <summary>
    /// Capability interface implemented by every style that can be assigned to a Markdown
    /// heading level via <see cref="SpectreDisplayOptions.Headers"/> or
    /// <see cref="SpectreDisplayOptions.Header"/>.
    /// </summary>
    /// <remarks>
    /// Three implementations ship with this package:
    /// <list type="bullet">
    /// <item><see cref="SpectreStyleHeaderStyle"/> – renders the heading as inline styled markup.</item>
    /// <item><see cref="SpectreFigletHeaderStyle"/> – renders the heading as large ASCII art using
    /// Spectre.Console's <c>FigletText</c> widget.</item>
    /// <item><see cref="SpectreRuleHeaderStyle"/> – renders the heading text as the title of a
    /// Spectre.Console <c>Rule</c> widget (e.g. <c>──── Overview ────</c>).</item>
    /// </list>
    /// Some members are only meaningful for a subset of implementations; in that case the
    /// implementation explicitly returns a hard-coded value (for example
    /// <see cref="SpectreFigletHeaderStyle.Background"/> and <see cref="SpectreRuleHeaderStyle.Background"/>
    /// are always <see langword="null"/>).
    /// </remarks>
    public interface ISpectreHeaderStyle
    {
        /// <summary>
        /// The foreground color used when rendering the heading.
        /// </summary>
        Color? Foreground { get; }

        /// <summary>
        /// The background color used when rendering the heading. Only honored by
        /// implementations that support a background color (currently <see cref="SpectreStyleHeaderStyle"/>);
        /// for <see cref="SpectreFigletHeaderStyle"/> and <see cref="SpectreRuleHeaderStyle"/> this is always
        /// <see langword="null"/>.
        /// </summary>
        Color? Background { get; }

        /// <summary>
        /// Text decoration (bold, italic, etc.) used when rendering the heading. Only honored
        /// by implementations that support text decoration (currently <see cref="SpectreStyleHeaderStyle"/>);
        /// for <see cref="SpectreFigletHeaderStyle"/> and <see cref="SpectreRuleHeaderStyle"/> this is always
        /// <see cref="Decoration.None"/>.
        /// </summary>
        Decoration Decoration { get; }
    }
}
