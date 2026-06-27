using System.Net;
using System.Text;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Test for validating <see cref="MarkdownDisplayer.DownloadAsync"/>
    /// </summary>
    [TestClass]
    public class DownloadTests : TestWithFileCleanupBase
    {
        [TestMethod]
        public async Task DownloadTests_HappyPath_TextAsync()
        {
            const string expectedContent = "If you can see this, then it worked";

            using var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(expectedContent, Encoding.UTF8, "text/plain")
            });
            using var client = new HttpClient(handler);
            using var displayer = new MarkdownDisplayer(new FakeHttpClientFactory(client));

            string path = await displayer.DownloadAsync(new Uri("https://example.com/file.txt"), TempFiles, expectImage: false);
            Assert.IsFalse(string.IsNullOrEmpty(path), "File download should have worked");
            TestUtilities.AssertTheseMatch(1, TempFiles.Count, shouldMatch: true, "Should have added new file for cleanup");
            Assert.IsTrue(TempFiles.Contains(path), "Should find path in the files to cleanup");
            Assert.IsTrue(Path.IsPathRooted(path), "Download should yield a full path");
            TestUtilities.AssertTheseMatch(expectedContent, await File.ReadAllTextAsync(path), shouldMatch: true);

            // Wrong content type (expecting image, got text/plain) should fail
            Assert.IsTrue(string.IsNullOrEmpty(await displayer.DownloadAsync(new Uri("https://example.com/file.txt"), TempFiles, expectImage: true)));
            TestUtilities.AssertTheseMatch(1, TempFiles.Count, shouldMatch: true, "Nothing should have been added for cleanup");
        }

        [TestMethod]
        public async Task DownloadTests_HappyPath_ImageAsync()
        {
            // JPEG magic bytes — enough to verify the binary content round-trips correctly
            var expectedBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46 };

            using var handler = new FakeHttpMessageHandler(_ =>
            {
                var content = new ByteArrayContent(expectedBytes);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
            });
            using var client = new HttpClient(handler);
            using var displayer = new MarkdownDisplayer(new FakeHttpClientFactory(client));

            string path = await displayer.DownloadAsync(new Uri("https://example.com/photo.jpg"), TempFiles, expectImage: true);
            Assert.IsFalse(string.IsNullOrEmpty(path), "File download should have worked");
            TestUtilities.AssertTheseMatch(1, TempFiles.Count, shouldMatch: true, "Should have added new file for cleanup");
            Assert.IsTrue(TempFiles.Contains(path), "Should find path in the files to cleanup");
            Assert.IsTrue(Path.IsPathRooted(path), "Download should yield a full path");
            CollectionAssert.AreEqual(expectedBytes, await File.ReadAllBytesAsync(path));

            // Wrong content type (expecting text, got image/jpeg) should fail
            Assert.IsTrue(string.IsNullOrEmpty(await displayer.DownloadAsync(new Uri("https://example.com/photo.jpg"), TempFiles, expectImage: false)));
            TestUtilities.AssertTheseMatch(1, TempFiles.Count, shouldMatch: true, "Nothing should have been added for cleanup");
        }

        [TestMethod]
        public async Task DownloadTest_BadStatusCodeAsync()
        {
            using var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            using var client = new HttpClient(handler);
            using var displayer = new MarkdownDisplayer(new FakeHttpClientFactory(client));

            string path = await displayer.DownloadAsync(new Uri("https://example.com/missing.txt"), TempFiles, expectImage: false);
            Assert.IsTrue(string.IsNullOrEmpty(path), "No file should be created for a non-2xx response");
            TestUtilities.AssertTheseMatch(0, TempFiles.Count, shouldMatch: true, "No files should be added for cleanup");
        }

        [TestMethod]
        public async Task DownloadTest_NetworkErrorAsync()
        {
            using var handler = new FakeHttpMessageHandler(_ => throw new HttpRequestException("Simulated network error"));
            using var client = new HttpClient(handler);
            using var displayer = new MarkdownDisplayer(new FakeHttpClientFactory(client));

            string path = await displayer.DownloadAsync(new Uri("https://example.com/unreachable.txt"), TempFiles, expectImage: false);
            Assert.IsTrue(string.IsNullOrEmpty(path), "No file should be created on network error");
            TestUtilities.AssertTheseMatch(0, TempFiles.Count, shouldMatch: true, "No files should be added for cleanup");
        }
    }
}