using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    /// <summary>
    /// An <see cref="ISpectreHeaderStyle"/> that renders the heading text as the title of a
    /// Spectre.Console <c>Rule</c> widget — for example <c>──── Overview ────</c>.
    /// </summary>
    public sealed class SpectreRuleHeaderStyle : ISpectreHeaderStyle
    {
        /// <summary>
        /// Creates a new <see cref="SpectreRuleHeaderStyle"/>.
        /// </summary>
        /// <param name="justification">Horizontal placement of the title within the rule line.
        /// When <see langword="null"/>, Spectre.Console's default justification is used.</param>
        /// <param name="foreground">Foreground colour applied to the heading text in the rule
        /// title. When <see langword="null"/>, the title inherits whatever colour
        /// Spectre.Console would otherwise use.</param>
        /// <param name="border">Border style for the rule line characters. When
        /// <see langword="null"/>, Spectre.Console's default <c>Rule</c> border is used.</param>
        public SpectreRuleHeaderStyle(
            Justify? justification = null,
            Color? foreground = null,
            BoxBorder? border = null)
        {
            Justification = justification;
            Foreground = foreground;
            Border = border;
        }

        /// <summary>The horizontal justification of the title, or <see langword="null"/> for Spectre.Console's default.</summary>
        public Justify? Justification { get; }

        /// <inheritdoc/>
        public Color? Foreground { get; }

        /// <summary>The border style for the rule line, or <see langword="null"/> for Spectre.Console's default.</summary>
        public BoxBorder? Border { get; }

        /// <inheritdoc/>
        /// <returns>Always <see langword="null"/>: <c>Rule</c> does not support a background colour.</returns>
        Color? ISpectreHeaderStyle.Background => null;

        /// <inheritdoc/>
        /// <returns>Always <see cref="Decoration.None"/>: <c>Rule</c> does not support text decoration.</returns>
        Decoration ISpectreHeaderStyle.Decoration => Decoration.None;

        public override bool Equals(object? obj)
            => obj is SpectreRuleHeaderStyle other
                && Justification == other.Justification
                && Foreground == other.Foreground
                && Equals(Border, other.Border);

        public override int GetHashCode() => HashCode.Combine(Justification, Foreground, Border);
    }
}
