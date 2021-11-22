using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Test for validating <see cref="ObjectRenderers.TempFileManager"/>
    /// </summary>
    [TestClass]
    public class TempFileManagerTests : TestWithFileCleanupBase
    {
        [TestMethod]
        public void TempFileManagerTests_E2E()
        {
            
            Assert.AreEqual(0, TempFiles.Count, "TempFiles should be empty when test started");
            var temp = TempFiles.GetTempFile();
            Assert.AreEqual(1, TempFiles.Count, "TempFiles should be empty when test started");
            Assert.IsTrue(File.Exists(temp), $"The temp files should have been created");
            TempFiles.Dispose();
            Assert.AreEqual(0, TempFiles.Count, "After disposing, TempFiles should be empty");
            Assert.IsFalse(File.Exists(temp), $"After dispoosing, the files should have been deleted, but {temp} exist");
        }
    }
}