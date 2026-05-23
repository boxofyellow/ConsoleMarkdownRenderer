namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Shared helpers for tests that verify one enum stays in sync with another.
    /// </summary>
    internal static class EnumCoverage
    {
        /// <summary>
        /// Asserts that every name defined on <typeparamref name="TSource"/> (other than the
        /// provided <paramref name="exclusions"/>) also appears on <typeparamref name="TTarget"/>.
        /// Used by tests that ensure our locally-defined styling enums keep up with the Spectre.Console
        /// enums they mirror.
        /// </summary>
        public static void ValidateEnumCoverage<TSource, TTarget>(params string[] exclusions)
            where TSource : struct, Enum
            where TTarget : struct, Enum
        {
            var sourceValues = Enum.GetNames<TSource>()
                .Where(name => !exclusions.Contains(name))
                .ToList();

            var targetValues = Enum.GetNames<TTarget>()
                .Where(name => !exclusions.Contains(name))
                .ToList();

            var missing = sourceValues
                .Where(name => !targetValues.Contains(name))
                .ToList();

            if (missing.Count > 0)
            {
                Assert.Fail(
                    $"{typeof(TTarget).Name} is missing the following values from {typeof(TSource).Name}: {string.Join(", ", missing)}");
            }
        }
    }
}
