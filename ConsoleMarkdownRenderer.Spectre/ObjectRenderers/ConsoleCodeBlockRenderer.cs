using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Markdig.Syntax;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;

[SpectreSourceFile]
internal class ConsoleCodeBlockRenderer : ConsoleObjectRendererBase<CodeBlock>
{
    protected override void Write(ConsoleRenderer renderer, CodeBlock obj)
    {
        renderer
            .NewFrame()
            .AddFilledBlock(obj, renderer.Options.CodeBlock, indent: "  ");

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
