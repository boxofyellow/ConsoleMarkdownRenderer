namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class NamespaceTests
{
    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void All_Test_Namespaces_Start_With_BoxOfYellow() 
        => NamespaceReflectionsUtilities.AssertAllNamespacesStartWithBoxOfYellow(typeof(NamespaceTests).Assembly);

    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void All_Public_Tests_Are_The_TestNamespace() 
        => NamespaceReflectionsUtilities.AssertAllNamespacesOfPublicTypesAreInValidateNamesSpaces(
            typeof(NamespaceTests).Assembly,
            [typeof(NamespaceTests).Namespace!]);

    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void All_Namespaces_Start_With_BoxOfYellow() 
        => NamespaceReflectionsUtilities.AssertAllNamespacesStartWithBoxOfYellow(typeof(MarkdownRenderer).Assembly);

    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void All_Tests_Are_The_TestNamespace() 
        => NamespaceReflectionsUtilities.AssertAllNamespacesOfPublicTypesAreInValidateNamesSpaces(
            typeof(MarkdownRenderer).Assembly,
            [
                "BoxOfYellow.ConsoleMarkdownRenderer.Spectre",
                "BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling",
                "BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support",
            ]);
}