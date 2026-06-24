using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    [SpectreSourceFile]
    public interface ISpectreMarkdownRenderer
    {
        MarkdownRenderResult Render(string text, SpectreDisplayOptions? options = null);
    }
}