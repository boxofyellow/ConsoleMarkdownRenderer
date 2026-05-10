namespace BoxOfYellow.ConsoleMarkdownRenderer
{
    /// <summary>
    /// Full interactive display interface for markdown content.
    /// No dependency types are exposed in this interface.
    /// </summary>
    public interface IMarkdownDisplayer : IDisposable
    {
        /// <summary>
        /// Displays markdown content from the provided URI (local or web).
        /// Optionally presents a list of links for navigation.
        /// </summary>
        /// <param name="uri">The URI to pull content from</param>
        /// <param name="options">Options to control display styles</param>
        /// <param name="allowFollowingLinks">When true, links are presented for navigation</param>
        Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true);

        /// <summary>
        /// Displays the provided markdown text.
        /// Optionally presents a list of links for navigation.
        /// </summary>
        /// <param name="text">The markdown text to display</param>
        /// <param name="baseUri">Base URI for relative links. If null, defaults to current directory</param>
        /// <param name="options">Options to control display styles</param>
        /// <param name="allowFollowingLinks">When true, links are presented for navigation</param>
        Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true);
    }
}
