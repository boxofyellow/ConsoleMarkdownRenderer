using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="TextTableBorder"/> to ensure it stays in sync with the static
    /// <see cref="Spectre.Console.TableBorder"/> instances exposed by Spectre.Console.
    /// </summary>
    [TestClass]
    public class TextTableBorderTests
    {
        /// <summary>
        /// Verifies that every static <see cref="Spectre.Console.TableBorder"/> property has a
        /// corresponding value in our <see cref="TextTableBorder"/> enum. If Spectre adds a new
        /// named border, this test will fail to remind us to update.
        /// </summary>
        [TestMethod]
        public void TextTableBorder_HasMatchingSpectreTableBorderProperties()
        {
            var spectreBorderPropertyNames = typeof(TableBorder)
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Where(p => p.PropertyType == typeof(TableBorder))
                .Select(p => p.Name)
                .ToList();

            var textTableBorderValues = Enum.GetNames<TextTableBorder>().ToList();

            var missing = spectreBorderPropertyNames
                .Where(name => !textTableBorderValues.Contains(name))
                .ToList();

            if (missing.Count > 0)
            {
                Assert.Fail(
                    $"TextTableBorder is missing values for the following static Spectre.Console.TableBorder properties: {string.Join(", ", missing)}");
            }
        }
    }
}
