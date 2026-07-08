using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Extensions.Mathematics;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal class ConsoleMathBlockRenderer : ConsoleObjectRendererBase<MathBlock>
{
    protected override void Write(ConsoleRenderer renderer, MathBlock obj)
    {
        renderer
            .NewFrame()
            .AddFilledBlock(obj, renderer.Options.MathBlock, indent: "  ");

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
