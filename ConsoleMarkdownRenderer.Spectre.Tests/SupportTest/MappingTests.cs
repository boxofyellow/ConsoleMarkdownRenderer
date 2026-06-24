using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class MappingTests
    {
        [TestMethod]
        public void Named_Colors_Match()
        {
            foreach (var kvp in Mappings.Colors.Forward)
            {
                var name = kvp.Key;
                var color = kvp.Value;

                var lookup = Color.FromName(name);
                Assert.IsNotNull(lookup, $"Extra color '{name}'.");
                TestUtilities.AssertTheseMatch(color, lookup, shouldMatch: true, $"Color '{name}' does not match.");
            }
        }
    }
}