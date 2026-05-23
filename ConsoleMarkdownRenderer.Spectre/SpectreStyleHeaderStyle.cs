using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    /// <summary>
    /// An <see cref="ISpectreHeaderStyle"/> that renders the heading as inline styled markup
    /// (the default heading path). The heading text is wrapped with the configured
    /// <see cref="Style"/> applied as Spectre.Console markup tags.
    /// </summary>
    /// <remarks>
    /// This is the Spectre-native equivalent of the main package's <c>TextStyle</c> when
    /// used as a heading style. Assign an instance to a level in
    /// <see cref="SpectreDisplayOptions.Headers"/> (or to <see cref="SpectreDisplayOptions.Header"/>)
    /// to opt that level in to styled-markup rendering.
    /// </remarks>
    public sealed class SpectreStyleHeaderStyle : ISpectreHeaderStyle
    {
        /// <summary>
        /// Creates a new <see cref="SpectreStyleHeaderStyle"/> with the given style.
        /// </summary>
        /// <param name="style">The Spectre.Console style to apply to the heading text.</param>
        public SpectreStyleHeaderStyle(Style style)
        {
            Style = style;
        }

        /// <inheritdoc/>
        public Style Style { get; }

        /// <inheritdoc/>
        /// <returns>Always <see langword="null"/>; this style uses the styled-markup path.</returns>
        public IRenderable? TryRenderHeading(string plainText) => null;

        public override bool Equals(object? obj)
            => obj is SpectreStyleHeaderStyle other && Style == other.Style;

        public override int GetHashCode() => Style.GetHashCode();
    }
}
