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
        /// Creates a PromptResult based on the given kind string.
        /// </summary>
        private static PromptResult CreatePromptResult(string kindName) => kindName switch
        {
            "Done" => PromptResult.CreateDone(),
            "Back" => PromptResult.CreateBack(),
            _ => throw new ArgumentException($"Unsupported kind for this helper: {kindName}")
        };

        /// <summary>
        /// Gets the PromptResultKind from a string name.
        /// </summary>
        private static PromptResultKind GetPromptResultKind(string kindName) => kindName switch
        {
            "Done" => PromptResultKind.Done,
            "Back" => PromptResultKind.Back,
            _ => throw new ArgumentException($"Unsupported kind: {kindName}")
        };

        // ---------------------------------------------------------------------------
        // CreateDone/CreateBack Tests (Parameterized)
        // ---------------------------------------------------------------------------

        [TestMethod]
        [DataRow("Done")]
        [DataRow("Back")]
        public void CreateDoneOrBack_KindIsCorrect(string kindName)
        {
            var result = CreatePromptResult(kindName);
            Assert.AreEqual(GetPromptResultKind(kindName), result.Kind);
        }

        [TestMethod]
        [DataRow("Done")]
        [DataRow("Back")]
        public void CreateDoneOrBack_AccessingLinkItemThrowsNullReferenceException(string kindName)
        {
            var result = CreatePromptResult(kindName);
            var ex = Assert.ThrowsExactly<NullReferenceException>(() => _ = result.LinkItem);
            StringAssert.Contains(ex.Message, kindName);
        }

        [TestMethod]
        [DataRow("Done", "Done")]
        [DataRow("Back", "Back")]
        public void CreateDoneOrBack_ToDisplayStringReturnsExpected(string kindName, string expected)
        {
            var result = CreatePromptResult(kindName);
            Assert.AreEqual(expected, result.ToDisplayString());
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
