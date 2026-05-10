using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using ConsoleMarkdownRenderer.ObjectRenderers;
using Markdig;
using Spectre.Console;

[assembly: InternalsVisibleTo("ConsoleMarkdownRenderer.Tests")]
[assembly: InternalsVisibleTo("ConsoleMarkdownRenderer.Fakes")]

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// Concrete implementation of <see cref="IMarkdownDisplayer"/>.
    /// Internally uses Spectre.Console for rendering and display.
    /// Consumers can instantiate this directly or inject via <see cref="IMarkdownDisplayer"/>.
    /// </summary>
    public class MarkdownDisplayer : IMarkdownDisplayer
    {
        /// <summary>
        /// Creates a <see cref="MarkdownDisplayer"/> that manages its own <see cref="HttpClient"/> using an
        /// internally created <see cref="System.Net.Http.SocketsHttpHandler"/> with a 15-minute pooled connection lifetime.
        /// </summary>
        public MarkdownDisplayer() { }

        /// <summary>
        /// Creates a <see cref="MarkdownDisplayer"/> that obtains an <see cref="HttpClient"/> from the supplied
        /// <see cref="IHttpClientFactory"/> for every web request.
        /// The factory (and the clients it creates) is owned and managed by the caller.
        /// </summary>
        /// <param name="httpClientFactory">The factory to use when creating HTTP clients. Must not be <see langword="null"/>.</param>
        /// <param name="httpClientName">
        /// The named client to request from the factory.
        /// Defaults to <see cref="string.Empty"/>, which corresponds to the default unnamed client.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpClientFactory"/> is <see langword="null"/>.</exception>
        public MarkdownDisplayer(IHttpClientFactory httpClientFactory, string httpClientName = "")
        {
            m_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            m_httpClientName = httpClientName;
        }

        /// <inheritdoc/>
        public async Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            using var tempFiles = new TempFileManager();
            string path = string.Empty;

            if (uri.IsFile)
            {
                if (File.Exists(uri.LocalPath))
                {
                    path = uri.LocalPath;
                }
                else
                {
                    AnsiConsole.WriteLine($"Failed to find {uri}");
                }
            }
            else
            {
                path = await DownloadAsync(uri, tempFiles, expectImage: false);
            }

            if (!string.IsNullOrEmpty(path))
            {
                var text = await File.ReadAllTextAsync(path);
                await DisplayMarkdownAsync(text, uri, options, allowFollowingLinks, tempFiles);
            }
        }

        /// <inheritdoc/>
        public async Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            baseUri ??= new(Path.Combine(Directory.GetCurrentDirectory(), "."));
            using var tempFiles = new TempFileManager();
            await DisplayMarkdownAsync(text, baseUri, options, allowFollowingLinks, tempFiles);
        }

        /// <summary>
        /// Designed to aid in testing, returns the default MarkdownPipeline that the renderer is designed to work with
        /// NOTE: internal for testing
        /// </summary>
        internal static MarkdownPipeline DefaultPipeline 
            => new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

        /// <summary>
        /// Download the content as the specific location after checking its header suggests it is the correct content type
        /// The contents are saved into a new temporary file, the full path of file is returned (as well as added to tempFiles)
        /// NOTE: internal for testing
        /// </summary>
        /// <param name="uri">the address to download</param>
        /// <param name="tempFiles">a manager for temp files, the caller is expected to clean these up</param>
        /// <param name="expectImage">when true the file will only be downloaded if the response content looks like an image, when false, only plain text files can be downloaded</param>
        /// <returns>The full path to the temporarily downloaded file where the content was downloaded to, or string.Empty if the file can't be downloaded</returns>
        internal async Task<string> DownloadAsync(Uri uri, TempFileManager tempFiles, bool expectImage)
        {
            try
            {
                var client = GetClient();

                using HttpRequestMessage request = new HttpRequestMessage(method: HttpMethod.Get, uri);
                using HttpResponseMessage response = await client.SendAsync(request);
                // HttpStatusCode.Ambiguous = 300
                if (!(response.StatusCode >= HttpStatusCode.OK && response.StatusCode < HttpStatusCode.Ambiguous))
                {
                    AnsiConsole.WriteLine($"Failed to make web request {uri}.  Got {(int)response.StatusCode}-{response.StatusCode}");
                    return string.Empty;
                }

                string contextPrefix = expectImage ? "image/" : "text/plain";
                var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                if (!mediaType.StartsWith(contextPrefix))
                {
                    AnsiConsole.WriteLine($"Content Type ({mediaType}) for {uri} did not start with {contextPrefix}");
                    return string.Empty;
                }

                string tempFile = tempFiles.GetTempFile();
                using var fileStream = File.Create(tempFile);
                await response.Content.CopyToAsync(fileStream, context: default, CancellationToken.None);
                return tempFile;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteLine($"Caught {ex.GetType().Name} attempting to download {uri}");
                return string.Empty;
            }
        }

        /// <summary>
        /// This does most of the work for displaying mark down.  The public version setups the required parameters
        /// NOTE: internal for testing
        /// </summary>
        /// <param name="text">the content to display</param>
        /// <param name="baseUri">uri for that content, this base is used to calculate relative links</param>
        /// <param name="options">options to control how to display the content</param>
        /// <param name="allowFollowingLinks">when true the user will be allow to follow links in the document</param>
        /// <param name="tempFiles">a manager for temp files, the caller is expected to clean these up</param>
        /// <param name="rendererOverride">optional renderer override, primarily for testing</param>
        internal async Task DisplayMarkdownAsync(string text, Uri baseUri, DisplayOptions? options, bool allowFollowingLinks, TempFileManager tempFiles, ConsoleRenderer? rendererOverride = null)
        {
            options ??= new DisplayOptions();

            var pipeline = DefaultPipeline;
            var renderer = rendererOverride ?? new ConsoleRenderer(options);

            // As the user browses the links, this stack allows us to display the previous content at their request
            var stack = new Stack<(string Text, Uri RelativePath)>();

            // Just keep looping until they select "Done"
            while (true)
            {
                renderer.Clear();

                var document = Markdown.Parse(text, pipeline);
                renderer.Render(document);

                RendererInspector?.Invoke(renderer);

                // These will only be computed if the includedDebug is provided
                if (renderer.UnhandledTypes?.Any() ?? false)
                {
                    foreach (var unhandled in renderer.UnhandledTypes)
                    {
                        AnsiConsole.Write(new Markup($"[yellow]Unhandled [bold]{Markup.Escape(unhandled.Name)}[/][/]"));
                        AnsiConsole.WriteLine();
                    }
                }

                if (renderer.Root == default)
                {
                    // Nothing to display... Not much we can do
                    AnsiConsole.WriteLine("No content to display");
                    if (stack.Any())
                    {
                        (text, baseUri) = stack.Pop();
                        continue;
                    }

                    // If there are no items in the stack and there is no content, we must be done
                    break;
                }

                AnsiConsole.Write(renderer.Root);
                AnsiConsole.WriteLine();

                var links = renderer
                    .Links
                    .Where(x => !string.IsNullOrEmpty(x.Url))
                    .ToArray();

                if (!allowFollowingLinks || !(links.Any() || stack.Any()))
                {
                    // if following links is disabled or there are are no links (in the dock or options to go back) to pick then we must be done
                    break;
                }

                // Check if we're in a non-interactive terminal (e.g., CI environment)
                if (!ShouldTreatAsInteractive())
                {
                    if (links.Any())
                    {
                        AnsiConsole.WriteLine();
                        AnsiConsole.Write(new Markup("[yellow]Warning: Non-interactive terminal detected. The following links are available but cannot be followed interactively:[/]"));
                        AnsiConsole.WriteLine();
                        foreach (var link in links)
                        {
                            var displayText = !string.IsNullOrEmpty(link.Content) ? link.Content : link.Url;
                            AnsiConsole.Write(new Markup($"  [blue]• {Markup.Escape(displayText)}[/]"));
                            if (!string.IsNullOrEmpty(link.Content) && link.Content != link.Url)
                            {
                                AnsiConsole.Write(new Markup($" [dim]({Markup.Escape(link.Url)})[/]"));
                            }
                            AnsiConsole.WriteLine();
                        }
                    }
                    // Exit since we can't prompt in a non-interactive terminal
                    break;
                }

                // To indicate that the user is done and want to give control back to the caller
                var doneResult = PromptResult.CreateDone();
                // To indicate that the user wants to view the previously displayed content
                var backResult = PromptResult.CreateBack();

                var prompt = new SelectionPrompt<PromptResult>();

                // If the prompt is canceled (e.g. Ctrl+C), treat it as if the user selected Done
                prompt.AddCancelResult(doneResult);

                // Done is always an option
                prompt.AddChoice(doneResult);

                if (stack.Any())
                {
                    // Add the back option next if there is anywhere to go back to
                    prompt.AddChoice(backResult);
                }

                // Build PromptResult entries for each link, embedding the LinkItem directly
                var linkResults = links.Select(l => PromptResult.CreateLink(l)).ToArray();

                prompt.AddChoices(linkResults);

                prompt.Converter = (r) => r.ToDisplayString();

                var needToPrompt = true;

                // loop until they select "Done" or select a new markdown to show
                while(needToPrompt)
                {
                    // in later versions of the library we should be able to call await AnsiConsole.PromptAsync(prompt)
                    var selected = await prompt.ShowAsync(AnsiConsole.Console, CancellationToken.None);

                    switch (selected.Kind)
                    {
                        case PromptResultKind.Done:
                            return;

                        case PromptResultKind.Back:
                            (text, baseUri) = stack.Pop();
                            needToPrompt = false;
                            break;

                        case PromptResultKind.Link:
                            string newText;
                            Uri newUri;
                            (newText, newUri, needToPrompt) = await HandleLinkItemAsync(baseUri, selected.LinkItem, tempFiles);
                            if (!needToPrompt)
                            {
                                // they selected a new markdown to display, so add the old one to the stack before updating our locals 
                                // Do this in preparation for the next iteration of the outer loop
                                stack.Push(new (text, baseUri));
                                text = newText;
                                baseUri = newUri;
                            }
                            break;

                        default:
                            throw new InvalidOperationException($"Unexpected {nameof(PromptResultKind)}: {selected.Kind}");
                    }
                }
            }
        }

        /// <summary>
        /// Responsible for handling selected links and determining what should happen next
        ///  - When the user selects a markdown file: Its content and new Uri are returned and NeedToPrompt is set to false to indicate we should exit the menu
        ///  - When the user selects an image (and it looks like we can) the image is displayed in line and NeedToPrompt is set to true to indicate they user should pick again
        ///  - When the user selects anything else, let the OS deal with it and set NeedToPRompt to true
        /// NOTE: Internal for testing
        /// </summary>
        /// <param name="baseUri">The uri to use for relative links</param>
        /// <param name="item">the item that was selected</param>
        /// <param name="tempFiles">a manager for temp files, the caller is expected to clean these up</param>
        /// <returns>
        ///   <param name="Text">When a new markdown is selected, this will hold its text, else string.Empty</param>
        ///   <param name="BaseUri">When a new markdown is selected, its uri (local or from the web)</param>
        ///   <param name="NeedToPrompt">true to indicate that new items should be prompted for, false indicates the new content should be displayed</param>
        /// </returns>
        internal async Task<(string Text, Uri BaseUri, bool NeedToPrompt)> HandleLinkItemAsync(Uri baseUri, LinkItem item, TempFileManager tempFiles)
        {
            if (!Uri.TryCreate(item.Url, UriKind.Absolute, out Uri? uri))
            {
                uri = new Uri(baseUri, item.Url); 
            }

            var extension = Path.GetExtension(uri.AbsolutePath);
            // We could include a check for item.IsImage, but there are things that you can mark as images
            // that we can't display locally.  By not treating them as an image we let the OS deal with it.
            // Doing that means we may not show it inline, but at least we will show it.
            bool isImage = !string.IsNullOrEmpty(extension) && s_imageExtensions.Contains(extension) && ShouldInlineImage();
            bool isMarkdown = !isImage && !string.IsNullOrEmpty(extension) && s_markdownExtensions.Contains(extension);

            string? localPath = default;
            if (uri.IsFile)
            {
                localPath = uri.LocalPath;
            }
            else if (isImage || isMarkdown)
            {
                // for things that we are going to handel, we need to pull them locally. 
                localPath = await DownloadAsync(uri, tempFiles, expectImage: isImage);
            }

            if (!string.IsNullOrEmpty(localPath))
            {
                if (!File.Exists(localPath))
                {
                    AnsiConsole.WriteLine($"Failed to find file {localPath} [(\"{baseUri.OriginalString}\") \"{item.Url}\"]");
                    return (string.Empty, // We are not changing the text
                        baseUri,          // Nor are we changing what relative links should start from 
                        true);            // We did something, but don't have next markdown to show
                }
                else if (isImage)
                {
                    DisplayImageInITerm(localPath); 
                    return (string.Empty, baseUri, true);
                }
                else if (isMarkdown)
                {
                    return (
                        await File.ReadAllTextAsync(localPath),  // Use this markdown
                        uri,                                     // Update relative links to this things parent
                        false);                                  // stop prompting so we can display the next thing
                }
            }

            // At this point there is not something that we can handle within this app
            // So throw it at the OS to see what it can do with it 🤞
            await OpenAsync(uri);

            return (string.Empty, baseUri, true);
        }

        /// <summary>
        /// Ask the OS to do "something" with this URI, it could be a local file, it be a web address 🤷🏽‍♂️
        /// </summary>
        /// <param name="uri">the Uri deal with</param>
        private static async Task OpenAsync(Uri uri)
        {
            string target = uri.IsFile 
                ? uri.LocalPath
                : uri.ToString();

            target = $"\"{target}\"";

            var prompt = new ConfirmationPrompt($"Do you want to open {target}")
            {
                DefaultValue = true,
            };

            // Don't want to accidentally run arbitrary code 
            // when updating the Spectre.Console library we can likely change this to await !AnsiConsole.ConfirmAsync($"Do you want to open {target}"
            if (!await prompt.ShowAsync(AnsiConsole.Console, CancellationToken.None))
            {
                return;
            }

            // https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
            try
            {
                Process.Start(uri.ToString());
            }
            catch (Exception ex)
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    target = target.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {target}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", target);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", target);
                }
                else
                {
                    AnsiConsole.WriteLine($"Caught {ex.GetType().Name} while opening {uri}");
                }
            }
        }

        /// <summary>
        /// Determines whether the terminal is interactive (i.e., can accept user input).
        /// Returns false in CI environments or when stdin is redirected.
        /// </summary>
        /// <returns>true if the terminal is interactive; otherwise, false.</returns>
        private static bool IsInteractiveTerminal()
            => !Console.IsInputRedirected && Environment.UserInteractive;

        /// <summary>
        /// When set, overrides the interactive terminal check for testing purposes.
        /// - <c>true</c>: Forces interactive mode (prompting will occur).
        /// - <c>false</c>: Forces non-interactive mode (links will be listed but not prompted).
        /// - <c>null</c> (default): Uses actual terminal detection.
        /// NOTE: internal for testing
        /// </summary>
        internal bool? ForceInteractiveForTesting { get; set; }

        /// <summary>
        /// Optional hook invoked after each <see cref="ConsoleRenderer.Render"/> call performed
        /// during <see cref="DisplayMarkdownAsync(string, Uri?, DisplayOptions?, bool)"/> (and
        /// the URI overload). Lets test fakes inspect the renderer state (e.g.
        /// <see cref="ConsoleRendererBase.UnhandledTypes"/>,
        /// <see cref="ConsoleRendererBase.UnknownEmphasisDelimiters"/>,
        /// <see cref="ConsoleRendererBase.Links"/>) without parsing console output.
        /// NOTE: internal for testing / fakes.
        /// </summary>
        internal Action<ConsoleRenderer>? RendererInspector { get; set; }

        /// <summary>
        /// Checks if we should treat the terminal as interactive, considering both actual state and test overrides.
        /// </summary>
        private bool ShouldTreatAsInteractive()
            => ForceInteractiveForTesting ?? IsInteractiveTerminal();

        /// <summary>
        /// A Simple factory to let us reuse http client
        /// </summary>
        /// <returns>an http client, the caller should NOT dispose this</returns>
        private HttpClient GetClient()
        {
            if (m_httpClientFactory is not null)
            {
                return m_httpClientFactory.CreateClient(m_httpClientName);
            }

            lock (m_lockObject)
            {
                if (m_client is null)
                {
                    var handler = new SocketsHttpHandler
                    {
                        // I don't Really expect the DNS to change much for these, but as a library we don't really know how long we will hang around
                        // or all the things that folks would be pointing at...
                        // so **_some_** limit makes sense
                        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
                    };
                    m_client = new HttpClient(handler);
                }
            }
            return m_client;
        }

        private readonly IHttpClientFactory? m_httpClientFactory;
        private readonly string m_httpClientName = string.Empty;
        private HttpClient? m_client;
        private readonly object m_lockObject = new();

        /// <summary>
        /// Releases the internally-managed <see cref="HttpClient"/>, if one was created.
        /// Has no effect when this instance was constructed with an <see cref="IHttpClientFactory"/>
        /// (the factory and its clients are owned by the caller).
        /// </summary>
        public void Dispose()
        {
            lock (m_lockObject)
            {
                m_client?.Dispose();
                m_client = null;
            }
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Try to determine if this an iTerm2 console
        /// </summary>
        /// <returns>true if looks like this iTerm2</returns>
        private static bool ShouldInlineImage() 
            //https://github.com/DavidDeSloovere/giphy-cli/pull/2/files
            => "iTerm.app".Equals(Environment.GetEnvironmentVariable("TERM_PROGRAM"), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Inline an image, only works in iTerm2 console
        /// </summary>
        /// <param name="path">full path of the file to inline</param>
        private static void DisplayImageInITerm(string path)
        {
            //https://github.com/DavidDeSloovere/giphy-cli/pull/2/files
            var bytes = File.ReadAllBytes(path);
            AnsiConsole.Write("\u001B]1337");  // ESC]1337
            AnsiConsole.Write(";File=;inline=1:");
            AnsiConsole.Write(Convert.ToBase64String(bytes));
            AnsiConsole.WriteLine("\u0007");  // ^G
        }

        // https://iterm2.com/documentation-images.html
        private readonly static HashSet<string> s_imageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".bmp",
            ".gif",
            ".png",
            ".icon",

            ".pdf",
            ".pict",
            ".eps",
        };

        // https://superuser.com/questions/249436/file-extension-for-markdown-files
        private readonly static HashSet<string> s_markdownExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".markdown",
            ".mdown",
            ".mkdn",
            ".md",
            ".mkd",
            ".mdwn",
            ".mdtxt",
            ".mdtext",
            ".text",
            ".Rmd",
        };

        /// <summary>
        /// Returns <see langword="true"/> when the supplied file extension is one this
        /// displayer treats as markdown content. Comparison is case-insensitive.
        /// </summary>
        internal static bool IsMarkdownExtension(string? extension)
            => !string.IsNullOrEmpty(extension) && s_markdownExtensions.Contains(extension);
    }
}
