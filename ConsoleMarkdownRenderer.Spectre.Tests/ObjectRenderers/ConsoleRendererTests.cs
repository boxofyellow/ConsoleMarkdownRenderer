using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class ConsoleRendererTests : ConsoleTestBase
    {
        [TestMethod]
        public void RendererTests_UnknownEmphasisDelimiterTest()
        {
            // Construct a MarkdownDocument with an EmphasisInline using an unknown delimiter ('!')
            // There is no standard markdown syntax that produces this, so we build the AST directly.
            var document = new MarkdownDocument();
            var paragraph = new ParagraphBlock();
            var containerInline = new ContainerInline();
            var emphasisInline = new EmphasisInline { DelimiterChar = '!', DelimiterCount = 1 };
            emphasisInline.AppendChild(new LiteralInline("content"));
            containerInline.AppendChild(emphasisInline);
            paragraph.Inline = containerInline;
            document.Add(paragraph);

            var options = new SpectreDisplayOptions { IncludeDebug = true };
            var renderer = new ConsoleRenderer(options);
            renderer.Render(document);

            Assert.IsNotNull(renderer.Root);
            // EmphasisInline is handled (via the else branch), so no unhandled types
            Assert.IsNull(renderer.UnhandledTypes, "EmphasisInline should be handled even with an unknown delimiter");

            ConsoleUnderTest.Write(renderer.Root);
            var output = ConsoleUnderTest.Output;
            // The else branch emits the delimiter char and count as a marker: (!1)
            Assert.Contains("(!1)", output, $"Expected unknown delimiter marker '(!1)' in output:\n{output}");
            Assert.Contains("content", output, $"Expected 'content' in output:\n{output}");
        }
    }
}