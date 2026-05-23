using Spectre.Console;
using Spectre.Console.Rendering;

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
    /// </remarks>
    public interface ISpectreHeaderStyle
    {
        /// <summary>
        /// The Spectre.Console <see cref="Style"/> to use for the default styled-markup heading
        /// path (where the heading text is wrapped in markup tags). Ignored when
        /// <see cref="TryRenderHeading"/> returns a non-<see langword="null"/> renderable.
        /// </summary>
        Style Style { get; }

        /// <summary>
        /// Attempts to produce a fully-formed <see cref="IRenderable"/> for the heading.
        /// When this method returns <see langword="null"/>, the caller falls back to the
        /// styled-markup path using <see cref="Style"/>.
        /// </summary>
        /// <param name="plainText">The plain (unescaped) text content of the heading.</param>
        /// <returns>
        /// An <see cref="IRenderable"/> (e.g. <c>FigletText</c> or <c>Rule</c>), or
        /// <see langword="null"/> to indicate the caller should use the default styled-markup
        /// path instead.
        /// </returns>
        IRenderable? TryRenderHeading(string plainText);
    }
}
