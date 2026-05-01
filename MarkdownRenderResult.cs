using System;
using System.Collections.Generic;

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// The result of rendering markdown content.
    /// Contains no types from external dependencies — safe for use in public interfaces.
    /// </summary>
    public class MarkdownRenderResult
    {
        public MarkdownRenderResult(string renderedText, IReadOnlyList<MarkdownLink> links, IReadOnlySet<Type>? unhandledTypes = null)
        {
            RenderedText = renderedText;
            Links = links;
            UnhandledTypes = unhandledTypes;
        }

        /// <summary>
        /// The plain-text representation of the rendered output
        /// </summary>
        public string RenderedText { get; }

        /// <summary>
        /// Links found in the document
        /// </summary>
        public IReadOnlyList<MarkdownLink> Links { get; }

        /// <summary>
        /// Types that were not handled during rendering (populated when <see cref="DisplayOptions.IncludeDebug"/> is set)
        /// </summary>
        public IReadOnlySet<Type>? UnhandledTypes { get; }
    }
}
