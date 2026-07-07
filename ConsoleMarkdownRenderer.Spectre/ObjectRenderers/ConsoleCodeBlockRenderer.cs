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
        // Pad every rendered line to the width of the widest line so the code block
        // style's background color spans the full width of the block. Without this the
        // background only covers the text and short lines look jagged.
        int maxTextWidth = 0;
        for (int i = 0; i < obj.Lines.Lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(obj.Lines.Lines[i].Slice.Text))
            {
                maxTextWidth = Math.Max(maxTextWidth, obj.Lines.Lines[i].Slice.Length);
            }
        }

        // Each code line is indented by two spaces; the blank rows above and below the
        // code fill that same total width so the background forms a solid rectangle.
        var blankRow = new string(' ', maxTextWidth + 2);

        renderer
            .NewFrame()
            .PushStyle(renderer.Options.CodeBlock)
            .StartInline()
            .AddInLine(blankRow)
            .AddInLine(Environment.NewLine);

        for (int i = 0; i < obj.Lines.Lines.Length; i++)
        {
            if (!string.IsNullOrEmpty(obj.Lines.Lines[i].Slice.Text))
            {
                renderer
                    .AddInLine("  ")
                    .WriteEscape(ref obj.Lines.Lines[i].Slice)
                    .AddInLine(new string(' ', maxTextWidth - obj.Lines.Lines[i].Slice.Length))
                    .AddInLine(Environment.NewLine);
            }
        }

        renderer
            .AddInLine(blankRow)
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