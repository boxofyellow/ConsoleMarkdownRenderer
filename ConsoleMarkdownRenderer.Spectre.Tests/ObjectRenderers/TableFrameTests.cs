using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using Markdig.Extensions.Tables;
using Markdig.Syntax;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class TableFrameTests : ConsoleTestBase
{
    [Timeout(TestTimeouts.Render)]
    [TestMethod]
    public void TableFrame_RejectsCellBeyondAllocatedColumnCountWithDiagnostics()
    {
        var table = new Table();
        table.Add(CreateRow());
        table.Add(CreateRow());

        var renderer = new ConsoleRenderer(new SpectreDisplayOptions());
        renderer.ObjectRenderers.Insert(0, new WideningTableRenderer());

        var failure = Assert.Throws<InvalidOperationException>(() => renderer.Render(CreateDocument(table)));

        Assert.Contains("TableFrame", failure.Message);
        Assert.Contains("Markdig table", failure.Message);
        Assert.Contains("row 1", failure.Message);
        Assert.Contains("expected 1 columns", failure.Message);
        Assert.Contains("actual cell index attempted 1", failure.Message);
    }

    [Timeout(TestTimeouts.Render)]
    [TestMethod]
    public void TableFrame_RendersTableWithNoRows()
    {
        var renderer = new ConsoleRenderer(new SpectreDisplayOptions());

        renderer.Render(CreateDocument(new Table()));

        Assert.IsNotNull(renderer.Root);
        ConsoleUnderTest.Write(renderer.Root);
    }

    private static MarkdownDocument CreateDocument(Table table)
    {
        var document = new MarkdownDocument();
        document.Add(table);
        return document;
    }

    private static TableRow CreateRow()
    {
        var row = new TableRow();
        row.Add(new TableCell());
        return row;
    }

    private sealed class WideningTableRenderer : ConsoleObjectRendererBase<Table>
    {
        protected override void WriteImplementation(ConsoleRenderer renderer, Table obj)
        {
            renderer.NewTableFrame(obj);
            ((TableRow)obj[1]).Add(new TableCell());
            renderer.WriteChildrenChain(obj);
            renderer.CompleteTableFrame();
        }
    }
}
