namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[SpectreSourceFile]
public sealed class NamedTypeCollection<T>
    where T : notnull
{
    public NamedTypeCollection(IEnumerable<(string Name, T Value)> items)
    {
        NameMap = new(items, StringComparer.OrdinalIgnoreCase);
        TypeNameMap = new(NameMap.Forward.Select(kvp => (kvp.Value.GetType().Name, kvp.Value)), StringComparer.OrdinalIgnoreCase);
    }

    public readonly BidirectionalMap<string, T> NameMap;
    public readonly BidirectionalMap<string, T> TypeNameMap;
}