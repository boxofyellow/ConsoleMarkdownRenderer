using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleMarkdownRenderer;
using ConsoleMarkdownRenderer.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleMarkdownRenderer.ExampleTests
{
    [TestClass]
    public class FakeMarkdownRendererTests
    {
        [TestMethod]
        public void RenderMarkdown_RecordsCalls()
        {
            // Arrange
            var fake = new FakeMarkdownRenderer();
            var markdown = "# Hello World\nSome text with a [link](https://example.com)";

            // Act
            var result = fake.RenderMarkdown(markdown);

            // Assert
            Assert.AreEqual(1, fake.Calls.Count);
            Assert.AreEqual(markdown, fake.Calls[0].Text);
            Assert.IsNull(fake.Calls[0].Options);
        }

        [TestMethod]
        public void RenderMarkdown_ReturnsConfiguredResult()
        {
            // Arrange
            var fake = new FakeMarkdownRenderer();
            var expectedLinks = new List<MarkdownLink>
            {
                new MarkdownLink("https://example.com", "Example", isImage: false),
            };
            var expectedResult = new MarkdownRenderResult(
                renderedText: "Hello World",
                links: expectedLinks,
                unhandledTypes: null);
            fake.ResultToReturn = expectedResult;

            // Act
            var result = fake.RenderMarkdown("anything");

            // Assert
            Assert.AreSame(expectedResult, result);
            Assert.AreEqual(1, result.Links.Count);
            Assert.AreEqual("https://example.com", result.Links[0].Url);
            Assert.AreEqual("Example", result.Links[0].Content);
            Assert.IsFalse(result.Links[0].IsImage);
        }

        [TestMethod]
        public void RenderMarkdown_ReturnsConfiguredUnhandledTypes()
        {
            // Arrange
            var fake = new FakeMarkdownRenderer();
            var unhandledTypes = new HashSet<Type> { typeof(string), typeof(int) };
            fake.ResultToReturn = new MarkdownRenderResult(
                renderedText: "test",
                links: Array.Empty<MarkdownLink>(),
                unhandledTypes: unhandledTypes);

            // Act
            var result = fake.RenderMarkdown("test");

            // Assert
            Assert.IsNotNull(result.UnhandledTypes);
            Assert.AreEqual(2, result.UnhandledTypes.Count);
            Assert.IsTrue(result.UnhandledTypes.Contains(typeof(string)));
            Assert.IsTrue(result.UnhandledTypes.Contains(typeof(int)));
        }

        [TestMethod]
        public void RenderMarkdown_WithOptions_RecordsOptions()
        {
            // Arrange
            var fake = new FakeMarkdownRenderer();
            var options = new DisplayOptions { IncludeDebug = true };

            // Act
            fake.RenderMarkdown("# Test", options);

            // Assert
            Assert.AreEqual(1, fake.Calls.Count);
            Assert.AreSame(options, fake.Calls[0].Options);
        }
    }

    [TestClass]
    public class FakeMarkdownDisplayerTests
    {
        [TestMethod]
        public async Task DisplayMarkdownAsync_WithUri_RecordsCall()
        {
            // Arrange
            var fake = new FakeMarkdownDisplayer();
            var uri = new Uri("https://example.com/readme.md");

            // Act
            await fake.DisplayMarkdownAsync(uri);

            // Assert
            Assert.AreEqual(1, fake.Calls.Count);
            Assert.AreEqual(uri, fake.Calls[0].Uri);
            Assert.IsNull(fake.Calls[0].Text);
            Assert.IsTrue(fake.Calls[0].AllowFollowingLinks);
        }

        [TestMethod]
        public async Task DisplayMarkdownAsync_WithText_RecordsCall()
        {
            // Arrange
            var fake = new FakeMarkdownDisplayer();
            var text = "# Hello";
            var baseUri = new Uri("https://example.com/");

            // Act
            await fake.DisplayMarkdownAsync(text, baseUri, allowFollowingLinks: false);

            // Assert
            Assert.AreEqual(1, fake.Calls.Count);
            Assert.IsNull(fake.Calls[0].Uri);
            Assert.AreEqual(text, fake.Calls[0].Text);
            Assert.AreEqual(baseUri, fake.Calls[0].BaseUri);
            Assert.IsFalse(fake.Calls[0].AllowFollowingLinks);
        }

        [TestMethod]
        public async Task DisplayMarkdownAsync_MultipleCalls_RecordsAll()
        {
            // Arrange
            var fake = new FakeMarkdownDisplayer();

            // Act
            await fake.DisplayMarkdownAsync(new Uri("https://example.com/a.md"));
            await fake.DisplayMarkdownAsync("# Test");
            await fake.DisplayMarkdownAsync(new Uri("file:///tmp/b.md"), allowFollowingLinks: false);

            // Assert
            Assert.AreEqual(3, fake.Calls.Count);
        }
    }
}
