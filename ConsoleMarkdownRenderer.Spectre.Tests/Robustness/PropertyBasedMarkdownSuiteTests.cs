using System.Reflection;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Runs a small fixed seed set through a grammar-driven Markdown generator. This is a robustness
/// oracle, not an output snapshot: every generated document must render without unhandled nodes or
/// exceptions, write to the console, and render identically a second time through the shared harness.
/// </summary>
[TestClass]
public class PropertyBasedMarkdownSuiteTests : ConsoleTestBase
{
    private static readonly int[] s_seeds = [104_729, 1_618_033, 190_734_863, 2_147_483];

    [Timeout(TestTimeouts.Suite)]
    [TestMethod]
    [DynamicData(nameof(GetCases), DynamicDataDisplayName = nameof(GetCaseDisplayName))]
    public void GeneratedMarkdown_RendersRobustly(int width, string markdown, string caseLabel)
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

    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void Generator_IsDeterministicForFixedSeeds()
    {
        foreach (var seed in s_seeds)
        {
            var first = MarkdownGenerator.Generate(seed);
            var second = MarkdownGenerator.Generate(seed);

            CollectionAssert.AreEqual(
                first.Select(generated => generated.Markdown).ToList(),
                second.Select(generated => generated.Markdown).ToList(),
                $"Generator output changed for seed {seed}.");
        }
    }

    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void Generator_StaysWithinConfiguredBounds()
    {
        foreach (var generated in s_seeds.SelectMany(MarkdownGenerator.Generate))
        {
            Assert.IsTrue(
                generated.Markdown.Length <= MarkdownGenerator.MaximumDocumentLength,
                $"{generated.Label} exceeded the {MarkdownGenerator.MaximumDocumentLength}-character limit.");
            Assert.IsTrue(
                generated.MaximumNestingDepth <= MarkdownGenerator.MaximumNestingDepth,
                $"{generated.Label} exceeded the nesting depth limit.");
        }
    }

    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void Generator_ProducesStructurallyUsableReplayCases()
    {
        foreach (var generated in s_seeds.SelectMany(MarkdownGenerator.Generate))
        {
            Assert.IsFalse(string.IsNullOrEmpty(generated.Markdown), $"{generated.Label} generated an empty document.");
            Assert.IsFalse(generated.Markdown.Contains('\0'), $"{generated.Label} generated a null character.");
            Assert.IsTrue(IsValidUtf16(generated.Markdown), $"{generated.Label} generated invalid UTF-16.");
            Assert.AreEqual(
                generated.Markdown,
                MarkdownGenerator.Generate(generated.Seed)[generated.Index].Markdown,
                $"{generated.Label} cannot be replayed from its seed and index.");
        }
    }

    public static string GetCaseDisplayName(MethodInfo methodInfo, object?[] data)
    {
        Assert.IsTrue(data.Length >= 3, "Expected (width, markdown, caseLabel) test data");
        return $"{methodInfo.Name} ({data[2]}, width={data[0]})";
    }

    public static IEnumerable<object[]> GetCases()
    {
        foreach (var generated in s_seeds.SelectMany(MarkdownGenerator.Generate))
        {
            foreach (var width in RenderRobustness.Widths)
            {
                yield return [width, generated.Markdown, generated.Label];
            }
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

    private static bool IsValidUtf16(string text)
    {
        for (var index = 0; index < text.Length; index++)
        {
            if (char.IsHighSurrogate(text[index]))
            {
                if (index + 1 >= text.Length || !char.IsLowSurrogate(text[++index]))
                {
                    return false;
                }
            }
            else if (char.IsLowSurrogate(text[index]))
            {
                return false;
            }
        }

        return true;
    }

    private static string Escape(string markdown)
        => markdown
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n");
}
