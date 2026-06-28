using System.Runtime.InteropServices;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class PathComparisonTests
{

    private const string c_path = @"C:\folder\file.txt";
    private const string c_differentPath = @"C:\folder\subfolder\file.txt";
    private const string c_differentCase = @"C:\FoLdeR\FiLe.Txt";

    [TestMethod]
    public void Different_Paths_Are_Different()
        => AssertTheseMatch(c_path, c_differentPath, shouldMatch: false);

    [TestMethod]
    public void Same_Paths_Are_Equal()
        => AssertTheseMatch(c_path, c_path, shouldMatch: true);

    [TestMethod]
    public void Different_Case_Are_Sometime_Different()
    {
        var path1 = c_path;
        var path2 = c_differentCase;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ||
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Assert.IsTrue(string.Equals(path1, path2, PathComparison.Comparison));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            Assert.IsFalse(string.Equals(path1, path2, PathComparison.Comparison));
        }
        else
        {
            Assert.Inconclusive("Unknown OS platform");
        }
    }

    private static void AssertTheseMatch(string str1, string str2, bool shouldMatch)
    {
        TestUtilities.AssertTheseMatch(str1, str2, shouldMatch);
        HashSet<string> set = new(StringComparer.FromComparison(PathComparison.Comparison))
        {
            str1,
            str2
        };
        Assert.HasCount(shouldMatch ? 1 : 2, set);
    }
}