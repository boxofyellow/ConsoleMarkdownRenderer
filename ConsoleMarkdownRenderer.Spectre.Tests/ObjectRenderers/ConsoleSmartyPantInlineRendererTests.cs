using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using Markdig.Extensions.SmartyPants;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class ConsoleSmartyPantInlineRendererTests
    {
        [TestMethod]
        public void RendererTests_SmartyPantInlineRenderer_HandlesAllEnumValues()
        {
            // Look to see if any new SmartyPantType have shown up
            foreach (var value in Enum.GetValues<SmartyPantType>())
            {
                Assert.IsFalse(string.IsNullOrEmpty(ConsoleSmartyPantInlineRenderer.GetReplacement(value)));
            }
        }
    }
}