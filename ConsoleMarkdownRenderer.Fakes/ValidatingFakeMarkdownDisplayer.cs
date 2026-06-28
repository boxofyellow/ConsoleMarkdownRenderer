using BoxOfYellow.ConsoleMarkdownRenderer.Fakes.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.ObjectRenderers;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Fakes;

[FakeSourceFile]
public sealed class ValidatingFakeMarkdownDisplayer : IMarkdownDisplayer
{
    private readonly IHttpClientFactory? _httpClientFactory;
    private readonly string _httpClientName;
    private readonly bool _recursive;
    private readonly int _maxDepth;
    private readonly int _maxFiles;
    private readonly List<ValidatedDisplayCall> _calls = new();

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
        using var tempFileManager = new TempFileManager();
        using var displayer = CreateDisplayer();
        var visited = new HashSet<string>();
        await ValidateUriAsync(
            displayer,
            uri,
            options,
            allowFollowingLinks,
            _recursive,
            visited,
            parent: null,
            depth: 0,
            tempFileManager);
    }

    /// <inheritdoc/>
    public async Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
    {
        using var tempFileManager = new TempFileManager();
        using var displayer = CreateDisplayer();
        var visited = new HashSet<string>();
        await ValidateTextAsync(
            displayer,
            text,
            baseUri,
            options,
            allowFollowingLinks,
            _recursive,
            visited,
            parent: null,
            depth: 0,
            tempFileManager);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Nothing to dispose in the fake itself; per top-level call we create and dispose
        // a real MarkdownDisplayer + TestConsole.
    }

    #region Assertions

    /// <summary>True if any recorded call detected at least one unhandled markdown object type.</summary>
    public bool HasUnhandledTypes => _calls.Any(c => c.Result.UnhandledTypes.Count > 0);

    /// <summary>True if any recorded call detected at least one unknown-emphasis catch-all hit.</summary>
    public bool HasUnknownEmphasisDelimiters => _calls.Any(c => c.Result.UnknownEmphasisDelimiters.Count > 0);

    /// <summary>
    /// True if any recorded call would surface the non-interactive-terminal "links
    /// cannot be followed interactively" warning. That occurs when
    /// <c>allowFollowingLinks</c> is true and the document contains at least one
    /// followable link.
    /// </summary>
    public bool HasUnusableLinkWarnings
        => _calls.Any(c => c.AllowFollowingLinks && c.Result.Links.Count > 0);

    /// <summary>
    /// Asserts that none of the recorded calls produced any warning condition, and
    /// that recursive link-following did not hit either of the
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
        var offending = _calls.Where(c => c.Result.UnhandledTypes.Count > 0).ToList();
        if (offending.Count == 0)
        {
            return;
        }
        var details = string.Join(
            Environment.NewLine,
            offending.Select(c =>
                $"  Call #{_calls.IndexOf(c)} {DescribeSource(c)}: " +
                $"{string.Join(", ", c.Result.UnhandledTypes.Select(t => t.Name))}"));
        throw new MarkdownValidationException(
            $"Found unhandled markdown object types in {offending.Count} call(s):{Environment.NewLine}{details}");
    }

    /// <summary>Asserts that no recorded call detected an unknown-emphasis catch-all hit.</summary>
    public void AssertNoUnknownEmphasisDelimiters()
    {
        var offending = _calls.Where(c => c.Result.UnknownEmphasisDelimiters.Count > 0).ToList();
        if (offending.Count == 0)
        {
            return;
        }
        var details = string.Join(
            Environment.NewLine,
            offending.Select(c =>
                $"  Call #{_calls.IndexOf(c)} {DescribeSource(c)}: " +
                $"{string.Join(", ", c.Result.UnknownEmphasisDelimiters)}"));
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
            .Where(c => c.AllowFollowingLinks && c.Result.Links.Count > 0)
            .ToList();
        if (offending.Count == 0)
        {
            return;
        }
        var details = string.Join(
            Environment.NewLine,
            offending.Select(c =>
                $"  Call #{_calls.IndexOf(c)} {DescribeSource(c)}: " +
                $"{string.Join(", ", c.Result.Links.Select(l => l.Url))}"));
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

    /// <summary>
    /// When set to <see langword="true"/>, the underlying <see cref="ConsoleRenderer"/> with
    /// <c>omitAutolinkInlineRenderer: true</c>, causing
    /// <see cref="Markdig.Syntax.Inlines.AutolinkInline"/> to fall through to the
    /// unhandled-type code path. Intended for exercising the unhandled-type validation
    /// branch from tests.
    /// </summary>
    public bool OmitAutolinkInlineRendererForTesting { get; set; }

    #region Validation core

    private MarkdownDisplayer CreateDisplayer() 
        => _httpClientFactory is not null
        ? new MarkdownDisplayer(_httpClientFactory, _httpClientName)
        : new MarkdownDisplayer();

    private (SpectreDisplayOptions Options, ConsoleRenderer ConsoleRenderer) CreateObjects(DisplayOptions? options)
    {
        var spectreOptions = options?.ToSpectreOptions() ?? new SpectreDisplayOptions();
        spectreOptions.IncludeDebug = true; // Force debug info to get unhandled-type data
        ConsoleRenderer rendererConsoleRender = new(spectreOptions, omitAutolinkInlineRenderer: OmitAutolinkInlineRendererForTesting);
        return (spectreOptions, rendererConsoleRender);
    }

    private async Task ValidateUriAsync(
        MarkdownDisplayer displayer,
        Uri uri,
        DisplayOptions? options,
        bool allowFollowingLinks,
        bool recursive,
        HashSet<string> visited,
        ValidatedDisplayCall? parent,
        int depth,
        TempFileManager tempFileManager)
    {
        if (!visited.Add(uri.AbsoluteUri))
        {
            // Already validated this URI in the current top-level traversal.
            return;
        }

        var (spectreOptions, consoleRender) = CreateObjects(options);
        MarkdownRenderer markdownRenderer = new();
        
        string path = uri.IsFile 
                    ? uri.LocalPath 
                    : await displayer.DownloadAsync(uri, tempFileManager, expectImage: false).ConfigureAwait(false);
        var text = await File.ReadAllTextAsync(path).ConfigureAwait(false);

        var result = markdownRenderer.Render(text, spectreOptions, consoleRender);

        var call = new ValidatedDisplayCall(
            uri: uri,
            text: null,
            baseUri: null,
            options: options,
            allowFollowingLinks: allowFollowingLinks,
            parent: parent,
            depth: depth,
            result: result);
        _calls.Add(call);

        if (recursive)
        {
            await RecurseAsync(displayer, result.Links, parentUri: uri, options, allowFollowingLinks, visited, call, depth, tempFileManager);
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
        int depth,
        TempFileManager tempFileManager)
    {
        var (spectreOptions, consoleRender) = CreateObjects(options);
        MarkdownRenderer markdownRenderer = new();

        var result = markdownRenderer.Render(text, spectreOptions, consoleRender);

        var call = new ValidatedDisplayCall(
            uri: null,
            text: text,
            baseUri: baseUri,
            options: options,
            allowFollowingLinks: allowFollowingLinks,
            parent: parent,
            depth: depth,
            result: result);
        _calls.Add(call);

        if (recursive)
        {
            var effectiveBase = baseUri
                ?? new Uri(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar);
            await RecurseAsync(displayer, result.Links, parentUri: effectiveBase, options, allowFollowingLinks, visited, call, depth, tempFileManager);
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
        int parentDepth,
        TempFileManager tempFileManager)
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

            await ValidateUriAsync(displayer, childUri, options, allowFollowingLinks, recursive: true, visited, parent, parentDepth + 1, tempFileManager);
        }
    }

    #endregion Validation core
}

/// <summary>A single recorded call to <see cref="ValidatingFakeMarkdownDisplayer"/>.</summary>
[FakeSourceFile]
public sealed class ValidatedDisplayCall
{
    internal ValidatedDisplayCall(
        Uri? uri,
        string? text,
        Uri? baseUri,
        DisplayOptions? options,
        bool allowFollowingLinks,
        ValidatedDisplayCall? parent,
        int depth,
        MarkdownRenderResult result)
    {
        Uri = uri;
        Text = text;
        BaseUri = baseUri;
        Options = options;
        AllowFollowingLinks = allowFollowingLinks;
        ParentCall = parent;
        Depth = depth;
        Result = result;
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
    public MarkdownRenderResult Result { get; }
}

/// <summary>
/// Thrown by <see cref="ValidatingFakeMarkdownDisplayer"/> assertion helpers when a
/// warning condition is detected.
/// </summary>
[FakeSourceFile]
public sealed class MarkdownValidationException : Exception
{
    public MarkdownValidationException(string message) : base(message) { }
}
