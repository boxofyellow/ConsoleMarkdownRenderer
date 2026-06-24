using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.SmartyPants;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    [SpectreSourceFile]
    internal class ConsoleSmartyPantInlineRenderer : ConsoleObjectRendererBase<SmartyPant>
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
