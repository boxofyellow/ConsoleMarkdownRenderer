using System;
using Markdig.Syntax;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleHeadingBlockRenderer : ConsoleObjectRenderer<HeadingBlock>
    {
        protected override void Write(ConsoleRenderer renderer, HeadingBlock obj)
        {
            string wrap = renderer.Options.WrapHeader 
                ? new('#', obj.Level)
                : string.Empty;

            renderer
                .StartInline()
                .AddInLine(Environment.NewLine)
                .AddInLine($" [{renderer.Options.Header.ToMarkup()}] ")
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