using System.Reflection;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class ConventionsHelperTests
{
    [TestMethod]
    [Timeout(TestTimeouts.Unit)]
    public void AssertAllTestMethodsHaveTimeouts_DoesNotThrow_WhenEveryMarkedMethodHasTheRequiredAttribute()
        => ConventionsHelper.AssertAllTestMethodsHaveTimeouts(
            typeof(ConventionsHelperTests).Assembly,
            testMethodMarkerAttribute: typeof(CompliantMarkerAttribute),
            requiredAttribute: typeof(CompliantRequiredAttribute));

    [TestMethod]
    [Timeout(TestTimeouts.Unit)]
    public void AssertAllTestMethodsHaveTimeouts_Fails_WhenAMarkedMethodIsMissingTheRequiredAttribute()
    {
        var ex = Assert.ThrowsExactly<AssertFailedException>(() =>
            ConventionsHelper.AssertAllTestMethodsHaveTimeouts(
                typeof(ConventionsHelperTests).Assembly,
                testMethodMarkerAttribute: typeof(NonCompliantMarkerAttribute),
                requiredAttribute: typeof(CompliantRequiredAttribute)));

        StringAssert.Contains(
            ex.Message,
            $"{typeof(ConventionsHelperTests).FullName}.{nameof(MethodMissingTheRequiredAttribute)}");
    }

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class CompliantMarkerAttribute : Attribute;

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class CompliantRequiredAttribute : Attribute;

    [AttributeUsage(AttributeTargets.Method)]
    private sealed class NonCompliantMarkerAttribute : Attribute;

    // Carries both the marker and required stand-in attributes, so the "no violations" path above
    // exercises real methods rather than an empty result set.
    [CompliantMarkerAttribute]
    [CompliantRequiredAttribute]
    private void MethodWithTheRequiredAttribute()
    {
    }

    // Carries only the marker stand-in attribute (via NonCompliantMarkerAttribute) with no
    // CompliantRequiredAttribute, so the helper's failure path (and its violation-message
    // formatting) gets real code coverage.
    [NonCompliantMarkerAttribute]
    private void MethodMissingTheRequiredAttribute()
    {
    }
}
