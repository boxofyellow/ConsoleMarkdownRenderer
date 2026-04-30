using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConsoleMarkdownRenderer.ObjectRenderers;
using Markdig.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Test for E2E Tests for <see cref="Displayer"/>
    /// </summary>
    [TestClass]
    public class DisplayTests : ConsoleTestBase
    {
        [TestMethod]
        public async Task DisplayTests_AllowFollowingLinksIsRespectedAsync()
            // This should not prompt, if it does it will throw
            => await Displayer.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")), allowFollowingLinks: false);

        [TestMethod]
        public async Task DisplayTests_ExitWorksAsync()
        {
            // This is going to prompt, accept the default to exit.
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            await Displayer.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")));
            AssertCrossPlatStringMatch(@"- [](sub/sub.md)

> Done
  [](sub/sub.md)", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_CanPassTextAsync()
        {
            // This is going to prompt, accept the default to exit.
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);

            var text = await File.ReadAllTextAsync(Path.Combine(DataPath, "start.md"));
            await Displayer.DisplayMarkdownAsync(text, new Uri(Path.Combine(DataPath, "start.md")));
            AssertCrossPlatStringMatch(@"- [](sub/sub.md)

> Done
  [](sub/sub.md)", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_CanFollowingLinksAsync()
        {
            // This is going to prompt, down to select the link.
            ConsoleUnderTest.Input.PushKey(ConsoleKey.DownArrow);
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            // then enter to exit
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            await Displayer.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")));

            AssertCrossPlatStringMatch(@"- [](sub/sub.md)

> Done
  [](sub/sub.md)   Done
> [](sub/sub.md) -[](../start.md)

> Done
  Back
  [](../start.md)", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_BackWorksAsync()
        {
            // This is going to prompt, down to select the link.
            ConsoleUnderTest.Input.PushKey(ConsoleKey.DownArrow);
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            // down to select back
            ConsoleUnderTest.Input.PushKey(ConsoleKey.DownArrow);
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            // then enter to exit
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            await Displayer.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")));

            AssertCrossPlatStringMatch(@"- [](sub/sub.md)

> Done
  [](sub/sub.md)   Done
> [](sub/sub.md) -[](../start.md)

> Done
  Back
  [](../start.md)   Done
> Back
  [](../start.md) - [](sub/sub.md)

> Done
  [](sub/sub.md)", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_MissingFilesAreReportedAsync()
        {
            var uri = new Uri(Path.Combine(DataPath, "not-a-file.md"));
            // This should not prompt, if it does it will throw
            await Displayer.DisplayMarkdownAsync(uri);

            AssertCrossPlatStringMatch($@"Failed to find {uri}
", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_BadUrlsAreReportedAsync()
        {
            var uri = new Uri("https://OkForReallyRealsThisNotAPlace.com/Bad/Path");
            // This should not prompt, if it does it will throw
            await Displayer.DisplayMarkdownAsync(uri);
            AssertCrossPlatStringMatch(@"Caught HttpRequestException attempting to download https://okforreallyrealsthisnotaplace.com/Bad/Path
", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_BadUrlsAreYieldErrorCodeAsync()
        {
            var uri = new Uri("https://github.com/ForReallyRealsThisIsNotAUSer");
            // This should not prompt, if it does it will throw
            await Displayer.DisplayMarkdownAsync(uri);
            AssertCrossPlatStringMatch(@"Failed to make web request https://github.com/ForReallyRealsThisIsNotAUSer.  Got 404-NotFound
", TrimmedConsoleOutput);
        }

        /// <summary>
        /// Verifies that unhandled types are displayed when <see cref="DisplayOptions.IncludeDebug"/> is true.
        /// <see cref="ConsoleAutolinkInlineRenderer.IsEnabled"/> is temporarily set to false so that
        /// AutolinkInline has no registered renderer and is reported as unhandled.
        /// </summary>
        [TestMethod]
        public async Task DisplayTests_UnhandledTypesDisplayedAsync()
        {
            ConsoleAutolinkInlineRenderer.IsEnabled = false;
            try
            {
                await Displayer.DisplayMarkdownAsync(
                    "<https://example.com>",
                    options: new DisplayOptions { IncludeDebug = true },
                    allowFollowingLinks: false);
            }
            finally
            {
                ConsoleAutolinkInlineRenderer.IsEnabled = true;
            }

            AssertCrossPlatStringMatch(@"Unhandled AutolinkInline
┌──┐
│  │
└──┘

", TrimmedConsoleOutput);
        }

        /// <summary>
        /// Verifies the "No content to display" code path: when the renderer produces a null Root,
        /// the displayer prints the message, pops the previous page from the back-stack, and
        /// eventually exits when the stack is empty.
        /// </summary>
        [TestMethod]
        public async Task DisplayTests_NoContentToDisplayAsync()
        {
            var renderer = new NullRootRenderer();

            // Select the link from start.md to push it onto the stack before Root becomes null
            ConsoleUnderTest.Input.PushKey(ConsoleKey.DownArrow);
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);

            using var tempFiles = new TempFileManager();
            var startMdText = await File.ReadAllTextAsync(Path.Combine(DataPath, "start.md"));
            var startUri = new Uri(Path.Combine(DataPath, "start.md"));

            await Displayer.DisplayMarkdownAsync(
                text: startMdText,
                baseUri: startUri,
                options: null,
                allowFollowingLinks: true,
                tempFiles: tempFiles,
                rendererOverride: renderer);

            AssertCrossPlatStringMatch(@"- [](sub/sub.md)

> Done
  [](sub/sub.md)   Done
> [](sub/sub.md) No content to display
No content to display
", TrimmedConsoleOutput);
        }

        // There is often trailing spaces included, which we don't need to worry about validating exactly
        private string TrimmedConsoleOutput 
            => string.Join(
                Environment.NewLine,
                CrossPlatNormalizeString(ConsoleUnderTest.Output)
                    .Split(LineBreak).Select(x => x.TrimEnd()));

        /// <summary>
        /// A renderer that works normally on the first Render call but forces Root to null
        /// on all subsequent calls, used to trigger the "No content to display" code path.
        /// </summary>
        private class NullRootRenderer : ConsoleRenderer
        {
            private bool _firstRender = true;

            public NullRootRenderer() : base(new DisplayOptions()) { }

            public override object Render(MarkdownObject markdownObject)
            {
                var result = base.Render(markdownObject);
                if (!_firstRender)
                {
                    // Clearing after the base render sets Root back to null
                    Clear();
                }
                _firstRender = false;
                return result;
            }
        }
    }
}