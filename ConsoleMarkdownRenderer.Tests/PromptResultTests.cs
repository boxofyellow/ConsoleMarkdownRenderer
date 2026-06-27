using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using SC = Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="PromptResult"/> and <see cref="PromptResultKind"/>
    /// </summary>
    [TestClass]
    public class PromptResultTests : TestBase
    {
        // ---------------------------------------------------------------------------
        // CreateDone/CreateBack Tests (Parameterized)
        // ---------------------------------------------------------------------------

        [TestMethod]
        [DataRow("Done")]
        [DataRow("Back")]
        public void CreateDoneOrBack_KindIsCorrect(string kindName)
        {
            var kind = Enum.Parse<PromptResultKind>(kindName);
            var result = kind switch
            {
                PromptResultKind.Done => PromptResult.CreateDone(),
                PromptResultKind.Back => PromptResult.CreateBack(),
                _ => throw new ArgumentException($"Unsupported kind for this helper: {kind}")
            };

            TestUtilities.AssertTheseMatch(kind, result.Kind, shouldMatch: true, "Kind should match the one used to create the result.");
            TestUtilities.AssertTheseMatch(kindName, result.ToDisplayString(), shouldMatch: true, "Display string should match the kind name.");
            var ex = Assert.ThrowsExactly<NullReferenceException>(() => _ = result.LinkItem, "Accessing LinkItem should throw NullReferenceException for Done/Back results.");
            Assert.Contains(kindName, ex.Message, $"Exception message should contain the kind name '{kindName}' to indicate which result type caused the issue.");
        }

        // ---------------------------------------------------------------------------
        // CreateLink
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void CreateLink_KindIsLink()
        {
            var result = PromptResult.CreateLink(NewLinkItem("https://example.com"));
            TestUtilities.AssertTheseMatch(PromptResultKind.Link, result.Kind, shouldMatch: true);
        }

        [TestMethod]
        public void CreateLink_LinkItemIsReturned()
        {
            var linkItem = NewLinkItem("https://example.com");
            var result = PromptResult.CreateLink(linkItem);
            TestUtilities.AssertTheseMatch(linkItem, result.LinkItem, shouldMatch: true);
        }

        [TestMethod]
        public void CreateLink_ToDisplayStringReturnsEscapedLinkItemString()
        {
            var linkItem = NewLinkItem("https://example.com");
            var result = PromptResult.CreateLink(linkItem);
            // Markup.Escape replaces '[' and ']' with "[[" and "]]"
            var expected = SC.Markup.Escape(linkItem.ToString());
            TestUtilities.AssertTheseMatch(expected, result.ToDisplayString(), shouldMatch: true);
        }

        // ---------------------------------------------------------------------------
        // Helper
        // ---------------------------------------------------------------------------

        private static LinkItem NewLinkItem(string url)
            => new(url: url, content: "content");
    }
}
