using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[SpectreSourceFile]
internal sealed class BackgroundFillRenderable : Renderable
{
    // Wraps a child renderable and pads every rendered line - after any wrapping the console
    // performs - out to the full width it is given, filling the remainder with a styled space
    // run. This lets a background color span the whole block instead of only sitting behind the
    // text, and because the padding is added per rendered line it stays a solid rectangle even
    // when the terminal is too narrow and the content wraps.
    private readonly IRenderable m_child;
    private readonly Style m_style;

    public BackgroundFillRenderable(IRenderable child, Style style)
    {
        m_child = child;
        m_style = style;
    }

    protected override Measurement Measure(RenderOptions options, int maxWidth)
        => m_child.Measure(options, maxWidth);

    protected override IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        foreach (var line in Segment.SplitLines(m_child.Render(options, maxWidth)))
        {
            foreach (var segment in line)
            {
                yield return segment;
            }

            int remaining = maxWidth - line.CellCount();
            if (remaining > 0)
            {
                yield return new Segment(new string(' ', remaining), m_style);
            }

            yield return Segment.LineBreak;
        }
    }
}
