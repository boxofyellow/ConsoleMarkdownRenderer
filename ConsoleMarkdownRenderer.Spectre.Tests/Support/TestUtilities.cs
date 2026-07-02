using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

public class TestOptions
{
    public static TestOptions GetOptions()
    {
        const string testOptionsKey = "TestOptions";

        // This seems like a good fit for now, and if we have to change it later we can.
        #pragma warning disable MSTESTEXP
        var context = TestContext.Current;
        #pragma warning restore MSTESTEXP

        Assert.IsNotNull(context, "TestContext is not available. Make sure to set TestContext property in the test class.");
        if (!context.Properties.TryGetValue(testOptionsKey, out var optionsObject))
        {
            var result = new TestOptions();
            result.RegisterDifferenceFinder<SpectreDisplayOptions>((e, a) => TestUtilities.FindFirstDifferenceSpectreDisplayOptions(e, a, details: true));
            result.RegisterDifferenceFinder<string>((e, a) => TestUtilities.FindFirstDifferenceString(e, a, details: true));
            result.SerializerOptions.Add(SpectreDisplayOptions.BuildEffectiveOptions(caller: null));
            context.Properties[testOptionsKey] = result;
            return result;
        }
        else
        {
            if (optionsObject is TestOptions result)
            {
                return result;
            }
            else
            {
                Assert.Fail($"TestContext property '{testOptionsKey}' is not of type {nameof(TestOptions)}.");
                throw new InvalidOperationException(); // This will never be hit but is needed to satisfy the compiler
            }
        }
    }

    public List<JsonSerializerOptions> SerializerOptions = [];

    public void RegisterDifferenceFinder<T>(Func<T, T, string> finder)
    {
        if (!_differenceFinders.TryAdd(typeof(T), new DifferenceFinder<T>(finder)))
        {
            throw new InvalidOperationException($"A difference finder for type {typeof(T)} is already registered.");
        }
    }

    internal string? FindDifference(Type type, object? expected, object? actual)
    {
        if (expected is null && actual is null)
        {
            return null;
        }
        if (expected is null)
        {
            return "Expected is null, actual is not null.";
        }
        if (actual is null)
        {
            return "Actual is null, expected is not null.";
        }   
        if (_differenceFinders.TryGetValue(type, out var finder))
        {
            return finder.FindDifference(expected, actual);
        }
        return null;
    }

    private readonly Dictionary<Type, IDifferenceFinder> _differenceFinders = [];

    private interface IDifferenceFinder
    {
        string FindDifference(object expected, object actual);
    }

    private record class DifferenceFinder<T>(Func<T, T, string> Finder) : IDifferenceFinder
    {
        public string FindDifference(object expected, object actual)
        {
            if (expected is T expectedTyped && actual is T actualTyped)
            {
                return Finder(expectedTyped, actualTyped);
            }
            throw new ArgumentException("Invalid types for difference finder.");
        }
    }
}

public static class TestUtilities
{
    public static void AssertTheseMatch<T>(T expected, T actual, bool shouldMatch, string? message = null) 
        => AssertTheseMatch(typeof(T), expected, actual, shouldMatch, message);

    private static void AssertTheseMatch(Type type, object? expected, object? actual, bool shouldMatch, string? message)
    {
        if (expected is not string && expected is IEnumerable expectedEnumerable && actual is IEnumerable actualEnumerable)
        {
            AssertTheseEnumerableMatch(type, expectedEnumerable, actualEnumerable, shouldMatch, message);
            return;
        }

        bool matches = expected?.Equals(actual) ?? (actual == null);
        if (shouldMatch && !matches)
        {
            var difference = TestOptions.GetOptions().FindDifference(type, expected, actual);
            if (!string.IsNullOrEmpty(difference))
            {
                message = $"Values did not match. First difference: {difference}. {message}";
            }
            else
            {
                message = $"Values did not match. {message}.";
            }
        }
        Assert.AreEqual(shouldMatch, matches, message);
        if (shouldMatch)
        {
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected?.GetHashCode(), actual?.GetHashCode());
        }
        else
        {
            Assert.AreNotEqual(expected, actual);
        }

        CheckIsDefaultImplementations(expected, actual);
    }

    private static void AssertTheseEnumerableMatch(Type enumerableType, IEnumerable expected, IEnumerable actual, bool shouldMatch, string? message = null)
    {
        var expectedList = expected.Cast<object?>().ToArray();
        var actualList = actual.Cast<object?>().ToArray();

        bool isDifferent = false;

        // its ok for the length to be different if shouldMatch is false, b/c one or more of the children might be different
        if (shouldMatch)
        {
            AssertTheseMatch(expectedList.Length, actualList.Length, shouldMatch, $"Enumerable counts did not match. Expected count: {expectedList.Length}, actual count: {actualList.Length}. {message}");
        }
        else
        {
            isDifferent = expectedList.Length != actualList.Length;
        }

        // this should really be be used on IList<T> so just get the first generic argument if there is exactly one 
        Type? genericEnumerableType = null;
        if (enumerableType.IsGenericType && enumerableType.GenericTypeArguments.Length == 1)
        {
            genericEnumerableType = enumerableType.GenericTypeArguments.Single();
        }

        for (int i = 0; i < expectedList.Length; i++)
        {
            var expectedItem = expectedList[i];
            var actualItem = actualList[i];

            // if we have a generic argument, use that if, if not the type of the element (assuming they match)
            var type = genericEnumerableType;
            if (type is null && expectedItem?.GetType() == actualItem?.GetType())
            {
                type = expectedItem?.GetType();
            }

            if (shouldMatch)
            {
                // If we got a type, we will use to check these elements, if not throw it at Assert.AreXXXEqual to see what sticks
                if (type is null)
                {
                    if (shouldMatch)
                    {
                        Assert.AreEqual(expectedItem, actualItem, $"Enumerable items at index {i} did not match. {message}");
                    }
                    else
                    {
                        Assert.AreNotEqual(expectedItem, actualItem, $"Enumerable items at index {i} should not have matched. {message}");
                    }
                }
                else
                {
                    AssertTheseMatch(type, expectedItem, actualItem, shouldMatch, $"Enumerable items at index {i} did not match. {message}");
                }
            }
            else
            {
                isDifferent |= !Equals(expectedItem, actualItem);
                CheckIsDefaultImplementations(expectedItem, actualItem);
            }
        }

        if (!shouldMatch)
        {
            Assert.IsTrue(isDifferent, $"Enumerables should not have matched, but all items were equal. {message}");
        }   

        CheckIsDefaultImplementations(expected, actual);
    }

    private static void CheckIsDefaultImplementations(object? expected, object? actual)
    {
        var expectedIdentifier = AssertAtMostOneConverterHasIsDefaultImplementation(expected);
        var actualIdentifier = AssertAtMostOneConverterHasIsDefaultImplementation(actual);

        if (expected is not null && actual is not null && expected.GetType() == actual.GetType())
        {
            Assert.AreEqual(expectedIdentifier, actualIdentifier, $"Expected and actual have different converters with IsDefault implementations. Expected converter: {expectedIdentifier?.GetType().FullName}, actual converter: {actualIdentifier?.GetType().FullName}");
        }
    }

    private static IDefaultIdentifier? AssertAtMostOneConverterHasIsDefaultImplementation(object? obj)
    {
        if (obj is null)
        {
            return null;
        }

        // Check if any of our converters have a specific IsDefault implementation for this type
        // we don't really care if it thinks it is default or not, but we do care about is that
        // at most one converter has an implementation for this type
        IDefaultIdentifier? defaultIdentifier = null;
        foreach (var defaultOptions in TestOptions.GetOptions().SerializerOptions)
        {
            foreach (var converter in defaultOptions.Converters)
            {
                if (converter is IDefaultIdentifier identifier)
                {
                    var value = identifier.IsDefault(obj);
                    if (value is not null)
                    {
                        Assert.IsNull(defaultIdentifier, $"Multiple converters with IsDefault implementation found that apply to this object. Previous: {defaultIdentifier?.GetType().FullName}, new: {identifier.GetType().FullName}");
                        defaultIdentifier = identifier;
                    }
                }   
            }
        }
        return defaultIdentifier;
    }

    public static void AssertTestNamespaceMatch(Type type)
    {
        var types = type.Assembly.GetTypes()
            .Where(t => !IsCompilerGenerated(t))
            .Where(t => t.Namespace != type.Namespace);

        var hasViolations = false;
        foreach (var t in types)
        {
            Logger.LogMessage($"Type {t} is not in the {type.Namespace} namespace. {Environment.NewLine}");
            hasViolations = true;
        }

        if (hasViolations)
        {
            Assert.Fail($"All test classes should should be in the {type.Namespace} namespace.");
        }
    }

    // Identifies types the SDK injects (e.g. <PrivateImplementationDetails>,
    // collection-expression helpers like <>z__ReadOnlyArray, and the MSTest auto-generated
    // Program).
    private static bool IsCompilerGenerated(Type type)
    {
        for (var current = type; current != null; current = current.DeclaringType)
        {
            if (current.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
            {
                return true;
            }

            if (string.IsNullOrEmpty(current.Namespace) && current.Name.Contains('<'))
            {
                return true;
            }
        }

        return type.Name == "AutoGeneratedProgram";
    }


    public const string CrazyFormat = "red on purple";
    public static SpectreDisplayOptions Crazy
    {
        get
        {
            var result = new SpectreDisplayOptions();
            foreach (var property in Mappings.SpectreDisplayOptionsProperties)
            {
                if (property.Value.Type == typeof(Style))
                {
                    property.Value.Setter(result, (Style)CrazyFormat);
                }
                else if (property.Value.Type == typeof(int))
                {
                    var val = (int)property.Value.Getter(result);
                    property.Value.Setter(result, val + 1);
                }
                else if (property.Value.Type == typeof(ISpectreHeaderStyle))
                {
                    property.Value.Setter(result, (SpectreTextStyle)CrazyFormat);
                }
                else if (property.Value.Type == typeof(bool))
                {
                    var val = (bool)property.Value.Getter(result);
                    property.Value.Setter(result, !val);
                }
                else if (property.Value.Type == typeof(string))
                {
                    // We only have one string property (MathBlockLabelText)... maybe change this later...
                    property.Value.Setter(result, "math");
                }
                else if (property.Value.Type == typeof(TableBorder))
                {
                    property.Value.Setter(result, TableBorder.Heavy);
                }
                else if (property.Value.Type == typeof(BoxBorder))
                {
                    property.Value.Setter(result, BoxBorder.Heavy);
                }
                else if (property.Value.Type == typeof(List<ISpectreHeaderStyle>))
                {
                    var list = new List<ISpectreHeaderStyle>
                    {
                        (SpectreTextStyle)CrazyFormat
                    };
                    property.Value.Setter(result, list);
                }
                else
                {
                    throw new NotSupportedException($"Type {property.Value.Type} not supported for crazy options generation");
                }
            }
            return result;
        }
    }

    public static string FindFirstDifferenceSpectreDisplayOptions(SpectreDisplayOptions expected, SpectreDisplayOptions actual, bool details)
    {
        foreach (var property in Mappings.SpectreDisplayOptionsProperties)
        {
            var type = property.Value.Type;
            if (type == typeof(Style) 
             || type == typeof(int)
             || type == typeof(ISpectreHeaderStyle)
             || type == typeof(bool)
             || type == typeof(string)
             || type == typeof(TableBorder)
             || type == typeof(BoxBorder))
            {
                var expectedValue = property.Value.Getter(expected);
                var actualValue = property.Value.Getter(actual);
                if (!Equals(expectedValue, actualValue))
                {
                    if (details)
                    {
                        return $"{property.Key} (expected: {expectedValue}, actual: {actualValue})";
                    }
                    return property.Key;
                }
            }
            else if (type == typeof(List<ISpectreHeaderStyle>))
            {
                var expectedList = (List<ISpectreHeaderStyle>)property.Value.Getter(expected);
                var actualList = (List<ISpectreHeaderStyle>)property.Value.Getter(actual);
                if (expectedList.Count != actualList.Count)
                {
                    if (details)
                    {
                        return $"{property.Key} (expected count: {expectedList.Count}, actual count: {actualList.Count})";
                    }
                    return property.Key;
                }
                for (int i = 0; i < expectedList.Count; i++)
                {
                    if (!Equals(expectedList[i], actualList[i]))
                    {
                        if (details)
                        {
                            return $"{property.Key}[{i}] (expected: {expectedList[i]}, actual: {actualList[i]})";
                        }
                        return $"{property.Key}[{i}]";
                    }
                }
            }
            else
            {
                throw new NotSupportedException($"Type {type} not supported for comparison in FindFirstDifference");
            }
        }
        return "";
    }

    public static string FindFirstDifferenceString(string expected, string actual, bool details)
    {
        if (expected == actual)
        {
            return "";
        }

        int minLength = Math.Min(expected.Length, actual.Length);
        for (int i = 0; i < minLength; i++)
        {
            if (expected[i] != actual[i])
            {
                if (details)
                {
                    return $@"Index {i} (expected: '{expected[i]}', actual: '{actual[i]}')
raw expected: {expected}
raw actual: {actual}
";
                }
                return $"Index {i}";
            }
        }

        if (expected.Length != actual.Length)
        {
            if (details)
            {
                return $@"Length mismatch (expected: {expected.Length}, actual: {actual.Length})
raw expected: {expected}
raw actual: {actual}
";
            }
            return "Length mismatch";
        }

        return "";
    }
}
