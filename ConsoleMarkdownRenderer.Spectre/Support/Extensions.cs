using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support
{
    [SpectreSourceFile]
    public static class Extensions
    {
        public static bool IsDefault(this Color color) => color == Color.Default;
    }
}