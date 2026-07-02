using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;

[SpectreSourceFile]
internal sealed class QuoteBlockTableBorder : TableBorder
{
    private QuoteBlockTableBorder() { }
    public static TableBorder QuoteBlock { get; } = new QuoteBlockTableBorder();

    // The border only draws the right edge of each cell, so the horizontal
    // top/bottom rows and row separators must never be emitted.
    public override bool SupportsRowSeparator => false;

    public override string GetPart(TableBorderPart part)
        => part switch
        {
            TableBorderPart.HeaderRight => "\u2502", // │
            TableBorderPart.CellRight   => "\u2502", // │
            _ => string.Empty,
        };
}
