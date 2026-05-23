using Markdig.Syntax.Inlines;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers
{
    internal class ConsoleHtmlInlineRenderer : ConsoleObjectRenderer<HtmlInline>
    {
        protected override void Write(ConsoleRenderer renderer, HtmlInline obj)
        {
            var isStart = !obj.Tag.StartsWith("</");
            var isContentless = obj.Tag.EndsWith("/>");

            if (isStart)
            {
                renderer.AddInLine($"[{renderer.Options.HtmlInline.ToMarkup()}]");
            }
            renderer.WriteEscape(obj.Tag);

            if (!isStart || isContentless)
            {
                renderer.AddInLine("[/]");
            }
        }
    }
}
