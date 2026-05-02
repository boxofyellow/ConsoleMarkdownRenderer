using System;

namespace ConsoleMarkdownRenderer.Styling
{
    /// <summary>
    /// Flags enum representing text decorations for console rendering.
    /// These mirror the Spectre.Console Decoration values used in DisplayOptions.
    /// </summary>
    [Flags]
    public enum TextDecoration
    {
        None = 0,
        Bold = 1 << 0,
        Dim = 1 << 1,
        Italic = 1 << 2,
        Underline = 1 << 3,
        SlowBlink = 1 << 4,
        RapidBlink = 1 << 5,
        Invert = 1 << 6,
        Conceal = 1 << 7,
        Strikethrough = 1 << 8,
    }
}
