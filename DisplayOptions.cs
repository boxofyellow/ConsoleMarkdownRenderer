using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer
{
    /// <summary>
    /// Class for controlling the styling and other display options for the Markdown elements 
    /// </summary>
    public sealed class DisplayOptions
    {
        /// <summary>
        /// Style applied to the expansion title that follows an
        /// <see cref="Markdig.Extensions.Abbreviations.AbbreviationInline"/> (e.g. the
        /// <c>HyperText Markup Language</c> portion of <c>HTML (HyperText Markup Language)</c>).
        /// Only used when <see cref="ShowAbbreviationTitle"/> is <see langword="true"/>.
        /// </summary>
        public TextStyle AbbreviationTitle { get; set; } = new(decoration: TextDecoration.Dim);

        public TextStyle Bold { get; set; } = new(decoration: TextDecoration.Bold);
        public TextStyle CodeBlock { get; set; } = new(foreground: TextColor.Yellow, background: TextColor.Blue);
        public TextStyle CodeInLine { get; set; } = new(foreground: TextColor.Yellow, background: TextColor.Blue);

        /// <summary>
        /// Style applied to the body of a <see cref="Markdig.Extensions.CustomContainers.CustomContainer"/>
        /// (e.g. an admonition / callout block such as <c>:::note</c>, <c>:::warning</c>, or <c>:::tip</c>).
        /// </summary>
        public TextStyle CustomContainer { get; set; } = new(decoration: TextDecoration.None);

        /// <summary>
        /// Style applied to the <see cref="Markdig.Extensions.CustomContainers.CustomContainer.Info"/> label
        /// (e.g. <c>note</c> / <c>warning</c> / <c>tip</c>) emitted at the top of a custom container block.
        /// </summary>
        public TextStyle CustomContainerInfo { get; set; } = new(decoration: TextDecoration.Bold);

        /// <summary>
        /// Style applied to the contents of an inline
        /// <see cref="Markdig.Extensions.CustomContainers.CustomContainerInline"/> (e.g. <c>::tag content::</c>).
        /// </summary>
        public TextStyle CustomContainerInline { get; set; } = new(decoration: TextDecoration.Bold);

        /// <summary>
        /// Style applied to the contents of a <see cref="Markdig.Extensions.DefinitionLists.DefinitionItem"/>
        /// (all children of each item in a definition list, including its terms and definitions).
        /// </summary>
        public TextStyle DefinitionItem { get; set; } = new(decoration: TextDecoration.None);

        /// <summary>
        /// Style applied to the contents of a <see cref="Markdig.Extensions.DefinitionLists.DefinitionList"/>.
        /// </summary>
        public TextStyle DefinitionList { get; set; } = new(decoration: TextDecoration.None);

        /// <summary>
        /// Style applied to the term label of a <see cref="Markdig.Extensions.DefinitionLists.DefinitionTerm"/>.
        /// </summary>
        public TextStyle DefinitionTerm { get; set; } = new(decoration: TextDecoration.Bold);

        /// <summary>
        /// When <see langword="true"/> (the default), the expansion title of an
        /// <see cref="Markdig.Extensions.Abbreviations.AbbreviationInline"/> is appended in
        /// parentheses after the abbreviation text (e.g. <c>HTML (HyperText Markup Language)</c>),
        /// styled with <see cref="AbbreviationTitle"/>.
        /// When <see langword="false"/>, only the abbreviation text is rendered, keeping
        /// the output compact.
        /// </summary>
        public bool ShowAbbreviationTitle { get; set; } = true;

        /// <summary>
        /// When set to true, the Info field from <see cref="Markdig.Syntax.FencedCodeBlock"/> (e.g., the language identifier) will be displayed.
        /// </summary>
        public bool ShowFencedCodeBlockInfo { get; set; } = false;

        /// <summary>
        /// Style for the Info field of a <see cref="Markdig.Syntax.FencedCodeBlock"/> when <see cref="ShowFencedCodeBlockInfo"/> is true.
        /// </summary>
        public TextStyle FencedCodeBlockInfo { get; set; } = new(foreground: TextColor.Green, background: TextColor.Blue);

        /// <summary>
        /// Style applied to the inline content of a <see cref="Markdig.Extensions.Figures.FigureCaption"/>
        /// (the optional caption line of a Markdig <see cref="Markdig.Extensions.Figures.Figure"/> block).
        /// Italic by default to visually distinguish it from the figure's body content.
        /// </summary>
        public TextStyle FigureCaption { get; set; } = new(decoration: TextDecoration.Italic);

        // List of Styles to use for headers the first will be used for #, the second for ## and so on
        // If the document referenced more than the length of the list, the Style in header will be used.
        // By default the first entry is a FigletTextStyle, so top-level (#) headings render as
        // FIGlet ASCII art. Replace or remove that entry (or assign a plain TextStyle) to opt
        // H1 into the styled-markup path used by deeper levels.
        public List<IHeaderStyle> Headers { get; set; } = new()
        {
            FigletTextStyle.Create(justification: TextJustification.Left),
        };
        public IHeaderStyle Header { get; set; } = new TextStyle(decoration: TextDecoration.Bold | TextDecoration.Underline | TextDecoration.Invert);

        public TextStyle HtmlBlock { get; set; } = new(foreground: TextColor.Black, background: TextColor.Green);
        public TextStyle HtmlInline { get; set; } = new(foreground: TextColor.Black, background: TextColor.Green);
        /// <summary>
        /// Style applied to the body of a <see cref="Markdig.Extensions.Footnotes.Footnote"/>, including the
        /// label prefix (e.g. <c>[^1]:</c>) that precedes the footnote content.
        /// </summary>
        public TextStyle Footnote { get; set; } = new(decoration: TextDecoration.Bold);

        /// <summary>
        /// Style applied to the contents of a <see cref="Markdig.Extensions.Footnotes.FootnoteGroup"/>
        /// (the collection of footnotes typically displayed at the end of the document).
        /// </summary>
        public TextStyle FootnoteGroup { get; set; } = new(decoration: TextDecoration.Italic);

        /// <summary>
        /// Style applied to a <see cref="Markdig.Extensions.Footnotes.FootnoteLink"/> marker
        /// (both the inline reference and its back-link in the rendered footnote).
        /// </summary>
        public TextStyle FootnoteLink { get; set; } = new(foreground: TextColor.Blue, decoration: TextDecoration.Underline);

        /// <summary>
        /// When <see langword="true"/> (the default), emoji shortcodes and text smileys parsed by Markdig's
        /// <see cref="Markdig.MarkdownExtensions.UseEmojiAndSmiley(Markdig.MarkdownPipelineBuilder, bool)"/> extension
        /// (e.g. <c>:smile:</c> or <c>:-)</c>) are rendered as their Unicode emoji equivalents.
        /// When <see langword="false"/>, the original shortcode/smiley text is emitted instead.
        /// Note that emoji shortcodes inside code spans and code blocks are never substituted regardless of this setting,
        /// because Markdig does not parse inline content within code.
        /// </summary>
        public bool Emojis { get; set; } = true;

        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Inserted"/>
        public TextStyle Inserted { get; set; } = new(decoration: TextDecoration.Underline);
        public TextStyle Italic { get; set; } = new(decoration: TextDecoration.Italic);
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Marked"/>
        public TextStyle Marked { get; set; } = new(foreground: TextColor.Black, background: TextColor.Yellow);

        /// <summary>
        /// Style applied to the verbatim source of a <see cref="Markdig.Extensions.Mathematics.MathBlock"/>
        /// (display math delimited by <c>$$ ... $$</c>). Terminals cannot typeset LaTeX, so the raw
        /// source is rendered with this style inside a fenced presentation similar to a code block.
        /// </summary>
        public TextStyle MathBlock { get; set; } = new(foreground: TextColor.Green, background: TextColor.Purple);

        /// <summary>
        /// Style applied to the optional label emitted at the top of a <see cref="Markdig.Extensions.Mathematics.MathBlock"/>
        /// when <see cref="MathBlockLabelText"/> is non-empty.
        /// </summary>
        public TextStyle MathBlockLabel { get; set; } = new(foreground: TextColor.Yellow, background: TextColor.Purple);

        /// <summary>
        /// Text used for the optional <see cref="Markdig.Extensions.Mathematics.MathBlock"/> label, rendered at the top
        /// of each math block similar to how <see cref="ShowFencedCodeBlockInfo"/> emits the language identifier for
        /// a fenced code block. When <see langword="null"/> or empty, no label is emitted (the default).
        /// </summary>
        public string MathBlockLabelText { get; set; } = string.Empty;

        /// <summary>
        /// Style applied to the verbatim source of a <see cref="Markdig.Extensions.Mathematics.MathInline"/>
        /// (inline math delimited by <c>$ ... $</c>). Rendered with a code-like style so callers can
        /// distinguish it visually from prose; defaults differ from <see cref="CodeInLine"/> so math is
        /// also distinguishable from code.
        /// </summary>
        public TextStyle MathInline { get; set; } = new(foreground: TextColor.Green, background: TextColor.Purple);

        public TextStyle QuotedBlock { get; set; } = new(decoration: TextDecoration.Italic);

        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough"/>
        public TextStyle Strikethrough { get; set; } = new(decoration: TextDecoration.Strikethrough);

        /// <summary>
        /// Style applied to the rule line emitted for a <see cref="Markdig.Syntax.ThematicBreakBlock"/>
        /// (a Markdown thematic break / horizontal rule). The style is passed through to the
        /// underlying <see cref="Spectre.Console.Rule"/> widget via its <see cref="Spectre.Console.Rule.Style"/>
        /// property so callers can colour or decorate chapter / section dividers.
        /// </summary>
        public TextStyle ThematicBreak { get; set; } = new();

        // Hey, I'm sure there might be something better for subscript... but sometimes you have to make do with what you have 
        // And the blink does not seem to render well
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Subscript"/>
        public TextStyle Subscript { get; set; } = new(decoration: TextDecoration.SlowBlink);

        // This another one.  Don't have an exact match for superscript
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Superscript"/>
        public TextStyle Superscript { get; set; } = new(decoration: TextDecoration.RapidBlink);


        // Yes, these are more than a style, but it should help identify where things need updating
        public TextStyle UnknownDelimiterChar { get; set; } = new(decoration: TextDecoration.Dim);
        public TextStyle UnknownDelimiterContent { get; set; } = new(decoration: TextDecoration.Invert);

        // When set to true wrap Headers with '#'s 
        public bool WrapHeader { get; set; } = true;

        /// <summary>
        /// When <see langword="true"/> (the default), links rendered for
        /// <see cref="Markdig.Syntax.Inlines.LinkInline"/> and <see cref="Markdig.Syntax.Inlines.AutolinkInline"/>
        /// are wrapped with Spectre.Console's <c>[link=...]...[/]</c> markup so that
        /// supported terminals (iTerm2, Windows Terminal, GNOME Terminal, etc.) render them as
        /// clickable <a href="https://gist.github.com/egmontkob/eb114294efbcd5adb1944c9f3cb5feda">OSC 8 hyperlinks</a>.
        /// Set to <see langword="false"/> to disable for terminals that render the escape sequences as garbage.
        /// </summary>
        public bool UseTerminalHyperlinks { get; set; } = true;
 
        // When set to true the content structure is displayed and detail of unsupported markdown is displayed
        public bool IncludeDebug = false;

        public DisplayOptions Clone() => new()
        {
            AbbreviationTitle = this.AbbreviationTitle,
            Bold = this.Bold,
            CodeBlock = this.CodeBlock,
            CodeInLine = this.CodeInLine,
            CustomContainer = this.CustomContainer,
            CustomContainerInfo = this.CustomContainerInfo,
            CustomContainerInline = this.CustomContainerInline,
            DefinitionItem = this.DefinitionItem,
            DefinitionList = this.DefinitionList,
            DefinitionTerm = this.DefinitionTerm,
            Emojis = this.Emojis,
            FencedCodeBlockInfo = this.FencedCodeBlockInfo,
            FigureCaption = this.FigureCaption,
            Footnote = this.Footnote,
            FootnoteGroup = this.FootnoteGroup,
            FootnoteLink = this.FootnoteLink,
            Header = this.Header,
            Headers = new(this.Headers),
            HtmlBlock = this.HtmlBlock,
            HtmlInline = this.HtmlInline,
            IncludeDebug = this.IncludeDebug,
            Inserted = this.Inserted,
            Italic = this.Italic,
            Marked = this.Marked,
            MathBlock = this.MathBlock,
            MathBlockLabel = this.MathBlockLabel,
            MathBlockLabelText = this.MathBlockLabelText,
            MathInline = this.MathInline,
            QuotedBlock = this.QuotedBlock,
            ShowAbbreviationTitle = this.ShowAbbreviationTitle,
            ShowFencedCodeBlockInfo = this.ShowFencedCodeBlockInfo,
            Strikethrough = this.Strikethrough,
            Subscript = this.Subscript,
            Superscript = this.Superscript,
            ThematicBreak = this.ThematicBreak,
            UnknownDelimiterChar = this.UnknownDelimiterChar,
            UnknownDelimiterContent = this.UnknownDelimiterContent,
            UseTerminalHyperlinks = this.UseTerminalHyperlinks,
            WrapHeader = this.WrapHeader,
        };

        /// <summary>
        /// Computes which style to use for given Object Level
        /// </summary>
        /// <param name="level">The level of the Object for `#` it will 1, for `##` it will be 2, and so on</param>
        /// <returns>The style to use</returns>
        internal IHeaderStyle EffectiveHeader(int level) => 
            level <= Headers.Count 
                   ? Headers[level - 1]
                   : Header;

        /// <summary>
        /// Serializes this <see cref="DisplayOptions"/> to JSON using the converters honored
        /// by <see cref="DeserializeAsync(string, JsonSerializerOptions?, CancellationToken)"/>
        /// so that the result round-trips back to an equivalent <see cref="DisplayOptions"/>
        /// instance.
        /// </summary>
        /// <param name="options">
        /// Optional caller-supplied <see cref="JsonSerializerOptions"/>. Pass an instance to
        /// control output settings such as <see cref="JsonSerializerOptions.WriteIndented"/>;
        /// the library copies the provided options and adds the converters required to
        /// serialize <see cref="DisplayOptions"/>, so the caller's instance is not mutated.
        /// When <see langword="null"/> (the default) compact JSON is emitted with the
        /// library's default settings.
        /// </param>
        public string Serialize(JsonSerializerOptions? options = null)
            => JsonSerializer.Serialize(this, BuildEffectiveOptions(options));

        /// <summary>
        /// Deserializes a <see cref="DisplayOptions"/> from a JSON <paramref name="json"/>
        /// string and awaits <see cref="FigletTextStyle.EnsureFontLoadedAsync"/> on every
        /// <see cref="FigletTextStyle"/> in <see cref="Headers"/> and <see cref="Header"/>
        /// before returning, so the result is ready to hand to a renderer.
        /// </summary>
        /// <param name="json">The JSON text to deserialize.</param>
        /// <param name="options">
        /// Optional caller-supplied <see cref="JsonSerializerOptions"/>. The library copies
        /// the provided options and adds the converters required to deserialize a
        /// <see cref="DisplayOptions"/>, so the caller's instance is not mutated. When
        /// <see langword="null"/> (the default) the library's default settings are used.
        /// </param>
        /// <param name="cancellationToken">A token to observe while loading FIGlet fonts.</param>
        public static async Task<DisplayOptions> DeserializeAsync(
            string json,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(json);
            var result = JsonSerializer.Deserialize<DisplayOptions>(json, BuildEffectiveOptions(options))
                ?? throw new JsonException($"{nameof(DisplayOptions)} JSON deserialized to null.");
            await EnsureHeaderFontsLoadedAsync(result, cancellationToken).ConfigureAwait(false);
            return result;
        }

        /// <summary>
        /// Deserializes a <see cref="DisplayOptions"/> from a UTF-8 JSON
        /// <paramref name="utf8Json"/> stream and awaits
        /// <see cref="FigletTextStyle.EnsureFontLoadedAsync"/> on every
        /// <see cref="FigletTextStyle"/> in <see cref="Headers"/> and <see cref="Header"/>
        /// before returning, so the result is ready to hand to a renderer.
        /// </summary>
        /// <param name="utf8Json">The UTF-8 JSON stream to deserialize.</param>
        /// <param name="options">
        /// Optional caller-supplied <see cref="JsonSerializerOptions"/>. The library copies
        /// the provided options and adds the converters required to deserialize a
        /// <see cref="DisplayOptions"/>, so the caller's instance is not mutated. When
        /// <see langword="null"/> (the default) the library's default settings are used.
        /// </param>
        /// <param name="cancellationToken">A token to observe while reading the stream and loading FIGlet fonts.</param>
        public static async Task<DisplayOptions> DeserializeAsync(
            Stream utf8Json,
            JsonSerializerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(utf8Json);
            var result = await JsonSerializer.DeserializeAsync<DisplayOptions>(utf8Json, BuildEffectiveOptions(options), cancellationToken).ConfigureAwait(false)
                ?? throw new JsonException($"{nameof(DisplayOptions)} JSON deserialized to null.");
            await EnsureHeaderFontsLoadedAsync(result, cancellationToken).ConfigureAwait(false);
            return result;
        }

        private static async Task EnsureHeaderFontsLoadedAsync(DisplayOptions options, CancellationToken cancellationToken)
        {
            foreach (var headerStyle in options.Headers)
            {
                if (headerStyle is FigletTextStyle figlet)
                {
                    await figlet.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            if (options.Header is FigletTextStyle headerFiglet)
            {
                await headerFiglet.EnsureFontLoadedAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static JsonSerializerOptions BuildEffectiveOptions(JsonSerializerOptions? caller)
        {
            // Copy the caller's options (if any) so that we can safely append our converters
            // without mutating the instance they supplied. When no caller options are
            // supplied, fall back to the library defaults — including a
            // JsonStringEnumConverter so the standalone TextStyle / enum properties on
            // DisplayOptions (e.g. Bold.Decoration) round-trip via friendly enum names.
            // Callers that pass their own JsonSerializerOptions take responsibility for
            // their own enum-handling policy; the IHeaderStyle / TextColor converters
            // remain self-sufficient either way.
            var copy = caller is null
                ? new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true,
                    Converters = { new JsonStringEnumConverter() },
                }
                : new JsonSerializerOptions(caller);

            bool hasHeader = false;
            bool hasColor = false;
            foreach (var converter in copy.Converters)
            {
                hasHeader |= converter is HeaderStyleJsonConverter;
                hasColor |= converter is TextColorJsonConverter;
            }
            if (!hasHeader)
            {
                copy.Converters.Add(new HeaderStyleJsonConverter());
            }
            if (!hasColor)
            {
                copy.Converters.Add(new TextColorJsonConverter());
            }
            return copy;
        }
    }
}