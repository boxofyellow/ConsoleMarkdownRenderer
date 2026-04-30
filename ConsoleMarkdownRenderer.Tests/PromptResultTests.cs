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
        // CreateDone
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void CreateDone_KindIsDone()
        {
            var result = PromptResult.CreateDone();
            Assert.AreEqual(PromptResultKind.Done, result.Kind);
        }

        [TestMethod]
        public void CreateDone_AccessingLinkItemThrowsNullReferenceException()
        {
            var result = PromptResult.CreateDone();
            var ex = Assert.ThrowsExactly<NullReferenceException>(() => _ = result.LinkItem);
            StringAssert.Contains(ex.Message, PromptResultKind.Done.ToString());
        }

        [TestMethod]
        public void CreateDone_ToDisplayStringReturnsDone()
        {
            var result = PromptResult.CreateDone();
            Assert.AreEqual("Done", result.ToDisplayString());
        }

        // ---------------------------------------------------------------------------
        // CreateBack
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void CreateBack_KindIsBack()
        {
            var result = PromptResult.CreateBack();
            Assert.AreEqual(PromptResultKind.Back, result.Kind);
        }

        [TestMethod]
        public void CreateBack_AccessingLinkItemThrowsNullReferenceException()
        {
            var result = PromptResult.CreateBack();
            var ex = Assert.ThrowsExactly<NullReferenceException>(() => _ = result.LinkItem);
            StringAssert.Contains(ex.Message, PromptResultKind.Back.ToString());
        }

        [TestMethod]
        public void CreateBack_ToDisplayStringReturnsBack()
        {
            var result = PromptResult.CreateBack();
            Assert.AreEqual("Back", result.ToDisplayString());
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
