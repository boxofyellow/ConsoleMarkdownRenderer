using System;
using System.Linq;
using ConsoleMarkdownRenderer.Styling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectre.Console;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="TextDecoration"/> to ensure it stays in sync with Spectre.Console's <see cref="Decoration"/>.
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
            var spectreValues = Enum.GetNames(typeof(Decoration))
                .Where(name => name != nameof(Decoration.None))
                .ToList();

            var textDecorationValues = Enum.GetNames(typeof(TextDecoration))
                .Where(name => name != nameof(TextDecoration.None))
                .ToList();

            var missing = spectreValues
                .Where(name => !textDecorationValues.Contains(name))
                .ToList();

            if (missing.Count > 0)
            {
                Assert.Fail(
                    $"TextDecoration is missing the following values from Spectre.Console.Decoration: {string.Join(", ", missing)}");
            }
        }
    }
}
