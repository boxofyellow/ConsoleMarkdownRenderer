using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class ConventionTests
    {
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
    }
}