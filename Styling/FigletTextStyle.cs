using System.Text.Json.Serialization;
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
    /// Instances can be obtained from <see cref="Create"/> (sync; the font file, if any, is
    /// loaded lazily on first use) or from
    /// <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>
    /// (async; the font file is loaded eagerly so I/O / parse failures surface at construction
    /// time rather than at render time). Either factory may also be invoked indirectly by
    /// <see cref="System.Text.Json.JsonSerializer"/> via the public <see cref="FigletTextStyle"/>
    /// constructor, which is the form used when a <see cref="DisplayOptions"/> graph is
    /// deserialized from JSON.
    /// </para>
    /// <para>
    /// When an instance with a non-<see langword="null"/> <see cref="FontPath"/> is rendered,
    /// the font must be loaded first. <see cref="MarkdownDisplayer"/> takes care of this by
    /// awaiting <see cref="EnsureFontLoadedAsync"/> on every <see cref="FigletTextStyle"/> in
    /// the supplied <see cref="DisplayOptions"/> before invoking the renderer. Callers that
    /// drive a renderer directly (without going through
    /// <see cref="MarkdownDisplayer.DisplayMarkdownAsync(System.Uri, DisplayOptions?, bool)"/>)
    /// are responsible for calling <see cref="EnsureFontLoadedAsync"/> themselves.
    /// </para>
    /// </remarks>
    public sealed class FigletTextStyle : IHeaderStyle
    {
        /// <summary>
        /// Constructs a new <see cref="FigletTextStyle"/>. The font file referenced by
        /// <paramref name="fontPath"/>, if any, is <em>not</em> read here; it is loaded lazily
        /// the first time the font is needed (either via <see cref="EnsureFontLoadedAsync"/>
        /// or implicitly during rendering). Use
        /// <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>
        /// when you want I/O / parse failures to surface at construction time instead.
        /// </summary>
        /// <remarks>
        /// This constructor is public primarily so that <see cref="System.Text.Json.JsonSerializer"/>
        /// can deserialize a <see cref="FigletTextStyle"/> using its parameter-name / property-name
        /// matching rules. Application code should normally prefer the <see cref="Create"/> or
        /// <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>
        /// factory methods.
        /// </remarks>
        [JsonConstructor]
        public FigletTextStyle(
            TextJustification? justification = null,
            TextColor? foreground = null,
            string? fontPath = null)
        {
            Justification = justification;
            Foreground = foreground;
            FontPath = fontPath;
            m_fontLazy = new Lazy<Task<FigletFont?>>(
                () => LoadFontAsync(fontPath),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// Creates a new <see cref="FigletTextStyle"/>. When <paramref name="fontPath"/> is
        /// <see langword="null"/> (the default) Spectre.Console's built-in default FIGlet font
        /// is used at render time; otherwise the referenced <c>.flf</c> file is loaded lazily
        /// the first time the font is needed. Use
        /// <see cref="CreateAsync(string, TextJustification?, TextColor?, CancellationToken)"/>
        /// when you want I/O / parse failures to surface at construction time.
        /// </summary>
        public static FigletTextStyle Create(
            TextJustification? justification = null,
            TextColor? foreground = null,
            string? fontPath = null)
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
            cancellationToken.ThrowIfCancellationRequested();
            await style.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
            return style;
        }

        /// <summary>
        /// Materializes the cached FIGlet font (when <see cref="FontPath"/> is non-empty) so
        /// that subsequent reads of <see cref="Font"/> return the parsed font synchronously.
        /// Safe — and cheap — to call repeatedly; the underlying file is read at most once
        /// per <see cref="FigletTextStyle"/> instance.
        /// </summary>
        internal Task EnsureFontLoadedAsync(CancellationToken cancellationToken = default)
        {
            var task = m_fontLazy.Value;
            return cancellationToken.CanBeCanceled
                ? task.WaitAsync(cancellationToken)
                : task;
        }

        private static async Task<FigletFont?> LoadFontAsync(string? fontPath)
        {
            if (string.IsNullOrEmpty(fontPath))
            {
                return null;
            }
            var source = await File.ReadAllTextAsync(fontPath).ConfigureAwait(false);
            return FigletFont.Parse(source);
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
        /// Optional path to a custom FIGlet font file (<c>.flf</c>). When non-<see langword="null"/>
        /// the referenced file is loaded the first time <see cref="EnsureFontLoadedAsync"/> is
        /// awaited (or the first time <see cref="Font"/> is accessed after such an await);
        /// when <see langword="null"/> Spectre.Console's built-in default font is used at render time.
        /// </summary>
        public string? FontPath { get; }

        /// <summary>
        /// The cached FIGlet font. Returns <see langword="null"/> when no <see cref="FontPath"/>
        /// was supplied, when the lazy load has not yet completed, or when the load faulted.
        /// In the last two cases <see cref="EnsureFontLoadedAsync"/> should be awaited before
        /// reading this property.
        /// </summary>
        internal FigletFont? Font
            => m_fontLazy.IsValueCreated && m_fontLazy.Value.Status == TaskStatus.RanToCompletion
                ? m_fontLazy.Value.Result
                : null;

        /// <summary>
        /// Always <see langword="null"/>: <c>FigletText</c> does not support a background color.
        /// </summary>
        [JsonIgnore]
        TextColor? IHeaderStyle.Background => null;

        /// <summary>
        /// Always <see cref="TextDecoration.None"/>: <c>FigletText</c> does not support text
        /// decoration (bold, italic, etc.).
        /// </summary>
        [JsonIgnore]
        TextDecoration IHeaderStyle.Decoration => TextDecoration.None;

        public override bool Equals(object? obj)
            => obj is FigletTextStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground)
                && string.Equals(FontPath, other.FontPath, StringComparison.Ordinal);

        public override int GetHashCode() => HashCode.Combine(Justification, Foreground, FontPath);

        private readonly Lazy<Task<FigletFont?>> m_fontLazy;
    }
}
