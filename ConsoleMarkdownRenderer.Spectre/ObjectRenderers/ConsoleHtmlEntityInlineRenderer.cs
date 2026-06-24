using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Syntax.Inlines;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    [SpectreSourceFile]
    internal class ConsoleHtmlEntityInlineRenderer : ConsoleObjectRendererBase<HtmlEntityInline>
    {
        protected override void Write(ConsoleRenderer renderer, HtmlEntityInline obj)
        {
            var transcoded = obj.Transcoded;
            renderer.WriteEscape(ref transcoded);
        }
    }
}
