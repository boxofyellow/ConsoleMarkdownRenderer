using System;
using System.IO;
using System.Threading.Tasks;
using ConsoleMarkdownRenderer.ObjectRenderers;
using Spectre.Console;

namespace ConsoleMarkdownRenderer
{
    /// <summary>
    /// Concrete implementation of <see cref="IMarkdownDisplayer"/>.
    /// Internally uses Spectre.Console for rendering and display.
    /// Consumers can instantiate this directly or inject via <see cref="IMarkdownDisplayer"/>.
    /// </summary>
    public class MarkdownDisplayer : IMarkdownDisplayer
    {
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
                path = await Displayer.DownloadAsync(uri, tempFiles, expectImage: false);
            }

            if (!string.IsNullOrEmpty(path))
            {
                var text = await File.ReadAllTextAsync(path);
                await Displayer.DisplayMarkdownAsync(text, uri, options, allowFollowingLinks, tempFiles);
            }
        }

        /// <inheritdoc/>
        public async Task DisplayMarkdownAsync(string text, Uri? baseUri = default, DisplayOptions? options = default, bool allowFollowingLinks = true)
        {
            baseUri ??= new(Path.Combine(Directory.GetCurrentDirectory(), "."));
            using var tempFiles = new TempFileManager();
            await Displayer.DisplayMarkdownAsync(text, baseUri, options, allowFollowingLinks, tempFiles);
        }
    }
}
