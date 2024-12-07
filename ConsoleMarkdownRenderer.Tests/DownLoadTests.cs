using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Test for validating <see cref="Displayer.DownloadAsync"/>
    /// </summary>
    [TestClass]
    public class DownloadTests : TestWithFileCleanupBase
    {
        [DataTestMethod]
        [DataRow("testGist.txt", "https://gist.githubusercontent.com/boxofyellow/dbddb3d120cdd806afb5e3bad8b069e3/raw/cd401aed633da852d7acfa758d8bdea76c02004b/gistfile1.txt", false)]
        // This really came from https://images.radiopaedia.org/images/9846512/7e77f1307a537a38fb121d6a64cba9_thumb.jpg, but I found multiple download would yield different file content 🤷🏽‍♂️
        // FYI this file lives under ConsoleMarkdownRenderer.Example
        [DataRow("xray.jpg",     "https://gist.githubusercontent.com/boxofyellow/dbddb3d120cdd806afb5e3bad8b069e3/raw/257ca135b5936416389f2ff8996e4693a36dce0e/img.jpg", true)]
        public async Task DownloadTests_HappyPathAsync(string fileName, string url, bool isImage)
        {
            string path = await Displayer.DownloadAsync(new Uri(url), TempFiles, isImage);
            Assert.IsFalse(string.IsNullOrEmpty(path), "File down load should have worked");
            Assert.AreEqual(1, TempFiles.Count, "Should have added new file for cleanup");
            Assert.IsTrue(TempFiles.Contains(path), "Should find path in the files to cleanup");
            Assert.IsTrue(Path.IsPathRooted(path), "Download should yield a full path");
            AssertFileMatchesRawResource(fileName, path);

            // This should yield nothing b/c the headers don't match
            Assert.IsTrue(string.IsNullOrEmpty(await Displayer.DownloadAsync(new Uri(url), TempFiles, !isImage)));
            Assert.AreEqual(1, TempFiles.Count, "Nothing should have been added for cleanup");
        }

        [TestMethod]
        public async Task DownloadTest_BadUrlAsync()
        {
            string path = await Displayer.DownloadAsync(new Uri("https://NotAPlace.com/Bad/Path"), TempFiles, expectImage: false);
            Assert.IsTrue(string.IsNullOrEmpty(path), "No file be crated");
            Assert.AreEqual(0, TempFiles.Count, "No files should be added for cleanup");
        }

        private void AssertFileMatchesRawResource(string fileName, string path)
        {
            Assert.IsTrue(File.Exists(path));
            using var expectedSteam = GetType().Assembly.GetManifestResourceStream(Path.Combine("resources", "raw", fileName));
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            Assert.IsNotNull(expectedSteam);
            Assert.AreEqual(expectedSteam.Length, fileStream.Length, $"Length of {fileName} did not match {path}");

            for (int i = 0; i < expectedSteam.Length; i++)
            {
                Assert.AreEqual(expectedSteam.ReadByte(), fileStream.ReadByte(), $"Did not match @ {i}");
            }
        }
    }
}