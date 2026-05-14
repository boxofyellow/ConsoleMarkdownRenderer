using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// A heading style that renders the heading text as large ASCII art via Spectre.Console's
    /// <c>FigletText</c> widget. Implementations of <c>FigletText</c> are intentionally limited
    /// to the small set of options exposed here — <see cref="Justification"/>,
    /// <see cref="Foreground"/> and <see cref="FontPath"/> — because <c>FigletText</c> does
    /// not support the decoration or background facilities of <see cref="TextStyle"/>. It is
    /// therefore modeled as a peer of <see cref="TextStyle"/> (both implement
    /// <see cref="IHeaderStyle"/>) rather than as a subclass.
    /// </summary>
    /// <remarks>
    /// Assign an instance to a level in <see cref="DisplayOptions.Headers"/> (or to
    /// <see cref="DisplayOptions.Header"/>) to opt that level in to FIGlet rendering. By
    /// default <see cref="DisplayOptions.Headers"/> configures <c>#</c>-level headings (H1)
    /// to use a <see cref="FigletTextStyle"/>; deeper levels continue to use the styled,
    /// <c>#</c>-wrapped markup unless explicitly overridden.
    /// <para>
    /// Instances are created exclusively via the static factory methods. Use
    /// <see cref="Create"/> when Spectre.Console's built-in default FIGlet font is sufficient,
    /// and <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>
    /// to load a custom FIGlet font file (<c>.flf</c>); the asynchronous factory ensures file
    /// I/O is never performed synchronously on the caller's thread.
    /// </para>
    /// </remarks>
    public sealed class FigletTextStyle : IHeaderStyle
    {
        private FigletTextStyle(
            TextJustification? justification,
            TextColor? foreground,
            string? fontPath,
            FigletFont? font)
        {
            Justification = justification;
            Foreground = foreground;
            FontPath = fontPath;
            Font = font;
        }

        /// <summary>
        /// Creates a new <see cref="FigletTextStyle"/> that uses Spectre.Console's built-in
        /// default FIGlet font. To load a custom <c>.flf</c> font, use
        /// <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>.
        /// </summary>
        public static FigletTextStyle Create(
            TextJustification? justification = null,
            TextColor? foreground = null)
            => new(justification, foreground, fontPath: null, font: null);

        /// <summary>
        /// Asynchronously reads the FIGlet font file at <paramref name="fontPath"/> and
        /// returns a <see cref="FigletTextStyle"/> with the parsed font cached on it. Any I/O
        /// or parse failure surfaces at this factory call (not at render time), and the
        /// parsed font is reused for every subsequent render of the returned style.
        /// </summary>
        public static async Task<FigletTextStyle> CreateAsync(
            string fontPath,
            TextJustification? justification = null,
            TextColor? foreground = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(fontPath);
            var source = await File.ReadAllTextAsync(fontPath, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var font = FigletFont.Parse(source);
            return new FigletTextStyle(justification, foreground, fontPath, font);
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

        /// <summary>
        /// Optional path to a custom FIGlet font file (<c>.flf</c>) that was used to load
        /// <see cref="Font"/>. Non-<see langword="null"/> only for instances created via
        /// <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>.
        /// </summary>
        public string? FontPath { get; }

        /// <summary>
        /// The cached FIGlet font, eagerly loaded from <see cref="FontPath"/>. <see langword="null"/>
        /// when no <see cref="FontPath"/> was supplied, in which case Spectre.Console's
        /// built-in default font is used at render time.
        /// </summary>
        internal FigletFont? Font { get; }

        /// <summary>
        /// Always <see langword="null"/>: <c>FigletText</c> does not support a background color.
        /// </summary>
        TextColor? IHeaderStyle.Background => null;

        /// <summary>
        /// Always <see cref="TextDecoration.None"/>: <c>FigletText</c> does not support text
        /// decoration (bold, italic, etc.).
        /// </summary>
        TextDecoration IHeaderStyle.Decoration => TextDecoration.None;

        public override bool Equals(object? obj)
            => obj is FigletTextStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground)
                && string.Equals(FontPath, other.FontPath, StringComparison.Ordinal);

        public override int GetHashCode() => HashCode.Combine(Justification, Foreground, FontPath);
    }
}
