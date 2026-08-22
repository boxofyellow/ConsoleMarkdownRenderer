namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Expanded deterministic property-based coverage for the scheduled fuzz workflow. Invoke it with
/// <c>--filter TestCategory=ScheduledFuzz</c>; normal CI excludes this category.
/// </summary>
[TestClass]
public class PropertyBasedMarkdownScheduledSuiteTests : PropertyBasedMarkdownSuiteTestBase
{
    [Timeout(TestTimeouts.Suite)]
    [TestCategory(PropertyBasedMarkdownTestCases.ScheduledFuzzCategory)]
    [TestMethod]
    [DynamicData(nameof(PropertyBasedMarkdownTestCases.GetScheduledCases), typeof(PropertyBasedMarkdownTestCases), DynamicDataDisplayName = nameof(GetCaseDisplayName))]
    public void GeneratedMarkdown_RendersRobustly(int width, string markdown, string caseLabel)
        => AssertGeneratedMarkdownRendersRobustly(width, markdown, caseLabel);

    public static string GetCaseDisplayName(System.Reflection.MethodInfo methodInfo, object?[] data)
        => PropertyBasedMarkdownTestCases.GetCaseDisplayName(methodInfo, data);
}
