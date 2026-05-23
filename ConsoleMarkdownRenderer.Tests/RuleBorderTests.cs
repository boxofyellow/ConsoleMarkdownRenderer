using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="RuleBorder"/> to ensure it stays in sync with the static
    /// <see cref="Spectre.Console.BoxBorder"/> instances exposed by Spectre.Console.
    /// </summary>
    [TestClass]
    public class RuleBorderTests
    {
        /// <summary>
        /// Verifies that every static <see cref="Spectre.Console.BoxBorder"/> property has a
        /// corresponding value in our <see cref="RuleBorder"/> enum. If Spectre adds a new
        /// named border, this test will fail to remind us to update.
        /// </summary>
        [TestMethod]
        public void RuleBorder_HasMatchingSpectreBoxBorderProperties()
        {
            var spectreBorderPropertyNames = typeof(BoxBorder)
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(BoxBorder))
                .Select(p => p.Name)
                .ToList();

            var ruleBorderValues = Enum.GetNames(typeof(RuleBorder)).ToList();

            var missing = spectreBorderPropertyNames
                .Where(name => !ruleBorderValues.Contains(name))
                .ToList();

            if (missing.Count > 0)
            {
                Assert.Fail(
                    $"RuleBorder is missing values for the following static Spectre.Console.BoxBorder properties: {string.Join(", ", missing)}");
            }
        }
    }
}
