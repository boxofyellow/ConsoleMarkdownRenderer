using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="PromptResult"/> and <see cref="PromptResultKind"/>
    /// </summary>
    [TestClass]
    public class PromptResultTests
    {
        // ---------------------------------------------------------------------------
        // Helper Methods
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Creates a PromptResult based on the given kind.
        /// </summary>
        private static PromptResult CreatePromptResult(PromptResultKind kind) => kind switch
        {
            PromptResultKind.Done => PromptResult.CreateDone(),
            PromptResultKind.Back => PromptResult.CreateBack(),
            _ => throw new ArgumentException($"Unsupported kind for this helper: {kind}")
        };

        // ---------------------------------------------------------------------------
        // CreateDone/CreateBack Tests (Parameterized)
        // ---------------------------------------------------------------------------

        [TestMethod]
        [DataRow("Done")]
        [DataRow("Back")]
        public void CreateDoneOrBack_KindIsCorrect(string kindName)
        {
            var kind = Enum.Parse<PromptResultKind>(kindName);
            var result = CreatePromptResult(kind);
            Assert.AreEqual(kind, result.Kind);
        }

        [TestMethod]
        [DataRow("Done")]
        [DataRow("Back")]
        public void CreateDoneOrBack_AccessingLinkItemThrowsNullReferenceException(string kindName)
        {
            var kind = Enum.Parse<PromptResultKind>(kindName);
            var result = CreatePromptResult(kind);
            var ex = Assert.ThrowsExactly<NullReferenceException>(() => _ = result.LinkItem);
            StringAssert.Contains(ex.Message, kindName);
        }

        [TestMethod]
        [DataRow("Done")]
        [DataRow("Back")]
        public void CreateDoneOrBack_ToDisplayStringReturnsExpected(string kindName)
        {
            var kind = Enum.Parse<PromptResultKind>(kindName);
            var result = CreatePromptResult(kind);
            // The display string matches the kind name
            Assert.AreEqual(kindName, result.ToDisplayString());
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
            var expected = Spectre.Console.Markup.Escape(linkItem.ToString());
            Assert.AreEqual(expected, result.ToDisplayString());
        }

        // ---------------------------------------------------------------------------
        // Helper
        // ---------------------------------------------------------------------------

        private static LinkItem NewLinkItem(string url)
            => new(url: url, content: "content");
    }
}
