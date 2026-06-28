using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

public static class TestJsonHelper
{
    public static readonly JsonSerializerOptions EnumJsonOptions = new() { Converters = { new JsonStringEnumConverter() }};

    // Start with json, does it yield the expected value? Then serializing back to json and deserializing again does it still yield the expected value
    // This uses equality checks (not reference checks)
    // Then run all our JsonSerializerOptions cases
    public static void RoundTrip<T>(string json, T expected, JsonSerializerOptions? options, bool assertNoDefaultEnums)
    {
        T result = Deserialize<T>(json, options);
        var newJson = Serialize(result, options);
        TestUtilities.AssertTheseMatch(expected, result, shouldMatch: true, $"Started with {json} got {newJson} did not match expected");

        result = Deserialize<T>(newJson, options);
        TestUtilities.AssertTheseMatch(expected, result, shouldMatch: true, $"Started with {json} got {newJson} did not match expected");

        foreach (var validationOptions in _validationCases)
        {
            ValidateOptions(result, options, validationOptions, assertNoDefaultEnums);
        }
    }

    private class DummyClass<T>
        where T : class
    {
        public T? Value { get; set; }
    }

    // Start with object, serialize to json and back, do we get the same object back (reference check)?
    public static void RoundTripClass<T>(T? value, JsonSerializerOptions? options = null)
        where T : class
    {
        var dummy = new DummyClass<T> { Value = value };
        var json = Serialize(dummy, options);
        var result = Deserialize<DummyClass<T>>(json, options);
        Assert.IsNotNull(result);
        TestUtilities.AssertTheseMatch(dummy.Value, result.Value, shouldMatch: true);
    }

    // This is a dummy class that **_uses_** a struct... hence the name
    private class DummyStruct<T>
        where T : struct
    {
        public T? Value { get; set; }
    }

    // Start with object, serialize to json and back, do we get the same value back (equality check)?
    public static void RoundTripStruct<T>(T? value, JsonSerializerOptions? options = null)
        where T : struct
    {
        var dummy = new DummyStruct<T> { Value = value };
        var json = Serialize(dummy, options);
        var result = Deserialize<DummyStruct<T>>(json, options);
        Assert.IsNotNull(result);
        TestUtilities.AssertTheseMatch(dummy.Value, result.Value, shouldMatch: true);
    }

    public static T Deserialize<T>(string json, JsonSerializerOptions? options)
    {
        T? result;
        try
        {
            result = JsonSerializer.Deserialize<T>(json, options);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to deserialize {typeof(T).Name} JSON. Json: {json}. Exception: {ex}");
            throw;
        }
        Assert.IsNotNull(result);
        return result;
    }

    public static string Serialize<T>(T obj, JsonSerializerOptions? options)
    {
        string json;
        try
        {
            json = JsonSerializer.Serialize(obj, options);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Failed to serialize object to JSON. Object: {obj}. Exception: {ex}");
            throw;
        }
        return json;
    }

    private static JsonSerializerOptions GetOptions(JsonSerializerOptions? options, JsonValidationOptions validationOptions)
    {
        var result = new JsonSerializerOptions();

        if (validationOptions.HasFlag(JsonValidationOptions.PropertyNameCaseInsensitive))
        {
            result.PropertyNameCaseInsensitive = true;
        }
        if (validationOptions.HasFlag(JsonValidationOptions.StringEnumHandling))
        {
            result.Converters.Add(new JsonStringEnumConverter());
        }
        if (validationOptions.HasFlag(JsonValidationOptions.DefaultIgnoreConditionWhenWritingNull))
        {
            result.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            if (validationOptions.HasFlag(JsonValidationOptions.DefaultIgnoreConditionWhenWritingDefault))
            {
                Assert.Fail("Cannot have both DefaultIgnoreConditionWhenWritingNull and DefaultIgnoreConditionWhenWritingDefault");
            }
        }
        if (validationOptions.HasFlag(JsonValidationOptions.DefaultIgnoreConditionWhenWritingDefault))
        {
            if (!validationOptions.HasFlag(JsonValidationOptions.StringEnumHandling))
            {
                // basically without it this is becomes very hard deal with nullable enums that use a value of 0
                Assert.Fail("DefaultIgnoreConditionWhenWritingDefault requires StringEnumHandling to be set as well");
            }
            result.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        }
        if (validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicyCamelCase))
        {
            result.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            if (validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicySnakeCaseLower))
            {
                Assert.Fail("Cannot have both JsonNamingPolicyCamelCase and JsonNamingPolicySnakeCase");
            }
            if (validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicyKebabCaseUpper))
            {
                Assert.Fail("Cannot have both JsonNamingPolicyCamelCase and JsonNamingPolicyKebabCaseUpper");
            }
        }
        if (validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicySnakeCaseLower))
        {
            result.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
            if (validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicyKebabCaseUpper))
            {
                Assert.Fail("Cannot have both JsonNamingPolicySnakeCaseLower and JsonNamingPolicyKebabCaseUpper");
            }
        }
        if (validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicyKebabCaseUpper))
        {
            result.PropertyNamingPolicy = JsonNamingPolicy.KebabCaseUpper;
        }
        if (validationOptions.HasFlag(JsonValidationOptions.CheckExtraProperties))
        {
            // This will allow us to easily just add extra properties
            result.AllowTrailingCommas = true;
            result.PropertyNameCaseInsensitive = true;
        }
        if (validationOptions.HasFlag(JsonValidationOptions.ReorderProperties))
        {
            // This will make it easy to just reorder the lines.
            result.WriteIndented = true;
        }

        if (options is not null)
        {
            foreach (var converter in options.Converters )
            {
                if (converter is JsonStringEnumConverter)
                {
                    // we handle this via the flag, so skip it to avoid duplicates
                    continue;
                }
                result.Converters.Add(converter);
            }
        }
        return result;
    }

    internal static (IEnumerable<string> AllEnums, IEnumerable<string> DefaultEnums) GetEnumProperties<T>(T obj)
    {
        var enums = obj!.GetType()
            .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Where(p =>
            {
                var t = p.PropertyType;
                return t.IsEnum || (Nullable.GetUnderlyingType(t)?.IsEnum ?? false);
            })
            .ToList();
        
        return (
            enums
                .Select(p => p.Name),
            enums
                .Where(p =>
                {
                    var t = p.PropertyType;
                    var enumType = t.IsEnum 
                        ? t
                        : (Nullable.GetUnderlyingType(t) ?? throw new ApplicationException($"How did did type {t.FullName} get in this list"));
                    Assert.IsTrue(enumType.IsEnum);

                    var value = p.GetValue(obj);
                    if (value is null)
                    {
                        return true;                                       // null
                    }
                    var defaultValue = Activator.CreateInstance(enumType); // enum's 0 value
                    return value.Equals(defaultValue);                     // default
                })
                .Select(p => p.Name)
            );
    }

    internal static void ValidateOptions<T>(T obj, JsonSerializerOptions? testOptions, JsonValidationOptions validationOptions, bool assertNoDefaultEnums)
    {
        Assert.IsNotNull(obj);

        var option = GetOptions(testOptions, validationOptions);
        var json = Serialize(obj, option);

        if (validationOptions.HasFlag(JsonValidationOptions.CheckExtraProperties))
        {
            json = AddExtraProperty(json);
            var withSkip = new JsonSerializerOptions(option)
            {
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
            };
            // This should work without errors...
            Deserialize<T>(json, withSkip);

            // Now this should throw
            var withDisallow = new JsonSerializerOptions(option)
            {
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
            };
            // Don't change this to Deserialize, that will mask the error a test assertion error
            Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<T>(json, withDisallow));
            return;
        }
        if (validationOptions.HasFlag(JsonValidationOptions.ReorderProperties))
        {
            json = JsonReverser.ReverseJsonPropertyOrder(json);
        }

        // We could look at children... but those should have their own tests
        (var enums, var defaultEnums) = GetEnumProperties(obj);

        foreach (var enumPropertyName in enums)
        {
            // These checks could lead false positive, after all child could also hve a property with this name that is not enum
            // if that happens we deal with it then.
            int quoted = Regex.Count(json, $"\"{enumPropertyName}\": \"", RegexOptions.IgnoreCase);

            if (validationOptions.HasFlag(JsonValidationOptions.StringEnumHandling))
            {
                int total  = Regex.Count(json, $"\"{enumPropertyName}\": ",  RegexOptions.IgnoreCase);
                int quotedNumbers = Regex.Count(json, $"\"{enumPropertyName}\": \"\\d", RegexOptions.IgnoreCase);
                TestUtilities.AssertTheseMatch(total, quoted, shouldMatch: true, $"Expected all occurrences of enum property {enumPropertyName} to be serialized as string when StringEnumHandling is enabled. Options: {validationOptions}");
                TestUtilities.AssertTheseMatch(0, quotedNumbers, shouldMatch: true, $"Expected no occurrences of enum property {enumPropertyName} to be serialized as quoted numbers when StringEnumHandling is enabled. Options: {validationOptions}");
            }
            else
            {
                TestUtilities.AssertTheseMatch(0, quoted, shouldMatch: true, $"Expected no occurrences of enum property {enumPropertyName} to be serialized as string when StringEnumHandling is not enabled. Options: {validationOptions}");
            }
        }

        // The previous foreach only works if we have object that has all the fields filled in so lets test or test
        // But we really only need to this once
        if (assertNoDefaultEnums && validationOptions.HasFlag(JsonValidationOptions.CheckForDefaultEnums))
        {
            var defaultEnumNames = defaultEnums.ToList();
            TestUtilities.AssertTheseMatch(0, defaultEnumNames.Count, shouldMatch: true, $"Expected no default enum properties to be present in JSON when CheckForDefaultEnums is enabled. Options: {validationOptions}");
        }

        if (option.PropertyNamingPolicy is not null 
            || validationOptions.HasFlag(JsonValidationOptions.PropertyNameCaseInsensitive)
            || validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicySnakeCaseLower)
            || validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicyKebabCaseUpper))
        {
            var propertyNames = JsonPropertyCollector.GetAllPropertyNames(json);
            foreach (var name in propertyNames)
            {
                var expected = option.PropertyNamingPolicy?.ConvertName(name) ?? name;
                TestUtilities.AssertTheseMatch(expected, name, shouldMatch: true, $"Property name {name} was not converted as expected by the naming policy. Options: {validationOptions}");

                if (validationOptions.HasFlag(JsonValidationOptions.PropertyNameCaseInsensitive))
                {
                    json = json.Replace($"\"{name}\"", $"\"{name.ToUpper()}\"");
                }
                if (validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicySnakeCaseLower))
                {
                    TestUtilities.AssertTheseMatch(name.ToLower(), name, shouldMatch: true, $"Expected name to be in LOWERCASE snake case. Options: {validationOptions}");
                }   
                if (validationOptions.HasFlag(JsonValidationOptions.JsonNamingPolicyKebabCaseUpper))
                {
                    TestUtilities.AssertTheseMatch(name.ToUpper(), name, shouldMatch: true, $"Expected name to be in UPPERCASE kebab case. Options: {validationOptions}");
                }
            }
        }

        if (validationOptions.HasFlag(JsonValidationOptions.DefaultIgnoreConditionWhenWritingNull))
        {
            var nullProperties = JsonPropertyCollector.FindNullProperties(json);
            Assert.IsEmpty(nullProperties, $"Expected no properties with null values to be serialized when DefaultIgnoreCondition is WhenWritingNull. Options: {validationOptions}. Null properties: {string.Join(", ", nullProperties)}. Json: {json}");
        }

        if (validationOptions.HasFlag(JsonValidationOptions.DefaultIgnoreConditionWhenWritingDefault))
        {
            var defaultProperties = JsonPropertyCollector.FindDefaultProperties(json);
            Assert.IsEmpty(defaultProperties, $"Expected no properties with default values to be serialized when DefaultIgnoreCondition is WhenWritingDefault. Options: {validationOptions}. Default properties: {string.Join(", ", defaultProperties)}. Json: {json}");
        }

        T deserialized = Deserialize<T>(json, option);
        TestUtilities.AssertTheseMatch(obj, deserialized, shouldMatch: true, $"Deserialized object did not match original. From {json}, Options: {validationOptions}");
    }

    private static string AddExtraProperty(string json)
    {
        int i = json.IndexOf('{');
        Assert.IsGreaterThanOrEqualTo(0, i, $"Expected JSON");

        return string.Concat(
            json.AsSpan(0, i + 1),
            @"""thisisnotapropertyonanyobject"": 123,",
            json.AsSpan(i + 1));
    }

    [Flags]
    public enum JsonValidationOptions
    {
        None = 0,
        PropertyNameCaseInsensitive = 1 << 1,
        StringEnumHandling = 1 << 2,
        DefaultIgnoreConditionWhenWritingNull = 1 << 3,
        DefaultIgnoreConditionWhenWritingDefault = 1 << 4,
        JsonNamingPolicyCamelCase = 1 << 5,
        JsonNamingPolicySnakeCaseLower = 1 << 6,
        JsonNamingPolicyKebabCaseUpper = 1 << 7,
        CheckForDefaultEnums = 1 << 8,
        CheckExtraProperties = 1 << 9,
        ReorderProperties = 1 << 10,
    }

    private static readonly JsonValidationOptions[] _validationCases =
    [
        JsonValidationOptions.PropertyNameCaseInsensitive 
        | JsonValidationOptions.StringEnumHandling
        | JsonValidationOptions.DefaultIgnoreConditionWhenWritingDefault
        | JsonValidationOptions.JsonNamingPolicyCamelCase
        | JsonValidationOptions.CheckForDefaultEnums,
        
        JsonValidationOptions.DefaultIgnoreConditionWhenWritingNull
        | JsonValidationOptions.JsonNamingPolicySnakeCaseLower,

        // string enums without any ignored values
        JsonValidationOptions.StringEnumHandling
        | JsonValidationOptions.JsonNamingPolicyKebabCaseUpper,

        JsonValidationOptions.CheckExtraProperties,

        JsonValidationOptions.ReorderProperties,
    ];
}