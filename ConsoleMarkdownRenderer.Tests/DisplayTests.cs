using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Test for E2E Tests for <see cref="Displayer"/>
    /// </summary>
    [TestClass]
    public class DisplayTests : ConsoleTestBase
    {
        [TestMethod]
        public void DisplayTests_AllowFollingLinksIsRespected()
            // This should not prompt, if it does it will throw
            => Displayer.DisplayMarkdown(new Uri(Path.Combine(DataPath, "start.md")), allowFollowingLinks: false);

        [TestMethod]
        public void DisplayTests_ExitWorks()
        {
            // This is going to prompt, accept the default to exit.
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            Displayer.DisplayMarkdown(new Uri(Path.Combine(DataPath, "start.md")));
            Assert.AreEqual(@"- [](sub/sub.md)

> Done
  {}(sub/sub.md)", TrimmedConsoleOutput);
        }

        [TestMethod]
        public void DisplayTests_CanFollowingLinks()
        {
            // This is going to prompt, down to select the link.
            ConsoleUnderTest.Input.PushKey(ConsoleKey.DownArrow);
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            // then enter to exit
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            Displayer.DisplayMarkdown(new Uri(Path.Combine(DataPath, "start.md")));

            Assert.AreEqual(@"- [](sub/sub.md)

> Done
  {}(sub/sub.md)   Done
> {}(sub/sub.md) -[](../start.md)

> Done
  Back
  {}(../start.md)", TrimmedConsoleOutput);
        }

        [TestMethod]
        public void DisplayTests_BackWorks()
        {
            // This is going to prompt, down to select the link.
            ConsoleUnderTest.Input.PushKey(ConsoleKey.DownArrow);
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            // down to select back
            ConsoleUnderTest.Input.PushKey(ConsoleKey.DownArrow);
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            // then enter to exit
            ConsoleUnderTest.Input.PushKey(ConsoleKey.Enter);
            Displayer.DisplayMarkdown(new Uri(Path.Combine(DataPath, "start.md")));

            Assert.AreEqual(@"- [](sub/sub.md)

> Done
  {}(sub/sub.md)   Done
> {}(sub/sub.md) -[](../start.md)

> Done
  Back
  {}(../start.md)   Done
> Back
  {}(../start.md) - [](sub/sub.md)

> Done
  {}(sub/sub.md)", TrimmedConsoleOutput);
        }

        [TestMethod]
        public void DisplayTests_MissingFilesAreReported()
        {
            var uri = new Uri(Path.Combine(DataPath, "not-a-file.md"));
            // This should not prompt, if it does it will throw
            Displayer.DisplayMarkdown(uri);

            Assert.AreEqual($@"Failed to find {uri}
", TrimmedConsoleOutput);
        }

        [TestMethod]
        public void DisplayTests_BadUrlsAreReportedFilesAreReported()
        {
            var uri = new Uri("https://NotAPlace.com/Bad/Path");
            // This should not prompt, if it does it will throw
            Displayer.DisplayMarkdown(uri);

            Assert.AreEqual(@"Caught WebException attempting to download https://notaplace.com/Bad/Path
", TrimmedConsoleOutput);
        }

        // There is often trailing spaces included, which we don't need to worry about validating exactly
        private string TrimmedConsoleOutput 
            => string.Join(
                Environment.NewLine,
                ConsoleUnderTest.Output.Split(Environment.NewLine).Select(x => x.TrimEnd()));
    }
}