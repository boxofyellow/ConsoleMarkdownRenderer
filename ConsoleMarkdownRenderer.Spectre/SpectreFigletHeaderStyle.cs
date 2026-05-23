using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    /// <summary>
    /// An <see cref="ISpectreHeaderStyle"/> that renders the heading as large ASCII art via
    /// Spectre.Console's <c>FigletText</c> widget.
    /// </summary>
    /// <remarks>
    /// Unlike the main package's <c>FigletTextStyle</c>, this type does not perform any
    /// async font loading — it accepts an already-loaded <see cref="FigletFont"/> (or
    /// <see langword="null"/> to use Spectre.Console's built-in default font). The higher-level
    /// <c>DisplayOptions</c> / <c>FigletTextStyle</c> in the main package is responsible for
    /// async font loading and then hands the loaded font to an instance of this class via its
    /// adapter layer.
    /// </remarks>
    public sealed class SpectreFigletHeaderStyle : ISpectreHeaderStyle
    {
        /// <summary>
        /// Creates a new <see cref="SpectreFigletHeaderStyle"/>.
        /// </summary>
        /// <param name="font">
        /// An already-loaded FIGlet font, or <see langword="null"/> to use Spectre.Console's
        /// built-in default font.
        /// </param>
        /// <param name="justification">
        /// The horizontal justification of the rendered FIGlet text. When <see langword="null"/>,
        /// Spectre.Console's default justification is used.
        /// </param>
        /// <param name="foreground">
        /// The foreground color forwarded to <c>FigletText.Color</c>. When <see langword="null"/>,
        /// the FIGlet text inherits whatever color Spectre.Console would otherwise use.
        /// </param>
        public SpectreFigletHeaderStyle(
            FigletFont? font = null,
            Justify? justification = null,
            Color? foreground = null)
        {
            Font = font;
            Justification = justification;
            Foreground = foreground;
        }

        /// <summary>The FIGlet font to use, or <see langword="null"/> for the default font.</summary>
        public FigletFont? Font { get; }

        /// <summary>The horizontal justification, or <see langword="null"/> for Spectre.Console's default.</summary>
        public Justify? Justification { get; }

        /// <summary>The foreground color, or <see langword="null"/> to inherit.</summary>
        public Color? Foreground { get; }

        /// <inheritdoc/>
        /// <returns>Always <see cref="Style.Plain"/>; FigletText does not use the markup path.</returns>
        public Style Style => Style.Plain;

        /// <inheritdoc/>
        public IRenderable? TryRenderHeading(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return null;
            }
            var figlet = Font is { } font
                ? new FigletText(font, plainText)
                : new FigletText(plainText);
            if (Justification.HasValue)
            {
                figlet.Justification = Justification.Value;
            }
            if (Foreground.HasValue)
            {
                figlet.Color = Foreground.Value;
            }
            return figlet;
        }

        public override bool Equals(object? obj)
            => obj is SpectreFigletHeaderStyle other
                && Equals(Font, other.Font)
                && Justification == other.Justification
                && Foreground == other.Foreground;

        public override int GetHashCode() => HashCode.Combine(Font, Justification, Foreground);
    }
}
