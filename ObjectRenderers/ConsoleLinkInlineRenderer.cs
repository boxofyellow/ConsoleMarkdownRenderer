using Markdig.Syntax.Inlines;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleLinkInlineRenderer : ConsoleObjectRenderer<LinkInline>
    {
        protected override void Write(ConsoleRenderer renderer, LinkInline obj)
            => renderer.WriteLink(r => r.WriteChildrenChain(obj), obj.Url ?? string.Empty, obj.IsImage);
    }
}