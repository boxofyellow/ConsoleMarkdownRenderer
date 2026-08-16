namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class RenderRobustnessTests : ConsoleTestBase
{
    [TestMethod]
    [DataRow(RenderRobustness.WideWidth,   "hello",                             "plain text"        )]
    [DataRow(RenderRobustness.NarrowWidth, "hello",                             "plain text narrow" )]
    [DataRow(RenderRobustness.WideWidth,   "# Heading\n\nSome *emphasis* text.", "heading + emphasis")]
    [DataRow(RenderRobustness.NarrowWidth, "# Heading\n\nSome *emphasis* text.", "heading + emphasis narrow")]
    public void RenderRobustness_PassesForKnownGoodInput(int width, string markdown, string caseLabel)
        => this.AssertRendersRobustly(markdown, options: null, width, caseLabel);

    // Regression coverage for #300: an unclosed, empty fenced code block used to throw a
    // NullReferenceException while rendering. Exercise both shared widths so a future regression
    // that only surfaces during wrapping/truncation is also caught.
    [TestMethod]
    [DataRow(RenderRobustness.WideWidth)]
    [DataRow(RenderRobustness.NarrowWidth)]
    public void RenderRobustness_PassesForUnclosedEmptyFencedCodeBlock(int width)
        => this.AssertRendersRobustly(
            GetResourceContent("unclosedEmptyFencedCodeBlock"),
            options: null,
            width,
            caseLabel: nameof(RenderRobustness_PassesForUnclosedEmptyFencedCodeBlock));

    [TestMethod]
    [DataRow(RenderRobustness.WideWidth)]
    [DataRow(RenderRobustness.NarrowWidth)]
    public void RenderRobustness_PassesForUnclosedEmptyFencedCodeBlockWithInfo(int width)
        => this.AssertRendersRobustly(
            GetResourceContent("unclosedEmptyFencedCodeBlockWithInfo"),
            options: null,
            width,
            caseLabel: nameof(RenderRobustness_PassesForUnclosedEmptyFencedCodeBlockWithInfo));

    // Sanity check that the assertions inside AssertRendersRobustly are actually wired up: a
    // renderer that throws must fail the test (not be silently swallowed), and the failure message
    // must carry the caller-supplied case label back to whoever is looking at the failure.
    [TestMethod]
    public void RenderRobustness_FailsWhenRendererThrows()
    {
        const string caseLabel = "throwing-renderer";

        var failure = Assert.Throws<AssertFailedException>(() =>
            this.AssertRendersRobustly(
                "hello",
                options: null,
                RenderRobustness.WideWidth,
                caseLabel,
                renderer: new ThrowingRenderer()));

        Assert.Contains(caseLabel, failure.Message, "Failure message should echo the caller-supplied case label");
        Assert.Contains("Render(...) threw", failure.Message, "Failure message should explain that the renderer threw");
    }

    [TestMethod]
    public void RenderRobustness_FailureMessageDescribesDefaultOptions()
    {
        var failure = Assert.Throws<AssertFailedException>(() =>
            this.AssertRendersRobustly(
                "hello",
                options: null,
                RenderRobustness.WideWidth,
                caseLabel: "default-options",
                renderer: new ThrowingRenderer()));

        Assert.Contains("[options: <default>]", failure.Message, "Null options should be described as <default>");
    }

    [TestMethod]
    public void RenderRobustness_FailureMessageDescribesCustomizedOptionsAsDiffFromDefault()
    {
        // Only the properties that differ from `new SpectreDisplayOptions()` should show up, not
        // the whole options object - keeps the message short even when generated/mutated suites
        // (see #302) start feeding this harness large, oddly-configured options.
        var options = new SpectreDisplayOptions { TableExpand = true };

        var failure = Assert.Throws<AssertFailedException>(() =>
            this.AssertRendersRobustly(
                "hello",
                options,
                RenderRobustness.WideWidth,
                caseLabel: "customized-options",
                renderer: new ThrowingRenderer()));

        Assert.Contains($"{nameof(SpectreDisplayOptions.TableExpand)}=True", failure.Message, "Failure message should call out the customized property and its value");
        Assert.DoesNotContain(nameof(SpectreDisplayOptions.CodeBlock), failure.Message, "Failure message should not list properties that still match the default");
    }

    private sealed class ThrowingRenderer : ISpectreMarkdownRenderer
    {
        public MarkdownRenderResult Render(string text, SpectreDisplayOptions? options = null)
            => throw new InvalidOperationException("Simulated renderer failure for RenderRobustness tests.");
    }

    private static string GetResourceContent(string name)
    {
        var path = Path.Combine("resources", Path.ChangeExtension(name, "md"));
        using var markdownStream = typeof(RenderRobustnessTests).Assembly.GetManifestResourceStream(path);
        Assert.IsNotNull(markdownStream, $"Failed to find resource for {path}");
        using var reader = new StreamReader(markdownStream);
        return reader.ReadToEnd();
    }
}
