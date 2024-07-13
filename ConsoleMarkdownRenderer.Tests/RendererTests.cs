using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConsoleMarkdownRenderer.ObjectRenderers;
using Markdig;
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
        /// This test checks all the the .md/.txt pairs to see if the rendering the markdown yields the expected result
        /// <see cref="Spectre.Console.Testing.TestConsole.Output"/> only contains the raw text (and does not include any character formatting)
        /// So this is really checking that the text is laid out as expected, we use includeDebug with our <see cref="ConsoleRenderer"/> so that the structure (aka all the boxes) is visiable
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

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_CodeInlineTest(bool useCrazy)
        {
            AssertMarkdownYieldsFormat("codeInline", "in line code", new Style(foreground: Color.Yellow, background: Color.Blue), useCrazy);
        }

        [DataTestMethod]
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

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_MarkedTest(bool useCrazy)
        {
            AssertMarkdownYieldsFormat("emphasisInline", "marked", new Style(foreground: Color.Black, background: Color.Yellow), useCrazy);
        }

        [DataTestMethod]
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

        [DataTestMethod]
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
                Assert.AreEqual(expected[i].Url, renderer.Links[i].Link.Url, $"Url: {expected[i]} {renderer.Links[i]}");
                Assert.AreEqual(expected[i].IsImage, renderer.Links[i].Link.IsImage, $"IsImage: {expected[i]} {renderer.Links[i]}");
            }
        }

        [DataTestMethod]
        [DataRow("quote 2." , Decoration.Italic)]
        [DataRow("should even" , Decoration.Italic | Decoration.Bold)]
        public void RendererTests_QuoteBlockTest(string text, Decoration decoration) 
        {
            AssertMarkdownYieldsFormat("quoteBlock", text, new Style(decoration: decoration), useCrazy: false);
            AssertMarkdownYieldsFormat("quoteBlock", text, new Style(decoration: decoration), useCrazy: true);
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
            public TestRenderHook(string text, Style style)
            {
                m_text = text;
                m_style = style;
                m_counts = Counts(text);
            }

            public void AssertFormattedTextFound() 
                => Assert.AreEqual(m_counts.Sum(x => x.Value), m_count, $"Failed to find {m_text} with the correct style {m_style.ToMarkup()}");

            public IEnumerable<IRenderable> Process(RenderContext context, IEnumerable<IRenderable> renderables)
            {
                m_count = renderables
                    .SelectMany(x => x.Render(context, maxWidth: 360))
                    .Count(Check);
                return renderables;
            }

            private bool Check(Segment segment)
            {
                Logger.LogMessage($"{segment.Style.ToMarkup()}:{segment.Text}");
                return m_counts.ContainsKey(segment.Text) && m_style.Equals(segment.Style);
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