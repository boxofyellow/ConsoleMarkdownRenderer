using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Base class for tests that need a place to put temp files that should be deleted once the test is complete
    /// </summary>
    public class TestWithFileCleanupBase : ConsoleTestBase
    {
        [TestCleanup]
        public override void TestCleanup()
        {
            foreach (var file in TempFiles)
            {
                if (File.Exists(file))
                {
                    Logger.LogMessage($"Deleting {file}");
                    File.Delete(file);
                }
            }
            TempFiles.Clear();

            base.TestCleanup();
        }

        protected List<string> TempFiles = new();
    }
}