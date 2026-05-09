using ConsoleMarkdownRenderer.Styling;
using Spectre.Console;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="TextDecoration"/> and <see cref="NamedColor"/> to ensure they stay in sync with Spectre.Console types.
    /// </summary>
    [TestClass]
    public class TextDecorationTests
    {
        /// <summary>
        /// Verifies that every named value in Spectre.Console's Decoration enum
        /// has a corresponding value in our TextDecoration enum (excluding None).
        /// If Spectre adds new decorations, this test will fail to remind us to update.
        /// </summary>
        [TestMethod]
        public void TextDecoration_HasAllSpectreDecorationValues()
        {
            ValidateEnumCoverage<Decoration, TextDecoration>("None");
        }

        /// <summary>
        /// Verifies that every NamedColor value has a corresponding static property on Spectre.Console.Color.
        /// If we add a new NamedColor, this test will fail if there's no matching Spectre Color property.
        /// </summary>
        [TestMethod]
        public void NamedColor_HasMatchingSpectreColorProperties()
        {
            var spectreColorPropertyNames = typeof(Color)
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(Color))
                .Select(p => p.Name)
                .ToList();

            var namedColorValues = Enum.GetNames(typeof(NamedColor)).ToList();

            var missing = namedColorValues
                .Where(name => !spectreColorPropertyNames.Contains(name))
                .ToList();

            if (missing.Count > 0)
            {
                Assert.Fail(
                    $"NamedColor has values with no matching static property on Spectre.Console.Color: {string.Join(", ", missing)}");
            }
        }

        private static void ValidateEnumCoverage<TSource, TTarget>(params string[] exclusions) where TSource : struct, Enum where TTarget : struct, Enum
        {
            var sourceValues = Enum.GetNames(typeof(TSource))
                .Where(name => !exclusions.Contains(name))
                .ToList();

            var targetValues = Enum.GetNames(typeof(TTarget))
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
