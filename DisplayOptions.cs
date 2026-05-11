using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer
{
    /// <summary>
    /// Class for controlling the styling and other display options for the Markdown elements 
    /// </summary>
    public class DisplayOptions
    {
        public TextStyle Bold { get; set; } = new(decoration: TextDecoration.Bold);
        public TextStyle CodeBlock { get; set; } = new(foreground: TextColor.Yellow, background: TextColor.Blue);
        public TextStyle CodeInLine { get; set; } = new(foreground: TextColor.Yellow, background: TextColor.Blue);

        /// <summary>
        /// Style applied to the contents of a <see cref="Markdig.Extensions.DefinitionLists.DefinitionItem"/>
        /// (the collection of definition entries within a definition list).
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
        /// When set to true, the Info field from <see cref="Markdig.Syntax.FencedCodeBlock"/> (e.g., the language identifier) will be displayed.
        /// </summary>
        public bool ShowFencedCodeBlockInfo { get; set; } = false;

        /// <summary>
        /// Style for the Info field of a <see cref="Markdig.Syntax.FencedCodeBlock"/> when <see cref="ShowFencedCodeBlockInfo"/> is true.
        /// </summary>
        public TextStyle FencedCodeBlockInfo { get; set; } = new(foreground: TextColor.Green, background: TextColor.Blue);

        // List of Styles to use for headers the first will be used for #, the second for ## and so on
        // If the document referenced more than the length of the list, the Style in header will be used.
        public List<TextStyle> Headers {get; set; } = new();
        public TextStyle Header { get; set; } = new(decoration: TextDecoration.Bold | TextDecoration.Underline | TextDecoration.Invert);

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

        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Inserted"/>
        public TextStyle Inserted { get; set; } = new(decoration: TextDecoration.Underline);
        public TextStyle Italic { get; set; } = new(decoration: TextDecoration.Italic);
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Marked"/>
        public TextStyle Marked { get; set; } = new(foreground: TextColor.Black, background: TextColor.Yellow);

        public TextStyle QuotedBlock { get; set; } = new(decoration: TextDecoration.Italic);

        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough"/>
        public TextStyle Strikethrough { get; set; } = new(decoration: TextDecoration.Strikethrough);

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
 
        // When set to true the content structure is displayed and detail of unsupported markdown is displayed
        public bool IncludeDebug = false;

        public DisplayOptions Clone() => new()
        {
            Bold = this.Bold,
            CodeBlock = this.CodeBlock,
            CodeInLine = this.CodeInLine,
            DefinitionItem = this.DefinitionItem,
            DefinitionList = this.DefinitionList,
            DefinitionTerm = this.DefinitionTerm,
            FencedCodeBlockInfo = this.FencedCodeBlockInfo,
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
            ShowFencedCodeBlockInfo = this.ShowFencedCodeBlockInfo,
            Strikethrough = this.Strikethrough,
            Subscript = this.Subscript,
            Superscript = this.Superscript,
            UnknownDelimiterChar = this.UnknownDelimiterChar,
            UnknownDelimiterContent = this.UnknownDelimiterContent,
            WrapHeader = this.WrapHeader,
        };

        /// <summary>
        /// Computes which style to use for given Object Level
        /// </summary>
        /// <param name="level">The level of the Object for `#` it will 1, for `##` it will be 2, and so on</param>
        /// <returns>The style to use</returns>
        public TextStyle EffectiveHeader(int level) => 
            level <= Headers.Count 
                   ? Headers[level - 1]
                   : Header;
    }
}