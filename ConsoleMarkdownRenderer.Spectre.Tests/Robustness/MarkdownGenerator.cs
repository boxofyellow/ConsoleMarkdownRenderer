using System.Text;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

internal static class MarkdownGenerator
{
    public const int CasesPerSeed = 12;
    public const int MaximumDocumentLength = 1_200;
    public const int MaximumNestingDepth = 3;

    public static IReadOnlyList<GeneratedMarkdownCase> Generate(int seed)
    {
        var random = new DeterministicRandom(unchecked((uint)seed));
        var cases = new List<GeneratedMarkdownCase>(CasesPerSeed);

        for (var caseIndex = 0; caseIndex < CasesPerSeed; caseIndex++)
        {
            var maximumDepth = 0;
            var builder = new StringBuilder();
            var blockCount = random.Next(2, 6);

            for (var blockIndex = 0; blockIndex < blockCount; blockIndex++)
            {
                if (builder.Length > 0)
                {
                    AppendBounded(builder, "\n\n");
                }

                var preferredKind = blockIndex == 0 ? caseIndex % 7 : random.Next(7);
                AppendBounded(builder, GenerateBlock(random, preferredKind, 0, allowWideCharacters: true, ref maximumDepth));
            }

            if (random.Next(4) == 0)
            {
                TruncateLikePartialInput(builder, random);
            }

            cases.Add(new GeneratedMarkdownCase(seed, caseIndex, builder.ToString(), maximumDepth));
        }

        return cases;
    }

    private static string GenerateBlock(
        DeterministicRandom random,
        int kind,
        int depth,
        bool allowWideCharacters,
        ref int maximumDepth)
    {
        maximumDepth = Math.Max(maximumDepth, depth);

        return kind switch
        {
            0 => GenerateParagraph(random, allowWideCharacters),
            1 => GenerateQuote(random, depth, allowWideCharacters, ref maximumDepth),
            2 => GenerateList(random, depth, ref maximumDepth),
            3 => GenerateFencedCodeBlock(random),
            4 => GenerateTable(random, allowWideCharacters),
            5 => GenerateHtml(random, allowWideCharacters),
            _ => GenerateContainer(random, depth, allowWideCharacters, ref maximumDepth),
        };
    }

    private static string GenerateParagraph(DeterministicRandom random, bool allowWideCharacters)
    {
        var fragmentCount = random.Next(2, 5);
        var fragments = Enumerable.Range(0, fragmentCount)
            .Select(_ => GenerateInline(random, allowWideCharacters));

        return string.Join(" ", fragments);
    }

    private static string GenerateQuote(
        DeterministicRandom random,
        int depth,
        bool allowWideCharacters,
        ref int maximumDepth)
    {
        var content = depth < MaximumNestingDepth
            ? GenerateBlock(random, random.Next(7), depth + 1, allowWideCharacters, ref maximumDepth)
            : GenerateParagraph(random, allowWideCharacters);

        return string.Join("\n", content.Split('\n').Select(line => $"> {line}"));
    }

    private static string GenerateList(DeterministicRandom random, int depth, ref int maximumDepth)
    {
        var itemCount = random.Next(1, 4);
        var lines = Enumerable.Range(0, itemCount)
            .Select(index => $"- {GenerateInline(random, allowWideCharacters: false)}")
            .ToList();

        if (depth < MaximumNestingDepth && random.Next(2) == 0)
        {
            var nested = GenerateQuote(random, depth + 1, allowWideCharacters: false, ref maximumDepth);
            lines.AddRange(nested.Split('\n').Select(line => $"  {line}"));
        }

        return string.Join("\n", lines);
    }

    private static string GenerateFencedCodeBlock(DeterministicRandom random)
    {
        var fence = random.Next(2) == 0 ? "```" : "~~~";
        var info = random.Next(3) switch
        {
            0 => string.Empty,
            1 => "csharp",
            _ => "json",
        };
        var body = random.Next(3) switch
        {
            0 => string.Empty,
            1 => "value = [red]",
            _ => "Console.WriteLine(\"text\");",
        };

        return random.Next(3) == 0
            ? $"{fence}{info}\n{body}"
            : $"{fence}{info}\n{body}\n{fence}";
    }

    private static string GenerateTable(DeterministicRandom random, bool allowWideCharacters)
    {
        var header = $"| {GenerateInline(random, allowWideCharacters)} | {GenerateInline(random, allowWideCharacters)} |";
        var row = random.Next(3) switch
        {
            0 => $"| {GenerateInline(random, allowWideCharacters)} |",
            1 => $"| {GenerateInline(random, allowWideCharacters)} | {GenerateInline(random, allowWideCharacters)} |",
            _ => $"| {GenerateInline(random, allowWideCharacters)} | {GenerateInline(random, allowWideCharacters)} | extra |",
        };

        return $"{header}\n|---|---|\n{row}";
    }

    private static string GenerateHtml(DeterministicRandom random, bool allowWideCharacters)
    {
        var content = GenerateInline(random, allowWideCharacters);
        return random.Next(3) switch
        {
            0 => $"<span class=\"x\">{content}</span>",
            1 => $"<div>\n{content}\n</div>",
            _ => $"<span>{content}",
        };
    }

    private static string GenerateContainer(
        DeterministicRandom random,
        int depth,
        bool allowWideCharacters,
        ref int maximumDepth)
    {
        var content = depth < MaximumNestingDepth
            ? GenerateBlock(random, random.Next(7), depth + 1, allowWideCharacters, ref maximumDepth)
            : GenerateParagraph(random, allowWideCharacters);

        return random.Next(3) == 0
            ? $":::note\n{content}"
            : $":::note\n{content}\n:::";
    }

    private static string GenerateInline(DeterministicRandom random, bool allowWideCharacters)
    {
        var text = GenerateText(random, allowWideCharacters);
        return random.Next(9) switch
        {
            0 => text,
            1 => $"*{text}*",
            2 => random.Next(2) == 0 ? $"**{text}**" : $"**{text}",
            3 => random.Next(2) == 0 ? $"[{text}](https://example.test/{random.Next(100)})" : $"[{text}](",
            4 => random.Next(2) == 0 ? $"`{text}`" : $"`{text}",
            5 => $"\\*{text}\\*",
            6 => $"[red]{text}[/]",
            7 => $"![{text}](image.png)",
            _ => $"<{text}>",
        };
    }

    private static string GenerateText(DeterministicRandom random, bool allowWideCharacters)
    {
        var safeText = new[]
        {
            "alpha",
            "bracket[",
            "slash\\",
            "e\u0301",
            "space text",
        };
        var wideText = new[]
        {
            "漢字",
            "emoji 👩🏽‍💻",
            "wide ＡＢＣ",
        };

        var choices = allowWideCharacters && random.Next(3) == 0 ? wideText : safeText;
        return choices[random.Next(choices.Length)];
    }

    private static void AppendBounded(StringBuilder builder, string text)
    {
        var remaining = MaximumDocumentLength - builder.Length;
        if (remaining <= 0)
        {
            return;
        }

        if (text.Length <= remaining)
        {
            builder.Append(text);
            return;
        }

        var length = remaining;
        if (length > 0 && char.IsHighSurrogate(text[length - 1]))
        {
            length--;
        }

        builder.Append(text, 0, length);
    }

    private static void TruncateLikePartialInput(StringBuilder builder, DeterministicRandom random)
    {
        if (builder.Length < 2)
        {
            return;
        }

        var length = random.Next(1, builder.Length);
        if (char.IsHighSurrogate(builder[length - 1]))
        {
            length--;
        }

        if (length > 0)
        {
            builder.Length = length;
        }
    }

    private sealed class DeterministicRandom(uint state)
    {
        private uint _state = state == 0 ? 0x6D2B79F5 : state;

        public int Next(int exclusiveMaximum)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveMaximum);

            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (int)(_state % (uint)exclusiveMaximum);
        }

        public int Next(int inclusiveMinimum, int exclusiveMaximum)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(inclusiveMinimum, exclusiveMaximum);
            return inclusiveMinimum + Next(exclusiveMaximum - inclusiveMinimum);
        }
    }
}

internal sealed record GeneratedMarkdownCase(int Seed, int Index, string Markdown, int MaximumNestingDepth)
{
    public string Label => $"seed={Seed}, case={Index}";
}
