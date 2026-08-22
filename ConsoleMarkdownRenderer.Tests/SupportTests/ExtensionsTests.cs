using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

[TestClass]
public class ExtensionsTests : TestBase
{
    private class BadHeaderStyle : IHeaderStyle
    {
        public TextColor? Foreground => throw new NotImplementedException();
        public TextColor? Background => throw new NotImplementedException();

        public TextDecoration Decoration => throw new NotImplementedException();
    }

    private class BadISpectreHeaderStyle : ISpectreHeaderStyle
    {
        public Color? Foreground => throw new NotImplementedException();

        public Color? Background => throw new NotImplementedException();

        public Decoration Decoration => throw new NotImplementedException();
    }

    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void Test_BadHeaderStyle()
    {
        var badHeaderStyle = new BadHeaderStyle();
        Assert.Throws<ArgumentException>(badHeaderStyle.ToSpectreHeaderStyle);
    }

    [Timeout(TestTimeouts.Unit)]
    [TestMethod]
    public void Test_BadISpectreHeaderStyle()
    {
        var badISpectreHeaderStyle = new BadISpectreHeaderStyle();
        Assert.Throws<ArgumentException>(badISpectreHeaderStyle.ToHeaderStyle);
    }
}