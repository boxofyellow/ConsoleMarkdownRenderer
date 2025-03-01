using System;
using Markdig.Syntax;

namespace ConsoleMarkdownRenderer.ObjectRenderers
{
    public class ConsoleCodeBlockRenderer : ConsoleObjectRenderer<CodeBlock>
    {
        protected override void Write(ConsoleRenderer renderer, CodeBlock obj)
        {
            renderer
                .NewFrame()
                .PushStyle(renderer.Options.CodeBlock)
                .StartInline()
                .AddInLine(Environment.NewLine);

            for (int i = 0; i < obj.Lines.Lines.Length; i++)
            {
                if (!string.IsNullOrEmpty(obj.Lines.Lines[i].Slice.Text))
                {
                    renderer
                        .AddInLine("  ")
                        .WriteEscape(ref obj.Lines.Lines[i].Slice)
                        .AddInLine(Environment.NewLine);
                }
            }

            renderer
                .EndInline()
                .PopStyle()                
                .CompleteFrame();
        }
    }
}