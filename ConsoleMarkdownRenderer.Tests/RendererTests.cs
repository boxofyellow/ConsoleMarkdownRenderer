using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="ConsoleRenderer"/>
    /// </summary>
    [TestClass]
    public class RendererTests : ConsoleTestBase
    {
        /// <summary>
        /// This test checks all the the .md/.txt pairs to see if the rendering of the markdown yields the expected result
        /// <see cref="Spectre.Console.Testing.TestConsole.Output"/> only contains the raw text (and does not include any character formatting)
        /// So this is really checking that the text is laid out as expected, we use includeDebug with our <see cref="ConsoleRenderer"/> so that the structure (aka all the boxes) is visible
        /// The additional tests in this class will validate the character formatting
        /// </summary>
        [TestMethod]
        public async Task RendererTests_TextValidation()
        {
            var markdowns = new HashSet<string>();
            var expected = new HashSet<string>();
            var optionResources = new HashSet<string>();

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
                    else if (extension == ".json")
                    {
                        optionResources.Add(name);
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
            // .json sidecars are optional but each one must pair with an .md
            var orphanOptions = optionResources.Where(x => !markdowns.Contains(x));
            if (orphanOptions.Any())
            {
                Assert.Fail($"Has options json but no markdown {string.Join(", ", orphanOptions)}");
            }

            var defaultOptions = new DisplayOptions { IncludeDebug = true, ShowFencedCodeBlockInfo = true };

            foreach (var markdown in markdowns)
            {
                var markdownText = GetResourceContent(markdown, "md");
                var expectedText = GetResourceContent(markdown, "txt");

                DisplayOptions options;
                if (optionResources.Contains(markdown))
                {
                    options = await DisplayOptions.DeserializeAsync(GetResourceContent(markdown, "json")).ConfigureAwait(false);
                }
                else
                {
                    options = defaultOptions;
                }

                NewConsole().Write(Renderer(markdownText, options));

                AssertCrossPlatStringMatch(
                    expectedText,
                    ConsoleUnderTest.Output,
                    $"""
                    {markdown} Did not get the expect result
                    {markdownText}
                    Yielded
                    {ConsoleUnderTest.Output}
                    Expected
                    {expectedText}
                    """);
            }
        }

        [TestMethod]
        public async Task RendererTests_DisplayOptionsJson_RoundTrip()
        {
            // Sanity check that a DisplayOptions graph can be deserialized via the public
            // DisplayOptions.DeserializeAsync helper, and that a deserialized
            // FigletTextStyle has had its font materialized before the call returns.
            var fontPath = Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");
            var json = $$"""
                {
                    "showFencedCodeBlockInfo": true,
                    "bold": { "decoration": "{{TextDecoration.Bold}}", "foreground": null, "background": null },
                    "headers": [
                        {
                            "$type": "{{nameof(FigletTextStyle)}}",
                            "justification": "{{TextJustification.Left}}",
                            "foreground": { "named": "{{NamedColor.Green}}" },
                            "fontPath": {{JsonSerializer.Serialize(fontPath)}}
                        },
                        {
                            "$type": "{{nameof(TextStyle)}}",
                            "decoration": "{{TextDecoration.Bold | TextDecoration.Underline}}",
                            "foreground": null,
                            "background": null
                        }
                    ],
                    "header": {
                        "$type": "{{nameof(TextStyle)}}",
                        "decoration": "{{TextDecoration.Italic}}",
                        "foreground": null,
                        "background": null
                    }
                }
                """;

            var options = await DisplayOptions.DeserializeAsync(json).ConfigureAwait(false);

            Assert.IsTrue(options.ShowFencedCodeBlockInfo);
            Assert.AreEqual(2, options.Headers.Count);

            // H1 → FigletTextStyle (font already materialized by DeserializeAsync)
            var figlet = (FigletTextStyle)options.Headers[0];
            Assert.AreEqual(TextJustification.Left, figlet.Justification);
            Assert.AreEqual(TextColor.Green, figlet.Foreground);
            Assert.AreEqual(fontPath, figlet.FontPath);
            Assert.IsNotNull(figlet.Font);

            // H2 → TextStyle
            var h2 = (TextStyle)options.Headers[1];
            Assert.AreEqual(TextDecoration.Bold | TextDecoration.Underline, h2.Decoration);

            // Fallback Header → TextStyle
            var header = (TextStyle)options.Header;
            Assert.AreEqual(TextDecoration.Italic, header.Decoration);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_CodeInlineTest(bool useCrazy)
        {
            AssertMarkdownYieldsFormat("codeInline", "in line code", new Style(foreground: Color.Yellow, background: Color.Blue), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_MathInlineTest(bool useCrazy)
        {
            AssertMarkdownYieldsFormat("mathInline", "E = mc^2", new Style(foreground: Color.Green, background: Color.Purple), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_MathBlockTest(bool useCrazy)
        {
            AssertMarkdownYieldsFormat("mathBlock", "\\int_0^1 x^2 dx = \\frac{1}{3}", new Style(foreground: Color.Green, background: Color.Purple), useCrazy);
        }

        [TestMethod]
        public void RendererTests_FencedCodeBlockInfoDisabledByDefault()
        {
            // By default, ShowFencedCodeBlockInfo is false, so info should not be shown
            const string markdown = "```csharp\nConsole.WriteLine(\"Hello\");\n```";
            var options = new DisplayOptions { IncludeDebug = true };

            ConsoleUnderTest.Write(Renderer(markdown, options));

            // Info should NOT appear in output
            Assert.IsFalse(ConsoleUnderTest.Output.Contains("csharp"), "Info should not be shown when ShowFencedCodeBlockInfo is false");
        }

        [TestMethod]
        [DataRow("```python\nprint('hello')\n```",           "[python]",     "")]
        [DataRow("```javascript\nconsole.log('test');\n```", "[javascript]", "red on yellow")]
        public void RendererTests_FencedCodeBlockInfoEnabled(string markdown, string expectedText, string customStyle)
        {
            // When ShowFencedCodeBlockInfo is true, info should be shown with correct styling
            // The default FencedCodeBlockInfo is green on blue, but we can also set a custom style (red on yellow)
            var options = new DisplayOptions { ShowFencedCodeBlockInfo = true };

            if (!string.IsNullOrEmpty(customStyle))
            {
                options.FencedCodeBlockInfo = TextStyle.FromMarkup(customStyle);
            }
            var expectedStyle = options.FencedCodeBlockInfo.ToSpectreStyle();

            var renderHook = new TestRenderHook(expectedText, expectedStyle);
            ConsoleUnderTest.Pipeline.Attach(renderHook);

            ConsoleUnderTest.Write(Renderer(markdown, options));

            renderHook.AssertFormattedTextFound();
        }

        [TestMethod]
        public void RendererTests_IndentedCodeBlockWithInfoOptionEnabled()
        {
            // Indented code blocks (non-fenced) should work correctly even when ShowFencedCodeBlockInfo is enabled
            const string markdown = "    var x = 1;";
            var options = new DisplayOptions 
            { 
                IncludeDebug = true,
                ShowFencedCodeBlockInfo = true 
            };

            ConsoleUnderTest.Write(Renderer(markdown, options));

            // Should contain the code but no info line (since this is indented, not fenced)
            Assert.Contains("var x = 1;", ConsoleUnderTest.Output, "Code should be rendered");
            // No info line should be present for non-fenced code blocks
            Assert.DoesNotContain("[", ConsoleUnderTest.Output,
                $"No language info line should appear for indented code blocks.\nOutput:\n{ConsoleUnderTest.Output}");
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
            // The default DisplayOptions configures H1 as a FigletTextStyle, so H1's literal
            // text ("Level One") is replaced by FIGlet ASCII art and is not asserted here.
            // H2 and H3 still fall through to the default Header style.
            => AssertMarkdownYieldsFormat(
                "headingBlock",
                text: useCrazy 
                    ? "Level Two with  here Level Three with bold word"
                    : "## Level Two with code here ## ### Level Three with bold word ###",
                new Style(decoration: Decoration.Bold | Decoration.Invert | Decoration.Underline),
                useCrazy);

        [TestMethod]
        public void RendererTests_LevelSpecificHeaderTest()
        {
            DisplayOptions options = new()
            {
                WrapHeader = false,
            };
            // Clear the default Headers list (which configures H1 as a FigletTextStyle) so the
            // for-loop below exercises the styled-markup path for the levels it specifies.
            options.Headers.Clear();
            options.Headers.Add((TextStyle)"blue on green");
            options.Headers.Add((TextStyle)"green on blue");

            string[] levels = ["One", "Two", "Three"];

            for (int index = 0; index < levels.Length; index++)
            {
                IHeaderStyle expected = index < options.Headers.Count 
                               ? options.Headers[index]
                               : options.Header;

                Assert.AreEqual(expected, options.EffectiveHeader(index + 1));

                AssertMarkdownYieldsFormat(
                        "headingBlock",
                        text: $"Level {levels[index]}",
                        ((TextStyle)expected).ToSpectreStyle(),
                        useCrazy: false,
                        options);
            }
        }

        [TestMethod]
        public void RendererTests_FigletHeaderOnlyAppliesToConfiguredLevel()
        {
            // Default Headers[0] uses FigletTextStyle for H1; H2 and H3 fall through to the
            // styled header style. Verify against a golden file that H1 renders as FIGlet
            // ASCII art and deeper levels keep their "#"-wrapping intact.
            DisplayOptions options = new();

            ConsoleUnderTest.Write(Renderer(GetResourceContent("headingBlock", "md"), options));

            var expectedPath = Path.Combine(DataPath, "expected", "figletHeaderOnlyAppliesToConfiguredLevel.txt");
            Assert.IsTrue(File.Exists(expectedPath), $"Expected output file should exist at {expectedPath}");
            var expected = File.ReadAllText(expectedPath);

            AssertCrossPlatStringMatch(expected, ConsoleUnderTest.Output);
        }

        [TestMethod]
        public void RendererTests_FigletEmptyHeadingFallsBackToStyledMarkup()
        {
            // FigletText cannot render an empty string. When the heading has no text the
            // renderer should fall through to the styled-markup path so the level marker
            // (e.g. "# #" with WrapHeader=true) is still emitted.
            DisplayOptions options = new();
            // Sanity check: H1 is configured to use FIGlet by default.
            Assert.IsInstanceOfType<FigletTextStyle>(options.EffectiveHeader(1));

            ConsoleUnderTest.Write(Renderer("#\n", options));

            var output = ConsoleUnderTest.Output;
            Assert.Contains("#", output,
                $"Empty H1 should fall back to styled '#'-wrapped markup:\n{output}");
        }

        [TestMethod]
        public async Task RendererTests_FigletFontPathLoadsCustomFont()
        {
            // When a FigletTextStyle is created with a custom .flf font, the renderer should
            // use that font to render the FIGlet text. Compare against a known-good expected
            // output produced by the bundled shadow.flf font.
            const string markdown = "# Hi\n";

            var fontPath = Path.Combine(DataPath, "fonts", "shadow.flf");
            Assert.IsTrue(File.Exists(fontPath), $"Test font file should exist at {fontPath}");

            var expectedPath = Path.Combine(DataPath, "expected", "figletCustomFont.txt");
            Assert.IsTrue(File.Exists(expectedPath), $"Expected output file should exist at {expectedPath}");
            var expected = File.ReadAllText(expectedPath);

            var customOptions = new DisplayOptions
            {
                Headers = new() { await FigletTextStyle.CreateAsync(fontPath) },
            };
            ConsoleUnderTest.Write(Renderer(markdown, customOptions));

            AssertCrossPlatStringMatch(expected, ConsoleUnderTest.Output);
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
        [DataRow(0, "",                          "one.md",                          false)]
        [DataRow(1, "2",                         "two.md",                          false)]
        [DataRow(2, "www.three.com",             "http://www.three.com",            false)]
        [DataRow(3, "https://www.four.com",      "https://www.four.com",            false)]
        [DataRow(4, "https://www.five.com/five", "https://www.five.com/five",       false)]
        [DataRow(5, "",                          "six.md",                          true)]
        [DataRow(6, "7",                         "seven.md",                        true)]
        [DataRow(7, "",                          "https://www.eight.com/eight.jpg", true)]
        [DataRow(8, "9",                         "https://www.nine.com/nine.jpg",   true)]
        public void RendererTests_LinkTest(int index, string expectedContent, string expectedUrl, bool expectedIsImage)
        {
            var options = new DisplayOptions() { IncludeDebug = true};
            var document = Markdown.Parse(GetResourceContent("linkInline", "md"), MarkdownDisplayer.BuildPipeline(options));
            var renderer = new ConsoleRenderer(options);
            renderer.Render(document);

            Assert.IsNotNull(renderer.Root);
            ConsoleUnderTest.Write(renderer.Root);

            Assert.IsTrue(renderer.Links.Count > index, $"Expected at least {index + 1} links, got {renderer.Links.Count}");
            var link = renderer.Links[index];
            Assert.AreEqual(expectedContent, link.Content, $"Content mismatch at index {index}");
            Assert.AreEqual(expectedUrl, link.Url, $"Url mismatch at index {index}");
            Assert.AreEqual(expectedIsImage, link.IsImage, $"IsImage mismatch at index {index}");
        }

        [TestMethod]
        [DataRow(0, "https://example.com", "https://example.com")]
        [DataRow(1, "user@example.com",    "mailto:user@example.com")]
        public void RendererTests_AutolinkTest(int index, string expectedContent, string expectedUrl)
        {
            var options = new DisplayOptions() { IncludeDebug = true };
            var document = Markdown.Parse(GetResourceContent("autolinkInline", "md"), MarkdownDisplayer.BuildPipeline(options));
            var renderer = new ConsoleRenderer(options);
            renderer.Render(document);

            Assert.IsNotNull(renderer.Root);
            ConsoleUnderTest.Write(renderer.Root);

            Assert.IsTrue(renderer.Links.Count > index, $"Expected at least {index + 1} links, got {renderer.Links.Count}");
            var link = renderer.Links[index];
            Assert.AreEqual(expectedContent, link.Content, $"Content mismatch at index {index}");
            Assert.AreEqual(expectedUrl, link.Url, $"Url mismatch at index {index}");
            Assert.IsFalse(link.IsImage, $"IsImage should be false at index {index}");
        }

        [TestMethod]
        public void RendererTests_TableColumnAlignment_HonorsMarkdownAlignment()
        {
            // Pipe-table column alignment specified in Markdown (`:---`, `:---:`, `---:`) should
            // be reflected in the Spectre.Console TableColumn.Alignment of the rendered table.
            const string markdown = "| left | center | right |\n| :--- | :----: | ----: |\n| a | b | c |\n";

            var options = new DisplayOptions();
            var document = Markdown.Parse(markdown, MarkdownDisplayer.BuildPipeline(options));
            var renderer = new ConsoleRenderer(options);
            renderer.Render(document);

            Assert.IsNotNull(renderer.Root);
            // Root is an outer wrapper Table whose single cell holds the rendered table.
            var outer = (Table)renderer.Root;
            var inner = (Table)outer.Rows.First().First();

            Assert.AreEqual(3, inner.Columns.Count, "Expected three table columns");
            Assert.AreEqual(Justify.Left, inner.Columns[0].Alignment, "First column should be left-aligned");
            Assert.AreEqual(Justify.Center, inner.Columns[1].Alignment, "Second column should be center-aligned");
            Assert.AreEqual(Justify.Right, inner.Columns[2].Alignment, "Third column should be right-aligned");
        }

        [TestMethod]
        public void RendererTests_TableColumnAlignment_DefaultsToLeftWhenUnspecified()
        {
            // When no alignment is specified (`---`), columns should default to left-aligned.
            const string markdown = "| a | b | c |\n| - | - | - |\n| 1 | 2 | 3 |\n";

            var options = new DisplayOptions();
            var document = Markdown.Parse(markdown, MarkdownDisplayer.BuildPipeline(options));
            var renderer = new ConsoleRenderer(options);
            renderer.Render(document);

            Assert.IsNotNull(renderer.Root);
            var outer = (Table)renderer.Root;
            var inner = (Table)outer.Rows.First().First();

            Assert.AreEqual(3, inner.Columns.Count);
            foreach (var column in inner.Columns)
            {
                Assert.AreEqual(Justify.Left, column.Alignment, "Unspecified alignment should default to left");
            }
        }

        [TestMethod]
        public void RendererTests_TableBorder_DefaultsToSquare()
        {
            // Default DisplayOptions should preserve the historical Spectre.Console
            // Square border so callers see no visual change unless they opt in.
            Assert.AreEqual(TextTableBorder.Square, new DisplayOptions().TableBorder,
                "TableBorder should default to Square to preserve current behavior.");

            const string markdown = "| a | b |\n| - | - |\n| 1 | 2 |\n";

            var document = Markdown.Parse(markdown, MarkdownDisplayer.BuildPipeline(new DisplayOptions()));
            var renderer = new ConsoleRenderer(new DisplayOptions());
            renderer.Render(document);

            var outer = (Table)renderer.Root!;
            var inner = (Table)outer.Rows.First().First();

            Assert.AreSame(Spectre.Console.TableBorder.Square, inner.Border,
                "Default rendered table border should be Square.");
        }

        [TestMethod]
        public void RendererTests_TableBorder_MapsToSpectreNamedBorder()
        {
            // Each named TextTableBorder should map to the like-named static
            // Spectre.Console.TableBorder instance.
            const string markdown = "| a | b |\n| - | - |\n| 1 | 2 |\n";

            foreach (TextTableBorder border in Enum.GetValues(typeof(TextTableBorder)))
            {
                var document = Markdown.Parse(markdown, MarkdownDisplayer.BuildPipeline(new DisplayOptions()));
                var renderer = new ConsoleRenderer(new DisplayOptions { TableBorder = border });
                renderer.Render(document);

                var outer = (Table)renderer.Root!;
                var inner = (Table)outer.Rows.First().First();

                var expected = (Spectre.Console.TableBorder)typeof(Spectre.Console.TableBorder)
                    .GetProperty(border.ToString(), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)!
                    .GetValue(null)!;
                Assert.AreSame(expected, inner.Border,
                    $"TextTableBorder.{border} should map to Spectre.Console.TableBorder.{border}.");
            }
        }

        [TestMethod]
        public void RendererTests_TableBorderStyle_AppliedToRenderedTable()
        {
            // TableBorderStyle should be passed through to the rendered table's BorderStyle.
            var options = new DisplayOptions
            {
                TableBorderStyle = new TextStyle(foreground: TextColor.Red, decoration: TextDecoration.Bold),
            };

            const string markdown = "| a | b |\n| - | - |\n| 1 | 2 |\n";
            var document = Markdown.Parse(markdown, MarkdownDisplayer.BuildPipeline(new DisplayOptions()));
            var renderer = new ConsoleRenderer(options);
            renderer.Render(document);

            var outer = (Table)renderer.Root!;
            var inner = (Table)outer.Rows.First().First();

            Assert.IsNotNull(inner.BorderStyle, "BorderStyle should be set when TableBorderStyle is provided.");
            var borderStyle = inner.BorderStyle.Value;
            Assert.AreEqual(Color.Red, borderStyle.Foreground,
                "Border foreground should reflect TableBorderStyle.Foreground.");
            Assert.IsTrue(borderStyle.Decoration.HasFlag(Decoration.Bold),
                "Border decoration should reflect TableBorderStyle.Decoration.");
        }


        [TestMethod]
        public void RendererTests_TerminalHyperlinks_DefaultEnabled()
        {
            // By default, UseTerminalHyperlinks is true so OSC 8 escape sequences should be
            // emitted around link text in supported terminals.
            Assert.IsTrue(new DisplayOptions().UseTerminalHyperlinks,
                "UseTerminalHyperlinks should default to true.");

            var output = RenderMarkdownWithLinkCapableConsole("[two](http://two.example/)", new DisplayOptions());

            // OSC 8 hyperlink wraps with ESC ] 8 ; <params> ; <url> ESC \ ... ESC ] 8 ; ; ESC \
            Assert.Contains("\u001B]8;", output, "Expected OSC 8 open sequence in output");
            Assert.Contains("http://two.example/", output, "Expected URL in OSC 8 sequence");
            Assert.Contains("\u001B]8;;\u001B\\", output, "Expected OSC 8 close sequence in output");
        }

        [TestMethod]
        public void RendererTests_TerminalHyperlinks_OptOut()
        {
            // When UseTerminalHyperlinks is false, no OSC 8 escape sequences should be emitted.
            var options = new DisplayOptions { UseTerminalHyperlinks = false };
            var output = RenderMarkdownWithLinkCapableConsole("[two](http://two.example/)", options);

            Assert.DoesNotContain("\u001B]8;", output,
                $"Expected no OSC 8 sequences when UseTerminalHyperlinks is false. Output: {output}");
            // Visible link text/URL should still be present in the rendered output.
            Assert.Contains("http://two.example/", output, "URL should still be rendered as visible text");
        }

        [TestMethod]
        public void RendererTests_TerminalHyperlinks_AutolinkEmitsOsc8()
        {
            // Autolinks should also be wrapped in OSC 8 hyperlinks by default.
            var output = RenderMarkdownWithLinkCapableConsole("<https://auto.example/>", new DisplayOptions());

            Assert.Contains("\u001B]8;", output, "Expected OSC 8 open sequence for autolink");
            Assert.Contains("https://auto.example/", output, "Expected autolink URL in output");
            Assert.Contains("\u001B]8;;\u001B\\", output, "Expected OSC 8 close sequence for autolink");
        }

        [TestMethod]
        public void RendererTests_TerminalHyperlinks_UrlWithBracketsIsEscaped()
        {
            // URLs may contain '[' or ']'. The link tag's URL parameter must be escaped via
            // Markup.Escape so it doesn't break Spectre's markup parser.
            const string url = "http://example.com/path[1]";
            var output = RenderMarkdownWithLinkCapableConsole($"[label](<{url}>)", new DisplayOptions());

            // The URL should appear unmodified inside the OSC 8 escape sequence.
            Assert.Contains($"\u001B]8;id=", output, "Expected OSC 8 open sequence");
            Assert.Contains(url, output, $"Expected the unescaped URL '{url}' in the output");
        }

        [TestMethod]
        public void RendererTests_TerminalHyperlinks_ClonePreservesOptOut()
        {
            var options = new DisplayOptions { UseTerminalHyperlinks = false };
            var clone = options.Clone();
            Assert.IsFalse(clone.UseTerminalHyperlinks, "Clone() should preserve UseTerminalHyperlinks");
        }

        private string RenderMarkdownWithLinkCapableConsole(string markdown, DisplayOptions options)
        {
            // Configure the test console to actually emit ANSI sequences and advertise OSC 8
            // hyperlink support, so the rendered Output contains the escape sequences we are
            // asserting on.
            var console = NewConsole();
            console.EmitAnsiSequences = true;
            console.Profile.Capabilities.Links = true;
            console.Write(Renderer(markdown, options));
            return console.Output;
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
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_FigureCaptionTest(bool useCrazy)
        {
            // The caption inline content should carry the FigureCaption style (italic by default)
            AssertMarkdownYieldsFormat("figure", "A descriptive caption for the figure.", new Style(decoration: Decoration.Italic), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_FootnoteLinkTest(bool useCrazy)
        {
            // Forward reference markers sit outside the FootnoteGroup so they only carry the FootnoteLink style
            AssertMarkdownYieldsFormat("footnote", "[^1]", new Style(foreground: Color.Blue, decoration: Decoration.Underline), useCrazy);
            AssertMarkdownYieldsFormat("footnote", "[^2]", new Style(foreground: Color.Blue, decoration: Decoration.Underline), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_FootnoteTest(bool useCrazy)
        {
            // Each footnote's label prefix should carry the Footnote style (plus the surrounding FootnoteGroup style in non-crazy mode)
            var labelDecoration = useCrazy ? Decoration.Bold : Decoration.Bold | Decoration.Italic;
            AssertMarkdownYieldsFormat("footnote", "[^1]:", new Style(decoration: labelDecoration), useCrazy);
            AssertMarkdownYieldsFormat("footnote", "[^longnote]:", new Style(decoration: labelDecoration), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_FootnoteGroupTest(bool useCrazy)
        {
            // Plain text inside a footnote inherits only the FootnoteGroup style
            AssertMarkdownYieldsFormat("footnote", "first", new Style(decoration: Decoration.Italic), useCrazy);
            AssertMarkdownYieldsFormat("footnote", "longer", new Style(decoration: Decoration.Italic), useCrazy);
        }

        [TestMethod]
        public void RendererTests_SmartyPantInlineRenderer_HandlesAllEnumValues()
        {
            // Enumerate every SmartyPantType via reflection so that if Markdig adds a new value in the
            // future this test will fail until ConsoleSmartyPantInlineRenderer.GetReplacement is updated
            // to map it (instead of silently falling through to the default-empty-string case).
            var values = Enum.GetValues(typeof(Markdig.Extensions.SmartyPants.SmartyPantType))
                .Cast<Markdig.Extensions.SmartyPants.SmartyPantType>();
            foreach (var value in values)
            {
                var replacement = ConsoleSmartyPantInlineRenderer.GetReplacement(value);
                Assert.IsFalse(
                    string.IsNullOrEmpty(replacement),
                    $"ConsoleSmartyPantInlineRenderer.GetReplacement returned empty for {value}; add a mapping for it.");
            }
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_FooterTest(bool useCrazy)
        {
            // Plain text inside the footer should carry the Footer style (dim italic by default).
            AssertMarkdownYieldsFormat("footer", "document", new Style(decoration: Decoration.Dim | Decoration.Italic), useCrazy);
            AssertMarkdownYieldsFormat("footer", "multiple", new Style(decoration: Decoration.Dim | Decoration.Italic), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_AbbreviationTitleTest(bool useCrazy)
        {
            // The expansion title is rendered in the AbbreviationTitle style (dim by default)
            AssertMarkdownYieldsFormat("abbreviation", "HyperText Markup Language", new Style(decoration: Decoration.Dim), useCrazy);
            AssertMarkdownYieldsFormat("abbreviation", "World Wide Web Consortium", new Style(decoration: Decoration.Dim), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_DefinitionTermTest(bool useCrazy)
        {
            // The term text should carry the DefinitionTerm style (bold by default)
            AssertMarkdownYieldsFormat("definitionList", "Apple", new Style(decoration: Decoration.Bold), useCrazy);
            AssertMarkdownYieldsFormat("definitionList", "Orange", new Style(decoration: Decoration.Bold), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_DefinitionListTest(bool useCrazy)
        {
            // Plain text inside a definition inherits only the DefinitionList style (no decoration by default)
            AssertMarkdownYieldsFormat("definitionList", "fruit", new Style(), useCrazy);
            AssertMarkdownYieldsFormat("definitionList", "citrus", new Style(), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_CustomContainerInfoTest(bool useCrazy)
        {
            // The container's Info label (e.g. "note", "warning") should carry the CustomContainerInfo style (bold by default)
            AssertMarkdownYieldsFormat("customContainer", "note", new Style(decoration: Decoration.Bold), useCrazy);
            AssertMarkdownYieldsFormat("customContainer", "warning", new Style(decoration: Decoration.Bold), useCrazy);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_CustomContainerInlineTest(bool useCrazy)
        {
            // Inline custom container content (::tag inline::) carries the CustomContainerInline style (bold by default)
            AssertMarkdownYieldsFormat("customContainer", "tag inline", new Style(decoration: Decoration.Bold), useCrazy);
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
            Assert.Contains("(!1)", output, $"Expected unknown delimiter marker '(!1)' in output:\n{output}");
            Assert.Contains("content", output, $"Expected 'content' in output:\n{output}");
        }

        [TestMethod]
        public void RendererTests_UnhandledTypeDetectedTest()
        {
            var options = new DisplayOptions { IncludeDebug = true };
            var renderer = new ConsoleRenderer(options, omitAutolinkInlineRenderer: true);

            var document = Markdown.Parse("<https://example.com>", MarkdownDisplayer.BuildPipeline(options));
            renderer.Render(document);

            Assert.IsNotNull(renderer.UnhandledTypes, "Should have detected at least one unhandled type");
            Assert.Contains(
                "AutolinkInline",
                renderer.UnhandledTypes.Select(t => t.Name), 
                $"Expected AutolinkInline to be in unhandled types; got: {string.Join(", ", renderer.UnhandledTypes.Select(t => t.Name))}");
        }

        [TestMethod]
        public void RendererTests_EmojiInlineDefaultTest()
        {
            // With the default DisplayOptions (Emojis = true) shortcodes/smileys should be
            // substituted with their Unicode equivalents.
            const string markdown = "Hello :smile: world :-)";
            const string expected = """
                ┌───────────────────┐
                │ Hello 😄 world 😃 │
                └───────────────────┘

                """;

            ConsoleUnderTest.Write(Renderer(markdown));

            AssertCrossPlatStringMatch(expected, ConsoleUnderTest.Output);
        }

        [TestMethod]
        public void RendererTests_EmojiInsideFencedCodeBlockNotSubstitutedTest()
        {
            // Markdig does not parse inline content inside fenced code blocks, so emoji shortcodes
            // and smileys should appear verbatim regardless of the Emojis option.
            const string markdown = "```\n:-)\n```";
            const string expected = """
                ┌───────────┐
                │ ┌───────┐ │
                │ │       │ │
                │ │   :-) │ │
                │ │       │ │
                │ └───────┘ │
                └───────────┘

                """;

            ConsoleUnderTest.Write(Renderer(markdown));

            AssertCrossPlatStringMatch(expected, ConsoleUnderTest.Output);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_ThematicBreakStyleTest(bool useCrazy)
        {
            // Render a thematic break and verify the dash segment produced by the Rule widget
            // carries the configured ThematicBreak style. The dashes are emitted as a single
            // continuous segment (not space-separated words), so the TestRenderHook helper that
            // splits on whitespace cannot match it directly; verify the style on the IRenderable
            // tree instead.
            const string markdown = "---";
            var options = useCrazy ? m_crazyOptions : new DisplayOptions();
            var expectedStyle = options.ThematicBreak.ToSpectreStyle();

            var root = Renderer(markdown, options);
            var segments = root.Render(new RenderOptions(NewConsole().Profile.Capabilities, new Size(360, 80)), maxWidth: 360).ToList();

            var dashSegment = segments.FirstOrDefault(s => s.Text.Length > 0 && s.Text.All(c => c == '─'));
            Assert.IsNotNull(dashSegment, $"Expected a dash segment from the thematic break.\nSegments: {string.Join("|", segments.Select(s => s.Text))}");
            Assert.AreEqual(expectedStyle.Foreground,  dashSegment.Style.Foreground);
            Assert.AreEqual(expectedStyle.Background,  dashSegment.Style.Background);
            Assert.AreEqual(expectedStyle.Decoration,  dashSegment.Style.Decoration);
        }

        [TestMethod]
        public void RendererTests_ThematicBreakNoTitleByDefault()
        {
            // By default the rule should be a plain dash line.
            const string markdown = "above\n\n---\n\nbelow";

            ConsoleUnderTest.Write(Renderer(markdown));

            Assert.Contains("─", ConsoleUnderTest.Output, "Rule line should be drawn.");
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_YamlFrontMatterShownTest(bool useCrazy)
        {
            // The YAML body lines should carry the YamlFrontMatter style.
            AssertMarkdownYieldsFormat(
                "yamlFrontMatter",
                "title: Example",
                new Style(decoration: Decoration.Italic | Decoration.Dim),
                useCrazy);
            AssertMarkdownYieldsFormat(
                "yamlFrontMatter",
                "author: Jane",
                new Style(decoration: Decoration.Italic | Decoration.Dim),
                useCrazy);
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

        private static IRenderable Renderer(string text, DisplayOptions? options = default)
        {
            options ??= new();
            options = options.Clone();
            options.IncludeDebug = true;
            var document = Markdown.Parse(text, MarkdownDisplayer.BuildPipeline(options));
            var renderer = new ConsoleRenderer(options);
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
            AbbreviationTitle = c_crazyFormat,
            Bold = c_crazyFormat,
            CodeBlock = c_crazyFormat,
            CodeInLine = c_crazyFormat,
            CustomContainer = c_crazyFormat,
            CustomContainerInfo = c_crazyFormat,
            CustomContainerInline = c_crazyFormat,
            DefinitionItem = c_crazyFormat,
            DefinitionList = c_crazyFormat,
            DefinitionTerm = c_crazyFormat,
            FencedCodeBlockInfo = c_crazyFormat,
            FigureCaption = c_crazyFormat,
            Footer = c_crazyFormat,
            Footnote = c_crazyFormat,
            FootnoteGroup = c_crazyFormat,
            FootnoteLink = c_crazyFormat,
            Header = (TextStyle)c_crazyFormat,
            HtmlBlock = c_crazyFormat,
            HtmlInline = c_crazyFormat,
            Inserted = c_crazyFormat,
            Italic = c_crazyFormat,
            Marked = c_crazyFormat,
            MathBlock = c_crazyFormat,
            MathBlockLabel = c_crazyFormat,
            MathBlockLabelText = "math",
            MathInline = c_crazyFormat,
            QuotedBlock = c_crazyFormat,
            ShowFencedCodeBlockInfo = true,
            Strikethrough = c_crazyFormat,
            Subscript = c_crazyFormat,
            Superscript = c_crazyFormat,
            ThematicBreak = c_crazyFormat,
            UnknownDelimiterChar = c_crazyFormat,
            UnknownDelimiterContent = c_crazyFormat,
            WrapHeader = false,
            YamlFrontMatter = c_crazyFormat,
        };

        private const string c_resources = "resources";
    }
}