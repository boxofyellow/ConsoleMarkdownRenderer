using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    /// <summary>
    /// Identifies an emphasis inline whose <c>DelimiterChar</c> / <c>DelimiterCount</c>
    /// combination fell into the catch-all branch of
    /// <see cref="ObjectRenderers.ConsoleEmphasisInlineRenderer"/>.
    /// </summary>
    [SpectreSourceFile]
    public sealed class UnknownEmphasisDelimiter : IEquatable<UnknownEmphasisDelimiter>
    {
        public UnknownEmphasisDelimiter(char delimiterChar, int delimiterCount)
        {
            DelimiterChar = delimiterChar;
            DelimiterCount = delimiterCount;
        }

        /// <summary>The unrecognized delimiter character.</summary>
        public char DelimiterChar { get; }

        /// <summary>The number of times the delimiter character was repeated.</summary>
        public int DelimiterCount { get; }

        public override string ToString() => $"({DelimiterChar}{DelimiterCount})";

        public bool Equals(UnknownEmphasisDelimiter? other)
            => other is not null && DelimiterChar == other.DelimiterChar && DelimiterCount == other.DelimiterCount;

        public override bool Equals(object? obj) => Equals(obj as UnknownEmphasisDelimiter);

        public override int GetHashCode() => HashCode.Combine(DelimiterChar, DelimiterCount);
    }
}
