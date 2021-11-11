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

        protected TestConsole ConsoleUnderTest => m_testConsole!;
        private readonly IAnsiConsole m_consoleAtStart;
        private TestConsole? m_testConsole;
        protected static readonly string DataPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "data"));
    }
}