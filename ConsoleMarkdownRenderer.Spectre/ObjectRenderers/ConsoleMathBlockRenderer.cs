using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.Mathematics;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

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

        var label = renderer.Options.MathBlockLabelText;
        if (string.IsNullOrEmpty(label))
        {
            renderer.CompleteFrame();
            return;
        }

        var style = renderer.Options.MathBlockLabel;
        var header = $"[{style.ToMarkup()}]{Markup.Escape(label)}[/]";
        renderer.CompleteFrame(t => new Panel(t)
        {
            Border = renderer.Options.MathBlockPanelBorder,
            BorderStyle = style,
            Header = new PanelHeader(header),
        });
    }
}
