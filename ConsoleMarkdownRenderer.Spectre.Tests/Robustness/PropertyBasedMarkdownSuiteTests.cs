namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Bounded, deterministic property-based Markdown coverage. By default this runs two fixed seeds;
/// set <c>CONSOLE_MARKDOWN_RENDERER_PROPERTY_TEST_BUDGET=scheduled</c> to run every explicit seed.
/// Each generated case remains replayable by its seed and index through <see cref="MarkdownGenerator.GetCase"/>.
/// </summary>
[TestClass]
public class PropertyBasedMarkdownSuiteTests : ConsoleTestBase
{
    [Timeout(TestTimeouts.Suite)]
    [TestMethod]
    [DynamicData(nameof(PropertyBasedMarkdownTestCases.GetGeneratedCases), typeof(PropertyBasedMarkdownTestCases), DynamicDataDisplayName = nameof(GetCaseDisplayName))]
    public void GeneratedMarkdown_RendersRobustly(int width, string markdown, string caseLabel)
        => AssertGeneratedMarkdownRendersRobustly(width, markdown, caseLabel);

    public static string GetCaseDisplayName(System.Reflection.MethodInfo methodInfo, object?[] data)
        => PropertyBasedMarkdownTestCases.GetCaseDisplayName(methodInfo, data);

    private void AssertGeneratedMarkdownRendersRobustly(int width, string markdown, string caseLabel)
    {
        try
        {
            this.AssertRendersRobustly(markdown, options: null, width, caseLabel);
        }
        catch (Exception exception)
        {
            var smallestFailure = MarkdownCaseShrinker.Shrink(
                markdown,
                candidate => CandidateFails(candidate, width, caseLabel));

            Assert.Fail(
                $"Generated Markdown failed [case: {caseLabel}] [width: {width}] "
                + $"[shrunk input: {Escape(smallestFailure)}]{Environment.NewLine}{exception}");
            throw;
        }
    }

    private bool CandidateFails(string markdown, int width, string caseLabel)
    {
        try
        {
            this.AssertRendersRobustly(markdown, options: null, width, caseLabel);
            return false;
        }
        catch (Exception)
        {
            return true;
        }
    }

    private static string Escape(string markdown)
        => markdown
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
}
