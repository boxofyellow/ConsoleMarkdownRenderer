using System.Text;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console.Testing;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Shared primitive for "render this Markdown and assert it does not blow up" - the same check
/// every detection technique that builds on this harness needs. Extends <see cref="ConsoleTestBase"/>
/// (reusing its <see cref="ConsoleTestBase.NewConsole()"/>/cleanup machinery) rather than managing a
/// separate console.
/// </summary>
public static class RenderRobustness
{
    /// <summary>
    /// Width wide enough that lines never wrap or get truncated. Matches the width
    /// <see cref="ConsoleTestBase"/> already uses so callers see consistent, unwrapped layout.
    /// </summary>
    public const int WideWidth = 360;

    /// <summary>
    /// Width narrow enough to force wrapping and truncation. Layout-time failures (as opposed to
    /// parse/render-time failures) tend to be width-sensitive, so callers should exercise this
    /// width in addition to <see cref="WideWidth"/>.
    /// </summary>
    public const int NarrowWidth = 10;

    /// <summary>
    /// The small shared set of widths callers should render at.
    /// </summary>
    public static readonly IReadOnlyList<int> Widths = [WideWidth, NarrowWidth];

    /// <summary>
    /// Renders <paramref name="markdown"/> and asserts none of it "blows up": parsing/rendering
    /// must produce a non-null result and root, writing that root to a <see cref="TestConsole"/>
    /// at <paramref name="width"/> must not throw, <see cref="MarkdownRenderResult.UnhandledTypes"/>
    /// must be empty, and rendering twice must produce identical console output (catching order- or
    /// state-dependence). On any failure the assertion message includes the exact input (escaped
    /// and length-bounded), the width, a description of the options used (as a diff against
    /// <c>new SpectreDisplayOptions()</c>), and <paramref name="caseLabel"/> so generated/looped
    /// callers can trace a failure back to it.
    /// </summary>
    /// <param name="console">The test's <see cref="ConsoleTestBase"/>, used for console swapping/cleanup.</param>
    /// <param name="markdown">The Markdown text to render.</param>
    /// <param name="options">The display options to render with, or <see langword="null"/> for the renderer's defaults.</param>
    /// <param name="width">The console width to write the rendered output at.</param>
    /// <param name="caseLabel">A caller-supplied label (e.g. a resource name or seed) so failures stay traceable back to their source.</param>
    /// <param name="renderer">The renderer to use, or <see langword="null"/> to use a new <see cref="MarkdownRenderer"/>.</param>
    public static void AssertRendersRobustly(
        this ConsoleTestBase console,
        string markdown,
        SpectreDisplayOptions? options,
        int width,
        string caseLabel,
        ISpectreMarkdownRenderer? renderer = null)
    {
        ArgumentNullException.ThrowIfNull(console);
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(caseLabel);

        renderer ??= new MarkdownRenderer();

        var firstOutput = RenderOnce(console, markdown, options, width, caseLabel, renderer);
        var secondOutput = RenderOnce(console, markdown, options, width, caseLabel, renderer);

        Assert.AreEqual(
            firstOutput,
            secondOutput,
            BuildFailureMessage(markdown, options, width, caseLabel, "Rendering the same input twice produced different output"));
    }

    private static string RenderOnce(
        ConsoleTestBase console,
        string markdown,
        SpectreDisplayOptions? options,
        int width,
        string caseLabel,
        ISpectreMarkdownRenderer renderer)
    {
        MarkdownRenderResult result;
        try
        {
            result = renderer.Render(markdown, options);
        }
        catch (Exception ex)
        {
            Assert.Fail(BuildFailureMessage(markdown, options, width, caseLabel, "Render(...) threw", ex));
            throw; // unreachable - Assert.Fail always throws
        }

        Assert.IsNotNull(result, BuildFailureMessage(markdown, options, width, caseLabel, "Render(...) returned a null result"));
        Assert.IsNotNull(result.Root, BuildFailureMessage(markdown, options, width, caseLabel, "Render(...) returned a null Root"));
        Assert.IsEmpty(
            result.UnhandledTypes,
            BuildFailureMessage(
                markdown,
                options,
                width,
                caseLabel,
                $"Found unhandled types: {string.Join(", ", result.UnhandledTypes.Select(t => t.Name))}"));

        var testConsole = console.NewConsole(width);
        try
        {
            testConsole.Write(result.Root!);
        }
        catch (Exception ex)
        {
            Assert.Fail(BuildFailureMessage(markdown, options, width, caseLabel, "Writing Root to the console threw", ex));
            throw; // unreachable - Assert.Fail always throws
        }

        return testConsole.Output;
    }

    // Bounds how much of the input we echo back in a failure message so a huge generated/mutated
    // input (once later suites in #302 start feeding this harness) doesn't produce an unreadable wall of text.
    private const int MaxEchoedInputLength = 500;

    private static string BuildFailureMessage(
        string markdown,
        SpectreDisplayOptions? options,
        int width,
        string caseLabel,
        string reason,
        Exception? exception = null)
    {
        var builder = new StringBuilder()
            .Append(reason)
            .Append(" [case: ").Append(caseLabel).Append(']')
            .Append(" [width: ").Append(width).Append(']')
            .Append(" [options: ").Append(DescribeOptions(options)).Append(']')
            .Append(" [input: ").Append(EscapeAndTruncate(markdown)).Append(']');

        if (exception is not null)
        {
            builder.Append(Environment.NewLine).Append(exception);
        }

        return builder.ToString();
    }

    // SpectreDisplayOptions is sealed (there is only ever one type here) and its default
    // Equals/GetHashCode aren't useful for a failure message: the hash tells you nothing about
    // the values involved and (since the type holds strings/collections) isn't even stable
    // across runs. Full JSON serialization is too verbose for a one-line failure message.
    // Instead, describe the options as a diff against `new SpectreDisplayOptions()` so the
    // message only grows with the properties the caller actually customized.
    private static string DescribeOptions(SpectreDisplayOptions? options)
    {
        if (options is null)
        {
            return "<default>";
        }

        var defaults = new SpectreDisplayOptions();
        var differences = Mappings.SpectreDisplayOptionsProperties
            .Where(property => !PropertyValueMatchesDefault(property.Value, defaults, options))
            .Select(property => $"{property.Key}={DescribePropertyValue(property.Value.Getter(options))}")
            .ToList();

        return differences.Count == 0 ? "<matches default>" : string.Join(", ", differences);
    }

    private static bool PropertyValueMatchesDefault(
        (Type Type, Action<SpectreDisplayOptions, object> Setter, Func<SpectreDisplayOptions, object> Getter) property,
        SpectreDisplayOptions defaults,
        SpectreDisplayOptions options)
    {
        var defaultValue = property.Getter(defaults);
        var actualValue = property.Getter(options);
        return property.Type == typeof(List<ISpectreHeaderStyle>)
            ? ((List<ISpectreHeaderStyle>)defaultValue).SequenceEqual((List<ISpectreHeaderStyle>)actualValue)
            : Equals(defaultValue, actualValue);
    }

    private static string DescribePropertyValue(object value)
        => value is List<ISpectreHeaderStyle> list
            ? $"[{string.Join(", ", list)}]"
            : value.ToString() ?? "null";

    private static string EscapeAndTruncate(string text)
    {
        var isTruncated = text.Length > MaxEchoedInputLength;
        var slice = isTruncated ? text[..MaxEchoedInputLength] : text;

        var escaped = slice
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");

        return isTruncated
            ? $"\"{escaped}\" (truncated, {text.Length} chars total)"
            : $"\"{escaped}\"";
    }
}
