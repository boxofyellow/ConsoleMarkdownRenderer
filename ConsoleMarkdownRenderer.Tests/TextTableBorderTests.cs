using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
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

        [TestMethod]
        public void TextTableBorder_DefaultsToSquare()
        {
            Assert.AreEqual(TextTableBorder.Square, new DisplayOptions().TableBorder,
                "TableBorder should default to Square to preserve current behavior.");
        }

        [TestMethod]
        public void TextTableBorder_MapsToSpectreNamedBorder()
        {
            // Each named TextTableBorder should map to the like-named static
            // Spectre.Console.TableBorder instance via DisplayOptions.ToSpectreDisplayOptions().
            foreach (TextTableBorder border in Enum.GetValues<TextTableBorder>())
            {
                var spectreOptions = new DisplayOptions { TableBorder = border }.ToSpectreDisplayOptions();
                var expected = (TableBorder)typeof(TableBorder)
                    .GetProperty(border.ToString(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
                    .GetValue(null)!;
                Assert.AreSame(expected, spectreOptions.TableBorder,
                    $"TextTableBorder.{border} should map to Spectre.Console.TableBorder.{border}.");
            }
        }
    }
}
