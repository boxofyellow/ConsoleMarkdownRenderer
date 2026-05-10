using ConsoleMarkdownRenderer.ObjectRenderers;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace ConsoleMarkdownRenderer.Fakes
{
    /// <summary>
    /// A fake implementation of <see cref="IMarkdownDisplayer"/> that records all calls
    /// (like <see cref="FakeMarkdownDisplayer"/>) and additionally inspects the supplied
    /// markdown text for conditions that would cause <see cref="MarkdownDisplayer"/> to
    /// emit a warning at runtime.
    /// <para>
    /// Consumers can use this fake to assert (e.g. in unit tests) that the markdown they
    /// provide does not surface any warnings:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>No unhandled markdown object types (the <c>IncludeDebug</c> warning).</description></item>
    ///   <item><description>No followable links would be reported as unusable when running in a non-interactive terminal (e.g., CI).</description></item>
    ///   <item><description>No emphasis inlines fall into the unknown-delimiter catch-all branch in <c>ConsoleEmphasisInlineRenderer</c>.</description></item>
    /// </list>
    /// <para>
    /// The URI overload of <see cref="DisplayMarkdownAsync(Uri, DisplayOptions?, bool)"/>
    /// records the call but does not perform validation, because validation requires the
    /// markdown text and this fake intentionally never performs network I/O.
    /// </para>
    /// </summary>
    public class ValidatingFakeMarkdownDisplayer : IMarkdownDisplayer
    {
        private readonly List<ValidatedDisplayCall> _calls = new();

        /// <summary>
        /// All calls that were made to <see cref="DisplayMarkdownAsync(Uri, DisplayOptions?, bool)"/>
        /// or <see cref="DisplayMarkdownAsync(string, Uri?, DisplayOptions?, bool)"/>, in order.
        /// </summary>
        public IReadOnlyList<ValidatedDisplayCall> Calls => _calls;

        /// <inheritdoc/>
        public Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            _calls.Add(new ValidatedDisplayCall(
                uri: uri,
                text: null,
                baseUri: null,
                options: options,
                allowFollowingLinks: allowFollowingLinks,
                validation: null));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            var validation = MarkdownValidationResult.Validate(text, options);
            _calls.Add(new ValidatedDisplayCall(
                uri: null,
                text: text,
                baseUri: baseUri,
                options: options,
                allowFollowingLinks: allowFollowingLinks,
                validation: validation));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Nothing to dispose in the fake implementation
        }

        /// <summary>
        /// True if any recorded call detected at least one unhandled markdown object type.
        /// </summary>
        public bool HasUnhandledTypes
            => _calls.Any(c => c.Validation?.UnhandledTypes.Count > 0);

        /// <summary>
        /// True if any recorded call detected at least one emphasis inline whose delimiter
        /// fell into the unknown-delimiter catch-all branch.
        /// </summary>
        public bool HasUnknownEmphasisDelimiters
            => _calls.Any(c => c.Validation?.UnknownEmphasisDelimiters.Count > 0);

        /// <summary>
        /// True if any recorded call would surface the non-interactive-terminal "links cannot
        /// be followed interactively" warning. That occurs when <c>allowFollowingLinks</c>
        /// is true and the document contains at least one followable link.
        /// </summary>
        public bool HasUnusableLinkWarnings
            => _calls.Any(c => c.AllowFollowingLinks && (c.Validation?.FollowableLinks.Count ?? 0) > 0);

        /// <summary>
        /// Asserts that none of the recorded calls produced a warning condition. Throws
        /// <see cref="MarkdownValidationException"/> if any warning is present.
        /// </summary>
        public void AssertNoWarnings()
        {
            AssertNoUnhandledTypes();
            AssertNoUnknownEmphasisDelimiters();
            AssertNoUnusableLinkWarnings();
        }

        /// <summary>
        /// Asserts that no recorded call detected an unhandled markdown object type.
        /// </summary>
        public void AssertNoUnhandledTypes()
        {
            var offending = _calls
                .Where(c => c.Validation?.UnhandledTypes.Count > 0)
                .ToList();
            if (offending.Count == 0)
            {
                return;
            }

            var details = string.Join(
                Environment.NewLine,
                offending.Select((c, i) =>
                    $"  Call #{_calls.IndexOf(c)}: {string.Join(", ", c.Validation!.UnhandledTypes.Select(t => t.Name))}"));
            throw new MarkdownValidationException(
                $"Found unhandled markdown object types in {offending.Count} call(s):{Environment.NewLine}{details}");
        }

        /// <summary>
        /// Asserts that no recorded call detected an emphasis inline that fell into the
        /// unknown-delimiter catch-all branch.
        /// </summary>
        public void AssertNoUnknownEmphasisDelimiters()
        {
            var offending = _calls
                .Where(c => c.Validation?.UnknownEmphasisDelimiters.Count > 0)
                .ToList();
            if (offending.Count == 0)
            {
                return;
            }

            var details = string.Join(
                Environment.NewLine,
                offending.Select(c =>
                    $"  Call #{_calls.IndexOf(c)}: {string.Join(", ", c.Validation!.UnknownEmphasisDelimiters)}"));
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
                .Where(c => c.AllowFollowingLinks && (c.Validation?.FollowableLinks.Count ?? 0) > 0)
                .ToList();
            if (offending.Count == 0)
            {
                return;
            }

            var details = string.Join(
                Environment.NewLine,
                offending.Select(c =>
                    $"  Call #{_calls.IndexOf(c)}: {string.Join(", ", c.Validation!.FollowableLinks.Select(l => l.Url))}"));
            throw new MarkdownValidationException(
                $"Found {offending.Count} call(s) that would emit the non-interactive unusable-links warning. " +
                $"Either pass allowFollowingLinks: false or remove the links:{Environment.NewLine}{details}");
        }
    }

    /// <summary>
    /// A single recorded call to <see cref="ValidatingFakeMarkdownDisplayer"/>.
    /// </summary>
    public class ValidatedDisplayCall
    {
        internal ValidatedDisplayCall(
            Uri? uri,
            string? text,
            Uri? baseUri,
            DisplayOptions? options,
            bool allowFollowingLinks,
            MarkdownValidationResult? validation)
        {
            Uri = uri;
            Text = text;
            BaseUri = baseUri;
            Options = options;
            AllowFollowingLinks = allowFollowingLinks;
            Validation = validation;
        }

        /// <summary>The URI passed (for the URI overload), or null for the text overload.</summary>
        public Uri? Uri { get; }

        /// <summary>The markdown text passed (for the text overload), or null for the URI overload.</summary>
        public string? Text { get; }

        /// <summary>The base URI for relative links (text overload only).</summary>
        public Uri? BaseUri { get; }

        /// <summary>The display options used.</summary>
        public DisplayOptions? Options { get; }

        /// <summary>Whether link following was enabled.</summary>
        public bool AllowFollowingLinks { get; }

        /// <summary>
        /// Validation results for this call. <see langword="null"/> for the URI overload (since
        /// the fake does not download the document); always non-null for the text overload.
        /// </summary>
        public MarkdownValidationResult? Validation { get; }
    }

    /// <summary>
    /// Result of inspecting a single markdown text input for warning conditions that
    /// <see cref="MarkdownDisplayer"/> would surface at runtime.
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
        /// At runtime these would each emit an <c>Unhandled &lt;Name&gt;</c> warning when
        /// <see cref="DisplayOptions.IncludeDebug"/> is true.
        /// </summary>
        public IReadOnlyList<Type> UnhandledTypes { get; }

        /// <summary>
        /// Links present in the document with a non-empty URL. At runtime, in a
        /// non-interactive terminal with <c>allowFollowingLinks=true</c>, these would each
        /// be listed under the "links cannot be followed interactively" warning.
        /// </summary>
        public IReadOnlyList<LinkItem> FollowableLinks { get; }

        /// <summary>
        /// Emphasis inlines whose <c>DelimiterChar</c> / <c>DelimiterCount</c> combination
        /// fell into the catch-all branch of <c>ConsoleEmphasisInlineRenderer</c>. Inspect
        /// these to learn what tripped the catch-all.
        /// </summary>
        public IReadOnlyList<UnknownEmphasisDelimiter> UnknownEmphasisDelimiters { get; }

        internal static MarkdownValidationResult Validate(string text, DisplayOptions? options)
        {
            // Clone so we don't mutate caller-supplied options, and force IncludeDebug so
            // that ConsoleRenderer populates UnhandledTypes.
            var validationOptions = options?.Clone() ?? new DisplayOptions();
            validationOptions.IncludeDebug = true;

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var document = Markdown.Parse(text, pipeline);

            var unknownDelimiters = new List<UnknownEmphasisDelimiter>();
            CollectUnknownEmphasisDelimiters(document, unknownDelimiters);

            var renderer = new ConsoleRenderer(validationOptions);
            renderer.Render(document);

            var followable = renderer.Links
                .Where(l => !string.IsNullOrEmpty(l.Url))
                .ToList();

            var unhandled = renderer.UnhandledTypes?.ToList() ?? new List<Type>();

            return new MarkdownValidationResult(
                unhandledTypes: unhandled,
                followableLinks: followable,
                unknownEmphasisDelimiters: unknownDelimiters);
        }

        private static void CollectUnknownEmphasisDelimiters(MarkdownObject obj, List<UnknownEmphasisDelimiter> result)
        {
            if (obj is EmphasisInline emphasis
                && IsUnknownEmphasisDelimiter(emphasis.DelimiterChar, emphasis.DelimiterCount))
            {
                result.Add(new UnknownEmphasisDelimiter(emphasis.DelimiterChar, emphasis.DelimiterCount));
            }

            if (obj is ContainerBlock containerBlock)
            {
                foreach (var child in containerBlock)
                {
                    CollectUnknownEmphasisDelimiters(child, result);
                }
            }

            if (obj is LeafBlock leafBlock && leafBlock.Inline != null)
            {
                CollectUnknownEmphasisDelimiters(leafBlock.Inline, result);
            }

            if (obj is ContainerInline containerInline)
            {
                for (var child = containerInline.FirstChild; child != null; child = child.NextSibling)
                {
                    CollectUnknownEmphasisDelimiters(child, result);
                }
            }
        }

        // Mirrors the branches handled in ConsoleEmphasisInlineRenderer.Write.
        // Anything else falls into the catch-all branch we want to detect.
        private static bool IsUnknownEmphasisDelimiter(char delimiterChar, int delimiterCount)
        {
            if (delimiterChar is '*' or '_')
            {
                return false;
            }
            if (delimiterChar == '~')
            {
                return false;
            }
            if (delimiterChar == '^')
            {
                return false;
            }
            if (delimiterChar == '+' && delimiterCount == 2)
            {
                return false;
            }
            if (delimiterChar == '=' && delimiterCount == 2)
            {
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// A single emphasis inline whose delimiter fell into the unknown-delimiter catch-all
    /// branch of <c>ConsoleEmphasisInlineRenderer</c>.
    /// </summary>
    public class UnknownEmphasisDelimiter
    {
        public UnknownEmphasisDelimiter(char delimiterChar, int delimiterCount)
        {
            DelimiterChar = delimiterChar;
            DelimiterCount = delimiterCount;
        }

        /// <summary>The unrecognized delimiter character.</summary>
        public char DelimiterChar { get; }

        /// <summary>The number of times the delimiter character was repeated.</summary>
        public int DelimiterCount { get; }

        public override string ToString() => $"({DelimiterChar}{DelimiterCount})";
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
