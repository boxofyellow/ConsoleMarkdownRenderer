using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using Markdig.Syntax;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class StackBalanceTests
{
    [TestMethod]
    [DataRow("frame",      "EndInline")]
    [DataRow("style",      "PopStyle")]
    [DataRow("table",      "CompleteTableRow")]
    [DataRow("list",       "SetNextListItemCheck")]
    [DataRow("link frame", "PopLink")]
    public void EmptyStackAccess_ReportsStackOperationAndRendererContext(string stackName, string operation)
    {
        var failure = RenderExpectingFailure(new UnderflowingRenderer(stackName));

        Assert.Contains(stackName, failure.Message);
        Assert.Contains(operation, failure.Message);
        Assert.Contains("expected depth 1, actual depth 0", failure.Message);
        Assert.Contains(nameof(UnderflowingRenderer), failure.Message);
        Assert.Contains(nameof(MarkdownDocument), failure.Message);
    }

    [TestMethod]
    public void RendererWrite_DetectsUncompletedFrame()
    {
        var failure = RenderExpectingFailure(new UnfinishedFrameRenderer());

        Assert.Contains("frame expected depth 0, actual depth 1", failure.Message);
        Assert.Contains(nameof(UnfinishedFrameRenderer), failure.Message);
        Assert.Contains(nameof(MarkdownDocument), failure.Message);
    }

    [TestMethod]
    public void RendererWrite_DetectsUnendedInlineContent()
    {
        var failure = RenderExpectingFailure(new UnfinishedInlineRenderer());

        Assert.Contains("inline content buffer expected depth 0, actual depth 7", failure.Message);
        Assert.Contains(nameof(UnfinishedInlineRenderer), failure.Message);
    }

    [TestMethod]
    public void StartInline_RejectsExistingInlineContent()
    {
        var failure = RenderExpectingFailure(new NonemptyInlineStartRenderer());

        Assert.Contains("inline content buffer expected depth 0, actual depth 7", failure.Message);
        Assert.Contains("StartInline", failure.Message);
        Assert.Contains(nameof(NonemptyInlineStartRenderer), failure.Message);
    }

    private static InvalidOperationException RenderExpectingFailure(ConsoleObjectRendererBase<MarkdownDocument> renderer)
    {
        var consoleRenderer = new ConsoleRenderer(new SpectreDisplayOptions());
        consoleRenderer.ObjectRenderers.Insert(0, renderer);

        return Assert.Throws<InvalidOperationException>(() => consoleRenderer.Render(new MarkdownDocument()));
    }

    private sealed class UnderflowingRenderer(string stackName) : ConsoleObjectRendererBase<MarkdownDocument>
    {
        protected override void WriteImplementation(ConsoleRenderer renderer, MarkdownDocument obj)
        {
            switch (stackName)
            {
                case "frame":
                    renderer.EndInline();
                    break;
                case "style":
                    renderer.PopStyle();
                    break;
                case "table":
                    renderer.CompleteTableRow();
                    break;
                case "list":
                    renderer.SetNextListItemCheck(isChecked: true);
                    break;
                case "link frame":
                    renderer.PopLink("https://example.com");
                    break;
                default:
                    Assert.Fail($"Unknown stack '{stackName}'.");
                    break;
            }
        }
    }

    private sealed class UnfinishedFrameRenderer : ConsoleObjectRendererBase<MarkdownDocument>
    {
        protected override void WriteImplementation(ConsoleRenderer renderer, MarkdownDocument obj)
            => renderer.NewFrame();
    }

    private sealed class UnfinishedInlineRenderer : ConsoleObjectRendererBase<MarkdownDocument>
    {
        protected override void WriteImplementation(ConsoleRenderer renderer, MarkdownDocument obj)
            => renderer.StartInline().AddInLine("content");
    }

    private sealed class NonemptyInlineStartRenderer : ConsoleObjectRendererBase<MarkdownDocument>
    {
        protected override void WriteImplementation(ConsoleRenderer renderer, MarkdownDocument obj)
            => renderer.StartInline().AddInLine("content").StartInline();
    }
}
