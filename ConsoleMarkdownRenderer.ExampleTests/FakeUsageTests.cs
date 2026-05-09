using ConsoleMarkdownRenderer.Fakes;

namespace ConsoleMarkdownRenderer.ExampleTests
{
    [TestClass]
    public class FakeMarkdownDisplayerTests
    {
        [TestMethod]
        public async Task DisplayMarkdownAsync_WithUri_RecordsCall()
        {
            // Arrange
            var fake = new FakeMarkdownDisplayer();
            var uri = new Uri("https://example.com/readme.md");

            // Act
            await fake.DisplayMarkdownAsync(uri);

            // Assert
            Assert.AreEqual(1, fake.Calls.Count);
            Assert.AreEqual(uri, fake.Calls[0].Uri);
            Assert.IsNull(fake.Calls[0].Text);
            Assert.IsTrue(fake.Calls[0].AllowFollowingLinks);
        }

        [TestMethod]
        public async Task DisplayMarkdownAsync_WithText_RecordsCall()
        {
            // Arrange
            var fake = new FakeMarkdownDisplayer();
            var text = "# Hello";
            var baseUri = new Uri("https://example.com/");

            // Act
            await fake.DisplayMarkdownAsync(text, baseUri, allowFollowingLinks: false);

            // Assert
            Assert.AreEqual(1, fake.Calls.Count);
            Assert.IsNull(fake.Calls[0].Uri);
            Assert.AreEqual(text, fake.Calls[0].Text);
            Assert.AreEqual(baseUri, fake.Calls[0].BaseUri);
            Assert.IsFalse(fake.Calls[0].AllowFollowingLinks);
        }

        [TestMethod]
        public async Task DisplayMarkdownAsync_MultipleCalls_RecordsAll()
        {
            // Arrange
            var fake = new FakeMarkdownDisplayer();

            // Act
            await fake.DisplayMarkdownAsync(new Uri("https://example.com/a.md"));
            await fake.DisplayMarkdownAsync("# Test");
            await fake.DisplayMarkdownAsync(new Uri("file:///tmp/b.md"), allowFollowingLinks: false);

            // Assert
            Assert.AreEqual(3, fake.Calls.Count);
        }

        [TestMethod]
        public async Task DisplayMarkdownAsync_WithOptions_RecordsOptions()
        {
            // Arrange
            var fake = new FakeMarkdownDisplayer();
            var options = new DisplayOptions { IncludeDebug = true };

            // Act
            await fake.DisplayMarkdownAsync("# Test", options: options);

            // Assert
            Assert.AreEqual(1, fake.Calls.Count);
            Assert.AreSame(options, fake.Calls[0].Options);
        }
    }
}
