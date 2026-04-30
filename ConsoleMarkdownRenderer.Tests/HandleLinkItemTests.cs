using System;
using System.IO;
using System.Threading.Tasks;
using Markdig.Syntax.Inlines;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="Displayer.HandleLinkItemAsync"/>
    /// </summary>
    [TestClass]
    public class HandleLinkItemTests : TestWithFileCleanupBase
    {
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
                (var text, var baseUri, var needToPrompt) = await Displayer.HandleLinkItemAsync(
                    new Uri(start),
                    NewLinkItem(url),
                    TempFiles);

                Assert.IsFalse(needToPrompt, "We should have new markdown to display");

                Assert.AreEqual(baseUri, new Uri(expectedFullPath), "The Uri should have been updated");
                await AssertFileMatchesTextAsync(text, expectedFullPath);
                Assert.AreEqual(0, TempFiles.Count, "No files should have been downloaded");
            }
        }

        [TestMethod]
        public async Task HandleLinkItemTests_MissingFilesAreIgnoredAsync()
        {
            var target = "not-a-file.txt";
            var started = Path.Combine(DataPath, "start.md");
            
            // This should not prompt, but if it does it will throw

            (var text, var baseUri, var needToPrompt) = await Displayer.HandleLinkItemAsync(
                new Uri(started),
                NewLinkItem(target),
                TempFiles);

            Assert.IsTrue(needToPrompt, "The files does not exists, so we should prompt again");

            Assert.AreEqual(baseUri, new Uri(started), "The uri should not be changed");
            Assert.IsTrue(string.IsNullOrEmpty(text), "No contents should be returned");
            Assert.AreEqual(0, TempFiles.Count, "No files should have been downloaded");
        }

        [TestMethod]
        public async Task HandleLinkItemTests_NonMarkdownAreOpenedAsync()
        {
            var target = "not-a-markdown.txt";
            var started = Path.Combine(DataPath, "start.md");

            // This is going to prompt, say "no" to avoid opening the file.
            ConsoleUnderTest.Input.PushTextWithEnter("n");

            (var text, var baseUri, var needToPrompt) = await Displayer.HandleLinkItemAsync(
                new Uri(started),
                NewLinkItem(target),
                TempFiles);

            Assert.IsTrue(needToPrompt, "The files does not exists, so we should prompt again");

            Assert.AreEqual(baseUri, new Uri(started), "The uri should not be changed");
            Assert.IsTrue(string.IsNullOrEmpty(text), "No contents should be returned");
            Assert.AreEqual(0, TempFiles.Count, "No files should have been downloaded");
        }

        /// <summary>
        /// Covers Displayer.cs L309: when a web URI with a markdown extension is selected,
        /// DownloadAsync is called to fetch it before processing. The download fails for a
        /// non-existent host, so OpenAsync is then called, and the test says "no".
        /// </summary>
        [TestMethod]
        public async Task HandleLinkItemTests_WebMarkdownTriesDownloadAsync()
        {
            // A web URL with .md extension triggers DownloadAsync (L309)
            var target = "https://definitely.not.real.invalid/document.md";
            var started = Path.Combine(DataPath, "start.md");

            // Say "no" when prompted to open the URL (download will fail)
            ConsoleUnderTest.Input.PushTextWithEnter("n");

            (var text, var baseUri, var needToPrompt) = await Displayer.HandleLinkItemAsync(
                new Uri(started),
                NewLinkItem(target),
                TempFiles);

            Assert.IsTrue(needToPrompt, "Should re-prompt after failed download");
            Assert.IsTrue(string.IsNullOrEmpty(text), "No text should be returned when download fails");
        }

        /// <summary>
        /// Covers Displayer.cs L367-L391: the OpenAsync try/catch block is entered when the
        /// user confirms opening a URL. On Linux, Process.Start throws for a plain URL string,
        /// which causes the catch block to invoke xdg-open instead.
        /// </summary>
        [TestMethod]
        public async Task HandleLinkItemTests_OpenAsyncCalledOnConfirmAsync()
        {
            // A web URL with no recognized extension is passed directly to OpenAsync (skips L309)
            var target = "https://example.com/some-page";
            var started = Path.Combine(DataPath, "start.md");

            // Say "yes" to trigger Process.Start inside OpenAsync (covers L367-391)
            ConsoleUnderTest.Input.PushTextWithEnter("y");

            (var text, var baseUri, var needToPrompt) = await Displayer.HandleLinkItemAsync(
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

            Assert.AreEqual(expectedSteam.Length, fileStream.Length, $"Length of text did not match {path}");

            for (int i = 0; i < expectedSteam.Length; i++)
            {
                Assert.AreEqual(expectedSteam.ReadByte(), fileStream.ReadByte(), $"Did not match @ {i}");
            }
        }

        private static LinkItem NewLinkItem(string url) 
            => new(new LinkInline(url: url, title: string.Empty), content: "content");
    }
}