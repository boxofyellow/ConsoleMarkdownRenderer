using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Markdig.Extensions.Abbreviations;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal class ConsoleAbbreviationInlineRenderer : ConsoleObjectRenderer<AbbreviationInline>
    {
        protected override void Write(ConsoleRenderer renderer, AbbreviationInline obj)
        {
            renderer.WriteEscape(obj.Abbreviation.Label);
            if (renderer.Options.ShowAbbreviationTitle)
            {
                renderer
                    .AddInLine($" ([{renderer.Options.AbbreviationTitle.ToSpectreStyle().ToMarkup()}]")
                    .WriteEscape(obj.Abbreviation.Text.ToString())
                    .AddInLine("[/])");
            }
        }
    }
}
