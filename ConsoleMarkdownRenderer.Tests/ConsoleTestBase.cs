using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Spectre.Console;
using Spectre.Console.Testing;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Base class used for test that will interact with <see cref="Spectre.Console.AnsiConsole">AnsiConsole</see>
    /// </summary>
    public class ConsoleTestBase
    {
        protected ConsoleTestBase()
        {
            m_consoleAtStart = AnsiConsole.Console;
            NewConsole();
        }

        [TestCleanup]
        public virtual void TestCleanup()
        {
            AnsiConsole.Console = m_consoleAtStart;
            CleanUpConsole();
        }

        public TestConsole NewConsole()
        {
            CleanUpConsole();
            m_testConsole = new TestConsole()
                // Juts set a width big enough that we don't need to worry about text wrapping or getting truncated
                .Width(360)
                .Interactive();
            AnsiConsole.Console = m_testConsole;
            return m_testConsole;
        }

        public void CleanUpConsole()
        {
            if (m_testConsole != default)
            {
                try
                {
                    // the value for intercept is ignored
                    var key = m_testConsole.Input.ReadKey(intercept: false);
                    if (key.HasValue)
                    {
                        Assert.Fail($"Found {key.Value.Key} waiting to be read");
                    }
                }
                catch (InvalidOperationException) { }
                finally
                {
                    m_testConsole.Dispose();
                }
                m_testConsole = default;
            }
        }

        protected static void AssertCrossPlatStringMatch(string expected, string actual, string? message = null)
        {
            expected = CrossPlatNormalizeString(expected);
            actual = CrossPlatNormalizeString(actual);

            if (string.IsNullOrEmpty(message))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.AreEqual(expected, actual, message);
            }
        }

        protected static string CrossPlatNormalizeString(string text) 
            => text.Replace(c_crlf, LineBreak).Replace(c_lf, LineBreak);

        protected TestConsole ConsoleUnderTest => m_testConsole!;
        private readonly IAnsiConsole m_consoleAtStart;
        private TestConsole? m_testConsole;
        protected static readonly string DataPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "data"));

        protected const string LineBreak = "\r";
        private const string c_lf = "\n";
        private const string c_crlf = LineBreak + c_lf;
    }
}