using Markdig.Syntax.Inlines;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleAutolinkInlineRenderer : ConsoleObjectRenderer<AutolinkInline>
    {
        protected override void Write(ConsoleRenderer renderer, AutolinkInline obj)
        {
            var url = obj.IsEmail ? $"mailto:{obj.Url}" : obj.Url;
            renderer.WriteLink(r => r.WriteEscape(obj.Url), url);
        }
    }
}
