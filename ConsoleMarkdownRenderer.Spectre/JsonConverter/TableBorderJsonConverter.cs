using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter
{
    [SpectreSourceFile]
    internal sealed class TableBorderJsonConverter : NamedTypeJsonConverterBase<TableBorder>
    {
        protected override IReadOnlyDictionary<string, TableBorder> ByType => Mappings.TableBorders.TypeNameMap.Forward;
        protected override IReadOnlyDictionary<string, TableBorder> ByName => Mappings.TableBorders.NameMap.Forward;
        protected override IReadOnlyDictionary<TableBorder, string> ValueToName => Mappings.TableBorders.NameMap.Reverse;
        public override bool? IsDefault(object value) 
            => value switch
            {
                TableBorder border => border == DefaultTableBorder.Default,
                _ => null
            };
    }
}
