using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleMarkdownRenderer.ObjectRenderers;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="ConsoleRenderer"/>
    /// </summary>
    [TestClass]
    public class RendererTests : ConsoleTestBase
    {
        /// <summary>
        /// This test checks all the the .md/.txt pairs to see if the rendering od the markdown yields the expected result
        /// <see cref="Spectre.Console.Testing.TestConsole.Output"/> only contains the raw text (and does not include any character formatting)
        /// So this is really checking that the text is laid out as expected, we use includeDebug with our <see cref="ConsoleRenderer"/> so that the structure (aka all the boxes) is visible
        /// The additional tests in this class will validate the character formatting
        /// </summary>
        [TestMethod]
        public void RendererTests_TextValidation()
        {
            var markdowns = new HashSet<string>();
            var expected = new HashSet<string>();

            foreach (var resourceName in GetType().Assembly.GetManifestResourceNames())
            {
                if (Path.GetDirectoryName(resourceName) == c_resources)
                {
                    var extension = Path.GetExtension(resourceName);
                    var name = Path.GetFileNameWithoutExtension(resourceName);
                    if (extension == ".md")
                    {
                        markdowns.Add(name);
                    }
                    else if (extension == ".txt")
                    {
                        expected.Add(name);
                    }
                }
            }

            var missing = markdowns.Where(x => !expected.Contains(x));
            if (missing.Any())
            {
                Assert.Fail($"Has markdowns but no expected {string.Join(", ", missing)}");
            }
            missing = expected.Where(x => !markdowns.Contains(x));
            if (missing.Any())
            {
                Assert.Fail($"Has expected but no markdown {string.Join(", ", missing)}");
            }

            var pipeline = Displayer.DefaultPipeline;
            var renderer = new ConsoleRenderer(new DisplayOptions() { IncludeDebug = true });

            foreach (var markdown in markdowns)
            {
                var markdownText = GetResourceContent(markdown, "md");
                var expectedText = GetResourceContent(markdown, "txt");

                NewConsole().Write(Renderer(markdownText, renderer, pipeline));

                AssertCrossPlatStringMatch(expectedText, ConsoleUnderTest.Output, $@"{markdown} Did not get the expect result
{markdownText}
Yielded
{ConsoleUnderTest.Output}
Expected
{expectedText}");
            }
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_CodeInlineTest(bool useCrazy)
        {
            AssertMarkdownYieldsFormat("codeInline", "in line code", new Style(foreground: Color.Yellow, background: Color.Blue), useCrazy);
        }

        [TestMethod]
        [DataRow("bold"          , Decoration.Bold)]
        [DataRow("italic"        , Decoration.Italic)]
        [DataRow("strike through", Decoration.Strikethrough)]
        [DataRow("subscript"     , Decoration.SlowBlink)]
        [DataRow("superscript"   , Decoration.RapidBlink)]
        [DataRow("inserted"      , Decoration.Underline)]
        public void RendererTests_EmphasisInlineTest(string text, Decoration decoration)
        {
            AssertMarkdownYieldsFormat("emphasisInline", text, new Style(decoration: decoration), useCrazy: false);
            AssertMarkdownYieldsFormat("emphasisInline", text, new Style(decoration: decoration), useCrazy: true);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_MarkedTest(bool useCrazy)
        {
            AssertMarkdownYieldsFormat("emphasisInline", "marked", new Style(foreground: Color.Black, background: Color.Yellow), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_HeaderTest(bool useCrazy)
            => AssertMarkdownYieldsFormat(
                "headingBlock",
                text: useCrazy 
                    ? "Level One Level Two Level Three"
                    : "# Level One # ## Level Two ## ### Level Three ###",
                new Style(decoration: Decoration.Bold | Decoration.Invert | Decoration.Underline),
                useCrazy);

        [TestMethod]
        public void RendererTests_LevelSpecificHeaderTest()
        {
            DisplayOptions options = new()
            {
                WrapHeader = false,
            };
            options.Headers.Add("blue on green");
            options.Headers.Add("green on blue");

            string[] levels = ["One", "Two", "Three"];

            for (int index = 0; index < levels.Length; index++)
            {
                Style expected = index < options.Headers.Count 
                               ? options.Headers[index]
                               : options.Header;

                Assert.AreEqual(expected, options.EffectiveHeader(index + 1));

                AssertMarkdownYieldsFormat(
                        "headingBlock",
                        text: $"Level {levels[index]}",
                        expected,
                        useCrazy: false,
                        options);
            }
        }

        [TestMethod]
        [DataRow("htmlBlock", "<table> <tr> <td>1</td> <td>2</td> </tr> <tr> <td>3</td> <td>4</td> </tr> </table>")]
        [DataRow("htmlInline", "<span>html</span>")]
        public void RendererTests_HtmlTest(string name, string text)
        {
            AssertMarkdownYieldsFormat(name, text, new Style(foreground: Color.Black, background: Color.Green), useCrazy: false);
            AssertMarkdownYieldsFormat(name, text, new Style(foreground: Color.Black, background: Color.Green), useCrazy: true);
        }

        [TestMethod]
        public void RendererTests_LinkTest()
        {
            var expected = new (string Content, string Url, bool IsImage)[]
            {
                new ("", "one.md", false),
                new ("2", "two.md", false),
                new ("www.three.com", "http://www.three.com", false),
                new ("https://www.four.com", "https://www.four.com", false),
                new ("https://www.five.com/five", "https://www.five.com/five", false),
                new ("", "six.md", true),
                new ("7", "seven.md", true),
                new ("", "https://www.eight.com/eight.jpg", true),
                new ("9", "https://www.nine.com/nine.jpg", true),
            };

            var renderer = new ConsoleRenderer(new DisplayOptions() { IncludeDebug = true});

            ConsoleUnderTest.Write(Renderer(GetResourceContent("linkInline", "md"), renderer));

            Assert.AreEqual(expected.Length, renderer.Links.Count, "Wrong number of items");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].Content, renderer.Links[i].Content, $"Content: {expected[i]} {renderer.Links[i]}");
                Assert.AreEqual(expected[i].Url, renderer.Links[i].Url, $"Url: {expected[i]} {renderer.Links[i]}");
                Assert.AreEqual(expected[i].IsImage, renderer.Links[i].IsImage, $"IsImage: {expected[i]} {renderer.Links[i]}");
            }
        }

        [TestMethod]
        public void RendererTests_AutolinkTest()
        {
            var expected = new (string Content, string Url, bool IsImage)[]
            {
                new ("https://example.com", "https://example.com", false),
                new ("user@example.com", "mailto:user@example.com", false),
            };

            var renderer = new ConsoleRenderer(new DisplayOptions() { IncludeDebug = true });

            ConsoleUnderTest.Write(Renderer(GetResourceContent("autolinkInline", "md"), renderer));

            Assert.AreEqual(expected.Length, renderer.Links.Count, "Wrong number of items");
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.AreEqual(expected[i].Content, renderer.Links[i].Content, $"Content: {expected[i]} {renderer.Links[i]}");
                Assert.AreEqual(expected[i].Url, renderer.Links[i].Url, $"Url: {expected[i]} {renderer.Links[i]}");
                Assert.AreEqual(expected[i].IsImage, renderer.Links[i].IsImage, $"IsImage: {expected[i]} {renderer.Links[i]}");
            }
        }

        [TestMethod]
        [DataRow("quote 2." , Decoration.Italic)]
        [DataRow("should even" , Decoration.Italic | Decoration.Bold)]
        public void RendererTests_QuoteBlockTest(string text, Decoration decoration) 
        {
            AssertMarkdownYieldsFormat("quoteBlock", text, new Style(decoration: decoration), useCrazy: false);
            AssertMarkdownYieldsFormat("quoteBlock", text, new Style(decoration: decoration), useCrazy: true);
        }

        [TestMethod]
        public void RendererTests_PlainTextUsesDefaultColors()
        {
            const string markdown = "Normal text should keep the console default colors.";
            var renderHook = new TestRenderHook(
                text: markdown,
                style: new Style(foreground: Color.Default, background: Color.Default),
                compareDefaultColors: true);

            ConsoleUnderTest.Pipeline.Attach(renderHook);
            ConsoleUnderTest.Write(Renderer(markdown));

            renderHook.AssertFormattedTextFound();
        }

        /// <summary>
        /// Covers the else branch of <see cref="ConsoleEmphasisInlineRenderer.Write"/> when
        /// the delimiter character is not any of the known emphasis characters.
        /// </summary>
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

            var options = new DisplayOptions { IncludeDebug = true };
            var renderer = new ConsoleRenderer(options);
            renderer.Render(document);

            Assert.IsNotNull(renderer.Root);
            // EmphasisInline is handled (via the else branch), so no unhandled types
            Assert.IsNull(renderer.UnhandledTypes, "EmphasisInline should be handled even with an unknown delimiter");

            ConsoleUnderTest.Write(renderer.Root);
            var output = ConsoleUnderTest.Output;
            // The else branch emits the delimiter char and count as a marker: (!1)
            Assert.IsTrue(output.Contains("(!1)"), $"Expected unknown delimiter marker '(!1)' in output:\n{output}");
            Assert.IsTrue(output.Contains("content"), $"Expected 'content' in output:\n{output}");
        }

        /// <summary>
        /// Covers unhandled type detection when <see cref="DisplayOptions.IncludeDebug"/> is true
        /// and a markdown element type has no registered renderer.
        /// <see cref="ConsoleAutolinkInlineRenderer.IsEnabled"/> is temporarily set to false so that
        /// AutolinkInline has no registered renderer, triggering the unhandled-type code path.
        /// </summary>
        [TestMethod]
        public void RendererTests_UnhandledTypeDetectedTest()
        {
            ConsoleAutolinkInlineRenderer.IsEnabled = false;
            try
            {
                var options = new DisplayOptions { IncludeDebug = true };
                var renderer = new ConsoleRenderer(options);

                var document = Markdown.Parse("<https://example.com>", Displayer.DefaultPipeline);
                renderer.Render(document);

                Assert.IsNotNull(renderer.UnhandledTypes, "Should have detected at least one unhandled type");
                Assert.IsTrue(
                    renderer.UnhandledTypes.Any(t => t.Name == "AutolinkInline"),
                    $"Expected AutolinkInline to be in unhandled types; got: {string.Join(", ", renderer.UnhandledTypes.Select(t => t.Name))}");
            }
            finally
            {
                ConsoleAutolinkInlineRenderer.IsEnabled = true;
            }
        }

        private void AssertMarkdownYieldsFormat(string name, string text, Style style, bool useCrazy, DisplayOptions? options = null)
        {
            Style format = useCrazy ? c_crazyFormat : style;
            options ??= useCrazy ? m_crazyOptions : new DisplayOptions();
            var markdown = GetResourceContent(name, "md");

            var renderHook = new TestRenderHook(text, format);
            ConsoleUnderTest.Pipeline.Attach(renderHook);

            Logger.LogMessage($"Rendering {name}");
            ConsoleUnderTest.Write(Renderer(markdown, options: options));

            renderHook.AssertFormattedTextFound();
        }

        private string GetResourceContent(string name, string extension)
        {
            string path = Path.Combine(c_resources, Path.ChangeExtension(name, extension));
            using var markdownSteam = GetType().Assembly.GetManifestResourceStream(path);
            Assert.IsNotNull(markdownSteam, $"Failed to find resource for {path}");
            using var reader = new StreamReader(markdownSteam);
            return reader.ReadToEnd();
        }

        private static IRenderable Renderer(string text, ConsoleRenderer? renderer = default, MarkdownPipeline? pipeline = default, DisplayOptions? options = default)
        {
            var document = Markdown.Parse(text, pipeline ?? Displayer.DefaultPipeline);
            options ??= new();
            options = options.Clone();
            options.IncludeDebug = true;
            renderer ??= new ConsoleRenderer(options);
            renderer.Clear();
            renderer.Render(document);
            Assert.IsNotNull(renderer.Root);
            if (renderer.UnhandledTypes != null)
            {
                Assert.Fail($"Found Unhandled Types {string.Join(Environment.NewLine, renderer.UnhandledTypes)}");
            }
            return renderer.Root;
        }

        public class TestRenderHook : IRenderHook
        {
            public TestRenderHook(string text, Style style, bool compareDefaultColors = false)
            {
                m_text = text;
                m_style = style;
                m_compareDefaultColors = compareDefaultColors;
                m_counts = Counts(text);
            }

            public void AssertFormattedTextFound() 
                => Assert.AreEqual(m_counts.Sum(x => x.Value), m_count, $"Failed to find {m_text} with the correct style {m_style.ToMarkup()}");

            public IEnumerable<IRenderable> Process(RenderOptions options, IEnumerable<IRenderable> renderables)
            {
                m_count = renderables
                    .SelectMany(x => x.Render(options, maxWidth: 360))
                    .Count(Check);
                return renderables;
            }

            private bool Check(Segment segment)
            {
                Logger.LogMessage($"{segment.Style.ToMarkup()}:{segment.Text}");
                if (!m_counts.ContainsKey(segment.Text))
                    return false;
                // Segment styles may include colors inherited from the rendering context (e.g. background).
                // Only compare default colors when the caller explicitly opts in.
                var seg = segment.Style;
                if ((m_compareDefaultColors || m_style.Foreground != Color.Default) && m_style.Foreground != seg.Foreground)
                    return false;
                if ((m_compareDefaultColors || m_style.Background != Color.Default) && m_style.Background != seg.Background)
                    return false;
                return m_style.Decoration == seg.Decoration;
            }

            // Given a string create a dictionary where the keys are the words and the value is the number of items those words appear
            private static Dictionary<string, int> Counts(string text)
            {
                var result = new Dictionary<string, int>();
                foreach (var x in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                {
                    result[x] = result.GetValueOrDefault(x) + 1;
                }
                return result;
            }

            private readonly string m_text;
            private readonly Style m_style;
            private readonly bool m_compareDefaultColors;
            private readonly Dictionary<string, int> m_counts;

            private int m_count;
        }

        private const string c_crazyFormat = "red on purple";
        private readonly static DisplayOptions m_crazyOptions = new DisplayOptions
        {
            Bold = c_crazyFormat,
            CodeBlock = c_crazyFormat,
            CodeInLine = c_crazyFormat,
            Header = c_crazyFormat,
            HtmlBlock = c_crazyFormat,
            HtmlInline = c_crazyFormat,
            Inserted = c_crazyFormat,
            Italic = c_crazyFormat,
            Marked = c_crazyFormat,
            QuotedBlock = c_crazyFormat,
            Strikethrough = c_crazyFormat,
            Subscript = c_crazyFormat,
            Superscript = c_crazyFormat,
            UnknownDelimiterChar = c_crazyFormat,
            UnknownDelimiterContent = c_crazyFormat,
            WrapHeader = false,
        };

        private const string c_resources = "resources";
    }
}