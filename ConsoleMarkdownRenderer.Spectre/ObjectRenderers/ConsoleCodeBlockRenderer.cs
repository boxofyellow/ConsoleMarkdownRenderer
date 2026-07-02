using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Syntax;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal class ConsoleCodeBlockRenderer : ConsoleObjectRendererBase<CodeBlock>
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
            .PopStyle();

        if (renderer.Options.ShowFencedCodeBlockInfo && obj is FencedCodeBlock fenced && !string.IsNullOrEmpty(fenced.Info))
        {
            var style = renderer.Options.FencedCodeBlockInfo;
            var header = $"[{style.ToMarkup()}]{Markup.Escape(fenced.Info)}[/]";
            renderer.CompleteFrame(t => new Panel(t)
            {
                Border = renderer.Options.FencedCodeBlockInfoPanelBorder,
                BorderStyle = style,
                Header = new PanelHeader(header),
            });
        }
        else
        {
            renderer.CompleteFrame();
        }
    }
}