using System;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleAutolinkInlineRenderer : ConsoleObjectRenderer<AutolinkInline>
    {
        /// <summary>
        /// When set to false this renderer will report that it does not support <see cref="AutolinkInline"/>
        /// and will decline to render it, allowing tests to verify the unhandled-type code path.
        /// </summary>
        internal static bool IsEnabled { get; set; } = true;

        public override bool SupportsType(RendererBase renderer, Type type)
            => IsEnabled && base.SupportsType(renderer, type);

        public override void Write(RendererBase renderer, MarkdownObject obj)
        {
            if (IsEnabled) base.Write(renderer, obj);
        }

        protected override void Write(ConsoleRenderer renderer, AutolinkInline obj)
        {
            var url = obj.IsEmail ? $"mailto:{obj.Url}" : obj.Url;
            renderer.WriteLink(r => r.WriteEscape(obj.Url), url);
        }
    }
}
