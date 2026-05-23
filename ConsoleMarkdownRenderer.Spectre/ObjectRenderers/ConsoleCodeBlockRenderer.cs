using Markdig.Syntax;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    internal class ConsoleCodeBlockRenderer : ConsoleObjectRenderer<CodeBlock>
    {
        protected override void Write(ConsoleRenderer renderer, CodeBlock obj)
        {
            renderer
                .NewFrame()
                .PushStyle(renderer.Options.CodeBlock)
                .StartInline()
                .AddInLine(Environment.NewLine);

            // If this is a FencedCodeBlock and ShowFencedCodeBlockInfo is enabled, display the Info field
            if (renderer.Options.ShowFencedCodeBlockInfo && obj is FencedCodeBlock fenced && !string.IsNullOrEmpty(fenced.Info))
            {
                renderer
                    .AddInLine($"[{renderer.Options.FencedCodeBlockInfo.ToMarkup()}]")
                    .WriteEscape($"  [{fenced.Info}]")
                    .AddInLine("[/]")
                    .AddInLine(Environment.NewLine);
            }

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
