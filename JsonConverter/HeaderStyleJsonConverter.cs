using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter
{
    [SourceFile]
    internal sealed class HeaderStyleJsonConverter : HeaderStyleJsonConverterBase<IHeaderStyle>
    {
        public HeaderStyleJsonConverter() : base(StaticConfigs) { }

        internal static readonly IReadOnlyList<ITypeConfig> StaticConfigs = [
            new TypeConfig<FigletTextStyle>(
                FigletTextStyle.Create,
                (style, writer, options) => style.Write(writer, options)),
            new TypeConfig<RuleHeaderStyle>(
                (props, options) => new RuleHeaderStyle(props, options),
                (style, writer, options) => style.Write(writer, options)),
            new TypeConfig<TextStyle>(
                (props, options) => new TextStyle(props, options),
                (style, writer, options) => style.Write(writer, options))
        ];

        public override bool? IsDefault(object value) => value switch
        {
            TextStyle textStyle 
                => (textStyle.Foreground is null || textStyle.Foreground.IsDefault())
                && (textStyle.Background is null || textStyle.Background.IsDefault())
                && textStyle.Decoration == TextDecoration.None,

            IList<IHeaderStyle> headerStyles => headerStyles.Count == 0,
            _ => null,
        };
    }
}
