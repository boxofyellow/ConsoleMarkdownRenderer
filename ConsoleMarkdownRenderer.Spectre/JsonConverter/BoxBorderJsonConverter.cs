using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter
{
    [SpectreSourceFile]
    internal sealed class BoxBorderJsonConverter : NamedTypeJsonConverterBase<BoxBorder>
    {
        protected override IReadOnlyDictionary<string, BoxBorder> ByType => Mappings.BoxBorders.TypeNameMap.Forward;
        protected override IReadOnlyDictionary<string, BoxBorder> ByName => Mappings.BoxBorders.NameMap.Forward;
        protected override IReadOnlyDictionary<BoxBorder, string> ValueToName => Mappings.BoxBorders.NameMap.Reverse;

        public override bool? IsDefault(object value) => null;
    }
}