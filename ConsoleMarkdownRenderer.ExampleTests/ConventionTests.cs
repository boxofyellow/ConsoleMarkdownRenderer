using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

namespace BoxOfYellow.ConsoleMarkdownRenderer.ExampleTests;

[TestClass]
public class ConventionTests
{
    [TestMethod]
    [Timeout(TestTimeouts.Unit)]
    public void Assert_Namespaces() => TestUtilities.AssertTestNamespaceMatch(GetType());

    [TestMethod]
    [Timeout(TestTimeouts.Unit)]
    public void Assert_All_Test_Methods_Have_Timeouts() => ConventionsHelper.AssertAllTestMethodsHaveTimeouts(GetType().Assembly);
}
