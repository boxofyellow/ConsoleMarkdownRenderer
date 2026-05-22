namespace BoxOfYellow.ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Selects the line characters used to draw a Spectre.Console <c>Rule</c> when a heading
    /// is rendered via <see cref="RuleHeaderStyle"/>. The members mirror the predefined
    /// <c>BoxBorder</c> static instances exposed by Spectre.Console
    /// (<c>BoxBorder.None</c>, <c>BoxBorder.Ascii</c>, <c>BoxBorder.Square</c>,
    /// <c>BoxBorder.Rounded</c>, <c>BoxBorder.Heavy</c>, <c>BoxBorder.Double</c>) and are
    /// translated to those instances internally.
    /// </summary>
    public enum RuleBorder
    {
        None,
        Ascii,
        Square,
        Rounded,
        Heavy,
        Double,
    }
}
