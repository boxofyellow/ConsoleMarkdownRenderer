using System.Collections.Generic;
using Spectre.Console;

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// Class for controlling the styling and other other display options for the Markdown elements 
    /// </summary>
    public class DisplayOptions
    {
        public Style Bold { get; set; } = new(decoration: Decoration.Bold);
        public Style CodeBlock { get; set; } = new(foreground: Color.Yellow, background: Color.Blue);
        public Style CodeInLine { get; set; } = new(foreground: Color.Yellow, background: Color.Blue);

        // List of Styles to use for headers the first will be used for #, the second for ## and so on
        // If the document referenced more than the length of the list, the Style in header will be used.
        public List<Style> Headers {get; set; } = new();
        public Style Header { get; set; } = new(decoration: Decoration.Bold | Decoration.Underline | Decoration.Invert);

        public Style HtmlBlock { get; set; } = new(foreground: Color.Black, background: Color.Green);
        public Style HtmlInline { get; set; } = new(foreground: Color.Black, background: Color.Green);
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Inserted"/>
        public Style Inserted { get; set; } = new(decoration: Decoration.Underline);
        public Style Italic { get; set; } = new(decoration: Decoration.Italic);
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Marked"/>
        public Style Marked { get; set; } = new(foreground: Color.Black, background: Color.Yellow);

        public Style QuotedBlock { get; set; } = new Style(decoration: Decoration.Italic);

        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Strikethrough"/>
        public Style Strikethrough { get; set; } = new(decoration: Decoration.Strikethrough);

        // Hey, I'm sure there might be something better for subscript... but sometimes you have to make due with what you got 
        // And the blink does not seem to render well
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Subscript"/>
        public Style Subscript { get; set; } = new(decoration: Decoration.SlowBlink);

        // This another one.  Don't have an exact match for superscript
        /// <see cref="Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Superscript"/>
        public Style Superscript { get; set; } = new(decoration: Decoration.RapidBlink);


        // Yes, these are more than a style, but it should help identify where things need updating
        public Style UnknownDelimiterChar { get; set; } = new(decoration: Decoration.Dim);
        public Style UnknownDelimiterContent { get; set; } = new(decoration: Decoration.Invert);

        // When set to true wrap Headers with '#'s 
        public bool WrapHeader { get; set; } = true;
 
        // When set to true the content structure is displayed and detail of unsupported markdown is displayed
        public bool IncludeDebug = false;

        public DisplayOptions Clone() => new()
        {
            Bold = this.Bold,
            CodeBlock = this.CodeBlock,
            CodeInLine = this.CodeInLine,
            Header = this.Header,
            Headers = new(this.Headers),
            HtmlBlock = this.HtmlBlock,
            HtmlInline = this.HtmlInline,
            IncludeDebug = this.IncludeDebug,
            Inserted = this.Inserted,
            Italic = this.Italic,
            Marked = this.Marked,
            QuotedBlock = this.QuotedBlock,
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
        public Style EffectiveHeader(int level) => 
            level <= Headers.Count 
                   ? Headers[level - 1]
                   : Header;
    }
}