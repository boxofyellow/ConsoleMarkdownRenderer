using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.Mathematics;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers
{
    [SpectreSourceFile]
    internal class ConsoleMathBlockRenderer : ConsoleObjectRendererBase<MathBlock>
    {
        protected override void Write(ConsoleRenderer renderer, MathBlock obj)
        {
            renderer
                .NewFrame()
                .PushStyle(renderer.Options.MathBlock)
                .StartInline()
                .AddInLine(Environment.NewLine);

            if (!string.IsNullOrEmpty(renderer.Options.MathBlockLabelText))
            {
                renderer
                    .AddInLine($"[{renderer.Options.MathBlockLabel.ToMarkup()}]")
                    .WriteEscape($"  [{renderer.Options.MathBlockLabelText}]")
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
