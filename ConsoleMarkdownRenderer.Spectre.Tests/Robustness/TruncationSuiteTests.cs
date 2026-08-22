using System.Reflection;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Deterministic truncation suite for #303. #300 came from streaming Markdown: a consumer
/// rendered a document that had not finished arriving, so the input was a *prefix* of valid
/// Markdown. This suite models that scenario for every construct the renderer supports: for one
/// small valid example per construct, it renders every construct-aware truncation point (not
/// every character - that would be unreadable and slow) through
/// <see cref="RenderRobustness.AssertRendersRobustly"/>, in trailing-newline-present/absent and
/// (where the prefix spans multiple lines) LF/CRLF forms.
/// </summary>
/// <remarks>
/// Per the working agreement on #303, this suite only ever ships green: every case here already
/// passed a local discovery run. Cases that failed during discovery are filed as issues (see
/// #305's fix-PR template) and are deliberately not represented here - a suite that ships red
/// teaches everyone to ignore it.
/// </remarks>
[TestClass]
public class TruncationSuiteTests : ConsoleTestBase
{
    // The case matrix (construct x truncation point x newline/line-ending variant x width) is
    // generated from s_breakpoints rather than hand-written per case, so DynamicData drives it
    // instead of a hand-maintained DataRow list - keeping s_breakpoints the single place a new
    // construct or truncation point gets added.
    [TestMethod]
    [DynamicData(nameof(GetCases), DynamicDataDisplayName = nameof(GetCaseDisplayName))]
    public void TruncatedInput_RendersRobustly(int width, string markdown, string caseLabel)
        => this.AssertRendersRobustly(markdown, options: null, width, caseLabel);

    public static string GetCaseDisplayName(MethodInfo methodInfo, object?[] data)
    {
        Assert.IsTrue(data.Length >= 3, "Expected (width, markdown, caseLabel) test data");
        return $"{methodInfo.Name} ({data[2]}, width={data[0]})";
    }

    public static IEnumerable<object[]> GetCases()
    {
        foreach (var truncationCase in BuildCases())
        {
            foreach (var width in RenderRobustness.Widths)
            {
                yield return [width, truncationCase.Markdown, $"{truncationCase.Construct}/{truncationCase.Breakpoint}"];
            }
        }
    }

    private sealed record TruncationCase(string Construct, string Breakpoint, string Markdown);

    // One small valid example per construct required by #303, truncated at the construct-aware
    // points a streaming consumer could plausibly have stopped at. Every prefix below is a literal
    // prefix (from index 0) of the small valid example named in its comment, so each row models a
    // real "document hasn't finished arriving yet" shape rather than an arbitrary string.
    private static readonly (string Construct, string Breakpoint, string Prefix)[] s_breakpoints =
    [
        // Full: "```csharp\nConsole.WriteLine(\"Hi\");\n```\n"
        ("FencedCodeBlock", "AfterOpeningFence",       "```"                                          ), // the #300 shape: unclosed, empty, no info string, no trailing newline
        ("FencedCodeBlock", "AfterInfoString",         "```csharp"                                    ),
        ("FencedCodeBlock", "MidBody",                 "```csharp\nConsole.WriteLine(\"Hi\""           ),
        ("FencedCodeBlock", "BeforeClosingFence",      "```csharp\nConsole.WriteLine(\"Hi\");\n"       ),

        // Full: "[label](https://example.com/path)\n"
        ("LinkInline", "AfterOpenBracket", "["                          ),
        ("LinkInline", "AfterLabel",       "[label"                     ),
        ("LinkInline", "AfterOpenParen",   "[label]("                   ),
        ("LinkInline", "MidUrl",           "[label](https://exam"       ),

        // Full: "![alt](https://example.com/img.png)\n"
        ("ImageInline", "AfterBangBracket", "!["                        ),
        ("ImageInline", "AfterAltText",     "![alt"                     ),
        ("ImageInline", "AfterOpenParen",   "![alt]("                   ),
        ("ImageInline", "MidUrl",           "![alt](https://exam"       ),

        // Full: "<div>\n  content\n</div>\n"
        ("HtmlBlock", "InsideOpeningTag", "<div"                        ),
        ("HtmlBlock", "MidBody",         "<div>\n  cont"                ),
        ("HtmlBlock", "InsideClosingTag", "<div>\n  content\n</div"     ),

        // Full: "Text with <span>html</span> inline.\n"
        //
        // Only the unterminated-opening-tag breakpoint is included here. Discovery on this branch
        // also tried truncating after the opening tag's content ("Text with <span>html") and
        // inside the closing tag ("Text with <span>html</spa") - both throw
        // `InvalidOperationException: Unbalanced markup stack` because the inline HTML renderer
        // pushes a Spectre style scope for the recognized opening tag and relies on a matching
        // closing-tag node (absent from a truncated parse tree) to pop it. That is a real bug,
        // filed as #311 per the #303 working agreement; it is fixed (with these two cases added
        // back) in a follow-up PR using the #305 template, not here.
        ("HtmlInline", "InsideOpeningTag", "Text with <span"            ),

        // Full: "| a | b |\n|---|---|\n| 1 | 2 |\n"
        ("Table", "AfterHeaderRow",    "| a | b |"                        ),
        ("Table", "AfterDelimiterRow", "| a | b |\n|---|---|"             ),
        ("Table", "MidRow",            "| a | b |\n|---|---|\n| 1 "       ),
        ("Table", "MidCell",           "| a | b |\n|---|---|\n| 1 | 2"    ),

        // Full: "---\ntitle: Example\n---\n\n# Hello\n"
        ("YamlFrontMatter", "AfterOpeningMarker",  "---"                       ),
        ("YamlFrontMatter", "MidDocument",         "---\ntitle: Exam"          ),
        ("YamlFrontMatter", "BeforeClosingMarker", "---\ntitle: Example\n"     ),

        // Full: "**bold**\n"
        ("Emphasis", "UnmatchedSingleDelimiter",        "*"        ),
        ("Emphasis", "UnmatchedOpenDelimiter",           "**"       ),
        ("Emphasis", "UnterminatedContent",              "**bold"   ),
        ("Emphasis", "PartiallyMatchedClosingDelimiter", "**bold*"  ),

        // Full: "私は**「重要」**だと思う\n"
        ("CjkFriendlyEmphasis", "UnmatchedSingleDelimiter",        "私は*"              ),
        ("CjkFriendlyEmphasis", "UnmatchedOpenDelimiter",          "私は**"             ),
        ("CjkFriendlyEmphasis", "UnterminatedContent",             "私は**「重要」"      ),
        ("CjkFriendlyEmphasis", "PartiallyMatchedClosingDelimiter", "私は**「重要」*"    ),

        // Full: ":::note\nThis is a sample admonition.\n:::\n"
        ("CustomContainer", "UnmatchedSingleColon", ":"                                          ), // partial opening marker, mirrors Emphasis's UnmatchedSingleDelimiter
        ("CustomContainer", "UnmatchedDoubleColon", "::"                                         ), // partial opening marker, mirrors Emphasis's UnmatchedOpenDelimiter
        ("CustomContainer", "AfterOpeningMarker",  ":::note"                                    ),
        ("CustomContainer", "MidBody",             ":::note\nThis is a sample admo"             ),
        ("CustomContainer", "BeforeClosingMarker", ":::note\nThis is a sample admonition.\n"     ),

        // Full: "> [!NOTE]\n> Useful information.\n"
        ("AlertBlock", "UnterminatedMarkerType", "> [!"                            ), // truncated before the alert type (NOTE/TIP/...) has arrived
        ("AlertBlock", "AfterMarkerLine", "> [!NOTE]"                       ),
        ("AlertBlock", "MidBody",         "> [!NOTE]\n> Useful inform"      ),

        // Full: "Ref[^1].\n\n[^1]: Note text.\n"
        ("Footnote", "UnterminatedReference", "Ref[^1"                     ),
        ("Footnote", "AfterDefinitionMarker", "Ref[^1].\n\n[^1]:"          ),
        ("Footnote", "MidDefinitionBody",     "Ref[^1].\n\n[^1]: Note"     ),

        // Full: "Term\n:   Definition text.\n"
        ("DefinitionList", "AfterMarkerNoContent", "Term\n:"          ),
        ("DefinitionList", "MidContent",           "Term\n:   Defin"  ),

        // Full: "$$\nx^2\n$$\n"
        ("MathBlock", "AfterOpeningMarker", "$$"       ),
        ("MathBlock", "MidBody",            "$$\nx^2"  ),

        // Full: "Value $E = mc^2$ here.\n"
        ("MathInline", "UnterminatedDelimiter", "Value $E = mc"),

        // Full: "- item one\n- item two\n"
        ("BulletList", "MarkerNoSpace",   "-"   ),
        ("BulletList", "MarkerNoContent", "- "  ),

        // Full: "- [ ] task one\n- [x] task two\n"
        ("TaskList", "MarkerIncomplete",  "- ["  ),
        ("TaskList", "MarkerNoContent",   "- [ ]"),
    ];

    private static IEnumerable<TruncationCase> BuildCases()
        => s_breakpoints.SelectMany(breakpoint => ExpandLineEndingVariants(breakpoint.Construct, breakpoint.Breakpoint, breakpoint.Prefix));

    // Each breakpoint gets a trailing-newline-absent and a trailing-newline-present variant - the
    // absent case is what actually triggered #300, so per #303 it must not be optional. A CRLF
    // variant is only distinct from its LF sibling when the prefix spans multiple lines (a
    // single-line prefix has no embedded "\n" for CRLF to change), so it is added only "where
    // practical" per #303, rather than doubling every single-line case with an identical copy.
    private static IEnumerable<TruncationCase> ExpandLineEndingVariants(string construct, string breakpoint, string prefix)
    {
        var withoutTrailingNewline = prefix.TrimEnd('\n');
        yield return new TruncationCase(construct, $"{breakpoint} (LF, no trailing newline)", withoutTrailingNewline);
        yield return new TruncationCase(construct, $"{breakpoint} (LF, trailing newline)", withoutTrailingNewline + "\n");

        if (withoutTrailingNewline.Contains('\n'))
        {
            var crlf = withoutTrailingNewline.Replace("\n", "\r\n");
            yield return new TruncationCase(construct, $"{breakpoint} (CRLF, no trailing newline)", crlf);
            yield return new TruncationCase(construct, $"{breakpoint} (CRLF, trailing newline)", crlf + "\r\n");
        }
    }
}
