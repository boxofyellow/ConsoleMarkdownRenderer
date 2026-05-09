using ConsoleMarkdownRenderer.ObjectRenderers;
using Markdig.Syntax;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Test for E2E Tests for <see cref="Displayer"/>
    /// </summary>
    [TestClass]
    public class DisplayTests : ConsoleTestBase
    {
        private readonly MarkdownDisplayer? _displayer = CreateInteractiveDisplayer();

        [TestCleanup]
        public override void TestCleanup()
        {
            _displayer?.Dispose();
            base.TestCleanup();
        }

        [TestMethod]
        public async Task DisplayTests_AllowFollowingLinksIsRespectedAsync()
            // This should not prompt, if it does it will throw
            // Leave this as the static method so we can get coverage there.
            => await Displayer.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")), allowFollowingLinks: false);

        [TestMethod]
        public async Task DisplayTests_AllowFollowingLinksIsRespectedForTextAsync()
        {
            var text = await File.ReadAllTextAsync(Path.Combine(DataPath, "start.md"));
            // This should not prompt, if it does it will throw
            // Leave this as the static method so we can get coverage there.
            await Displayer.DisplayMarkdownAsync(text, new Uri(Path.Combine(DataPath, "start.md")), allowFollowingLinks: false);
        }

        [TestMethod]
        public async Task DisplayTests_ExitWorksAsync()
        {
            // This is going to prompt, accept the default to exit.
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            await _displayer!.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")));
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
            await _displayer!.DisplayMarkdownAsync(text, new Uri(Path.Combine(DataPath, "start.md")));
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
            await _displayer!.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")));

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
            await _displayer!.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")));

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
            await _displayer!.DisplayMarkdownAsync(uri);

            AssertCrossPlatStringMatch($@"Failed to find {uri}
", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_BadUrlsAreReportedAsync()
        {
            var uri = new Uri("https://OkForReallyRealsThisNotAPlace.com/Bad/Path");
            // This should not prompt, if it does it will throw
            await _displayer!.DisplayMarkdownAsync(uri);
            AssertCrossPlatStringMatch(@"Caught HttpRequestException attempting to download https://okforreallyrealsthisnotaplace.com/Bad/Path
", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_BadUrlsAreYieldErrorCodeAsync()
        {
            var uri = new Uri("https://github.com/ForReallyRealsThisIsNotAUSer");
            // This should not prompt, if it does it will throw
            await _displayer!.DisplayMarkdownAsync(uri);
            AssertCrossPlatStringMatch(@"Failed to make web request https://github.com/ForReallyRealsThisIsNotAUSer.  Got 404-NotFound
", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_UnhandledTypesDisplayedAsync()
        {
            var options = new DisplayOptions { IncludeDebug = true };
            var renderer = new ConsoleRenderer(options, omitAutolinkInlineRenderer: true);

            using var tempFiles = new TempFileManager();
            await _displayer!.DisplayMarkdownAsync(
                text: "<https://example.com>",
                baseUri: new Uri(Path.Combine(DataPath, ".")),
                options: options,
                allowFollowingLinks: false,
                tempFiles: tempFiles,
                rendererOverride: renderer);

            AssertCrossPlatStringMatch(@"Unhandled AutolinkInline
┌──┐
│  │
└──┘

", TrimmedConsoleOutput);
        }

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

            await _displayer!.DisplayMarkdownAsync(
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

        [TestMethod]
        public async Task DisplayTests_NonInteractiveTerminalShowsLinksAsync()
        {
            // Use a non-interactive displayer to simulate CI environment
            using var nonInteractiveDisplayer = CreateNonInteractiveDisplayer();
            
            await nonInteractiveDisplayer.DisplayMarkdownAsync(new Uri(Path.Combine(DataPath, "start.md")));
            
            // Should show warning and list links instead of prompting
            AssertCrossPlatStringMatch(@"- [](sub/sub.md)


Warning: Non-interactive terminal detected. The following links are available but cannot be followed interactively:
  • sub/sub.md
", TrimmedConsoleOutput);
        }

        [TestMethod]
        public async Task DisplayTests_NonInteractiveTerminalWithNoLinksExitsCleanlyAsync()
        {
            // Use a non-interactive displayer with a markdown file that has no links
            using var nonInteractiveDisplayer = CreateNonInteractiveDisplayer();
            
            var text = "# Just a heading\n\nNo links here.";
            await nonInteractiveDisplayer.DisplayMarkdownAsync(text);
            
            // Should not show warning since there are no links to display
            AssertCrossPlatStringMatch(@"
# Just a heading #

No links here.

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