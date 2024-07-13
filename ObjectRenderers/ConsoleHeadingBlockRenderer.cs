using System;
using Markdig.Syntax;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleHeadingBlockRenderer : ConsoleObjectRenderer<HeadingBlock>
    {
        protected override void Write(ConsoleRenderer renderer, HeadingBlock obj)
        {
            string leftWrap;
            string rightWrap;

            if (renderer.Options.WrapHeader)
            {
                string decorate = new('#', obj.Level);
                leftWrap = $"{decorate} ";
                rightWrap = $" {decorate}";
            }
            else
            {
                leftWrap = rightWrap = string.Empty;
            }

            renderer
                .StartInline()
                .AddInLine(Environment.NewLine)
                .AddInLine($"[{renderer.Options.EffectiveHeader(obj.Level).ToMarkup()}]")
                .AddInLine(leftWrap)
                .WriteLeafInline(obj)
                .AddInLine(rightWrap)
                .AddInLine("[/]")
                .AddInLine(Environment.NewLine)
                .EndInline();
        }
    }
}