namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

/// <summary>
/// Named per-test-method timeout tiers (milliseconds) for use with MSTest's <c>[Timeout]</c>
/// attribute. Every <c>[TestMethod]</c>/<c>[DataTestMethod]</c> in every test assembly must carry
/// one of these (enforced by a convention test) so a hung or non-terminating render fails fast and
/// clearly instead of silently stalling a CI run. Centralizing the values here avoids scattering
/// magic numbers and keeps a single source of truth across the four test assemblies.
/// </summary>
public static class TestTimeouts
{
    /// <summary>Fast, pure-logic tests that do not perform a Spectre render.</summary>
    public const int Unit = 5_000;

    /// <summary>Tests that perform a single Spectre render (e.g. via <c>AssertRendersRobustly</c>).</summary>
    public const int Render = 15_000;

    /// <summary>
    /// Tests that iterate over many cases/corpus fixtures/mutations in a single test method.
    /// </summary>
    public const int Suite = 30_000;
}
