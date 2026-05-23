using Markdig.Extensions.SmartyPants;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    /// <summary>
    /// Renders Markdig's <see cref="SmartyPant"/> inline nodes (produced by
    /// <see cref="Markdig.MarkdownExtensions.UseSmartyPants(Markdig.MarkdownPipelineBuilder)"/>) as their
    /// Unicode typographic equivalents — curly quotes, en/em-dashes, angle quotes, and the ellipsis character.
    /// Markdig's default <see cref="SmartyPantOptions.Mapping"/> emits HTML named entities (<c>&amp;ldquo;</c>,
    /// etc.) which are appropriate for an HTML renderer but not for a console; this renderer substitutes the
    /// corresponding Unicode code points instead. SmartyPants only transforms literal text in prose, so
    /// punctuation inside code spans and fenced code blocks is left untouched by the parser and continues to
    /// render verbatim.
    /// </summary>
    internal class ConsoleSmartyPantInlineRenderer : ConsoleObjectRenderer<SmartyPant>
    {
        protected override void Write(ConsoleRenderer renderer, SmartyPant obj)
            => renderer.WriteEscape(GetReplacement(obj.Type));

        internal static string GetReplacement(SmartyPantType type) => type switch
        {
            SmartyPantType.Quote            => "\u2019", // ’  right single quote (apostrophe)
            SmartyPantType.LeftQuote        => "\u2018", // ‘  left single quote
            SmartyPantType.RightQuote       => "\u2019", // ’  right single quote
            SmartyPantType.DoubleQuote      => "\u201D", // ”  right double quote
            SmartyPantType.LeftDoubleQuote  => "\u201C", // “  left double quote
            SmartyPantType.RightDoubleQuote => "\u201D", // ”  right double quote
            SmartyPantType.LeftAngleQuote   => "\u00AB", // «  left angle quote
            SmartyPantType.RightAngleQuote  => "\u00BB", // »  right angle quote
            SmartyPantType.Ellipsis         => "\u2026", // …  horizontal ellipsis
            SmartyPantType.Dash2            => "\u2013", // –  en-dash
            SmartyPantType.Dash3            => "\u2014", // —  em-dash
            _                               => string.Empty,
        };
    }
}
