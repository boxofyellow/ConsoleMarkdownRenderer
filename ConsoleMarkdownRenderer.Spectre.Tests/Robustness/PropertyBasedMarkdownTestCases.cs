using System.Reflection;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

internal static class PropertyBasedMarkdownTestCases
{
    public const string ScheduledFuzzCategory = "ScheduledFuzz";

    private static readonly int[] s_smokeSeeds = [104_729];
    private static readonly int[] s_scheduledSeeds =
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

    public static IReadOnlyList<int> AllSeeds { get; } = s_scheduledSeeds;

    public static string GetCaseDisplayName(MethodInfo methodInfo, object?[] data)
    {
        Assert.IsTrue(data.Length >= 3, "Expected (width, markdown, caseLabel) test data");
        return $"{methodInfo.Name} ({data[2]}, width={data[0]})";
    }

    public static IEnumerable<object[]> GetSmokeCases()
        => GetCases(s_smokeSeeds, casesPerSeed: 1);

    public static IEnumerable<object[]> GetScheduledCases()
        => GetCases(s_scheduledSeeds, MarkdownGenerator.CasesPerSeed);

    private static IEnumerable<object[]> GetCases(IEnumerable<int> seeds, int casesPerSeed)
    {
        foreach (var seed in seeds)
        {
            for (var caseIndex = 0; caseIndex < casesPerSeed; caseIndex++)
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
