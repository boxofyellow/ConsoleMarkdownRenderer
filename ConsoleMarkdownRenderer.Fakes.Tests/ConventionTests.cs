using BoxOfYellow.ConsoleMarkdownRenderer.Fakes.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Fakes.Tests;

[TestClass]
public class ConventionTests
{
    [TestMethod]
    public void Assert_Namespaces() => TestUtilities.AssertTestNamespaceMatch(GetType());

    [TestMethod]
    public void Check_For_Convention_Violations()
    {
        var allowedDisplayLeaks = new Type[] {
            typeof(IMarkdownDisplayer),
            typeof(DisplayOptions),
        };

        var allowedSpectreLeaks = new Type[] {
            typeof(MarkdownRenderResult),
        };

        Assert.IsTrue(allowedDisplayLeaks.All(t => t.Namespace == "BoxOfYellow.ConsoleMarkdownRenderer"));
        Assert.IsTrue(allowedSpectreLeaks.All(t => t.Namespace == "BoxOfYellow.ConsoleMarkdownRenderer.Spectre"));

        ConventionsHelper.FindViolations<FakeSourceFileAttribute>(
                    typeof(FakeMarkdownDisplayer),
                    attr => attr.FilePath,
                    allowedPublicTypes: [
                        typeof(FakeMarkdownDisplayer),
                        typeof(ValidatingFakeMarkdownDisplayer),
                        typeof(ValidatedDisplayCall),
                        typeof(MarkdownValidationException),
                    ],
                    allowedPublicFolders: [],
                    allowedClassFileNameMismatch: [
                        "ValidatingFakeMarkdownDisplayer",
                    ],
                    allowedStaticFolders: [],
                    allowedApiLeaks: allowedDisplayLeaks
                                        .Union(allowedSpectreLeaks)
                    );
    }
}