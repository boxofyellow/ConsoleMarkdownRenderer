using System;
using Markdig.Syntax;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleHeadingBlockRenderer : ConsoleObjectRenderer<HeadingBlock>
    {
        protected override void Write(ConsoleRenderer renderer, HeadingBlock obj)
        {
            string wrap = new('#', obj.Level);
            renderer
                .StartInline()
                .AddInLine(Environment.NewLine)
                .AddInLine(" [bold underline invert] ")
                .AddInLine(wrap)
                .AddInLine(" ")
                .WriteLeafInline(obj)
                .AddInLine(" ")
                .AddInLine(wrap)
                .AddInLine(" [/]")
                .AddInLine(Environment.NewLine)
                .EndInline();
        }
    }
}