using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling
{
    [SpectreSourceFile]
    public sealed class SpectreFigletTextStyle : ISpectreHeaderStyle
    {
        private SpectreFigletTextStyle(
            Justify? justification,
            Color? foreground,
            string? fontPath,
            FigletFont? font)
        {
            Justification = justification;
            Foreground = foreground;
            FontPath = fontPath;
            m_font = font;
        }

        public static SpectreFigletTextStyle Create(
            Justify? justification = null,
            Color? foreground = null)
            => new(justification, foreground, fontPath: null, font: null);

        internal static SpectreFigletTextStyle Create(
            Justify? justification,
            Color? foreground,
            string? fontPath)
            // This null here for font, means the caller needs to make sure we invoke EnsureFontLoadedAsync before trying to access the Font property. This is necessary because loading the font is async, and we don't want to block on it in the constructor.
            => new(justification, foreground, fontPath, font: null);

        internal static SpectreFigletTextStyle Create(List<JsonProperty> properties, JsonSerializerOptions options)
        {
            Justify? justification = null;
            Color? foreground = null;
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
                        : prop.Value.Deserialize<Justify>(options);
                }
                else if (propNameLower == foregroundPropertyName)
                {
                    foreground = prop.Value.Deserialize<Color?>(options);
                }
                else if (propNameLower == fontPathPropertyName)
                {
                    fontPath = prop.Value.ValueKind == JsonValueKind.Null 
                        ? null 
                        : prop.Value.GetString();
                }
                else if (options.UnmappedMemberHandling == JsonUnmappedMemberHandling.Disallow)
                {
                    throw new JsonException($"Unrecognized property on {nameof(SpectreFigletTextStyle)}: '{prop.Name}'.");
                }
            }

            // This null here for font, means the caller needs to make sure we invoke EnsureFontLoadedAsync before trying to access the Font property. This is necessary because loading the font is async, and we don't want to block on it in the constructor.
            return new(justification, foreground, fontPath, font: null);
        }

        public static SpectreFigletTextStyle Create(
            Justify? justification,
            Color? foreground,
            string? fontPath,
            FigletFont? font)
        {
            if (string.IsNullOrEmpty(fontPath) != (font is null))
            {
                throw new ArgumentException($"If either {nameof(fontPath)} or {nameof(font)} is provided, the other must be provided as well.");
            }
            return new(justification, foreground, fontPath, font);
        }

        public static async Task<SpectreFigletTextStyle> CreateAsync(
            string fontPath,
            Justify? justification = null,
            Color? foreground = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(fontPath);
            var style = new SpectreFigletTextStyle(justification, foreground, fontPath, font: null);
            await style.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
            return style;
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

        public Justify? Justification { get; }

        public Color? Foreground { get; }

        public string? FontPath { get; }

        public FigletFont? Font => string.IsNullOrEmpty(FontPath)
                                ? null
                                : (m_font ?? throw new InvalidOperationException("Font has not been loaded"));

        Color? ISpectreHeaderStyle.Background => null;

        Decoration ISpectreHeaderStyle.Decoration => Decoration.None;

        public override bool Equals(object? obj)
            => obj is SpectreFigletTextStyle other
                && Justification == other.Justification
                && Equals(Foreground, other.Foreground)
                && string.Equals(FontPath ?? string.Empty, other.FontPath ?? string.Empty, PathComparison.Comparison);

        public override int GetHashCode() => HashCode.Combine(
            Justification,
            Foreground,
            (FontPath ?? string.Empty).ToLowerInvariant());

        internal void Write(Utf8JsonWriter writer, JsonSerializerOptions options)
        {
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Justification), Justification);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(Foreground), Foreground);
            JsonWriteHelpers.WriteProperty(writer, options, nameof(FontPath), FontPath);
        }

        private FigletFont? m_font;
    }
}
