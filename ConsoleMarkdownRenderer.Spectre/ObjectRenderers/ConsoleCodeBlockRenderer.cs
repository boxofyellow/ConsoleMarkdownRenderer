using System.Text;
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
        // Build the code text (with a blank row above and below) and hand it to a renderable
        // that pads every rendered line out to the full block width. That way the code block
        // style's background color spans the whole block - including short lines and the blank
        // rows - instead of only sitting behind the text and looking jagged. Because the
        // padding is applied per rendered line, the background stays a solid rectangle even
        // when the terminal is too narrow and the content wraps.
        var builder = new StringBuilder();
        builder.Append(Environment.NewLine);

        for (int i = 0; i < obj.Lines.Lines.Length; i++)
        {
            ref var slice = ref obj.Lines.Lines[i].Slice;
            if (!string.IsNullOrEmpty(slice.Text))
            {
                builder
                    .Append("  ")
                    .Append(slice.Text, slice.Start, slice.Length)
                    .Append(Environment.NewLine);
            }
        }

        var codeStyle = renderer.Options.CodeBlock;
        renderer
            .NewFrame()
            .AddRenderable(new BackgroundFillRenderable(new Text(builder.ToString(), codeStyle), codeStyle));

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