using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre;

[SpectreSourceFile]
public sealed class MarkdownRenderResult
{
    public required IRenderable? Root { get; init; }

    public required IReadOnlyList<LinkItem> Links { get; init; }

    public required IReadOnlySet<Type> UnhandledTypes { get; init; }

    public required IReadOnlySet<UnknownEmphasisDelimiter> UnknownEmphasisDelimiters { get; init; }
}