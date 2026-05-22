namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Named border styles for tables rendered by <see cref="ObjectRenderers.ConsoleTableRenderer"/>.
    /// These mirror the static <see cref="Spectre.Console.TableBorder"/> instances exposed by
    /// Spectre.Console so callers can pick a border without taking a direct dependency on
    /// Spectre.Console types from <see cref="DisplayOptions"/>.
    /// </summary>
    public enum TextTableBorder
    {
        None,
        Ascii,
        Ascii2,
        AsciiDoubleHead,
        Square,
        Rounded,
        Minimal,
        MinimalHeavyHead,
        MinimalDoubleHead,
        Simple,
        SimpleHeavy,
        Horizontal,
        Heavy,
        HeavyEdge,
        HeavyHead,
        Double,
        DoubleEdge,
        Markdown,
    }
}
