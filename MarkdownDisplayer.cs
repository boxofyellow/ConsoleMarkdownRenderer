using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleMarkdownRenderer.ObjectRenderers;
using Markdig;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// Concrete implementation of <see cref="IMarkdownRenderer"/> and <see cref="IMarkdownDisplayer"/>.
    /// Internally uses Spectre.Console for rendering but does not expose any dependency types in its public API.
    /// </summary>
    public class MarkdownDisplayer : IMarkdownRenderer, IMarkdownDisplayer
    {
        /// <inheritdoc/>
        public MarkdownRenderResult RenderMarkdown(string text, DisplayOptions? options = default)
        {
            options ??= new DisplayOptions();

            var pipeline = Displayer.DefaultPipeline;
            var renderer = new ConsoleRenderer(options);

            var document = Markdown.Parse(text, pipeline);
            renderer.Render(document);

            var renderedText = renderer.Root is not null
                ? RenderToPlainText(renderer.Root)
                : string.Empty;

            var links = renderer.Links
                .Where(x => !string.IsNullOrEmpty(x.Url))
                .Select(x => new MarkdownLink(x.Url, x.Content, x.IsImage))
                .ToList();

            IReadOnlySet<Type>? unhandledTypes = renderer.UnhandledTypes;

            return new MarkdownRenderResult(renderedText, links, unhandledTypes);
        }

        /// <inheritdoc/>
        public Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true)
            => Displayer.DisplayMarkdownAsync(uri, options, allowFollowingLinks);

        /// <inheritdoc/>
        public Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
            => Displayer.DisplayMarkdownAsync(text, baseUri, options, allowFollowingLinks);

        private static string RenderToPlainText(IRenderable renderable)
        {
            var console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Out = new AnsiConsoleOutput(new System.IO.StringWriter()),
            });
            console.Write(renderable);
            return ((System.IO.StringWriter)console.Profile.Out.Writer).ToString();
        }
    }
}
