using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer
{
    /// <summary>
    /// Class for controlling the styling and other display options for the Markdown elements 
    /// </summary>
    public sealed class DisplayOptions
    {
        private static readonly SpectreDisplayOptions s_spectreDefaults = new();

        /// <summary>
        /// Style applied to the expansion title that follows an
        /// <see cref="Markdig.Extensions.Abbreviations.AbbreviationInline"/> (e.g. the
        /// <c>HyperText Markup Language</c> portion of <c>HTML (HyperText Markup Language)</c>).
        /// </summary>
        public TextStyle AbbreviationTitle { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.AbbreviationTitle);

        public TextStyle Bold { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Bold);
        public TextStyle CodeBlock { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.CodeBlock);
        public TextStyle CodeInLine { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.CodeInLine);

        /// <summary>
        /// Style applied to the body of a <see cref="Markdig.Extensions.CustomContainers.CustomContainer"/>
        /// (e.g. an admonition / callout block such as <c>:::note</c>, <c>:::warning</c>, or <c>:::tip</c>).
        /// </summary>
        public TextStyle CustomContainer { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.CustomContainer);

        /// <summary>
        /// Style applied to the <see cref="Markdig.Extensions.CustomContainers.CustomContainer.Info"/> label
        /// (e.g. <c>note</c> / <c>warning</c> / <c>tip</c>) emitted at the top of a custom container block.
        /// </summary>
        public TextStyle CustomContainerInfo { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.CustomContainerInfo);

        /// <summary>
        /// Style applied to the contents of an inline
        /// <see cref="Markdig.Extensions.CustomContainers.CustomContainerInline"/> (e.g. <c>::tag content::</c>).
        /// </summary>
        public TextStyle CustomContainerInline { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.CustomContainerInline);

        /// <summary>
        /// Style applied to the contents of a <see cref="Markdig.Extensions.DefinitionLists.DefinitionItem"/>
        /// (all children of each item in a definition list, including its terms and definitions).
        /// </summary>
        public TextStyle DefinitionItem { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.DefinitionItem);

        /// <summary>
        /// Style applied to the contents of a <see cref="Markdig.Extensions.DefinitionLists.DefinitionList"/>.
        /// </summary>
        public TextStyle DefinitionList { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.DefinitionList);

        /// <summary>
        /// Style applied to the term label of a <see cref="Markdig.Extensions.DefinitionLists.DefinitionTerm"/>.
        /// </summary>
        public TextStyle DefinitionTerm { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.DefinitionTerm);

        /// <summary>
        /// When set to true, the Info field from <see cref="Markdig.Syntax.FencedCodeBlock"/> (e.g., the language identifier) will be displayed.
        /// </summary>
        public bool ShowFencedCodeBlockInfo { get; set; } = s_spectreDefaults.ShowFencedCodeBlockInfo;

        /// <summary>
        /// Style for the Info field of a <see cref="Markdig.Syntax.FencedCodeBlock"/> when <see cref="ShowFencedCodeBlockInfo"/> is true.
        /// </summary>
        public TextStyle FencedCodeBlockInfo { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.FencedCodeBlockInfo);

        /// <summary>
        /// Style applied to the inline content of a <see cref="Markdig.Extensions.Figures.FigureCaption"/>
        /// (the optional caption line of a Markdig <see cref="Markdig.Extensions.Figures.Figure"/> block).
        /// Italic by default to visually distinguish it from the figure's body content.
        /// </summary>
        public TextStyle FigureCaption { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.FigureCaption);

        // List of Styles to use for headers the first will be used for #, the second for ## and so on
        // If the document referenced more than the length of the list, the Style in header will be used.
        // By default the first entry is a FigletTextStyle, so top-level (#) headings render as
        // FIGlet ASCII art. Replace or remove that entry (or assign a plain TextStyle) to opt
        // H1 into the styled-markup path used by deeper levels.
        public List<IHeaderStyle> Headers { get; set; } = s_spectreDefaults.Headers
            .Select(FromSpectreHeaderStyle).ToList();
        public IHeaderStyle Header { get; set; } = FromSpectreHeaderStyle(s_spectreDefaults.Header);

        public TextStyle HtmlBlock { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.HtmlBlock);
        public TextStyle HtmlInline { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.HtmlInline);

        /// <summary>
        /// Style applied to the contents of a <see cref="Markdig.Extensions.Footers.FooterBlock"/>
        /// (a document-level footer section delimited by <c>+</c> markers, typically used for
        /// attribution, citations, or metadata rendered at the end of a document).
        /// </summary>
        public TextStyle Footer { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Footer);

        /// <summary>
        /// Style applied to the body of a <see cref="Markdig.Extensions.Footnotes.Footnote"/>, including the
        /// label prefix (e.g. <c>[^1]:</c>) that precedes the footnote content.
        /// </summary>
        public TextStyle Footnote { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Footnote);

        /// <summary>
        /// Style applied to the contents of a <see cref="Markdig.Extensions.Footnotes.FootnoteGroup"/>
        /// (the collection of footnotes typically displayed at the end of the document).
        /// </summary>
        public TextStyle FootnoteGroup { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.FootnoteGroup);

        /// <summary>
        /// Style applied to a <see cref="Markdig.Extensions.Footnotes.FootnoteLink"/> marker
        /// (both the inline reference and its back-link in the rendered footnote).
        /// </summary>
        public TextStyle FootnoteLink { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.FootnoteLink);

        /// <summary>
        /// When <see langword="true"/> (the default), emoji shortcodes and text smileys parsed by Markdig's
        /// <see cref="Markdig.MarkdownExtensions.UseEmojiAndSmiley(Markdig.MarkdownPipelineBuilder, bool)"/> extension
        /// (e.g. <c>:smile:</c> or <c>:-)</c>) are rendered as their Unicode emoji equivalents.
        /// When <see langword="false"/>, the original shortcode/smiley text is emitted instead.
        /// Note that emoji shortcodes inside code spans and code blocks are never substituted regardless of this setting,
        /// because Markdig does not parse inline content within code.
        /// </summary>
        public bool Emojis { get; set; } = s_spectreDefaults.Emojis;

        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Inserted"/>
        public TextStyle Inserted { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Inserted);
        public TextStyle Italic { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Italic);
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Marked"/>
        public TextStyle Marked { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Marked);

        /// <summary>
        /// Style applied to the verbatim source of a <see cref="Markdig.Extensions.Mathematics.MathBlock"/>
        /// (display math delimited by <c>$$ ... $$</c>). Terminals cannot typeset LaTeX, so the raw
        /// source is rendered with this style inside a fenced presentation similar to a code block.
        /// </summary>
        public TextStyle MathBlock { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.MathBlock);

        /// <summary>
        /// Style applied to the optional label emitted at the top of a <see cref="Markdig.Extensions.Mathematics.MathBlock"/>
        /// when <see cref="MathBlockLabelText"/> is non-empty.
        /// </summary>
        public TextStyle MathBlockLabel { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.MathBlockLabel);

        /// <summary>
        /// Text used for the optional <see cref="Markdig.Extensions.Mathematics.MathBlock"/> label, rendered at the top
        /// of each math block similar to how <see cref="ShowFencedCodeBlockInfo"/> emits the language identifier for
        /// a fenced code block. When <see langword="null"/> or empty, no label is emitted (the default).
        /// </summary>
        public string MathBlockLabelText { get; set; } = s_spectreDefaults.MathBlockLabelText;

        /// <summary>
        /// Style applied to the verbatim source of a <see cref="Markdig.Extensions.Mathematics.MathInline"/>
        /// (inline math delimited by <c>$ ... $</c>). Rendered with a code-like style so callers can
        /// distinguish it visually from prose; defaults differ from <see cref="CodeInLine"/> so math is
        /// also distinguishable from code.
        /// </summary>
        public TextStyle MathInline { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.MathInline);

        public TextStyle QuotedBlock { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.QuotedBlock);

        /// <summary>
        /// When <see langword="true"/> (the default), Markdig's
        /// <see cref="Markdig.MarkdownExtensions.UseSmartyPants(Markdig.MarkdownPipelineBuilder)"/> extension is
        /// added to the pipeline so that ASCII punctuation in prose is rewritten with its typographic equivalent:
        /// straight quotes become curly quotes, <c>--</c> becomes an en-dash (<c>–</c>), <c>---</c> becomes an
        /// em-dash (<c>—</c>), and <c>...</c> becomes a horizontal ellipsis (<c>…</c>).
        /// When <see langword="false"/>, the extension is omitted and punctuation is rendered verbatim.
        /// Note that punctuation inside code spans and fenced code blocks is always rendered verbatim regardless
        /// of this setting, because the SmartyPants extension only transforms inline literal text.
        /// </summary>
        public bool SmartyPants { get; set; } = s_spectreDefaults.SmartyPants;

        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough"/>
        public TextStyle Strikethrough { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Strikethrough);

        /// <summary>
        /// Style applied to the rule line emitted for a <see cref="Markdig.Syntax.ThematicBreakBlock"/>
        /// (a Markdown thematic break / horizontal rule). The style is passed through to the
        /// underlying <see cref="Spectre.Console.Rule"/> widget via its <see cref="Spectre.Console.Rule.Style"/>
        /// property so callers can colour or decorate chapter / section dividers.
        /// </summary>
        public TextStyle ThematicBreak { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.ThematicBreak);

        /// <summary>
        /// Border style applied to <see cref="Spectre.Console.Table"/> widgets produced by
        /// <see cref="ObjectRenderers.ConsoleTableRenderer"/> for Markdig pipe tables.
        /// Maps to one of the static <see cref="Spectre.Console.TableBorder"/> instances
        /// (e.g. <c>Rounded</c>, <c>Heavy</c>, <c>Ascii</c>, <c>Markdown</c>). Defaults to
        /// <see cref="TextTableBorder.Square"/>, which matches Spectre.Console's built-in default.
        /// </summary>
        public TextTableBorder TableBorder { get; set; } = TextStyleExtensions.FromSpectreTableBorder(s_spectreDefaults.TableBorder);

        /// <summary>
        /// Style (foreground / background / decoration) applied to the border characters of
        /// tables produced by <see cref="ObjectRenderers.ConsoleTableRenderer"/>. The style
        /// is passed through to Spectre.Console's <see cref="Spectre.Console.Table.BorderStyle"/>
        /// property. Defaults to an unstyled <see cref="TextStyle"/> so borders inherit the
        /// terminal's default colours.
        /// </summary>
        public TextStyle TableBorderStyle { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.TableBorderStyle);

        // Hey, I'm sure there might be something better for subscript... but sometimes you have to make do with what you have 
        // And the blink does not seem to render well
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Subscript"/>
        public TextStyle Subscript { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Subscript);

        // This another one.  Don't have an exact match for superscript
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Superscript"/>
        public TextStyle Superscript { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.Superscript);


        // Yes, these are more than a style, but it should help identify where things need updating
        public TextStyle UnknownDelimiterChar { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.UnknownDelimiterChar);
        public TextStyle UnknownDelimiterContent { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.UnknownDelimiterContent);

        /// <summary>
        /// Style applied to the raw source of a <see cref="Markdig.Extensions.Yaml.YamlFrontMatterBlock"/>
        /// (the optional metadata block delimited by <c>---</c> at the top of a Markdown document, as parsed by
        /// Markdig's <see cref="Markdig.MarkdownExtensions.UseYamlFrontMatter(Markdig.MarkdownPipelineBuilder)"/>
        /// extension).
        /// </summary>
        public TextStyle YamlFrontMatter { get; set; } = TextStyleExtensions.FromSpectreStyle(s_spectreDefaults.YamlFrontMatter);

        // When set to true wrap Headers with '#'s 
        public bool WrapHeader { get; set; } = s_spectreDefaults.WrapHeader;

        /// <summary>
        /// When <see langword="true"/> (the default), links rendered for
        /// <see cref="Markdig.Syntax.Inlines.LinkInline"/> and <see cref="Markdig.Syntax.Inlines.AutolinkInline"/>
        /// are wrapped with Spectre.Console's <c>[link=...]...[/]</c> markup so that
        /// supported terminals (iTerm2, Windows Terminal, GNOME Terminal, etc.) render them as
        /// clickable <a href="https://gist.github.com/egmontkob/eb114294efbcd5adb1944c9f3cb5feda">OSC 8 hyperlinks</a>.
        /// Set to <see langword="false"/> to disable for terminals that render the escape sequences as garbage.
        /// </summary>
        public bool UseTerminalHyperlinks { get; set; } = s_spectreDefaults.UseTerminalHyperlinks;
 
        // When set to true the content structure is displayed and detail of unsupported markdown is displayed
        public bool IncludeDebug = s_spectreDefaults.IncludeDebug;

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
            Footer = this.Footer,
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
            ShowFencedCodeBlockInfo = this.ShowFencedCodeBlockInfo,
            SmartyPants = this.SmartyPants,
            Strikethrough = this.Strikethrough,
            Subscript = this.Subscript,
            Superscript = this.Superscript,
            TableBorder = this.TableBorder,
            TableBorderStyle = this.TableBorderStyle,
            ThematicBreak = this.ThematicBreak,
            UnknownDelimiterChar = this.UnknownDelimiterChar,
            UnknownDelimiterContent = this.UnknownDelimiterContent,
            UseTerminalHyperlinks = this.UseTerminalHyperlinks,
            WrapHeader = this.WrapHeader,
            YamlFrontMatter = this.YamlFrontMatter,
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
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
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
        
        public readonly JsonSerializerOptions PrettyPrintJson = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = { new JsonStringEnumConverter() },
        };

        /// <summary>
        /// Converts this <see cref="DisplayOptions"/> to a <see cref="SpectreDisplayOptions"/> by mapping
        /// all <see cref="TextStyle"/> / <see cref="TextTableBorder"/> / <see cref="IHeaderStyle"/> properties
        /// to their Spectre.Console equivalents via <see cref="TextStyleExtensions"/>.
        /// </summary>
        internal SpectreDisplayOptions ToSpectreDisplayOptions() => new()
        {
            AbbreviationTitle       = AbbreviationTitle.ToSpectreStyle(),
            Bold                    = Bold.ToSpectreStyle(),
            CodeBlock               = CodeBlock.ToSpectreStyle(),
            CodeInLine              = CodeInLine.ToSpectreStyle(),
            CustomContainer         = CustomContainer.ToSpectreStyle(),
            CustomContainerInfo     = CustomContainerInfo.ToSpectreStyle(),
            CustomContainerInline   = CustomContainerInline.ToSpectreStyle(),
            DefinitionItem          = DefinitionItem.ToSpectreStyle(),
            DefinitionList          = DefinitionList.ToSpectreStyle(),
            DefinitionTerm          = DefinitionTerm.ToSpectreStyle(),
            ShowFencedCodeBlockInfo = ShowFencedCodeBlockInfo,
            FencedCodeBlockInfo     = FencedCodeBlockInfo.ToSpectreStyle(),
            FigureCaption           = FigureCaption.ToSpectreStyle(),
            Headers                 = Headers.Select(ToSpectreHeaderStyle).ToList(),
            Header                  = ToSpectreHeaderStyle(Header),
            HtmlBlock               = HtmlBlock.ToSpectreStyle(),
            HtmlInline              = HtmlInline.ToSpectreStyle(),
            Footer                  = Footer.ToSpectreStyle(),
            Footnote                = Footnote.ToSpectreStyle(),
            FootnoteGroup           = FootnoteGroup.ToSpectreStyle(),
            FootnoteLink            = FootnoteLink.ToSpectreStyle(),
            Emojis                  = Emojis,
            Inserted                = Inserted.ToSpectreStyle(),
            Italic                  = Italic.ToSpectreStyle(),
            Marked                  = Marked.ToSpectreStyle(),
            MathBlock               = MathBlock.ToSpectreStyle(),
            MathBlockLabel          = MathBlockLabel.ToSpectreStyle(),
            MathBlockLabelText      = MathBlockLabelText,
            MathInline              = MathInline.ToSpectreStyle(),
            QuotedBlock             = QuotedBlock.ToSpectreStyle(),
            SmartyPants             = SmartyPants,
            Strikethrough           = Strikethrough.ToSpectreStyle(),
            ThematicBreak           = ThematicBreak.ToSpectreStyle(),
            TableBorder             = TableBorder.ToSpectreTableBorder(),
            TableBorderStyle        = TableBorderStyle.ToSpectreStyle(),
            Subscript               = Subscript.ToSpectreStyle(),
            Superscript             = Superscript.ToSpectreStyle(),
            UnknownDelimiterChar    = UnknownDelimiterChar.ToSpectreStyle(),
            UnknownDelimiterContent = UnknownDelimiterContent.ToSpectreStyle(),
            YamlFrontMatter         = YamlFrontMatter.ToSpectreStyle(),
            WrapHeader              = WrapHeader,
            UseTerminalHyperlinks   = UseTerminalHyperlinks,
            IncludeDebug            = IncludeDebug,
        };

        private static ISpectreHeaderStyle ToSpectreHeaderStyle(IHeaderStyle headerStyle) => headerStyle switch
        {
            FigletTextStyle figlet => new SpectreFigletHeaderStyle(
                font:          figlet.Font,
                justification: figlet.Justification?.ToSpectreJustify(),
                foreground:    figlet.Foreground?.ToSpectreColor()),
            RuleHeaderStyle rule => new SpectreRuleHeaderStyle(
                justification: rule.Justification?.ToSpectreJustify(),
                foreground:    rule.Foreground?.ToSpectreColor(),
                border:        rule.Border?.ToSpectreBoxBorder()),
            TextStyle style => new SpectreStyleHeaderStyle(style.ToSpectreStyle()),
            _ => throw new ArgumentException($"Unsupported {nameof(IHeaderStyle)}: {headerStyle.GetType().FullName}", nameof(headerStyle)),
        };

        private static IHeaderStyle FromSpectreHeaderStyle(ISpectreHeaderStyle headerStyle) => headerStyle switch
        {
            SpectreFigletHeaderStyle figlet => FigletTextStyle.Create(
                justification: TextStyleExtensions.FromSpectreJustify(figlet.Justification),
                foreground:    figlet.Foreground is { } fg ? TextStyleExtensions.FromSpectreColor(fg) : null),
            SpectreRuleHeaderStyle rule => new RuleHeaderStyle(
                justification: TextStyleExtensions.FromSpectreJustify(rule.Justification),
                foreground:    rule.Foreground is { } fg ? TextStyleExtensions.FromSpectreColor(fg) : null,
                border:        TextStyleExtensions.FromSpectreBoxBorder(rule.Border)),
            SpectreStyleHeaderStyle styleHeader => TextStyleExtensions.FromSpectreStyle(styleHeader.Style),
            _ => throw new ArgumentException($"Unsupported {nameof(ISpectreHeaderStyle)}: {headerStyle.GetType().FullName}", nameof(headerStyle)),
        };
    }
}