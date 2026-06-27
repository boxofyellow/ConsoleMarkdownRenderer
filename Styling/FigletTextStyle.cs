using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
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
    [SourceFile]
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
            m_font = font;
        }

        public static FigletTextStyle Create(
            TextJustification? justification = null,
            TextColor? foreground = null)
            => new(justification, foreground, fontPath: null, font: null);

        internal static FigletTextStyle Create(
            TextJustification? justification,
            TextColor? foreground,
            string? fontPath)
            // This null here for font, means the caller needs to make sure we invoke EnsureFontLoadedAsync before trying to access the Font property. This is necessary because loading the font is async, and we don't want to block on it in the constructor.
            => new(justification, foreground, fontPath, font: null);

        internal static FigletTextStyle Create(
            TextJustification? justification,
            TextColor? foreground,
            string? fontPath,
            FigletFont? font)
        {
            if (string.IsNullOrEmpty(fontPath) != (font is null))
            {
                throw new ArgumentException($"If either {nameof(fontPath)} or {nameof(font)} is provided, the other must be provided as well.");
            }
            return new(justification, foreground, fontPath, font);
        }

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
            var style = new FigletTextStyle(justification, foreground, fontPath, font: null);
            await style.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
            return style;
        }

        internal static FigletTextStyle Create(List<JsonProperty> properties, JsonSerializerOptions options)
        {
            TextJustification? justification = null;
            TextColor? foreground = null;
            string? fontPath = null;

            var justificationPropertyName = JsonWriteHelpers.ConvertName(nameof(Justification), options).ToLowerInvariant();
            var foregroundPropertyName = JsonWriteHelpers.ConvertName(nameof(Foreground), options).ToLowerInvariant();
            var fontPathPropertyName = JsonWriteHelpers.ConvertName(nameof(FontPath), options).ToLowerInvariant();

            foreach (var prop in properties)
            {
                var propNameLower = prop.Name.ToLowerInvariant();
                if (propNameLower == justificationPropertyName)
                {
                    justification = prop.Value.ValueKind == JsonValueKind.Null
                        ? null
                        : prop.Value.Deserialize<TextJustification>(options);
                }
                else if (propNameLower == foregroundPropertyName)
                {
                    foreground = prop.Value.Deserialize<TextColor?>(options);
                }
                else if (propNameLower == fontPathPropertyName)
                {
                    fontPath = prop.Value.ValueKind == JsonValueKind.Null 
                        ? null 
                        : prop.Value.GetString();
                }
                else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
                {
                    throw new JsonException($"Unrecognized property on {nameof(FigletTextStyle)}: '{prop.Name}'.");
                }
            }

            // This null here for font, means the caller needs to make sure we invoke EnsureFontLoadedAsync before trying to access the Font property. This is necessary because loading the font is async, and we don't want to block on it in the constructor.
            return new(justification, foreground, fontPath, font: null);
        }

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

        internal void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Justification), Justification);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Foreground), Foreground);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(FontPath), FontPath);
        }

        public TextJustification? Justification { get; }

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
        /// <c>FigletText</c> does not support a background color.
        /// </summary>
        TextColor? IHeaderStyle.Background => null;

        /// <summary>
        /// <c>FigletText</c> does not support text decoration (bold, italic, etc.).
        /// </summary>
        TextDecoration IHeaderStyle.Decoration => TextDecoration.None;

        public override bool Equals(object? obj)
            => obj is FigletTextStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground)
                && string.Equals(FontPath ?? string.Empty, other.FontPath ?? string.Empty, PathComparison.Comparison);

        public override int GetHashCode() => HashCode.Combine(
            Justification,
            Foreground,
            (FontPath ?? string.Empty).ToLowerInvariant());

        private FigletFont? m_font;
    }
}
