namespace ConsoleMarkdownRenderer.Fakes
{
    /// <summary>
    /// A fake implementation of <see cref="IMarkdownDisplayer"/> for testing.
    /// Records all calls and arguments for assertion.
    /// </summary>
    public class FakeMarkdownDisplayer : IMarkdownDisplayer
    {
        private readonly List<DisplayCall> _calls = new();

        /// <summary>
        /// All calls that were made to <see cref="DisplayMarkdownAsync"/>.
        /// </summary>
        public IReadOnlyList<DisplayCall> Calls => _calls;

        /// <inheritdoc/>
        public Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            _calls.Add(new DisplayCall(uri: uri, text: null, baseUri: null, options: options, allowFollowingLinks: allowFollowingLinks));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            _calls.Add(new DisplayCall(uri: null, text: text, baseUri: baseUri, options: options, allowFollowingLinks: allowFollowingLinks));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to dispose in the fake implementation
        }

        /// <summary>
        /// Represents a recorded call to <see cref="DisplayMarkdownAsync"/>
        /// </summary>
        public class DisplayCall
        {
            public DisplayCall(Uri? uri, string? text, Uri? baseUri, DisplayOptions? options, bool allowFollowingLinks)
            {
                Uri = uri;
                Text = text;
                BaseUri = baseUri;
                Options = options;
                AllowFollowingLinks = allowFollowingLinks;
            }

            /// <summary>The URI passed (for the URI overload), or null for the text overload</summary>
            public Uri? Uri { get; }

            /// <summary>The markdown text passed (for the text overload), or null for the URI overload</summary>
            public string? Text { get; }

            /// <summary>The base URI for relative links (text overload only)</summary>
            public Uri? BaseUri { get; }

            /// <summary>The display options used</summary>
            public DisplayOptions? Options { get; }

            /// <summary>Whether link following was enabled</summary>
            public bool AllowFollowingLinks { get; }
        }
    }
}
