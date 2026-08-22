namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Pure generator invariants that protect deterministic seed/index replay for both property suites.
/// </summary>
[TestClass]
public class PropertyBasedMarkdownGeneratorTests
{
    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void Generator_IsDeterministicForFixedSeeds()
    {
        foreach (var seed in PropertyBasedMarkdownTestCases.AllSeeds)
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
        foreach (var generated in PropertyBasedMarkdownTestCases.AllSeeds.SelectMany(MarkdownGenerator.Generate))
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
        foreach (var generated in PropertyBasedMarkdownTestCases.AllSeeds.SelectMany(MarkdownGenerator.Generate))
        {
            Assert.IsFalse(string.IsNullOrEmpty(generated.Markdown), $"{generated.Label} generated an empty document.");
            Assert.IsFalse(generated.Markdown.Contains('\0'), $"{generated.Label} generated a null character.");
            Assert.IsTrue(IsValidUtf16(generated.Markdown), $"{generated.Label} generated invalid UTF-16.");
            Assert.AreEqual(
                generated.Markdown,
                MarkdownGenerator.GetCase(generated.Seed, generated.Index).Markdown,
                $"{generated.Label} cannot be replayed from its seed and index.");
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
}
