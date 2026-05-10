using Markdig.Syntax.Inlines;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal class ConsoleHtmlEntityInlineRenderer : ConsoleObjectRenderer<HtmlEntityInline>
    {
        protected override void Write(ConsoleRenderer renderer, HtmlEntityInline obj)
        {
            var transcoded = obj.Transcoded;
            renderer.WriteEscape(ref transcoded);
        }
    }
}
