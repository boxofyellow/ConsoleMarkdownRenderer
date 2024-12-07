using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using ConsoleMarkdownRenderer.ObjectRenderers;
using Markdig;
using Spectre.Console;

[assembly: InternalsVisibleTo("ConsoleMarkdownRenderer.Tests")]

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// This is the main class for this assembly and contains the method that is expected to be consumed by others
    /// It mainly provide a method <see cref="DisplayMarkdownAsync"/> (with a few overloads) for displaying markdown content in console
    /// </summary>
    public static class Displayer
    {
        /// <summary>
        /// Will display markdown content from the provided uri (local or from the web)
        /// Optionally after the markdown is displayed, a list of links from the document are presented and the user can select them to view more content
        /// The intend is use to display information/documentation so it is treated as best effort,
        /// and problems like (missing file or problems downloading content) are displayed in line as apposed to exceptions that bubble out.
        /// 
        /// Selected links are handled in the following way
        ///  - If they yield markdown, that content is displayed
        ///  - If they yield a image and it looks like this being run in iTerm2 (https://iterm2.com/) it will be displayed inline
        ///  - For everything else it is thrown at the OS to see if it can sort it out.
        /// </summary>
        /// <param name="uri">The uri to pull the content from</param>
        /// <param name="options">options to control how to display the content</param>
        /// <param name="allowFollowingLinks">when set to true, the list of links will be provided, when false the list is omitted</param>
        public static async Task DisplayMarkdownAsync(Uri uri, DisplayOptions? options = default, bool allowFollowingLinks = true)
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

        /// <summary>
        /// Will display markdown content from the provided uri (local or from the web)
        /// Optionally after the markdown is displayed, a list of links from the document are presented and the user can select them to view more content
        /// The intend is use to display information/documentation so it is treated as best effort,
        /// and problems like (missing file or problems downloading content) are displayed in line as apposed to exceptions that bubble out.
        /// 
        /// Selected links are handled in the following way
        ///  - If they yield markdown, that content is displayed
        ///  - If they yield a image and it looks like this being run in iTerm2 (https://iterm2.com/) it will be displayed inline
        ///  - For everything else it is thrown at the OS to see if it can sort it out.
        /// </summary>
        /// <param name="text">the content to display</param>
        /// <param name="baseUri">uri for that content, this base is used to calculate relative links.  If null, all links will be assumed to be relative to the current directory</param>
        /// <param name="options">options to control how to display the content</param>
        /// <param name="allowFollowingLinks">when set to true, the list of links will be provided, when false the list is omitted</param>
        public static async Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
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
        internal static async Task<string> DownloadAsync(Uri uri, TempFileManager tempFiles, bool expectImage)
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
                // We we make this method async we should flip this too.
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
        /// </summary>
        /// <param name="text">the content to display</param>
        /// <param name="baseUri">uri for that content, this base is used to calculate relative links</param>
        /// <param name="options">options to control how to display the content</param>
        /// <param name="allowFollowingLinks">when true the user will be allow to follow links in the document</param>
        /// <param name="tempFiles">a manager for temp files, the caller is expected to clean these up</param>
        private static async Task DisplayMarkdownAsync(string text, Uri baseUri, DisplayOptions? options, bool allowFollowingLinks, TempFileManager tempFiles)
        {
            // Two additional options that get included in the list links
            const int done = -1;  // To indicate that the user is done and want to give control back to the caller
            const int back = -2;  // To indicate that the user was to view the previously displayed content 

            options ??= new DisplayOptions();

            var pipeline = DefaultPipeline;
            var renderer = new ConsoleRenderer(options);

            // As the user browses the links, this stack allows us display the previous content at their request
            var stack = new Stack<(string Text, Uri RelativePath)>();

            // Just keep looping until they select "Done"
            while (true)
            {
                renderer.Clear();

                var document = Markdown.Parse(text, pipeline);
                renderer.Render(document);

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
                    .Where(x => !string.IsNullOrEmpty(x.Link.Url))
                    .ToArray();

                if (!allowFollowingLinks || !(links.Any() || stack.Any()))
                {
                    // if following links is disabled or there are would be no links (in the dock op options to go back) to pick then we must be done
                    break;
                }

                var prompt = new SelectionPrompt<int>();

                // Done is always an option
                prompt.AddChoice(done);

                if (stack.Any())
                {
                    // Add the back option next if there is anywhere to go back to
                    prompt.AddChoice(back);
                }
       
                // Add the rest for the links
                prompt.AddChoices(links.Select((l, i) => i));

                prompt.Converter = (i) => i switch
                {
                    done => "Done",
                    back => "Back",
                    _ =>  Markup.Escape(links[i].ToString()),
                };

                var needToPrompt = true;

                // loop until they select "Done" or select a new markdown to show
                while(needToPrompt)
                {
                    // in later versions of the library we should be able to do await AnsiConsole.PromptAsync(prompt)
                    var selected = await prompt.ShowAsync(AnsiConsole.Console, CancellationToken.None);

                    switch (selected)
                    {
                        case done:
                            return;

                        case back:
                            (text, baseUri) = stack.Pop();
                            needToPrompt = false;
                            break;

                        default:
                            string newText;
                            Uri newUri;
                            (newText, newUri, needToPrompt) = await HandleLinkItemAsync(baseUri, links[selected], tempFiles);
                            if (!needToPrompt)
                            {
                                // they selected a new markdown to display, so add the old one to the stack before updating our locals 
                                // Do this in preparation for the next iteration of the outer loop
                                stack.Push(new (text, baseUri));
                                text = newText;
                                baseUri = newUri;
                            }
                            break;
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
        ///   <param name="NeedToPrompt">true to indicate that anew items should be prompted for, false indicates the new content should be displayed</param>
        /// </returns>
        internal static async Task<(string Text, Uri BaseUri, bool NeedToPrompt)> HandleLinkItemAsync(Uri baseUri, LinkItem item, TempFileManager tempFiles)
        {
            if (!Uri.TryCreate(item.Link.Url, UriKind.Absolute, out Uri? uri))
            {
                uri = new Uri(baseUri, item.Link.Url); 
            }

            var extension = Path.GetExtension(uri.AbsolutePath);
            // We could include a check for item.Link.IsImage, but there are things that you can mark as images
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
                    AnsiConsole.WriteLine($"Failed to find file {localPath} [(\"{baseUri.OriginalString}\") \"{item.Link.Url}\"]");
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
        /// A Simple factory to let us reuse http client
        /// </summary>
        /// <returns>an http client, the call should NOT dispose this</returns>
        private static HttpClient GetClient()
        {
            lock (s_lockObject)
            {
                if (s_client is null)
                {
                    var handler = new SocketsHttpHandler
                    {
                        // Really I don't expect the DNS much for these, but as a library we don't really know how long we will hang around
                        // so **_some_** limit makes sense
                        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
                    };
                    s_client = new HttpClient(handler);
                }
            }
            return s_client;
        }

        private static HttpClient? s_client;
        private static readonly object s_lockObject = new();

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
    }
}
