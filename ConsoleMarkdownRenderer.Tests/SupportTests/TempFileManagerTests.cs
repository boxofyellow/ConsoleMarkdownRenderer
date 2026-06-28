using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

/// <summary>
/// Test for validating <see cref="ObjectRenderers.TempFileManager"/>
/// </summary>
[TestClass]
public class TempFileManagerTests : TestWithFileCleanupBase
{
    [TestMethod]
    public void TempFileManagerTests_E2E()
    {
        
        TestUtilities.AssertTheseMatch(0, TempFiles.Count, shouldMatch: true, "TempFiles should be empty when test started");
        var temp = TempFiles.GetTempFile();
        TestUtilities.AssertTheseMatch(1, TempFiles.Count, shouldMatch: true, "TempFiles should not be empty once a file is requested");
        Assert.IsTrue(File.Exists(temp), $"The temp files should have been created");
        TempFiles.Dispose();
        TestUtilities.AssertTheseMatch(0, TempFiles.Count, shouldMatch: true, "After disposing, TempFiles should be empty");
        Assert.IsFalse(File.Exists(temp), $"After disposing, the files should have been deleted, but {temp} exist");
    }
}