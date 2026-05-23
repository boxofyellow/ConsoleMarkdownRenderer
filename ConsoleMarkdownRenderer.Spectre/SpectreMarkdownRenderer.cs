using System.Runtime.CompilerServices;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using Markdig;
using Markdig.Syntax;

[assembly: InternalsVisibleTo("BoxOfYellow.ConsoleMarkdownRenderer")]
[assembly: InternalsVisibleTo("BoxOfYellow.ConsoleMarkdownRenderer.Fakes")]
[assembly: InternalsVisibleTo("ConsoleMarkdownRenderer.Spectre.Tests")]

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre
{
    /// <summary>
    /// Low-level "markdown text → Spectre.Console <c>IRenderable</c>" renderer.
    /// Consumers who are already building their own Spectre.Console document can use this class
    /// to splice rendered markdown into it without any of the interactive prompt loop, HTTP
    /// downloading, image inlining, or JSON-serializable options from the main package.
    /// </summary>
    /// <example>
    /// <code>
    /// var md = new SpectreMarkdownRenderer(new SpectreDisplayOptions
    /// {
    ///     CodeBlock = new Style(Color.Grey85, Color.Grey15),
    ///     TableBorder = TableBorder.Rounded,
    /// });
    /// var result = md.Render(File.ReadAllText("notes.md"));
    /// AnsiConsole.Write(result.Root!);
    /// foreach (var link in result.Links) { /* … */ }
    /// </code>
    /// </example>
    public class SpectreMarkdownRenderer
    {
        private readonly SpectreDisplayOptions m_options;
        private readonly bool m_omitAutolinkInlineRenderer;

        /// <summary>
        /// Creates a new <see cref="SpectreMarkdownRenderer"/> with the supplied options.
        /// </summary>
        /// <param name="options">
        /// Options controlling styling and rendering behavior. When <see langword="null"/>,
        /// <see cref="SpectreDisplayOptions"/> defaults are used.
        /// </param>
        public SpectreMarkdownRenderer(SpectreDisplayOptions? options = null)
            : this(options ?? new SpectreDisplayOptions(), omitAutolinkInlineRenderer: false)
        {
        }

        /// <summary>
        /// Internal constructor that also allows suppressing the autolink inline renderer
        /// (used by test fakes to exercise the unhandled-type code path).
        /// </summary>
        internal SpectreMarkdownRenderer(SpectreDisplayOptions options, bool omitAutolinkInlineRenderer)
        {
            m_options = options;
            m_omitAutolinkInlineRenderer = omitAutolinkInlineRenderer;
        }

        /// <summary>
        /// Parses <paramref name="markdown"/> and renders it, returning a fresh
        /// <see cref="MarkdownRenderResult"/> per call. No state is shared between calls.
        /// </summary>
        /// <param name="markdown">The Markdown text to render.</param>
        public MarkdownRenderResult Render(string markdown)
        {
            ArgumentNullException.ThrowIfNull(markdown);
            var pipeline = m_options.BuildPipeline();
            var document = Markdown.Parse(markdown, pipeline);
            return Render(document);
        }

        /// <summary>
        /// Renders an already-parsed <see cref="MarkdownDocument"/>, returning a fresh
        /// <see cref="MarkdownRenderResult"/> per call. No state is shared between calls.
        /// </summary>
        /// <param name="document">The pre-parsed Markdown document to render.</param>
        internal MarkdownRenderResult Render(MarkdownDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);
            var renderer = new ConsoleRenderer(m_options, m_omitAutolinkInlineRenderer);
            renderer.Render(document);
            return new()
            {
                Root = renderer.Root,
                Links = renderer.Links,
                UnhandledTypes = renderer.UnhandledTypes?.ToList() ?? (IReadOnlyList<Type>)Array.Empty<Type>(),
                UnknownEmphasisDelimiters = renderer.UnknownEmphasisDelimiters?.ToList() ?? (IReadOnlyList<UnknownEmphasisDelimiter>)Array.Empty<UnknownEmphasisDelimiter>(),
            };
        }
    }
}
