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
    /// When constructed with <c>recursive: true</c>, after rendering each document the
    /// fake also follows every markdown link discovered by the renderer, validates it the
    /// same way, and records a child call. Visited absolute URIs are tracked to avoid
    /// cycles. Recursion is bounded by <c>maxDepth</c> and <c>maxFiles</c> guardrails so
    /// pointing the fake at something like <c>https://en.wikipedia.org/</c> won't run away.
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
        private readonly string _httpClientName;
        private readonly bool _recursive;
        private readonly int _maxDepth;
        private readonly int _maxFiles;
        private readonly List<ValidatedDisplayCall> _calls = new();

        /// <summary>
        /// Creates a fake. All parameters are optional; supply the ones you need.
        /// </summary>
        /// <param name="httpClientFactory">
        /// Optional factory the underlying <see cref="MarkdownDisplayer"/> will use for HTTP requests.
        /// When <see langword="null"/> the displayer manages its own <see cref="HttpClient"/>.
        /// </param>
        /// <param name="httpClientName">Optional named-client key. Defaults to <see cref="string.Empty"/>.</param>
        /// <param name="recursive">
        /// When <see langword="true"/>, after rendering each document the fake follows every
        /// markdown link discovered by the renderer (resolved against the document's base URI)
        /// and validates it the same way. Visited absolute URIs are tracked per top-level call
        /// to avoid cycles.
        /// </param>
        /// <param name="maxDepth">
        /// Maximum recursion depth (root document has depth 0). Defaults to 10. Recursion that
        /// would exceed this is skipped and <see cref="ExceededMaxDepth"/> is set.
        /// </param>
        /// <param name="maxFiles">
        /// Maximum total number of documents (top-level + recursive) the fake will process across
        /// the lifetime of the instance. Defaults to 100. Recursion that would exceed this is
        /// skipped and <see cref="ExceededMaxFiles"/> is set.
        /// </param>
        public ValidatingFakeMarkdownDisplayer(
            IHttpClientFactory? httpClientFactory = null,
            string httpClientName = "",
            bool recursive = false,
            int maxDepth = 10,
            int maxFiles = 100)
        {
            _httpClientFactory = httpClientFactory;
            _httpClientName = httpClientName;
            _recursive = recursive;
            _maxDepth = maxDepth;
            _maxFiles = maxFiles;
        }

        /// <summary>
        /// All recorded calls, in order. Includes both top-level calls made by the consumer
        /// and any recursive child calls produced when the fake was constructed with
        /// <c>recursive: true</c>.
        /// </summary>
        public IReadOnlyList<ValidatedDisplayCall> Calls => _calls;

        /// <summary>
        /// The deepest recursion depth reached across all recorded calls. The root call has
        /// depth 0; immediate children depth 1, and so on. Returns 0 when no calls have been
        /// recorded.
        /// </summary>
        public int MaxDepthReached => _calls.Count == 0 ? 0 : _calls.Max(c => c.Depth);

        /// <summary>The total number of documents the fake has processed (equivalent to <see cref="Calls"/>.Count).</summary>
        public int FilesProcessed => _calls.Count;

        /// <summary>
        /// <see langword="true"/> if recursive link-following hit the <c>maxDepth</c>
        /// guardrail and at least one link was skipped as a result.
        /// </summary>
        public bool ExceededMaxDepth { get; private set; }

        /// <summary>
        /// <see langword="true"/> if recursive link-following hit the <c>maxFiles</c>
        /// guardrail and at least one link was skipped as a result.
        /// </summary>
        public bool ExceededMaxFiles { get; private set; }

        /// <inheritdoc/>
        public async Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            var previousConsole = AnsiConsole.Console;
            var testConsole = new TestConsole();
            AnsiConsole.Console = testConsole;
            try
            {
                using var displayer = CreateDisplayer();
                var visited = new HashSet<string>();
                await ValidateUriAsync(
                    displayer: displayer,
                    uri: uri,
                    options: options,
                    allowFollowingLinks: allowFollowingLinks,
                    recursive: _recursive,
                    visited: visited,
                    parent: null,
                    depth: 0);
            }
            finally
            {
                AnsiConsole.Console = previousConsole;
                testConsole.Dispose();
            }
        }

        /// <inheritdoc/>
        public async Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            var previousConsole = AnsiConsole.Console;
            var testConsole = new TestConsole();
            AnsiConsole.Console = testConsole;
            try
            {
                using var displayer = CreateDisplayer();
                var visited = new HashSet<string>();
                await ValidateTextAsync(
                    displayer: displayer,
                    text: text,
                    baseUri: baseUri,
                    options: options,
                    allowFollowingLinks: allowFollowingLinks,
                    recursive: _recursive,
                    visited: visited,
                    parent: null,
                    depth: 0);
            }
            finally
            {
                AnsiConsole.Console = previousConsole;
                testConsole.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to dispose in the fake itself; per top-level call we create and dispose
            // a real MarkdownDisplayer + TestConsole.
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
        /// Asserts that none of the recorded calls produced any warning condition, and
        /// that recursive link-following did not hit either of the
        /// <c>maxDepth</c>/<c>maxFiles</c> guardrails. Throws
        /// <see cref="MarkdownValidationException"/> with details otherwise.
        /// </summary>
        public void AssertNoWarnings()
        {
            AssertNoUnhandledTypes();
            AssertNoUnknownEmphasisDelimiters();
            AssertNoUnusableLinkWarnings();
            AssertWithinRecursionLimits();
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

        /// <summary>
        /// Asserts that recursive link-following did not hit either of the
        /// <c>maxDepth</c>/<c>maxFiles</c> guardrails.
        /// </summary>
        public void AssertWithinRecursionLimits()
        {
            if (ExceededMaxDepth)
            {
                throw new MarkdownValidationException(
                    $"Recursive validation exceeded the configured MaxDepth ({_maxDepth}); reached depth {MaxDepthReached}. " +
                    $"Increase maxDepth in the constructor, or trim the linked document tree.");
            }
            if (ExceededMaxFiles)
            {
                throw new MarkdownValidationException(
                    $"Recursive validation exceeded the configured MaxFiles ({_maxFiles}); processed {FilesProcessed}. " +
                    $"Increase maxFiles in the constructor, or trim the linked document tree.");
            }
        }

        private static string DescribeSource(ValidatedDisplayCall c)
            => c.Uri is not null ? $"({c.Uri})" : "(text)";

        #endregion Assertions

        #region Validation core

        private MarkdownDisplayer CreateDisplayer()
        {
            var displayer = _httpClientFactory is not null
                ? new MarkdownDisplayer(_httpClientFactory, _httpClientName)
                : new MarkdownDisplayer();

            // Force the non-interactive code path so the real displayer never tries to
            // prompt against the TestConsole.
            displayer.ForceInteractiveForTesting = false;
            return displayer;
        }

        private async Task ValidateUriAsync(
            MarkdownDisplayer displayer,
            Uri uri,
            DisplayOptions? options,
            bool allowFollowingLinks,
            bool recursive,
            HashSet<string> visited,
            ValidatedDisplayCall? parent,
            int depth)
        {
            if (!visited.Add(uri.AbsoluteUri))
            {
                // Already validated this URI in the current top-level traversal.
                return;
            }

            var validation = await RunValidatedAsync(
                displayer,
                (d, o) => d.DisplayMarkdownAsync(uri, o, allowFollowingLinks),
                options);

            var call = new ValidatedDisplayCall(
                uri: uri,
                text: null,
                baseUri: null,
                options: options,
                allowFollowingLinks: allowFollowingLinks,
                parent: parent,
                depth: depth,
                validation: validation);
            _calls.Add(call);

            if (recursive)
            {
                await RecurseAsync(displayer, validation.FollowableLinks, parentUri: uri, options, allowFollowingLinks, visited, call, depth);
            }
        }

        private async Task ValidateTextAsync(
            MarkdownDisplayer displayer,
            string text,
            Uri? baseUri,
            DisplayOptions? options,
            bool allowFollowingLinks,
            bool recursive,
            HashSet<string> visited,
            ValidatedDisplayCall? parent,
            int depth)
        {
            var validation = await RunValidatedAsync(
                displayer,
                (d, o) => d.DisplayMarkdownAsync(text, baseUri, o, allowFollowingLinks),
                options);

            var call = new ValidatedDisplayCall(
                uri: null,
                text: text,
                baseUri: baseUri,
                options: options,
                allowFollowingLinks: allowFollowingLinks,
                parent: parent,
                depth: depth,
                validation: validation);
            _calls.Add(call);

            if (recursive)
            {
                var effectiveBase = baseUri
                    ?? new Uri(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
                await RecurseAsync(displayer, validation.FollowableLinks, parentUri: effectiveBase, options, allowFollowingLinks, visited, call, depth);
            }
        }

        private async Task RecurseAsync(
            MarkdownDisplayer displayer,
            IReadOnlyList<LinkItem> links,
            Uri parentUri,
            DisplayOptions? options,
            bool allowFollowingLinks,
            HashSet<string> visited,
            ValidatedDisplayCall parent,
            int parentDepth)
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

                if (parentDepth + 1 > _maxDepth)
                {
                    ExceededMaxDepth = true;
                    continue;
                }

                if (FilesProcessed >= _maxFiles)
                {
                    ExceededMaxFiles = true;
                    continue;
                }

                await ValidateUriAsync(displayer, childUri, options, allowFollowingLinks, recursive: true, visited, parent, parentDepth + 1);
            }
        }

        /// <summary>
        /// Wires our inspector onto the supplied displayer, runs the supplied delegate
        /// (passing it the displayer plus a debug-forced clone of the caller options), and
        /// returns the captured warning data. The previous inspector is restored on exit.
        /// </summary>
        private static async Task<MarkdownValidationResult> RunValidatedAsync(
            MarkdownDisplayer displayer,
            Func<MarkdownDisplayer, DisplayOptions?, Task> action,
            DisplayOptions? options)
        {
            var unhandled = new HashSet<Type>();
            var unknownEmphasis = new HashSet<UnknownEmphasisDelimiter>();
            var followableLinks = new List<LinkItem>();

            // Inspector fires after every renderer.Render() inside MarkdownDisplayer. With
            // ForceInteractiveForTesting=false the displayer renders once and exits the
            // outer loop, so this fires exactly once per call.
            var previousInspector = displayer.RendererInspector;
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
                foreach (var l in renderer.Links)
                {
                    if (!string.IsNullOrEmpty(l.Url))
                    {
                        followableLinks.Add(l);
                    }
                }
            };

            try
            {
                await action(displayer, ForceDebug(options));
            }
            finally
            {
                displayer.RendererInspector = previousInspector;
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
            int depth,
            MarkdownValidationResult validation)
        {
            Uri = uri;
            Text = text;
            BaseUri = baseUri;
            Options = options;
            AllowFollowingLinks = allowFollowingLinks;
            ParentCall = parent;
            Depth = depth;
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

        /// <summary>The recursion depth of this call. Top-level calls are 0; immediate children 1; etc.</summary>
        public int Depth { get; }

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
