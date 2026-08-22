using System.Reflection;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Fast, deterministic property-based coverage for normal CI. The selected case is replayable from
/// its seed and index; the larger <see cref="PropertyBasedMarkdownScheduledSuiteTests"/> is excluded
/// from the normal CI filter and runs under the <c>ScheduledFuzz</c> test category.
/// </summary>
[TestClass]
public class PropertyBasedMarkdownSmokeSuiteTests : PropertyBasedMarkdownSuiteTestBase
{
    [Timeout(TestTimeouts.Suite)]
    [TestMethod]
    [DynamicData(nameof(PropertyBasedMarkdownTestCases.GetSmokeCases), typeof(PropertyBasedMarkdownTestCases), DynamicDataDisplayName = nameof(GetCaseDisplayName))]
    public void GeneratedMarkdown_RendersRobustly(int width, string markdown, string caseLabel)
        => AssertGeneratedMarkdownRendersRobustly(width, markdown, caseLabel);

    public static string GetCaseDisplayName(System.Reflection.MethodInfo methodInfo, object?[] data)
        => PropertyBasedMarkdownTestCases.GetCaseDisplayName(methodInfo, data);
}
