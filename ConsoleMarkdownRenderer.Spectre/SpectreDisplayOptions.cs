using Markdig;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    /// <summary>
    /// Controls the styling and other display options for a <see cref="SpectreMarkdownRenderer"/>.
    /// All style properties use Spectre.Console types directly, making this suitable for callers
    /// who are already working with Spectre.Console and want to splice markdown rendering into
    /// their own document without taking a dependency on the JSON-serializable wrapper types
    /// (<c>TextStyle</c>, <c>TextColor</c>, etc.) from the main package.
    /// </summary>
    public sealed class SpectreDisplayOptions
    {
        /// <summary>
        /// Style applied to the expansion title that follows an abbreviation inline
        /// (e.g. the <c>HyperText Markup Language</c> portion of <c>HTML (HyperText Markup Language)</c>).
        /// </summary>
        public Style AbbreviationTitle { get; set; } = new(decoration: Decoration.Dim);

        public Style Bold { get; set; } = new(decoration: Decoration.Bold);
        public Style CodeBlock { get; set; } = new(Color.Yellow, Color.Blue);
        public Style CodeInLine { get; set; } = new(Color.Yellow, Color.Blue);

        /// <summary>
        /// Style applied to the body of a custom container block (e.g. an admonition /
        /// callout block such as <c>:::note</c>, <c>:::warning</c>, or <c>:::tip</c>).
        /// </summary>
        public Style CustomContainer { get; set; } = Style.Plain;

        /// <summary>
        /// Style applied to the label (e.g. <c>note</c> / <c>warning</c> / <c>tip</c>)
        /// emitted at the top of a custom container block.
        /// </summary>
        public Style CustomContainerInfo { get; set; } = new(decoration: Decoration.Bold);

        /// <summary>
        /// Style applied to the contents of an inline custom container (e.g. <c>::tag content::</c>).
        /// </summary>
        public Style CustomContainerInline { get; set; } = new(decoration: Decoration.Bold);

        /// <summary>
        /// Style applied to the contents of a definition item (all children, including its
        /// terms and definitions).
        /// </summary>
        public Style DefinitionItem { get; set; } = Style.Plain;

        /// <summary>Style applied to the contents of a definition list.</summary>
        public Style DefinitionList { get; set; } = Style.Plain;

        /// <summary>Style applied to the term label of a definition term.</summary>
        public Style DefinitionTerm { get; set; } = new(decoration: Decoration.Bold);

        /// <summary>
        /// When <see langword="true"/>, the Info field from a fenced code block (e.g. the
        /// language identifier) will be displayed above the block content.
        /// </summary>
        public bool ShowFencedCodeBlockInfo { get; set; } = false;

        /// <summary>
        /// Style for the Info field of a fenced code block when <see cref="ShowFencedCodeBlockInfo"/> is <see langword="true"/>.
        /// </summary>
        public Style FencedCodeBlockInfo { get; set; } = new(Color.Green, Color.Blue);

        /// <summary>
        /// Style applied to the inline content of a figure caption. Italic by default to
        /// visually distinguish it from the figure's body content.
        /// </summary>
        public Style FigureCaption { get; set; } = new(decoration: Decoration.Italic);

        /// <summary>
        /// List of styles to use for headings, indexed by level (index 0 = H1, index 1 = H2, …).
        /// If the document contains more heading levels than this list, <see cref="Header"/> is used.
        /// By default the first entry is a <see cref="SpectreFigletHeaderStyle"/>, so top-level
        /// (#) headings render as FIGlet ASCII art.
        /// </summary>
        public List<ISpectreHeaderStyle> Headers { get; set; } = new()
        {
            new SpectreFigletHeaderStyle(justification: Justify.Left),
        };

        /// <summary>
        /// The fallback heading style used for heading levels beyond <see cref="Headers"/>.
        /// </summary>
        public ISpectreHeaderStyle Header { get; set; } = new SpectreStyleHeaderStyle(
            new Style(Color.Default, Color.Default, Decoration.Bold | Decoration.Underline | Decoration.Invert));

        public Style HtmlBlock { get; set; } = new(Color.Black, Color.Green);
        public Style HtmlInline { get; set; } = new(Color.Black, Color.Green);

        /// <summary>Style applied to the contents of a footer block.</summary>
        public Style Footer { get; set; } = new(decoration: Decoration.Dim | Decoration.Italic);

        /// <summary>Style applied to the body of a footnote, including the label prefix.</summary>
        public Style Footnote { get; set; } = new(decoration: Decoration.Bold);

        /// <summary>Style applied to the contents of a footnote group.</summary>
        public Style FootnoteGroup { get; set; } = new(decoration: Decoration.Italic);

        /// <summary>Style applied to a footnote link marker.</summary>
        public Style FootnoteLink { get; set; } = new(Color.Blue, Color.Default, Decoration.Underline);

        /// <summary>
        /// When <see langword="true"/> (the default), emoji shortcodes and text smileys are
        /// rendered as their Unicode emoji equivalents. When <see langword="false"/>, the
        /// original shortcode/smiley text is emitted instead.
        /// </summary>
        public bool Emojis { get; set; } = true;

        public Style Inserted { get; set; } = new(decoration: Decoration.Underline);
        public Style Italic { get; set; } = new(decoration: Decoration.Italic);
        public Style Marked { get; set; } = new(Color.Black, Color.Yellow);

        /// <summary>
        /// Style applied to the verbatim source of a display math block (<c>$$ … $$</c>).
        /// </summary>
        public Style MathBlock { get; set; } = new(Color.Green, Color.Purple);

        /// <summary>
        /// Style applied to the optional label emitted at the top of a math block.
        /// </summary>
        public Style MathBlockLabel { get; set; } = new(Color.Yellow, Color.Purple);

        /// <summary>
        /// Text used for the optional math block label. When <see langword="null"/> or empty,
        /// no label is emitted (the default).
        /// </summary>
        public string MathBlockLabelText { get; set; } = string.Empty;

        /// <summary>
        /// Style applied to the verbatim source of an inline math expression (<c>$ … $</c>).
        /// </summary>
        public Style MathInline { get; set; } = new(Color.Green, Color.Purple);

        public Style QuotedBlock { get; set; } = new(decoration: Decoration.Italic);

        /// <summary>
        /// When <see langword="true"/> (the default), Markdig's SmartyPants extension is added
        /// to the pipeline so that ASCII punctuation in prose is rewritten with typographic
        /// equivalents. When <see langword="false"/>, punctuation is rendered verbatim.
        /// </summary>
        public bool SmartyPants { get; set; } = true;

        public Style Strikethrough { get; set; } = new(decoration: Decoration.Strikethrough);

        /// <summary>
        /// Style applied to the rule line emitted for a thematic break / horizontal rule.
        /// </summary>
        public Style ThematicBreak { get; set; } = Style.Plain;

        /// <summary>
        /// Border style applied to Spectre.Console <c>Table</c> widgets for Markdown pipe tables.
        /// </summary>
        public TableBorder TableBorder { get; set; } = TableBorder.Square;

        /// <summary>
        /// Style applied to the border characters of rendered tables.
        /// </summary>
        public Style TableBorderStyle { get; set; } = Style.Plain;

        public Style Subscript { get; set; } = new(decoration: Decoration.SlowBlink);
        public Style Superscript { get; set; } = new(decoration: Decoration.RapidBlink);

        public Style UnknownDelimiterChar { get; set; } = new(decoration: Decoration.Dim);
        public Style UnknownDelimiterContent { get; set; } = new(decoration: Decoration.Invert);

        /// <summary>
        /// Style applied to the raw source of a YAML front matter block.
        /// </summary>
        public Style YamlFrontMatter { get; set; } = new(decoration: Decoration.Italic | Decoration.Dim);

        /// <summary>When <see langword="true"/>, wrap heading text with <c>#</c> characters.</summary>
        public bool WrapHeader { get; set; } = true;

        /// <summary>
        /// When <see langword="true"/> (the default), links are wrapped with Spectre.Console's
        /// <c>[link=…]…[/]</c> markup so that supported terminals render them as clickable
        /// OSC 8 hyperlinks. Set to <see langword="false"/> to disable for terminals that
        /// render the escape sequences as garbage.
        /// </summary>
        public bool UseTerminalHyperlinks { get; set; } = true;

        /// <summary>
        /// When <see langword="true"/>, the content structure is displayed and details of
        /// unsupported markdown are included in the output.
        /// </summary>
        public bool IncludeDebug { get; set; } = false;

        /// <summary>
        /// Returns the effective heading style for the given heading <paramref name="level"/>
        /// (1-based: 1 = H1, 2 = H2, …).
        /// </summary>
        internal ISpectreHeaderStyle EffectiveHeader(int level) =>
            level <= Headers.Count
                ? Headers[level - 1]
                : Header;

        /// <summary>
        /// Builds the <see cref="MarkdownPipeline"/> appropriate for these options. Some Markdig
        /// extensions transform the parsed AST (rather than affecting rendering), so they must
        /// be wired in based on the active options at parse time.
        /// </summary>
        internal MarkdownPipeline BuildPipeline()
        {
            var builder = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseEmojiAndSmiley()
                .UseYamlFrontMatter();
            if (SmartyPants)
            {
                builder.UseSmartyPants();
            }
            return builder.Build();
        }
    }
}
