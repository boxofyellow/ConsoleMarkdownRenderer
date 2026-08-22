namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Shared robustness assertion for the smoke and scheduled property-based suites.
/// </summary>
public abstract class PropertyBasedMarkdownSuiteTestBase : ConsoleTestBase
{
    protected void AssertGeneratedMarkdownRendersRobustly(int width, string markdown, string caseLabel)
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
