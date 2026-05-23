using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using Markdig;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// End-to-end tests for <see cref="SpectreMarkdownRenderer"/> and <see cref="MarkdownRenderResult"/>.
    /// </summary>
    [TestClass]
    public class SpectreMarkdownRendererTests : ConsoleTestBase
    {
        // ---------------------------------------------------------------------------
        // Render(string) — basic round-trip
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void Render_PlainParagraph_RootIsNotNull()
        {
            var renderer = new SpectreMarkdownRenderer();
            var result = renderer.Render("Hello, world!");

            Assert.IsNotNull(result.Root, "Root should not be null for non-empty markdown");
        }

        [TestMethod]
        public void Render_EmptyString_DoesNotThrow()
        {
            var renderer = new SpectreMarkdownRenderer();
            var result = renderer.Render(string.Empty);

            // Root may be null for empty input, but the call itself must not throw
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Render_WithLinks_ReturnsExpectedLinks()
        {
            const string markdown = "Visit [example](https://example.com) for more info.";
            var renderer = new SpectreMarkdownRenderer();
            var result = renderer.Render(markdown);

            Assert.IsNotNull(result.Links, "Links list must not be null");
            Assert.AreEqual(1, result.Links.Count, "Expected exactly one link");
            Assert.AreEqual("https://example.com", result.Links[0].Url);
            Assert.IsFalse(result.Links[0].IsImage, "Non-image link should not be flagged as image");
        }

        [TestMethod]
        public void Render_WithImageLinks_MarkedAsImage()
        {
            const string markdown = "![alt text](https://example.com/image.png)";
            var renderer = new SpectreMarkdownRenderer();
            var result = renderer.Render(markdown);

            Assert.IsNotNull(result.Links, "Links list must not be null");
            Assert.AreEqual(1, result.Links.Count, "Expected exactly one image link");
            Assert.IsTrue(result.Links[0].IsImage, "Image link should be flagged as image");
        }

        [TestMethod]
        public void Render_SupportedElements_NoUnhandledTypes()
        {
            const string markdown = """
                # Heading

                Paragraph with **bold** and *italic* text.

                - List item 1
                - List item 2

                1. Ordered item
                2. Another item

                > Block quote

                ```csharp
                var x = 42;
                ```
                """;

            var renderer = new SpectreMarkdownRenderer(new SpectreDisplayOptions { IncludeDebug = true });
            var result = renderer.Render(markdown);

            Assert.IsNotNull(result.UnhandledTypes, "UnhandledTypes must not be null");
            if (result.UnhandledTypes.Count > 0)
            {
                Assert.Fail($"Unhandled node types for common markdown: {string.Join(", ", result.UnhandledTypes)}");
            }
        }

        // ---------------------------------------------------------------------------
        // Stateless-per-call contract
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void Render_CalledTwice_LinksDoNotAccumulate()
        {
            const string markdown = "See [link A](https://a.example.com) and [link B](https://b.example.com).";
            var renderer = new SpectreMarkdownRenderer();

            var first = renderer.Render(markdown);
            var second = renderer.Render(markdown);

            Assert.AreEqual(first.Links.Count, second.Links.Count,
                "Consecutive renders of the same markdown must return the same number of links (no accumulation)");
            Assert.AreEqual(2, second.Links.Count,
                "Both renders must return exactly 2 links");
        }

        [TestMethod]
        public void Render_DifferentMarkdown_IndependentResults()
        {
            var renderer = new SpectreMarkdownRenderer();

            var withLink = renderer.Render("See [example](https://example.com).");
            var noLink = renderer.Render("No links here.");

            Assert.AreEqual(1, withLink.Links.Count, "First render should have 1 link");
            Assert.AreEqual(0, noLink.Links.Count, "Second render should have 0 links");
        }

        // ---------------------------------------------------------------------------
        // Render(MarkdownDocument) overload
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void Render_PreParsedDocument_RootIsNotNull()
        {
            var renderer = new SpectreMarkdownRenderer();
            var options = new SpectreDisplayOptions();
            var pipeline = options.BuildPipeline();
            var document = Markdown.Parse("# Title\n\nSome text.", pipeline);

            var result = renderer.Render(document);

            Assert.IsNotNull(result.Root, "Root should not be null for a parsed document with content");
        }

        [TestMethod]
        public void Render_PreParsedDocument_LinksExtracted()
        {
            var renderer = new SpectreMarkdownRenderer();
            var options = new SpectreDisplayOptions();
            var pipeline = options.BuildPipeline();
            var document = Markdown.Parse("Click [here](https://example.com).", pipeline);

            var result = renderer.Render(document);

            Assert.AreEqual(1, result.Links.Count, "Expected one link from pre-parsed document");
            Assert.AreEqual("https://example.com", result.Links[0].Url);
        }

        // ---------------------------------------------------------------------------
        // SpectreDisplayOptions customization
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void Render_CustomCodeBlockStyle_DoesNotThrow()
        {
            var opts = new SpectreDisplayOptions
            {
                CodeBlock = new Style(Color.Grey85, Color.Grey15),
                TableBorder = global::Spectre.Console.TableBorder.Rounded,
            };
            var renderer = new SpectreMarkdownRenderer(opts);

            // Table markdown
            const string markdown = """
                | Col A | Col B |
                | ----- | ----- |
                | 1     | 2     |
                """;

            var result = renderer.Render(markdown);
            Assert.IsNotNull(result.Root);
        }

        // ---------------------------------------------------------------------------
        // Output written to TestConsole via AnsiConsole.Write
        // ---------------------------------------------------------------------------

        [TestMethod]
        public void Render_WrittenToConsole_ProducesNonEmptyOutput()
        {
            var renderer = new SpectreMarkdownRenderer();
            var result = renderer.Render("Hello, **world**!");

            Assert.IsNotNull(result.Root);
            AnsiConsole.Write(result.Root);

            Assert.IsFalse(string.IsNullOrWhiteSpace(ConsoleUnderTest.Output),
                "Writing the renderable to the console should produce non-empty output");
        }
    }
}
