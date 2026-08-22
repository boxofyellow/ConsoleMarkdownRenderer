using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class ConventionTests
{
    [TestMethod]
    [Timeout(TestTimeouts.Unit)]
    public void Assert_Namespaces() => TestUtilities.AssertTestNamespaceMatch(GetType());

    [TestMethod]
    [Timeout(TestTimeouts.Unit)]
    public void Assert_All_Test_Methods_Have_Timeouts() => ConventionsHelper.AssertAllTestMethodsHaveTimeouts(GetType().Assembly);

    [TestMethod]
    [Timeout(TestTimeouts.Unit)]
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
    [Timeout(TestTimeouts.Unit)]
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