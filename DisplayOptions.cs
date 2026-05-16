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

        /// <summary>
        /// Optional title text rendered alongside the rule line for a
        /// <see cref="Markdig.Syntax.ThematicBreakBlock"/>. When <see langword="null"/> or empty
        /// (the default), the rule is drawn without a title. When provided, the value is passed to
        /// the <see cref="Spectre.Console.Rule"/> constructor so the title appears centered on the
        /// rule line.
        /// </summary>
        public string? ThematicBreakTitle { get; set; }

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
            QuotedBlock = this.QuotedBlock,
            ShowAbbreviationTitle = this.ShowAbbreviationTitle,
            ShowFencedCodeBlockInfo = this.ShowFencedCodeBlockInfo,
            Strikethrough = this.Strikethrough,
            Subscript = this.Subscript,
            Superscript = this.Superscript,
            ThematicBreak = this.ThematicBreak,
            ThematicBreakTitle = this.ThematicBreakTitle,
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
    }
}