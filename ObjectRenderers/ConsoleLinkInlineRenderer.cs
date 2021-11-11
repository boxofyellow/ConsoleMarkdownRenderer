using Markdig.Syntax.Inlines;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleLinkInlineRenderer : ConsoleObjectRenderer<LinkInline>
    {
        protected override void Write(ConsoleRenderer renderer, LinkInline obj)
        {
            if (obj.IsImage)
            {
                renderer.AddInLine("!");
            }

            renderer
                .WriteEscape("[")
                .PushLink()
                .WriteChildrenChain(obj)
                .PopLink(obj)
                .WriteEscape("](")
                .WriteEscape(obj.Url)
                .AddInLine(")");
        }
    }
}