using Spectre.Console;

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

        /// <summary>The Spectre.Console style to apply to the heading text.</summary>
        public Style Style { get; }

        /// <inheritdoc/>
        public Color? Foreground => Style.Foreground == Color.Default ? null : Style.Foreground;

        /// <inheritdoc/>
        public Color? Background => Style.Background == Color.Default ? null : Style.Background;

        /// <inheritdoc/>
        public Decoration Decoration => Style.Decoration;

        public override bool Equals(object? obj)
            => obj is SpectreStyleHeaderStyle other && Style == other.Style;

        public override int GetHashCode() => Style.GetHashCode();
    }
}
