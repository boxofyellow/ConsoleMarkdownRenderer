using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Reflection;
using System.Text.RegularExpressions;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class ConventionTests
{
    private static readonly Regex s_lineBackingArrayCapacityRead = new(
        @"\bLines\s*\.\s*Lines\s*\.\s*Length\b",
        RegexOptions.CultureInvariant);

    [TestMethod]
    public void Assert_Namespaces() => TestUtilities.AssertTestNamespaceMatch(GetType());

    [TestMethod]
    public void Check_For_Convention_Violations()
    {
        var allowedApiLeaks = new Type[] {
            typeof(IRenderable),
            typeof(Style),
            typeof(Color),
            typeof(TableBorder),
            typeof(BoxBorder),
            typeof(Decoration),
            typeof(FigletFont),
            typeof(TableBorderPart),
            typeof(TablePart),
            typeof(IColumn),
            typeof(Justify),
        };

        // We should only allow leaking Spectre.Console types
        Assert.IsTrue(allowedApiLeaks.All(t => t.Namespace?.StartsWith("Spectre.Console") ?? false));

        ConventionsHelper.FindViolations<SpectreSourceFileAttribute>(
                        typeof(MarkdownRenderer),
                        attr => attr.FilePath,
                        allowedPublicTypes: [
                            typeof(ISpectreMarkdownRenderer),
                            typeof(LinkItem),
                            typeof(MarkdownRenderer),
                            typeof(MarkdownRenderResult),
                            typeof(SpectreDisplayOptions),
                            typeof(UnknownEmphasisDelimiter),
                            typeof(BidirectionalMap<,>),
                            typeof(ColorJsonConverterBase<>),
                            typeof(Extensions),
                            typeof(HeaderStyleJsonConverterBase<>),
                            typeof(IDefaultIdentifier),
                            typeof(JsonWriteHelpers),
                            typeof(MappedJsonConverterBase<>),
                            typeof(Mappings),
                            typeof(NamedTypeCollection<>),
                            typeof(NamedTypeJsonConverterBase<>),
                            typeof(PathComparison),
                            typeof(DefaultTableBorder),
                            typeof(QuoteBlockTableBorder),
                            typeof(ISpectreHeaderStyle),
                            typeof(SpectreFigletTextStyle),
                            typeof(SpectreRuleHeaderStyle),
                            typeof(SpectreTextStyle),
                            typeof(Utilities),
                        ],
                        allowedPublicFolders: [
                            "Support",
                            "Styling",
                        ],
                        allowedClassFileNameMismatch: [
                            "ConsoleObjectRenderers",
                        ],
                        allowedStaticFolders: [],
                        allowedApiLeaks);
    }

    [TestMethod]
    public void Renderer_Source_Does_Not_Use_Line_Backing_Array_Capacity()
    {
        var violations = GetSpectreSourceFiles()
            .SelectMany(source => s_lineBackingArrayCapacityRead
                .Matches(source.Contents)
                .Select(match => $"{source.Path}:{GetLineNumber(source.Contents, match.Index)}"))
            .ToArray();

        Assert.IsFalse(
            violations.Length > 0,
            $"Renderer source must use Lines.Count for the populated line count. " +
            $"Lines.Lines is a capacity-sized backing array that Markdig may leave uninitialized. " +
            $"Indexing Lines.Lines[i] remains valid when i is bounded by Lines.Count.{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [TestMethod]
    public void Renderers_Register_Derived_Markdown_Types_Before_Their_Base_Types()
    {
        var registrations = new ConsoleRenderer(new SpectreDisplayOptions())
            .ObjectRenderers
            .OfType<IConsoleObjectRenderer>()
            .Select((renderer, index) => new RendererRegistration(renderer, GetHandledType(renderer), index))
            .ToArray();

        var violations = (
            from derived in registrations
            from @base in registrations
            where derived.HandledType != @base.HandledType
                && @base.HandledType.IsAssignableFrom(derived.HandledType)
                && derived.Index > @base.Index
            select $"{derived.Renderer.GetType().Name} handles {derived.HandledType.Name}, which derives from " +
                   $"{@base.HandledType.Name} handled by {@base.Renderer.GetType().Name}. " +
                   $"Register {derived.Renderer.GetType().Name} before {@base.Renderer.GetType().Name} because " +
                   "Markdig dispatches to the first renderer whose handled type is assignable from the node."
            ).ToArray();

        Assert.IsFalse(
            violations.Length > 0,
            "Renderer registrations must put derived Markdown types before their handled base types." +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<(string Path, string Contents)> GetSpectreSourceFiles()
        => typeof(MarkdownRenderer).Assembly
            .GetTypes()
            .Select(type => type.GetCustomAttribute<SpectreSourceFileAttribute>())
            .Where(attribute => attribute is not null)
            .Select(attribute => attribute!.FilePath)
            .Distinct(StringComparer.Ordinal)
            .Select(path => (path, File.ReadAllText(path)));

    private static int GetLineNumber(string contents, int characterIndex)
        => contents.AsSpan(0, characterIndex).Count('\n') + 1;

    private static Type GetHandledType(IConsoleObjectRenderer renderer)
    {
        var rendererBase = renderer.GetType()
            .BaseType;

        while (rendererBase is not null
            && (!rendererBase.IsGenericType
                || rendererBase.GetGenericTypeDefinition() != typeof(ConsoleObjectRendererBase<>)))
        {
            rendererBase = rendererBase.BaseType;
        }

        Assert.IsNotNull(
            rendererBase,
            $"{renderer.GetType().Name} must inherit from {typeof(ConsoleObjectRendererBase<>).Name}.");
        return rendererBase!.GetGenericArguments()[0];
    }

    private sealed record RendererRegistration(IConsoleObjectRenderer Renderer, Type HandledType, int Index);
}