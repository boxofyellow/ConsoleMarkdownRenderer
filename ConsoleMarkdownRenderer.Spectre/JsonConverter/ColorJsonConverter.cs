using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;

[SpectreSourceFile]
internal sealed class ColorJsonConverter : ColorJsonConverterBase<Color>
{
    protected override Color DefaultColor => Color.Default;

    protected override BidirectionalMap<string, Color> ColorMap => Mappings.Colors;

    protected override Color Create(ConsoleColor? consoleColor, byte r, byte g, byte b) 
        => consoleColor.HasValue 
         ? Color.FromConsoleColor(consoleColor.Value) 
         : new Color(r, g, b);

    protected override (byte R, byte G, byte B) ToRgb(Color color) => (color.R, color.G, color.B);
}
