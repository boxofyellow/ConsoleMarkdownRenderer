namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

[SpectreSourceFile]
public sealed class BidirectionalMap<TKey, TValue>
    where TKey : notnull
    where TValue : notnull
{
    public BidirectionalMap(
        IEnumerable<(TKey Key, TValue Value)> items,
        IEqualityComparer<TKey>? keyComparer = null,
        IEqualityComparer<TValue>? valueComparer = null,
        bool allowDuplicateValues = false)
    {
        _forward = new(keyComparer);
        _reverse = new(valueComparer);
        foreach (var (key, value) in items)
        {
            _forward.Add(key, value);
            if (allowDuplicateValues)
            {
                _reverse.TryAdd(value, key);
            }
            else
            {
                _reverse.Add(value, key);
            }
        }
    }

    public IReadOnlyDictionary<TKey, TValue> Forward => _forward;
    public IReadOnlyDictionary<TValue, TKey> Reverse => _reverse;

    public TValue GetForward(TKey key, TValue defaultValue) 
        => _forward.TryGetValue(key, out var result)
            ? result
            : defaultValue;

    public TKey GetReverse(TValue value, TKey defaultValue) 
        => _reverse.TryGetValue(value, out var result)
            ? result
            : defaultValue;

    private readonly Dictionary<TKey, TValue> _forward;
    private readonly Dictionary<TValue, TKey> _reverse;
}
