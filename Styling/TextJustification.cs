using BoxOfYellow.ConsoleMarkdownRenderer.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling;

/// <summary>
/// Specifies how text should be horizontally justified within its available width.
/// These mirror the values of Spectre.Console's <c>Justify</c> enum and are used by
/// styles such as <see cref="FigletTextStyle"/>.
/// </summary>
[SourceFile]
public enum TextJustification
{
    Left,
    Right,
    Center,
}
