using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;

[SpectreSourceFile]
public interface ISpectreHeaderStyle
{
    Color? Foreground { get; }

    Color? Background { get; }

    Decoration Decoration { get; }

    internal string ToMarkup() 
        => ToSpectreStyle().ToMarkup();

    internal Style ToSpectreStyle() => new(Foreground, Background, Decoration);
}
