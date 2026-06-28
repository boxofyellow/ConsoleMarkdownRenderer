using System.Reflection;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

[TestClass]
public class ConventionTests : TestBase
{
    [TestMethod]
    public void Assert_All_Test_Have_A_Valid_Base_Class()
    {
        var types = GetType().Assembly.GetTypes()
            .Where(t => t.GetCustomAttributes<TestClassAttribute>().Any())
            .Where(t => !t.IsAssignableTo(typeof(TestBase)));

        bool hasViolations = false;
        foreach (var type in types)
        {
            Logger.LogMessage($"Test class {type.FullName} does not inherit from TestBase. {Environment.NewLine}");
            hasViolations = true;
        }

        if (hasViolations)
        {
            Assert.Fail("All test classes should inherit from TestBase to ensure consistent test setup and utilities.");
        }

        TestUtilities.AssertTestNamespaceMatch(GetType());
    }

    [TestMethod]
    public void Check_For_Convention_Violations()
        => ConventionsHelper.FindViolations<SourceFileAttribute>(
            typeof(Displayer),
            attr => attr.FilePath,
            allowedPublicTypes: [
                typeof(Displayer),
                typeof(DisplayOptions),
                typeof(IMarkdownDisplayer),
                typeof(MarkdownDisplayer),
                typeof(FigletTextStyle),
                typeof(IHeaderStyle),
                typeof(RuleBorder),
                typeof(RuleHeaderStyle),
                typeof(TextColor),
                typeof(NamedColor),
                typeof(TextDecoration),
                typeof(TextJustification),
                typeof(TextStyle),
                typeof(TextTableBorder),
                typeof(TempFileManager),
                typeof(FakesConstants),
            ],
            allowedPublicFolders: [
                "Styling",
                "Support"
            ],
            allowedClassFileNameMismatch: [
                "PromptResult",
                "TextColor",
            ],
            allowedStaticFolders: [
                "",
            ],
            allowedApiLeaks: [] // We should never allow leaking any dependent types from this project
            );
}