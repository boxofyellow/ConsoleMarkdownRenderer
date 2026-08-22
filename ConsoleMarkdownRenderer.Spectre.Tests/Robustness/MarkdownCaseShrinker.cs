namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

internal static class MarkdownCaseShrinker
{
    private const int MaximumAttempts = 24;

    public static string Shrink(string markdown, Func<string, bool> stillFails)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(stillFails);

        var smallest = markdown;
        var attempts = 0;
        var madeProgress = true;

        while (madeProgress && attempts < MaximumAttempts)
        {
            madeProgress = false;

            foreach (var candidate in GetCandidates(smallest))
            {
                if (attempts++ >= MaximumAttempts)
                {
                    break;
                }

                if (candidate.Length < smallest.Length && stillFails(candidate))
                {
                    smallest = candidate;
                    madeProgress = true;
                    break;
                }
            }
        }

        return smallest;
    }

    private static IEnumerable<string> GetCandidates(string markdown)
    {
        var lines = markdown.Split('\n');
        if (lines.Length > 1)
        {
            for (var index = 0; index < lines.Length; index++)
            {
                yield return string.Join("\n", lines.Where((_, lineIndex) => lineIndex != index));
            }
        }

        for (var chunkLength = markdown.Length / 2; chunkLength > 0; chunkLength /= 2)
        {
            for (var start = 0; start + chunkLength <= markdown.Length; start += chunkLength)
            {
                yield return markdown.Remove(start, chunkLength);
            }
        }
    }
}
