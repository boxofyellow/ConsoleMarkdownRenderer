namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// Render-only interface for markdown content. Does not perform interactive display.
    /// No dependency types are exposed in this interface.
    /// </summary>
    public interface IMarkdownRenderer
    {
        /// <summary>
        /// Renders markdown text and returns the result without displaying it.
        /// </summary>
        /// <param name="text">The markdown text to render</param>
        /// <param name="options">Options to control rendering styles</param>
        /// <returns>The render result containing rendered text, links, and optionally unhandled types</returns>
        MarkdownRenderResult RenderMarkdown(string text, DisplayOptions? options = default);
    }
}
