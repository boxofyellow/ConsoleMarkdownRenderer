using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    [TestClass]
    public class TextJustificationTests : TestBase
    {
        [TestMethod]
        [DataRow(TextJustification.Left, Justify.Left)]
        [DataRow(TextJustification.Right, Justify.Right)]
        [DataRow(TextJustification.Center, Justify.Center)]
        public void TextJustification_ConvertsToSpectreJustify(TextJustification source, Justify expected)
        {
            TestUtilities.AssertTheseMatch(expected, source.ToSpectreJustify(), shouldMatch: true);
            TestUtilities.AssertTheseMatch(source, expected.ToTextJustification(), shouldMatch: true);
        }
    }
}
