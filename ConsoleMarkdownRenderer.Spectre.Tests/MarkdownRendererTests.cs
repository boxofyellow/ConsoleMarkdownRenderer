using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class MarkdownRendererTests : ConsoleTestBase
    {
        
        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_CodeInlineTest(bool useCrazy) 
            => AssertMarkdownYieldsFormat(
                "codeInline",
                "in line code",
                new Style(foreground: Color.Yellow, background: Color.Blue),
                useCrazy);

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_MathInlineTest(bool useCrazy) 
            => AssertMarkdownYieldsFormat(
                "mathInline",
                "E = mc^2",
                new Style(foreground: Color.Green, background: Color.Purple),
                useCrazy);

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_MathBlockTest(bool useCrazy) 
            => AssertMarkdownYieldsFormat(
                "mathBlock",
                "\\int_0^1 x^2 dx = \\frac{1}{3}",
                new Style(foreground: Color.Green, background: Color.Purple),
                useCrazy);

        [TestMethod]
        public void RendererTests_FencedCodeBlockInfoDisabledByDefault()
        {
            // By default, ShowFencedCodeBlockInfo is false, so info should not be shown
            const string markdown = "```csharp\nConsole.WriteLine(\"Hello\");\n```";
            ConsoleUnderTest.Write(Renderer(markdown));

            // Info should NOT appear in output
            Assert.DoesNotContain("csharp", ConsoleUnderTest.Output, "Info should not be shown when ShowFencedCodeBlockInfo is false");
        }

        [TestMethod]
        [DataRow("```python\nprint('hello')\n```",           "[python]",     "")]
        [DataRow("```javascript\nconsole.log('test');\n```", "[javascript]", "red on yellow")]
        public void RendererTests_FencedCodeBlockInfoEnabled(string markdown, string expectedText, string customStyle)
        {
            // When ShowFencedCodeBlockInfo is true, info should be shown with correct styling
            // The default FencedCodeBlockInfo is green on blue, but we can also set a custom style (red on yellow)
            var options = new SpectreDisplayOptions { ShowFencedCodeBlockInfo = true };

            if (!string.IsNullOrEmpty(customStyle))
            {
                options.FencedCodeBlockInfo = customStyle;
            }
            var expectedStyle = options.FencedCodeBlockInfo;

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
            var options = new SpectreDisplayOptions 
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
             => AssertMarkdownYieldsFormat(
                "emphasisInline",
                "marked",
                new Style(foreground: Color.Black, background: Color.Yellow),
                useCrazy);

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_HeaderTest(bool useCrazy)
        {
            // The default DisplayOptions configures H1 as a FigletTextStyle, so H1's literal
            // text ("Level One") is replaced by FIGlet ASCII art and is not asserted here.
            // H2 and H3 still fall through to the default Header style.
            AssertMarkdownYieldsFormat(
                "headingBlock",
                text: useCrazy
                    ? "Level One Level Two with code here Level Three with bold word"
                    : "## Level Two with code here ## ### Level Three with bold word ###",
                new Style(decoration: Decoration.Bold | Decoration.Invert | Decoration.Underline),
                useCrazy);
        }

        [TestMethod]
        public void RendererTests_LevelSpecificHeaderTest()
        {
            SpectreDisplayOptions options = new()
            {
                WrapHeader = false,
            };
            // Clear the default Headers list (which configures H1 as a FigletTextStyle) so the
            // for-loop below exercises the styled-markup path for the levels it specifies.
            options.Headers.Clear();
            options.Headers.Add((SpectreTextStyle)"blue on green");
            options.Headers.Add((SpectreTextStyle)"green on blue");

            string[] levels = ["One", "Two", "Three"];

            for (int index = 0; index < levels.Length; index++)
            {
                ISpectreHeaderStyle expected = index < options.Headers.Count 
                                                     ? options.Headers[index]
                                                     : options.Header;

                TestUtilities.AssertTheseMatch(expected, options.EffectiveHeader(index + 1), shouldMatch: true);

                AssertMarkdownYieldsFormat(
                        "headingBlock",
                        text: $"Level {levels[index]}",
                        expected.ToSpectreStyle(),
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
            ConsoleUnderTest.Write(Renderer(GetResourceContent("headingBlock", "md")));

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
            SpectreDisplayOptions options = new();
            Assert.IsInstanceOfType<SpectreFigletTextStyle>(options.EffectiveHeader(1));

            ConsoleUnderTest.Write(Renderer("#\n", options));

            var output = ConsoleUnderTest.Output;
            Assert.Contains("#", output,
                $"Empty H1 should fall back to styled '#'-wrapped markup:\n{output}");
        }

        [TestMethod]
        public async Task RendererTests_FigletFontPathLoadsCustomFont()
        {
            const string markdown = "# Hi\n";

            var fontPath = Path.Combine(DataPath, "fonts", "shadow.flf");
            Assert.IsTrue(File.Exists(fontPath), $"Test font file should exist at {fontPath}");

            var expectedPath = Path.Combine(DataPath, "expected", "figletCustomFont.txt");
            Assert.IsTrue(File.Exists(expectedPath), $"Expected output file should exist at {expectedPath}");
            var expected = File.ReadAllText(expectedPath);

            var customOptions = new SpectreDisplayOptions
            {
                Headers = new() { await SpectreFigletTextStyle.CreateAsync(fontPath) },
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
            var renderer = new MarkdownRenderer();
            var result = renderer.Render(GetResourceContent("linkInline", "md"));
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Root);

            Assert.IsGreaterThan(index, result.Links.Count, $"Expected at least {index + 1} links, got {result.Links.Count}");
            var link = result.Links[index];
            TestUtilities.AssertTheseMatch(expectedContent, link.Content, shouldMatch: true, message: $"Content mismatch at index {index}");
            TestUtilities.AssertTheseMatch(expectedUrl, link.Url, shouldMatch: true, message: $"Url mismatch at index {index}");
            TestUtilities.AssertTheseMatch(expectedIsImage, link.IsImage, shouldMatch: true, message: $"IsImage mismatch at index {index}");
        }


        [TestMethod]
        [DataRow(0, "https://example.com", "https://example.com")]
        [DataRow(1, "user@example.com",    "mailto:user@example.com")]
        public void RendererTests_AutolinkTest(int index, string expectedContent, string expectedUrl)
        {
            var renderer = new MarkdownRenderer();
            var result = renderer.Render(GetResourceContent("autolinkInline", "md"));
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Root);

            Assert.IsGreaterThan(index, result.Links.Count, $"Expected at least {index + 1} links, got {result.Links.Count}");
            var link = result.Links[index];
            TestUtilities.AssertTheseMatch(expectedContent, link.Content, shouldMatch: true, message: $"Content mismatch at index {index}");
            TestUtilities.AssertTheseMatch(expectedUrl, link.Url, shouldMatch: true, message: $"Url mismatch at index {index}");
            Assert.IsFalse(link.IsImage, $"IsImage should be false at index {index}");
        }

        [TestMethod]
        [DataRow("| - | - | - |"            , Justify.Left  , Justify.Left  , Justify.Left)]
        [DataRow("| :--- | :----: | ----: |", Justify.Left  , Justify.Center, Justify.Right)]
        [DataRow("| --- | --- | --- |"      , Justify.Left  , Justify.Left  , Justify.Left)]
        [DataRow("| ---: | :----: | :---- |", Justify.Right , Justify.Center, Justify.Left)]
        [DataRow("| :---: | ---- | :----: |", Justify.Center, Justify.Left  , Justify.Center)]
        public void RendererTests_TableColumnAlignment_HonorsMarkdownAlignment(string columnDefinition, Justify left, Justify middle, Justify right)
        {
            // Pipe-table column alignment specified in Markdown (`:---`, `:---:`, `---:`) should
            // be reflected in the Spectre.Console TableColumn.Alignment of the rendered table.
            string markdown =  $"| left | mid | right |\n{columnDefinition}\n| a | b | c |\n";

            var renderer = new MarkdownRenderer();
            var result = renderer.Render(markdown);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Root);

            var outer = (Table)result.Root;
            var inner = (Table)outer.Rows.First().First();

            TestUtilities.AssertTheseMatch(3, inner.Columns.Count, shouldMatch: true, message: "Expected three table columns");
            TestUtilities.AssertTheseMatch(left, inner.Columns[0].Alignment, shouldMatch: true, message: "First column");
            TestUtilities.AssertTheseMatch(middle, inner.Columns[1].Alignment, shouldMatch: true, message: "Second column");
            TestUtilities.AssertTheseMatch(right, inner.Columns[2].Alignment, shouldMatch: true, message: "Third column");
        }

        [TestMethod]
        public void RendererTests_TableBorder_MapsToSpectreNamedBorder()
        {
            const string markdown = "| a | b |\n| - | - |\n| 1 | 2 |\n";

            foreach (TableBorder border in Mappings.TableBorders.NameMap.Forward.Values)
            {
                var renderer = new MarkdownRenderer();
                var result = renderer.Render(markdown, new SpectreDisplayOptions { TableBorder = border });
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Root);

                var outer = (Table)result.Root!;
                var inner = (Table)outer.Rows.First().First();
                TestUtilities.AssertTheseMatch(border, inner.Border, shouldMatch: true, message: "Table border");
            }
        }

        [TestMethod]
        public void RendererTests_TableBorderStyle_AppliedToRenderedTable()
        {
            // TableBorderStyle should be passed through to the rendered table's BorderStyle.
            var options = new SpectreDisplayOptions
            {
                TableBorderStyle = new Style(foreground: Color.Red, decoration: Decoration.Bold),
            };

            const string markdown = "| a | b |\n| - | - |\n| 1 | 2 |\n";

            var renderer = new MarkdownRenderer();
            var result = renderer.Render(markdown, options);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Root);

            var outer = (Table)result.Root!;
            var inner = (Table)outer.Rows.First().First();

            Assert.IsNotNull(inner.BorderStyle, "BorderStyle should be set when TableBorderStyle is provided.");
            var borderStyle = inner.BorderStyle.Value;
            TestUtilities.AssertTheseMatch(Color.Red, borderStyle.Foreground, shouldMatch: true,
                message: "Border foreground should reflect TableBorderStyle.Foreground.");
            Assert.IsTrue(borderStyle.Decoration.HasFlag(Decoration.Bold),
                "Border decoration should reflect TableBorderStyle.Decoration.");
        }

        [TestMethod]
        public void RendererTests_TableExpand_DefaultsToFalse()
        {
            const string markdown = "| a | b |\n| - | - |\n| 1 | 2 |\n";

            var renderer = new MarkdownRenderer();
            var result = renderer.Render(markdown, new SpectreDisplayOptions());
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Root);

            var outer = (Table)result.Root!;
            var inner = (Table)outer.Rows.First().First();

            Assert.IsFalse(inner.Expand, "Table.Expand should default to false.");
        }

        [TestMethod]
        public void RendererTests_TableExpand_OptIn()
        {
            const string markdown = "| a | b |\n| - | - |\n| 1 | 2 |\n";

            var renderer = new MarkdownRenderer();
            var result = renderer.Render(markdown, new SpectreDisplayOptions { TableExpand = true });
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Root);

            var outer = (Table)result.Root!;
            var inner = (Table)outer.Rows.First().First();

            Assert.IsTrue(inner.Expand, "Table.Expand should be true when TableExpand is set.");
        }



        [TestMethod]
        public void RendererTests_TerminalHyperlinks_DefaultEnabled()
        {
            var output = RenderMarkdownWithLinkCapableConsole("[two](http://two.example/)", new SpectreDisplayOptions());

            // OSC 8 hyperlink wraps with ESC ] 8 ; <params> ; <url> ESC \ ... ESC ] 8 ; ; ESC \
            Assert.Contains("\u001B]8;", output, "Expected OSC 8 open sequence in output");
            Assert.Contains("http://two.example/", output, "Expected URL in OSC 8 sequence");
            Assert.Contains("\u001B]8;;\u001B\\", output, "Expected OSC 8 close sequence in output");
        }

        [TestMethod]
        public void RendererTests_TerminalHyperlinks_OptOut()
        {
            // When UseTerminalHyperlinks is false, no OSC 8 escape sequences should be emitted.
            var options = new SpectreDisplayOptions { UseTerminalHyperlinks = false };
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
            var output = RenderMarkdownWithLinkCapableConsole("<https://auto.example/>", new SpectreDisplayOptions());

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
            var output = RenderMarkdownWithLinkCapableConsole($"[label](<{url}>)", new SpectreDisplayOptions());

            // The URL should appear unmodified inside the OSC 8 escape sequence.
            Assert.Contains($"\u001B]8;id=", output, "Expected OSC 8 open sequence");
            Assert.Contains(url, output, $"Expected the unescaped URL '{url}' in the output");
        }


        private string RenderMarkdownWithLinkCapableConsole(string markdown, SpectreDisplayOptions options)
        {
            // Configure the test console to actually emit ANSI sequences and advertise OSC 8
            // hyperlink support, so the rendered Output contains the escape sequences we are
            // asserting on.
            ConsoleUnderTest.EmitAnsiSequences = true;
            ConsoleUnderTest.Profile.Capabilities.Links = true;
            ConsoleUnderTest.Write(Renderer(markdown, options));
            return ConsoleUnderTest.Output;
        }

        [TestMethod]
        [DataRow("quote 2." , Decoration.Italic)]
        [DataRow("should even" , Decoration.Italic | Decoration.Bold)]
        public void RendererTests_QuoteBlockTest(string text, Decoration decoration) 
        {
            foreach (bool useCrazy in new[] { false, true })
            {
                AssertMarkdownYieldsFormat("quoteBlock", text, new Style(decoration: decoration), useCrazy);
                AssertMarkdownYieldsFormat("quoteBlock", text, new Style(decoration: decoration), useCrazy);
            }
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
        public void RendererTests_CustomContainerInlineTest(bool useCrazy) =>
            // Inline custom container content (::tag inline::) carries the CustomContainerInline style (bold by default)
            AssertMarkdownYieldsFormat("customContainer", "tag inline", new Style(decoration: Decoration.Bold), useCrazy);

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
        public void RendererTests_UnhandledTypeDetectedTest()
        {
            var options = new SpectreDisplayOptions { IncludeDebug = true };
            var renderer = new MarkdownRenderer();
            var result = renderer.Render(
                "<https://example.com>",
                options,
                new ConsoleRenderer(options, omitAutolinkInlineRenderer: true));
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Root);

            Assert.IsNotNull(result.UnhandledTypes, "Should have detected at least one unhandled type");
            Assert.Contains(
                "AutolinkInline",
                result.UnhandledTypes.Select(t => t.Name), 
                $"Expected AutolinkInline to be in unhandled types; got: {string.Join(", ", result.UnhandledTypes.Select(t => t.Name))}");
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
            var options = useCrazy ? TestUtilities.Crazy : new SpectreDisplayOptions();
            var expectedStyle = options.ThematicBreak;

            var root = Renderer(markdown, options);
            var segments = root.Render(new RenderOptions(ConsoleUnderTest.Profile.Capabilities, new Size(360, 80)), maxWidth: 360).ToList();

            var dashSegment = segments.FirstOrDefault(s => s.Text.Length > 0 && s.Text.All(c => c == '─'));
            Assert.IsNotNull(dashSegment, $"Expected a dash segment from the thematic break.\nSegments: {string.Join("|", segments.Select(s => s.Text))}");
            TestUtilities.AssertTheseMatch(expectedStyle.Foreground,  dashSegment.Style.Foreground, shouldMatch: true);
            TestUtilities.AssertTheseMatch(expectedStyle.Background,  dashSegment.Style.Background, shouldMatch: true);
            TestUtilities.AssertTheseMatch(expectedStyle.Decoration,  dashSegment.Style.Decoration, shouldMatch: true);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RendererTests_YamlFrontMatterShownTest(bool useCrazy)
            => AssertMarkdownYieldsFormat(
                "yamlFrontMatter",
                "title: Example",
                new Style(decoration: Decoration.Italic | Decoration.Dim),
                useCrazy);

        private void AssertMarkdownYieldsFormat(string name, string text, Style style, bool useCrazy, SpectreDisplayOptions? options = null)
        {
            Style format = useCrazy ? TestUtilities.CrazyFormat : style;
            options ??= useCrazy ? TestUtilities.Crazy : new SpectreDisplayOptions();
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

        private static IRenderable Renderer(string text, SpectreDisplayOptions? options = default)
        {
            options = options?.Clone() ?? new();
            options.IncludeDebug = true;
            var renderer = new MarkdownRenderer();
            var result = renderer.Render(text, options);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Root);
            if (result.UnhandledTypes.Count > 0)
            {
                Assert.Fail($"Found Unhandled Types {string.Join(Environment.NewLine, result.UnhandledTypes)}");
            }
            if (result.UnknownEmphasisDelimiters.Count > 0)
            {
                Assert.Fail($"Found Unknown Emphasis Delimiters {string.Join(Environment.NewLine, result.UnknownEmphasisDelimiters)}");
            }   
            return result.Root;
        }

        private class TestRenderHook : IRenderHook
        {
            public TestRenderHook(string text, Style style, bool compareDefaultColors = false)
            {
                m_text = text;
                m_style = style;
                m_compareDefaultColors = compareDefaultColors;
                m_counts = Counts(text);
            }

            public void AssertFormattedTextFound() 
                => TestUtilities.AssertTheseMatch(m_counts.Sum(x => x.Value), m_count, shouldMatch: true, message: $"Failed to find {m_text} with the correct style {m_style.ToMarkup()} {m_count} != {m_counts.Sum(x => x.Value)}");

            public IEnumerable<IRenderable> Process(RenderOptions options, IEnumerable<IRenderable> renderables)
            {
                m_count = renderables
                    .SelectMany(x => x.Render(options, maxWidth: 360))
                    .Count(Check);
                return renderables;
            }

            private bool Check(Segment segment)
            {
                Logger.LogMessage($"[{segment.Style.ToMarkup()}:{segment.Text}]");
                if (!m_counts.ContainsKey(segment.Text))
                {
                    return false;
                }
                // Segment styles may include colors inherited from the rendering context (e.g. background).
                // Only compare default colors when the caller explicitly opts in.
                var seg = segment.Style;
                if ((m_compareDefaultColors || m_style.Foreground != Color.Default) && m_style.Foreground != seg.Foreground)
                {
                    return false;
                }
                if ((m_compareDefaultColors || m_style.Background != Color.Default) && m_style.Background != seg.Background)
                {
                    return false;
                }
                if (m_style.Decoration == seg.Decoration)
                {
                    Logger.LogMessage("*");
                }
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

        private const string c_resources = "resources";
    }
}