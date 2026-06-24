using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter
{
    [SpectreSourceFile]
    internal sealed class SpectreHeaderStyleJsonConverter : HeaderStyleJsonConverterBase<ISpectreHeaderStyle>
    {
        public SpectreHeaderStyleJsonConverter() : base(StaticConfigs) { }

        internal static readonly IReadOnlyList<ITypeConfig> StaticConfigs = [
            new TypeConfig<SpectreFigletTextStyle>(
                SpectreFigletTextStyle.Create,
                (style, writer, options) => style.Write(writer, options)),
            new TypeConfig<SpectreRuleHeaderStyle>(
                (props, options) => new SpectreRuleHeaderStyle(props, options),
                (style, writer, options) => style.Write(writer, options)),
            new TypeConfig<SpectreTextStyle>(
                (props, options) => new SpectreTextStyle(props, options),
                (style, writer, options) => style.Write(writer, options))
        ];

        public override bool? IsDefault(object value) => value switch
        {
            SpectreTextStyle textStyle 
                => (!textStyle.Foreground.HasValue || textStyle.Foreground.Value.IsDefault())
                && (!textStyle.Background.HasValue || textStyle.Background.Value.IsDefault())
                && textStyle.Decoration == Decoration.None,

            IList<ISpectreHeaderStyle> headerStyles => headerStyles.Count == 0,
            _ => null,
        };
    }
}
