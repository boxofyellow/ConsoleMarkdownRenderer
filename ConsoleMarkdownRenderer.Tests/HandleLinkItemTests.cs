using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="MarkdownDisplayer.HandleLinkItemAsync"/>
    /// </summary>
    [TestClass]
    public class HandleLinkItemTests : TestWithFileCleanupBase
    {
        private readonly MarkdownDisplayer _displayer = new();

        [TestCleanup]
        public override void TestCleanup()
        {
            _displayer.Dispose();
            base.TestCleanup();
        }

        [TestMethod]
        public async Task HandleLinkItemTests_RetriesMarkdownAsync()
        {
            var target = Path.Combine("sub", "sub.md");
            var expectedFullPath = Path.Combine(DataPath, target);

            // This should not prompt, but if does it will throw

            foreach ((var start, var url) in new (string, string)[]{
                new (Path.Combine(DataPath, "start.md"), target),    // Test Relative Links
                new (DataPath, expectedFullPath),                    // Test Absolute Links
                new ("https://www.google.com", expectedFullPath),    // Test Absolute Local Links with web base
            })
            {
                Logger.LogMessage($"Testing {start} -> {url}");
                (var text, var baseUri, var needToPrompt) = await _displayer.HandleLinkItemAsync(
                    new Uri(start),
                    NewLinkItem(url),
                    TempFiles);

                Assert.IsFalse(needToPrompt, "We should have new markdown to display");

                TestUtilities.AssertTheseMatch(baseUri, new Uri(expectedFullPath), shouldMatch: true, "The Uri should have been updated");
                await AssertFileMatchesTextAsync(text, expectedFullPath);
                TestUtilities.AssertTheseMatch(0, TempFiles.Count, shouldMatch: true, "No files should have been downloaded");
            }
        }

        [TestMethod]
        public async Task HandleLinkItemTests_MissingFilesAreIgnoredAsync()
        {
            var target = "not-a-file.txt";
            var started = Path.Combine(DataPath, "start.md");
            
            // This should not prompt, but if it does it will throw

            (var text, var baseUri, var needToPrompt) = await _displayer.HandleLinkItemAsync(
                new Uri(started),
                NewLinkItem(target),
                TempFiles);

            Assert.IsTrue(needToPrompt, "The files does not exists, so we should prompt again");

            TestUtilities.AssertTheseMatch(baseUri, new Uri(started), shouldMatch: true, "The uri should not be changed");
            Assert.IsTrue(string.IsNullOrEmpty(text), "No contents should be returned");
            TestUtilities.AssertTheseMatch(0, TempFiles.Count, shouldMatch: true, "No files should have been downloaded");
        }

        [TestMethod]
        public async Task HandleLinkItemTests_NonMarkdownAreOpenedAsync()
        {
            var target = "not-a-markdown.txt";
            var started = Path.Combine(DataPath, "start.md");

            // This is going to prompt, say "no" to avoid opening the file.
            ConsoleUnderTest.Input.PushTextWithEnter("n");

            (var text, var baseUri, var needToPrompt) = await _displayer.HandleLinkItemAsync(
                new Uri(started),
                NewLinkItem(target),
                TempFiles);

            Assert.IsTrue(needToPrompt, "The files does not exists, so we should prompt again");

            TestUtilities.AssertTheseMatch(baseUri, new Uri(started), shouldMatch: true, "The uri should not be changed");
            Assert.IsTrue(string.IsNullOrEmpty(text), "No contents should be returned");
            TestUtilities.AssertTheseMatch(0, TempFiles.Count, shouldMatch: true, "No files should have been downloaded");
        }

        [TestMethod]
        public async Task HandleLinkItemTests_WebMarkdownTriesDownloadAsync()
        {
            // A web URL with .md extension triggers DownloadAsync.
            // Use a fake factory that returns a 404 so no live network call is needed.
            var target = "https://example.com/document.md";
            var started = Path.Combine(DataPath, "start.md");

            using var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            using var client = new HttpClient(handler);
            using var displayer = new MarkdownDisplayer(new FakeHttpClientFactory(client));

            // Say "no" when prompted to open the URL (download will fail)
            ConsoleUnderTest.Input.PushTextWithEnter("n");

            (var text, var baseUri, var needToPrompt) = await displayer.HandleLinkItemAsync(
                new Uri(started),
                NewLinkItem(target),
                TempFiles);

            Assert.IsTrue(needToPrompt, "Should re-prompt after failed download");
            Assert.IsTrue(string.IsNullOrEmpty(text), "No text should be returned when download fails");
        }

        [TestMethod]
        public async Task HandleLinkItemTests_OpenAsyncCalledOnConfirmAsync()
        {
            if (Environment.GetEnvironmentVariable("SKIP_WEB_TESTS") == "1")
            {
                Assert.Inconclusive("This test is not suitable for automated environments that may not have a default browser configured.");
            }
            // A web URL with no recognized extension goes directly to OpenAsync
            var target = "https://example.com/some-page";
            var started = Path.Combine(DataPath, "start.md");

            // Say "yes" to trigger Process.Start inside OpenAsync
            ConsoleUnderTest.Input.PushTextWithEnter("y");

            (var text, var baseUri, var needToPrompt) = await _displayer.HandleLinkItemAsync(
                new Uri(started),
                NewLinkItem(target),
                TempFiles);

            Assert.IsTrue(needToPrompt, "Should re-prompt after opening the URL");
            Assert.IsTrue(string.IsNullOrEmpty(text), "No text should be returned after opening URL");
        }

        private async static Task AssertFileMatchesTextAsync(string text, string path)
        {
            Assert.IsTrue(File.Exists(path));
            using var expectedSteam = new MemoryStream();
            using var writer = new StreamWriter(expectedSteam);
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            await writer.WriteAsync(text);
            await writer.FlushAsync();
            expectedSteam.Position = 0;

            TestUtilities.AssertTheseMatch(expectedSteam.Length, fileStream.Length, shouldMatch: true, $"Length of text did not match {path}");

            for (int i = 0; i < expectedSteam.Length; i++)
            {
                TestUtilities.AssertTheseMatch(expectedSteam.ReadByte(), fileStream.ReadByte(), shouldMatch: true, $"Did not match @ {i}");
            }
        }

        private static LinkItem NewLinkItem(string url) 
            => new(url: url, content: "content");
    }
}