namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Base class for tests that need a place to put temp files that should be deleted once the test is complete
    /// </summary>
    public class TestWithFileCleanupBase : ConsoleTestBase
    {
        [TestCleanup]
        public override void TestCleanup()
        {
            TempFiles?.Dispose();
            base.TestCleanup();
        }

        internal TempFileManager TempFiles = new();
    }
}