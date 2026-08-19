using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Syntax.Inlines;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal class ConsoleHtmlInlineRenderer : ConsoleObjectRendererBase<HtmlInline>
{
    protected override void Write(ConsoleRenderer renderer, HtmlInline obj)
    {
        var isStart = !obj.Tag.StartsWith("</");
        var isContentless = obj.Tag.EndsWith("/>");

        if (isStart)
        {
            renderer.StartHtmlInlineStyle();
        }
        renderer.WriteEscape(obj.Tag);

        if (!isStart || isContentless)
        {
            renderer.EndHtmlInlineStyle();
        }
    }
}