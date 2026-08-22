using System.Reflection;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Applies a compact, deterministic set of damaging mutations to every Markdown resource. Unlike
/// <see cref="TruncationSuiteTests"/>, which only uses prefixes, these mutations change the middle
/// of complete documents so parser and renderer assumptions about paired delimiters, surrounding
/// lines, markup, nesting, and empty content are exercised against the maintained fixture corpus.
/// Each fixture contributes at most one case from each family; fixed selections from the sorted
/// corpus keep every mutation variant represented without making the CI matrix unbounded.
/// </summary>
[TestClass]
public class CorpusMutationSuiteTests : ConsoleTestBase
{
    private const int MaxMutationsPerFixture = 5;
    private const string ResourcesDirectory = "resources";

    // A narrow-width renderer regression can otherwise leave a test host stuck until the overall
    // test-run timeout. Individual mutations normally finish in milliseconds, so this surfaces the
    // responsible fixture/family label promptly without making healthy CI runs time-sensitive.
    [Timeout(30_000)]
    [TestMethod]
    [DynamicData(nameof(GetCases), DynamicDataDisplayName = nameof(GetCaseDisplayName))]
    public void MutatedCorpus_RendersRobustly(int width, string markdown, string caseLabel)
        => this.AssertRendersRobustly(markdown, options: null, width, caseLabel);

    public static string GetCaseDisplayName(MethodInfo methodInfo, object?[] data)
    {
        Assert.IsTrue(data.Length >= 3, "Expected (width, markdown, caseLabel) test data");
        return $"{methodInfo.Name} ({data[2]}, width={data[0]})";
    }

    public static IEnumerable<object[]> GetCases()
    {
        foreach (var mutation in BuildMutations())
        {
            foreach (var width in RenderRobustness.Widths)
            {
                yield return [width, mutation.Markdown, mutation.Label];
            }
        }
    }

    private static IEnumerable<MutationCase> BuildMutations()
    {
        foreach (var (fixture, fixtureIndex) in LoadFixtures().Select((fixture, index) => (fixture, index)))
        {
            var mutations = BuildFixtureMutations(fixture, fixtureIndex).ToList();
            if (mutations.Count > MaxMutationsPerFixture)
            {
                throw new InvalidOperationException(
                    $"Fixture '{fixture.Name}' produced {mutations.Count} mutations, exceeding the {MaxMutationsPerFixture} case limit.");
            }

            foreach (var mutation in mutations)
            {
                yield return mutation;
            }
        }
    }

    private static IEnumerable<MutationCase> BuildFixtureMutations(Fixture fixture, int fixtureIndex)
    {
        var markdown = NormalizeLineEndings(fixture.Markdown);

        var delimiterMutation = DamageFirstDelimiter(markdown, fixtureIndex);
        if (delimiterMutation is not null)
        {
            yield return CreateCase(
                fixture.Name,
                "DelimiterDamage",
                delimiterMutation.Value.Name,
                delimiterMutation.Value.Markdown);
        }

        var lines = SplitLines(markdown);
        var nonEmptyLineIndexes = lines
            .Select((line, index) => (line, index))
            .Where(item => !string.IsNullOrWhiteSpace(item.line))
            .Select(item => item.index)
            .ToList();

        if (nonEmptyLineIndexes.Count > 0)
        {
            yield return CreateCase(
                fixture.Name,
                "LineRemoval",
                "LastNonEmptyLine",
                RemoveLine(lines, nonEmptyLineIndexes[^1]));
        }

        var insertionIndex = GetContentInsertionIndex(lines);
        var injection = s_markupInjections[fixtureIndex % s_markupInjections.Length];
        yield return CreateCase(
            fixture.Name,
            "MarkupInjection",
            injection.Name,
            markdown.Insert(insertionIndex, injection.Text));

        var nesting = fixtureIndex == 0
            ? ("ListItem", WrapInListItem(lines))
            : ((fixtureIndex - 1) % 3) switch
        {
            0 => ("Quote", WrapInQuote(lines)),
            1 => ("TableCell", WrapInTableCell(markdown)),
            _ => ("CustomContainer", $":::note\n{markdown}\n:::\n"),
        };
        yield return CreateCase(fixture.Name, "Nesting", nesting.Item1, nesting.Item2);

        var contentLineIndex = GetContentLineIndex(lines);
        var replacement = fixtureIndex % 2 == 0 ? string.Empty : " \t";
        var replacementName = replacement.Length == 0 ? "EmptyContentLine" : "WhitespaceOnlyContentLine";
        yield return CreateCase(fixture.Name, "Emptiness", replacementName, ReplaceLine(lines, contentLineIndex, replacement));
    }

    private static MutationCase CreateCase(string fixtureName, string family, string mutationName, string markdown)
        => new($"{fixtureName}/{family}/{mutationName}", markdown);

    private static (string Name, string Markdown)? DamageFirstDelimiter(string markdown, int fixtureIndex)
    {
        var delimiter = s_structuralDelimiters
            .Select(candidate => (Candidate: candidate, Index: markdown.IndexOf(candidate.Token, StringComparison.Ordinal)))
            .Where(match => match.Index >= 0)
            .OrderBy(match => match.Index)
            .ThenByDescending(match => match.Candidate.Token.Length)
            .FirstOrDefault();

        if (delimiter.Candidate.Token is null)
        {
            return null;
        }

        return (fixtureIndex % 3) switch
        {
            0 => (
                "Delete",
                markdown.Remove(delimiter.Index, delimiter.Candidate.Token.Length)),
            1 => (
                "Duplicate",
                markdown.Insert(delimiter.Index, delimiter.Candidate.Token)),
            _ => (
                "Substitute",
                markdown.Remove(delimiter.Index, delimiter.Candidate.Token.Length)
                    .Insert(delimiter.Index, delimiter.Candidate.Replacement)),
        };
    }

    private static int GetContentInsertionIndex(IReadOnlyList<string> lines)
    {
        var lineIndex = GetContentLineIndex(lines);
        var offset = lines.Take(lineIndex).Sum(line => line.Length + 1);
        return offset + (lines[lineIndex].Length / 2);
    }

    private static int GetContentLineIndex(IReadOnlyList<string> lines)
    {
        var contentLine = lines
            .Select((line, index) => (line, index))
            .FirstOrDefault(item => !string.IsNullOrWhiteSpace(item.line) && !IsStandaloneDelimiter(item.line));

        if (contentLine.line is not null)
        {
            return contentLine.index;
        }

        return lines
            .Select((line, index) => (line, index))
            .First(item => !string.IsNullOrWhiteSpace(item.line))
            .index;
    }

    private static bool IsStandaloneDelimiter(string line)
    {
        var trimmed = line.Trim();
        return trimmed is "---" or "$$" or ":::"
            || trimmed.StartsWith("```", StringComparison.Ordinal)
            || trimmed.StartsWith("~~~", StringComparison.Ordinal);
    }

    private static string WrapInQuote(IEnumerable<string> lines)
        => string.Join("\n", lines.Select(line => $"> {line}"));

    private static string WrapInListItem(IReadOnlyList<string> lines)
        => $"- {lines[0]}\n{string.Join("\n", lines.Skip(1).Select(line => $"  {line}"))}";

    private static string WrapInTableCell(string markdown)
        => $"| mutation |\n|-|\n| {markdown.Replace("|", "\\|", StringComparison.Ordinal).Replace("\n", "<br>", StringComparison.Ordinal)} |";

    private static string RemoveLine(IReadOnlyList<string> lines, int lineIndex)
        => string.Join("\n", lines.Where((_, index) => index != lineIndex));

    private static string ReplaceLine(IReadOnlyList<string> lines, int lineIndex, string replacement)
        => string.Join("\n", lines.Select((line, index) => index == lineIndex ? replacement : line));

    private static List<string> SplitLines(string markdown)
        => markdown.Split('\n').ToList();

    private static string NormalizeLineEndings(string markdown)
        => markdown.Replace("\r\n", "\n", StringComparison.Ordinal);

    private static IEnumerable<Fixture> LoadFixtures()
    {
        var assembly = typeof(CorpusMutationSuiteTests).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(resourceName => Path.GetDirectoryName(resourceName) == ResourcesDirectory)
            .Where(resourceName => Path.GetExtension(resourceName) == ".md")
            .OrderBy(resourceName => resourceName, StringComparer.Ordinal);

        foreach (var resourceName in resourceNames)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Failed to find resource for {resourceName}");
            using var reader = new StreamReader(stream);
            yield return new Fixture(Path.GetFileNameWithoutExtension(resourceName), reader.ReadToEnd());
        }
    }

    private sealed record Fixture(string Name, string Markdown);
    private sealed record MutationCase(string Label, string Markdown);

    private static readonly (string Token, string Replacement)[] s_structuralDelimiters =
    [
        (":::", "::"),
        ("$$",  "$"),
        ("---", "--"),
        ("`",   "~"),
        ("|",   ";"),
        ("*",   "_"),
        ("_",   "*"),
        ("[",   "{"),
        ("]",   "}"),
        ("(",   "{"),
        (")",   "}"),
        ("#",   "!"),
        (">",   "<"),
    ];

    private static readonly (string Name, string Text)[] s_markupInjections =
    [
        ("Name",         "[name]"),
        ("ClosingScope", "[/]"),
        ("ColorScope",   "[red]"),
    ];
}
