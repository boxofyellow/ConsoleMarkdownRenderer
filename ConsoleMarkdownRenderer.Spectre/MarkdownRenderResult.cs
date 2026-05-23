using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    /// <summary>
    /// The result of rendering a Markdown document via <see cref="SpectreMarkdownRenderer"/>.
    /// Each call to <c>Render</c> produces a fresh, independent instance; no state is shared
    /// between consecutive renders.
    /// </summary>
    public sealed class MarkdownRenderResult
    {
        /// <summary>
        /// The top-level <see cref="IRenderable"/> that represents the entire document.
        /// May be <see langword="null"/> if the document produced no renderable content.
        /// Pass this directly to <c>AnsiConsole.Write</c> or embed it in a Spectre.Console
        /// layout widget.
        /// </summary>
        public required IRenderable? Root { get; init; }

        /// <summary>
        /// All hyperlinks discovered during rendering, in document order.
        /// Each <see cref="LinkItem"/> carries the URL, the visible link text, and a flag
        /// indicating whether the link is an image.
        /// </summary>
        public required IReadOnlyList<LinkItem> Links { get; init; }

        /// <summary>
        /// Markdown object types that were encountered during rendering but had no dedicated
        /// renderer registered. Only populated when
        /// <see cref="SpectreDisplayOptions.IncludeDebug"/> is <see langword="true"/>;
        /// otherwise returns an empty collection.
        /// </summary>
        public required IReadOnlyList<Type> UnhandledTypes { get; init; }

        /// <summary>
        /// Emphasis inline delimiter combinations that fell into the catch-all branch of the
        /// emphasis renderer (i.e. unrecognized delimiter character / count pairs).
        /// Populated regardless of <see cref="SpectreDisplayOptions.IncludeDebug"/>.
        /// </summary>
        public required IReadOnlyList<UnknownEmphasisDelimiter> UnknownEmphasisDelimiters { get; init; }
    }
}
