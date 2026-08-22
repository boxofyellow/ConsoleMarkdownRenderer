using System.Reflection;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

internal static class PropertyBasedMarkdownTestCases
{
    public const string BudgetEnvironmentVariable = "CONSOLE_MARKDOWN_RENDERER_PROPERTY_TEST_BUDGET";
    public const string ScheduledBudget = "scheduled";
    public const int DefaultSeedCount = 2;

    private static readonly int[] s_allSeeds =
    [
        104_729,
        1_618_033,
        190_734_863,
        2_147_483,
        2_718_281,
        3_141_592,
        5_772_156,
        11_235_813,
    ];

    public static IReadOnlyList<int> AllSeeds { get; } = s_allSeeds;

    public static string GetCaseDisplayName(MethodInfo methodInfo, object?[] data)
    {
        Assert.IsTrue(data.Length >= 3, "Expected (width, markdown, caseLabel) test data");
        return $"{methodInfo.Name} ({data[2]}, width={data[0]})";
    }

    public static IEnumerable<object[]> GetGeneratedCases()
        => GetCases(GetSeedsForBudget(Environment.GetEnvironmentVariable(BudgetEnvironmentVariable)));

    public static IReadOnlyList<int> GetSeedsForBudget(string? budget)
    {
        if (string.IsNullOrWhiteSpace(budget))
        {
            return s_allSeeds[..DefaultSeedCount];
        }

        if (string.Equals(budget, ScheduledBudget, StringComparison.OrdinalIgnoreCase))
        {
            return s_allSeeds;
        }

        throw new InvalidOperationException(
            $"Unsupported {BudgetEnvironmentVariable} value '{budget}'. "
            + $"Use '{ScheduledBudget}' or leave the variable unset for the default {DefaultSeedCount}-seed budget.");
    }

    private static IEnumerable<object[]> GetCases(IEnumerable<int> seeds)
    {
        foreach (var seed in seeds)
        {
            for (var caseIndex = 0; caseIndex < MarkdownGenerator.CasesPerSeed; caseIndex++)
            {
                var generated = MarkdownGenerator.GetCase(seed, caseIndex);

                foreach (var width in RenderRobustness.Widths)
                {
                    yield return [width, generated.Markdown, generated.Label];
                }
            }
        }
    }
}
