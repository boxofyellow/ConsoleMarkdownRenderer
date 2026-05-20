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
    /// I/O is never performed synchronously on the caller's thread. A <see cref="FigletTextStyle"/>
    /// that comes from a deserialized <see cref="DisplayOptions"/> graph (see
    /// <see cref="DisplayOptions.DeserializeAsync(string, CancellationToken)"/>) has already
    /// been awaited on its font load by the time it is returned to the caller.
    /// </para>
    /// </remarks>
    public sealed class FigletTextStyle : IHeaderStyle
    {
        private FigletTextStyle(
            TextJustification? justification,
            TextColor? foreground,
            string? fontPath)
        {
            Justification = justification;
            Foreground = foreground;
            FontPath = fontPath;
        }

        /// <summary>
        /// Creates a new <see cref="FigletTextStyle"/> that uses Spectre.Console's built-in
        /// default FIGlet font. To load a custom <c>.flf</c> font, use
        /// <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>.
        /// </summary>
        public static FigletTextStyle Create(
            TextJustification? justification = null,
            TextColor? foreground = null)
            => new(justification, foreground, fontPath: null);

        /// <summary>
        /// Constructs a <see cref="FigletTextStyle"/> instance without loading the font file
        /// referenced by <paramref name="fontPath"/>. The caller is responsible for awaiting
        /// <see cref="EnsureFontLoadedAsync"/> before the instance is rendered; otherwise
        /// reading <see cref="Font"/> will throw <see cref="InvalidOperationException"/>.
        /// Used by the internal JSON deserialization path, where the surrounding
        /// <see cref="DisplayOptions.DeserializeAsync(string, CancellationToken)"/> finalizes
        /// the load before returning.
        /// </summary>
        internal static FigletTextStyle Create(
            TextJustification? justification,
            TextColor? foreground,
            string? fontPath)
            => new(justification, foreground, fontPath);

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
            var style = new FigletTextStyle(justification, foreground, fontPath);
            await style.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
            return style;
        }

        /// <summary>
        /// If <see cref="FontPath"/> is non-empty, asynchronously reads and parses that file
        /// into <see cref="Font"/>. Safe to call repeatedly — subsequent calls reuse the
        /// already-parsed font. When <see cref="FontPath"/> is <see langword="null"/> or
        /// empty this is a no-op.
        /// </summary>
        internal async Task EnsureFontLoadedAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(FontPath) || m_font is not null)
            {
                return;
            }
            var source = await File.ReadAllTextAsync(FontPath, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            m_font = FigletFont.Parse(source);
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
        /// Optional path to a custom FIGlet font file (<c>.flf</c>). Non-<see langword="null"/>
        /// only for instances created via
        /// <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>
        /// or rehydrated from JSON by
        /// <see cref="DisplayOptions.DeserializeAsync(string, CancellationToken)"/>.
        /// </summary>
        public string? FontPath { get; }

        /// <summary>
        /// The cached FIGlet font. Returns <see langword="null"/> when no
        /// <see cref="FontPath"/> was supplied (Spectre.Console's built-in default font is
        /// used at render time). When a <see cref="FontPath"/> was supplied this property
        /// returns the parsed font, or throws <see cref="InvalidOperationException"/> if
        /// <see cref="EnsureFontLoadedAsync"/> has not yet completed.
        /// </summary>
        internal FigletFont? Font => string.IsNullOrEmpty(FontPath)
                                   ? null
                                   : (m_font ?? throw new InvalidOperationException("Font has not been loaded"));

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

        private FigletFont? m_font;
    }
}
