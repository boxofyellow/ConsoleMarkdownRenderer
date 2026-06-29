using ConsoleMarkdownRenderer.ApiLeakTests.TestsDonor;
using ConsoleMarkdownRenderer.ApiLeakTests.TestsConsumer;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class ApiLeakCheckerTests
{
    [TestMethod]
    public void Should_Find_All_Donated_Types()
    {
        var allowedLeaks = typeof(DonorBaseType)
            .Assembly
            .GetTypes()
            .Where(t => t.IsPublic || t.IsNestedPublic)
            .ToDictionary(t => t, t => true);

        var result = ApiLeakChecker.CheckForLeaks(typeof(BaseTypeConsumer).Assembly, allowedLeaks);
        Assert.IsFalse(result, "ApiLeakChecker should not find leaks");

        var untrackedLeaks = allowedLeaks
            .Where(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .ToList();

        Assert.HasCount(0, untrackedLeaks, $"ApiLeakChecker did not track all donated types {string.Join(", ", untrackedLeaks.Select(t => t.Name))}");
    }
}
