using System;
using System.Collections.Generic;

namespace ConsoleMarkdownRenderer.Fakes
{
    /// <summary>
    /// A fake implementation of <see cref="IMarkdownRenderer"/> for testing.
    /// Records all calls and returns pre-configured results.
    /// </summary>
    public class FakeMarkdownRenderer : IMarkdownRenderer
    {
        private readonly List<(string Text, DisplayOptions? Options)> _calls = new();

        /// <summary>
        /// The result to return from <see cref="RenderMarkdown"/>. 
        /// If null, a default empty result is returned.
        /// </summary>
        public MarkdownRenderResult? ResultToReturn { get; set; }

        /// <summary>
        /// All calls that were made to <see cref="RenderMarkdown"/>.
        /// </summary>
        public IReadOnlyList<(string Text, DisplayOptions? Options)> Calls => _calls;

        /// <inheritdoc/>
        public MarkdownRenderResult RenderMarkdown(string text, DisplayOptions? options = default)
        {
            _calls.Add((text, options));
            return ResultToReturn ?? new MarkdownRenderResult(
                renderedText: string.Empty,
                links: Array.Empty<MarkdownLink>(),
                unhandledTypes: null);
        }
    }
}
