using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter
{
    [SourceFile]
    internal sealed class TextColorJsonConverter : ColorJsonConverterBase<TextColor>
    {
        protected override TextColor DefaultColor => TextColor.Default;

        protected override BidirectionalMap<string, TextColor> ColorMap => Map;

        protected override TextColor Create(ConsoleColor? consoleColor, byte r, byte g, byte b) 
            => consoleColor.HasValue
            ? Color.FromConsoleColor(consoleColor.Value).ToTextColor()
            : TextColor.FromRgb(r, g, b);

        protected override (byte R, byte G, byte B) ToRgb(TextColor color) => (color.R, color.G, color.B);

        internal static readonly BidirectionalMap<string, TextColor> Map = new(
            Mappings.GetPropertyValues<TextColor>(new[] { typeof(TextColor) })
                // Default gets its own special handling 
                .Where(kvp => kvp.Name != nameof(TextColor.Default))
                .OrderBy(kvp => Mappings.SortColorNames(kvp.Name))
                .ThenBy(kvp => kvp.Name, StringComparer.OrdinalIgnoreCase),
            keyComparer: StringComparer.OrdinalIgnoreCase,
            allowDuplicateValues: true);
    }
}
