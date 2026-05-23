using Spectre.Console;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="PromptResult"/> and <see cref="PromptResultKind"/>
    /// </summary>
    [TestClass]
    public class PromptResultTests
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

            Assert.AreEqual(kind, result.Kind, "Kind should match the one used to create the result.");
            Assert.AreEqual(kindName, result.ToDisplayString(), "Display string should match the kind name.");
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
            Assert.AreEqual(PromptResultKind.Link, result.Kind);
        }

        [TestMethod]
        public void CreateLink_LinkItemIsReturned()
        {
            var linkItem = NewLinkItem("https://example.com");
            var result = PromptResult.CreateLink(linkItem);
            Assert.AreSame(linkItem, result.LinkItem);
        }

        [TestMethod]
        public void CreateLink_ToDisplayStringReturnsEscapedLinkItemString()
        {
            var linkItem = NewLinkItem("https://example.com");
            var result = PromptResult.CreateLink(linkItem);
            // Markup.Escape replaces '[' and ']' with "[[" and "]]"
            var expected = Markup.Escape(linkItem.ToString());
            Assert.AreEqual(expected, result.ToDisplayString());
        }

        // ---------------------------------------------------------------------------
        // Helper
        // ---------------------------------------------------------------------------

        private static LinkItem NewLinkItem(string url)
            => new(url: url, content: "content");
    }
}
