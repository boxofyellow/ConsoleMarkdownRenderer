using System;
using Markdig.Renderers;
using Markdig.Syntax.Inlines;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleContainerInlineRenderer : ConsoleObjectRenderer<ContainerInline>
    {
        protected override void Write(ConsoleRenderer renderer, ContainerInline obj)
        {
            if (obj.FirstChild != default)
            {
                var inline = obj.FirstChild;
                while (inline != default)
                {
                    renderer.Write(inline);
                    inline = inline.NextSibling;
                }
            }
        }

        // We don't want this one to handle any of types that derive from ContainerInline, only those that ARE
        public override bool SupportsType(RendererBase renderer, Type type) 
            => type == typeof(ContainerInline);
    }
}