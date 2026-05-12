using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="TextJustification"/> to ensure it stays in sync with
    /// Spectre.Console's <see cref="Justify"/> enum. The pattern mirrors
    /// <see cref="TextDecorationTests"/>.
    /// </summary>
    [TestClass]
    public class TextJustificationTests
    {
        /// <summary>
        /// Verifies that every value of Spectre.Console's <see cref="Justify"/> enum has a
        /// corresponding value in our <see cref="TextJustification"/> enum. If Spectre adds
        /// new justifications, this test will fail to remind us to update.
        /// </summary>
        [TestMethod]
        public void TextJustification_HasAllSpectreJustifyValues()
        {
            var spectreNames = Enum.GetNames(typeof(Justify)).ToList();
            var ourNames = Enum.GetNames(typeof(TextJustification)).ToList();

            var missing = spectreNames.Where(name => !ourNames.Contains(name)).ToList();

            if (missing.Count > 0)
            {
                Assert.Fail(
                    $"TextJustification is missing the following values from Spectre.Console.Justify: {string.Join(", ", missing)}");
            }
        }

        [TestMethod]
        [DataRow(TextJustification.Left,   Justify.Left)]
        [DataRow(TextJustification.Right,  Justify.Right)]
        [DataRow(TextJustification.Center, Justify.Center)]
        public void TextJustification_ConvertsToSpectreJustify(TextJustification source, Justify expected)
        {
            Assert.AreEqual(expected, source.ToSpectreJustify());
        }
    }
}
