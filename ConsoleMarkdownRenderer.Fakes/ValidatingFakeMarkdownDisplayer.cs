using ConsoleMarkdownRenderer.ObjectRenderers;
using Spectre.Console;
using Spectre.Console.Testing;

namespace ConsoleMarkdownRenderer.Fakes
{
    /// <summary>
    /// A fake implementation of <see cref="IMarkdownDisplayer"/> for testing that mirrors
    /// the recording behavior of <see cref="FakeMarkdownDisplayer"/> and additionally lets
    /// consumers assert their markdown does not produce any of the warning conditions
    /// surfaced by the real <see cref="MarkdownDisplayer"/>.
    /// <para>
    /// Under the covers each call delegates to a real <see cref="MarkdownDisplayer"/> that
    /// is wired up against an isolated <see cref="TestConsole"/> (so nothing is written to
    /// the surrounding test console) and forced into the non-interactive code path. The
    /// fake captures the post-render state of <see cref="ConsoleRenderer"/> via an
    /// inspector hook and exposes structured warning data:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Unhandled markdown object types (the <c>IncludeDebug</c> warning path).</description></item>
    ///   <item><description>Followable links combined with <c>allowFollowingLinks: true</c> (the non-interactive "links cannot be followed" warning).</description></item>
    ///   <item><description>Emphasis inlines whose delimiter fell into the catch-all branch in <see cref="ConsoleEmphasisInlineRenderer"/>.</description></item>
    /// </list>
    /// <para>
    /// When <see cref="Recursive"/> is <see langword="true"/>, after rendering each
    /// document the fake also follows every markdown link discovered by the renderer,
    /// validates it the same way, and records a child call. Visited absolute URIs are
    /// tracked to avoid cycles.
    /// </para>
    /// <para>
    /// <b>Thread safety:</b> Each call temporarily swaps <see cref="AnsiConsole.Console"/>.
    /// As with most things that touch <c>AnsiConsole.Console</c>, do not run multiple
    /// validations concurrently in the same process.
    /// </para>
    /// </summary>
    public class ValidatingFakeMarkdownDisplayer : IMarkdownDisplayer
    {
        private readonly IHttpClientFactory? _httpClientFactory;
        private readonly string _httpClientName = string.Empty;
        private readonly List<ValidatedDisplayCall> _calls = new();

        /// <summary>
        /// Creates a fake whose internal <see cref="MarkdownDisplayer"/> manages its own
        /// <see cref="HttpClient"/>. Suitable when the markdown does not reference any web
        /// URLs (or when you don't mind real network requests during validation).
        /// </summary>
        public ValidatingFakeMarkdownDisplayer() { }

        /// <summary>
        /// Creates a fake whose internal <see cref="MarkdownDisplayer"/> obtains
        /// <see cref="HttpClient"/> instances from the supplied factory. Use this overload
        /// to inject a hermetic test factory that maps URIs to canned responses.
        /// </summary>
        /// <param name="httpClientFactory">The factory the real displayer will use for HTTP requests.</param>
        /// <param name="httpClientName">Optional named-client key. Defaults to <see cref="string.Empty"/>.</param>
        public ValidatingFakeMarkdownDisplayer(IHttpClientFactory httpClientFactory, string httpClientName = "")
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpClientName = httpClientName;
        }

        /// <summary>
        /// All recorded calls, in order. Includes both top-level calls made by the consumer
        /// and any recursive child calls produced when <see cref="Recursive"/> is set.
        /// </summary>
        public IReadOnlyList<ValidatedDisplayCall> Calls => _calls;

        /// <summary>
        /// When <see langword="true"/>, after rendering each document the fake follows
        /// every markdown link discovered by the renderer (resolved against the document's
        /// base URI) and validates it the same way. Visited absolute URIs are tracked
        /// per-top-level-call to avoid cycles.
        /// </summary>
        public bool Recursive { get; set; }

        /// <inheritdoc/>
        public async Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            var visited = new HashSet<string>();
            await ValidateUriAsync(
                uri: uri,
                options: options,
                allowFollowingLinks: allowFollowingLinks,
                recursive: Recursive,
                visited: visited,
                parent: null);
        }

        /// <inheritdoc/>
        public async Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            var visited = new HashSet<string>();
            await ValidateTextAsync(
                text: text,
                baseUri: baseUri,
                options: options,
                allowFollowingLinks: allowFollowingLinks,
                recursive: Recursive,
                visited: visited,
                parent: null);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to dispose in the fake itself; per-call we create and dispose
            // a real MarkdownDisplayer + TestConsole inside ValidateAsync.
        }

        #region Assertions

        /// <summary>True if any recorded call detected at least one unhandled markdown object type.</summary>
        public bool HasUnhandledTypes => _calls.Any(c => c.Validation.UnhandledTypes.Count > 0);

        /// <summary>True if any recorded call detected at least one unknown-emphasis catch-all hit.</summary>
        public bool HasUnknownEmphasisDelimiters => _calls.Any(c => c.Validation.UnknownEmphasisDelimiters.Count > 0);

        /// <summary>
        /// True if any recorded call would surface the non-interactive-terminal "links
        /// cannot be followed interactively" warning. That occurs when
        /// <c>allowFollowingLinks</c> is true and the document contains at least one
        /// followable link.
        /// </summary>
        public bool HasUnusableLinkWarnings
            => _calls.Any(c => c.AllowFollowingLinks && c.Validation.FollowableLinks.Count > 0);

        /// <summary>
        /// Asserts that none of the recorded calls produced any warning condition. Throws
        /// <see cref="MarkdownValidationException"/> with details otherwise.
        /// </summary>
        public void AssertNoWarnings()
        {
            AssertNoUnhandledTypes();
            AssertNoUnknownEmphasisDelimiters();
            AssertNoUnusableLinkWarnings();
        }

        /// <summary>Asserts that no recorded call detected an unhandled markdown object type.</summary>
        public void AssertNoUnhandledTypes()
        {
            var offending = _calls.Where(c => c.Validation.UnhandledTypes.Count > 0).ToList();
            if (offending.Count == 0)
            {
                return;
            }
            var details = string.Join(
                Environment.NewLine,
                offending.Select(c =>
                    $"  Call #{_calls.IndexOf(c)} {DescribeSource(c)}: " +
                    $"{string.Join(", ", c.Validation.UnhandledTypes.Select(t => t.Name))}"));
            throw new MarkdownValidationException(
                $"Found unhandled markdown object types in {offending.Count} call(s):{Environment.NewLine}{details}");
        }

        /// <summary>Asserts that no recorded call detected an unknown-emphasis catch-all hit.</summary>
        public void AssertNoUnknownEmphasisDelimiters()
        {
            var offending = _calls.Where(c => c.Validation.UnknownEmphasisDelimiters.Count > 0).ToList();
            if (offending.Count == 0)
            {
                return;
            }
            var details = string.Join(
                Environment.NewLine,
                offending.Select(c =>
                    $"  Call #{_calls.IndexOf(c)} {DescribeSource(c)}: " +
                    $"{string.Join(", ", c.Validation.UnknownEmphasisDelimiters)}"));
            throw new MarkdownValidationException(
                $"Found emphasis inlines with unknown delimiter(s) in {offending.Count} call(s):{Environment.NewLine}{details}");
        }

        /// <summary>
        /// Asserts that no recorded call would surface the non-interactive-terminal
        /// unusable-links warning.
        /// </summary>
        public void AssertNoUnusableLinkWarnings()
        {
            var offending = _calls
                .Where(c => c.AllowFollowingLinks && c.Validation.FollowableLinks.Count > 0)
                .ToList();
            if (offending.Count == 0)
            {
                return;
            }
            var details = string.Join(
                Environment.NewLine,
                offending.Select(c =>
                    $"  Call #{_calls.IndexOf(c)} {DescribeSource(c)}: " +
                    $"{string.Join(", ", c.Validation.FollowableLinks.Select(l => l.Url))}"));
            throw new MarkdownValidationException(
                $"Found {offending.Count} call(s) that would emit the non-interactive unusable-links warning. " +
                $"Either pass allowFollowingLinks: false or remove the links:{Environment.NewLine}{details}");
        }

        private static string DescribeSource(ValidatedDisplayCall c)
            => c.Uri is not null ? $"({c.Uri})" : "(text)";

        #endregion Assertions

        #region Validation core

        private async Task ValidateUriAsync(
            Uri uri,
            DisplayOptions? options,
            bool allowFollowingLinks,
            bool recursive,
            HashSet<string> visited,
            ValidatedDisplayCall? parent)
        {
            if (!visited.Add(uri.AbsoluteUri))
            {
                // Already validated this URI in the current top-level traversal.
                return;
            }

            var validation = await RunValidatedAsync(
                d => d.DisplayMarkdownAsync(uri, ForceDebug(options), allowFollowingLinks));

            var call = new ValidatedDisplayCall(
                uri: uri,
                text: null,
                baseUri: null,
                options: options,
                allowFollowingLinks: allowFollowingLinks,
                parent: parent,
                validation: validation);
            _calls.Add(call);

            if (recursive)
            {
                await RecurseAsync(validation.FollowableLinks, parentUri: uri, options, allowFollowingLinks, visited, call);
            }
        }

        private async Task ValidateTextAsync(
            string text,
            Uri? baseUri,
            DisplayOptions? options,
            bool allowFollowingLinks,
            bool recursive,
            HashSet<string> visited,
            ValidatedDisplayCall? parent)
        {
            var validation = await RunValidatedAsync(
                d => d.DisplayMarkdownAsync(text, baseUri, ForceDebug(options), allowFollowingLinks));

            var call = new ValidatedDisplayCall(
                uri: null,
                text: text,
                baseUri: baseUri,
                options: options,
                allowFollowingLinks: allowFollowingLinks,
                parent: parent,
                validation: validation);
            _calls.Add(call);

            if (recursive)
            {
                var effectiveBase = baseUri
                    ?? new Uri(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
                await RecurseAsync(validation.FollowableLinks, parentUri: effectiveBase, options, allowFollowingLinks, visited, call);
            }
        }

        private async Task RecurseAsync(
            IReadOnlyList<LinkItem> links,
            Uri parentUri,
            DisplayOptions? options,
            bool allowFollowingLinks,
            HashSet<string> visited,
            ValidatedDisplayCall parent)
        {
            foreach (var link in links)
            {
                if (string.IsNullOrEmpty(link.Url) || link.IsImage)
                {
                    continue;
                }

                if (!Uri.TryCreate(link.Url, UriKind.Absolute, out var childUri))
                {
                    if (!Uri.TryCreate(parentUri, link.Url, out childUri))
                    {
                        continue;
                    }
                }

                var ext = Path.GetExtension(childUri.AbsolutePath);
                if (!MarkdownDisplayer.IsMarkdownExtension(ext))
                {
                    continue;
                }

                await ValidateUriAsync(childUri, options, allowFollowingLinks, recursive: true, visited, parent);
            }
        }

        /// <summary>
        /// Swaps in a <see cref="TestConsole"/>, runs the supplied delegate against a real
        /// <see cref="MarkdownDisplayer"/> wired with our inspector + non-interactive
        /// override, captures structured warning data, then unconditionally restores the
        /// previous <see cref="AnsiConsole.Console"/> and disposes everything we created.
        /// </summary>
        private async Task<MarkdownValidationResult> RunValidatedAsync(Func<MarkdownDisplayer, Task> action)
        {
            var unhandled = new HashSet<Type>();
            var unknownEmphasis = new HashSet<UnknownEmphasisDelimiter>();
            var followableLinks = new List<LinkItem>();

            var previousConsole = AnsiConsole.Console;
            var testConsole = new TestConsole().Width(360);
            AnsiConsole.Console = testConsole;
            try
            {
                using var displayer = _httpClientFactory is not null
                    ? new MarkdownDisplayer(_httpClientFactory, _httpClientName)
                    : new MarkdownDisplayer();

                // Force the non-interactive code path so the real displayer never tries
                // to prompt against the TestConsole.
                displayer.ForceInteractiveForTesting = false;

                // Inspector fires after every renderer.Render() inside MarkdownDisplayer.
                // Note: when allowFollowingLinks=true the displayer's outer loop may render
                // again (e.g. on Back); merging across invocations gives us the union of
                // everything observed.
                displayer.RendererInspector = renderer =>
                {
                    if (renderer.UnhandledTypes is { } u)
                    {
                        foreach (var t in u) unhandled.Add(t);
                    }
                    if (renderer.UnknownEmphasisDelimiters is { } e)
                    {
                        foreach (var d in e) unknownEmphasis.Add(d);
                    }
                    // Replace (rather than accumulate): the latest render reflects the
                    // links we'd actually surface to the user for navigation.
                    followableLinks.Clear();
                    foreach (var l in renderer.Links)
                    {
                        if (!string.IsNullOrEmpty(l.Url))
                        {
                            followableLinks.Add(l);
                        }
                    }
                };

                await action(displayer);
            }
            finally
            {
                AnsiConsole.Console = previousConsole;
                testConsole.Dispose();
            }

            return new MarkdownValidationResult(
                unhandledTypes: unhandled.ToList(),
                followableLinks: followableLinks,
                unknownEmphasisDelimiters: unknownEmphasis.ToList());
        }

        private static DisplayOptions ForceDebug(DisplayOptions? options)
        {
            // Clone so we don't mutate caller-supplied options, and force IncludeDebug so
            // ConsoleRendererBase populates UnhandledTypes.
            var clone = options?.Clone() ?? new DisplayOptions();
            clone.IncludeDebug = true;
            return clone;
        }

        #endregion Validation core
    }

    /// <summary>A single recorded call to <see cref="ValidatingFakeMarkdownDisplayer"/>.</summary>
    public class ValidatedDisplayCall
    {
        internal ValidatedDisplayCall(
            Uri? uri,
            string? text,
            Uri? baseUri,
            DisplayOptions? options,
            bool allowFollowingLinks,
            ValidatedDisplayCall? parent,
            MarkdownValidationResult validation)
        {
            Uri = uri;
            Text = text;
            BaseUri = baseUri;
            Options = options;
            AllowFollowingLinks = allowFollowingLinks;
            ParentCall = parent;
            Validation = validation;
        }

        /// <summary>The URI passed (URI overload, or recursive child), or <see langword="null"/> for the text overload.</summary>
        public Uri? Uri { get; }

        /// <summary>The markdown text passed (text overload), or <see langword="null"/> for URI/recursive calls.</summary>
        public string? Text { get; }

        /// <summary>Base URI for relative links (text overload only).</summary>
        public Uri? BaseUri { get; }

        /// <summary>The display options passed by the caller (unmodified, even if <see cref="DisplayOptions.IncludeDebug"/> was forced for the validation run).</summary>
        public DisplayOptions? Options { get; }

        /// <summary>Whether link following was enabled.</summary>
        public bool AllowFollowingLinks { get; }

        /// <summary>The parent call that produced this one via recursion, or <see langword="null"/> for top-level calls made by the consumer.</summary>
        public ValidatedDisplayCall? ParentCall { get; }

        /// <summary>True if this call was produced by recursive link-following, rather than by a direct consumer call.</summary>
        public bool IsRecursive => ParentCall is not null;

        /// <summary>Structured warning data captured from the renderer for this call.</summary>
        public MarkdownValidationResult Validation { get; }
    }

    /// <summary>
    /// Structured warning data captured from a single
    /// <see cref="ValidatingFakeMarkdownDisplayer"/> call.
    /// </summary>
    public class MarkdownValidationResult
    {
        internal MarkdownValidationResult(
            IReadOnlyList<Type> unhandledTypes,
            IReadOnlyList<LinkItem> followableLinks,
            IReadOnlyList<UnknownEmphasisDelimiter> unknownEmphasisDelimiters)
        {
            UnhandledTypes = unhandledTypes;
            FollowableLinks = followableLinks;
            UnknownEmphasisDelimiters = unknownEmphasisDelimiters;
        }

        /// <summary>
        /// Markdown object types that no <c>ConsoleObjectRenderer</c> declared support for.
        /// At runtime each of these would emit an <c>Unhandled &lt;Name&gt;</c> warning when
        /// <see cref="DisplayOptions.IncludeDebug"/> is true.
        /// </summary>
        public IReadOnlyList<Type> UnhandledTypes { get; }

        /// <summary>
        /// Links rendered with a non-empty URL. In a non-interactive terminal with
        /// <c>allowFollowingLinks=true</c>, each of these would be listed under the
        /// "links cannot be followed interactively" warning.
        /// </summary>
        public IReadOnlyList<LinkItem> FollowableLinks { get; }

        /// <summary>
        /// Emphasis inlines whose <c>DelimiterChar</c> / <c>DelimiterCount</c> combination
        /// fell into the catch-all branch in
        /// <see cref="ConsoleEmphasisInlineRenderer"/>.
        /// </summary>
        public IReadOnlyList<UnknownEmphasisDelimiter> UnknownEmphasisDelimiters { get; }
    }

    /// <summary>
    /// Thrown by <see cref="ValidatingFakeMarkdownDisplayer"/> assertion helpers when a
    /// warning condition is detected.
    /// </summary>
    public class MarkdownValidationException : Exception
    {
        public MarkdownValidationException(string message) : base(message) { }
    }
}
