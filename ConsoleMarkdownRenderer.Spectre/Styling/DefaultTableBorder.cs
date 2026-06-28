using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;

[SpectreSourceFile]
public sealed class DefaultTableBorder : TableBorder
{
    private DefaultTableBorder() { }
    public static TableBorder Default { get; } = new DefaultTableBorder();
    public override bool Visible => _noTableBorder.Visible;
    public override bool SupportsRowSeparator => _noTableBorder.SupportsRowSeparator;
    public override bool UsePadding => _noTableBorder.UsePadding;
    public override TableBorder? SafeBorder => _noTableBorder.SafeBorder;
    public override string GetPart(TableBorderPart part) => _noTableBorder.GetPart(part);
    public override string GetColumnRow(TablePart part, IReadOnlyList<int> widths, IReadOnlyList<IColumn> columns) => _noTableBorder.GetColumnRow(part, widths, columns);
    private static readonly NoTableBorder _noTableBorder = new();
}