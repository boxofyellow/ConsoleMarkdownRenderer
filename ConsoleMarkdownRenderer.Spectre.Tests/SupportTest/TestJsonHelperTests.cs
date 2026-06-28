using System.Text.Json;
using System.Text.Json.Serialization;
using JsonC = System.Text.Json.Serialization.JsonConverter;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class TestJsonHelperTests
{
    [TestMethod]
    [DataRow(TestJsonHelper.JsonValidationOptions.PropertyNameCaseInsensitive)]
    [DataRow(TestJsonHelper.JsonValidationOptions.JsonNamingPolicySnakeCaseLower)]
    [DataRow(TestJsonHelper.JsonValidationOptions.JsonNamingPolicyKebabCaseUpper)]
    [DataRow(TestJsonHelper.JsonValidationOptions.CheckExtraProperties)] // This not really case checking, but it will demo this the same
    public void Property_Case(TestJsonHelper.JsonValidationOptions option) 
        => CheckValidationOption(
            option,
            new C1 { IntegerOne = 42 },
            [new BadCase()]);

    [TestMethod]
    public void String_EnumHandling()
        => CheckValidationOption(
            TestJsonHelper.JsonValidationOptions.StringEnumHandling,
            new C1 { EnumOne = E1.ValueTwo },
            [new BadEnum()]);

    [TestMethod]
    public void Default_Ignore_Condition_When_Writing_Null()
        => CheckValidationOption(
            TestJsonHelper.JsonValidationOptions.DefaultIgnoreConditionWhenWritingNull,
            new C1(),
            [new BadNull()]);

    [TestMethod]
    public void Default_Ignore_Condition_When_Writing_Default()
        => CheckValidationOption(
            TestJsonHelper.JsonValidationOptions.DefaultIgnoreConditionWhenWritingDefault | TestJsonHelper.JsonValidationOptions.StringEnumHandling,
            new C1(),
            [new BadDefault()]);

    [TestMethod]
    public void Reorder_Properties()
        => CheckValidationOption(
            TestJsonHelper.JsonValidationOptions.ReorderProperties,
            new C1 { IntegerOne = 42, EnumOne = E1.ValueTwo },
            [new BadOrder()]);

    private static void CheckValidationOption<T>(TestJsonHelper.JsonValidationOptions option, T obj, IEnumerable<JsonC> converters)
    {
        var testOptions = new JsonSerializerOptions();
        foreach (var converter in converters)
        {
            testOptions.Converters.Add(converter);
        }
        // It should pass without the option
        TestJsonHelper.ValidateOptions(obj, testOptions, TestJsonHelper.JsonValidationOptions.None, assertNoDefaultEnums: false);
        // It should throw with the option
        Assert.Throws<AssertFailedException>(() => TestJsonHelper.ValidateOptions(obj, testOptions, option, assertNoDefaultEnums: false));
    }

    private enum E1
    {
        ValueOne = 1,
        ValueTwo = 2
    }

    private class C1
    {
        public int IntegerOne { get; set; }
        public E1 EnumOne { get; set; }
        public C1? COne { get; set; }

        public override bool Equals(object? obj) 
            => obj is C1 c1 && IntegerOne == c1.IntegerOne
                            && EnumOne == c1.EnumOne
                            && Equals(COne, c1.COne);

        public override int GetHashCode() => HashCode.Combine(
            IntegerOne,
            EnumOne,
            COne);
    }

    private class BadCase : JsonConverter<C1>
    {
        public override C1? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var c1 = new C1();
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name == nameof(C1.IntegerOne))
                {
                    c1.IntegerOne = prop.Value.GetInt32();
                }
            }
            return c1;
        }

        public override void Write(Utf8JsonWriter writer, C1 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(C1.IntegerOne), value.IntegerOne);
            writer.WriteEndObject();
        }
    }

    private class BadEnum : JsonConverter<C1>
    {
        public override C1? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var c1 = new C1();
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name == nameof(C1.EnumOne))
                {
                    c1.EnumOne = (E1)prop.Value.GetInt32();
                }
            }
            return c1;
        }

        public override void Write(Utf8JsonWriter writer, C1 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(C1.EnumOne));
            JsonSerializer.Serialize(writer, value.EnumOne, options);
            writer.WriteEndObject();
        }
    }

    private class BadNull : JsonConverter<C1>
    {
        public override C1? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var c1 = new C1();
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name == nameof(C1.COne))
                {
                    c1.COne = prop.Value.ValueKind == JsonValueKind.Null
                        ? null
                        : prop.Value.Deserialize<C1>(options);
                }
            }
            return c1;
        }

        public override void Write(Utf8JsonWriter writer, C1 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(nameof(C1.COne));
            JsonSerializer.Serialize(writer, value.COne, options);
            writer.WriteEndObject();
        }
    }

    private class BadDefault : JsonConverter<C1>
    {
        public override C1? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var c1 = new C1();
            foreach (var prop in root.EnumerateObject())
            {
                if (prop.Name == nameof(C1.IntegerOne))
                {
                    c1.IntegerOne = prop.Value.GetInt32();
                }
            }
            return c1;
        }

        public override void Write(Utf8JsonWriter writer, C1 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(C1.IntegerOne), value.IntegerOne);
            writer.WriteEndObject();
        }
    }

    private class BadOrder : JsonConverter<C1>
    {
        public override C1? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;
            var c1 = new C1();
            var properties = root.EnumerateObject().ToArray();
            if (properties.Length > 0)
            {
                c1.IntegerOne = properties[0].Value.GetInt32();
            }
            if (properties.Length > 1)
            {
                c1.EnumOne = (E1)properties[1].Value.GetInt32();
            }
            return c1;
        }

        public override void Write(Utf8JsonWriter writer, C1 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber(nameof(C1.IntegerOne), value.IntegerOne);
            writer.WritePropertyName(nameof(C1.EnumOne));
            JsonSerializer.Serialize(writer, value.EnumOne, options);
            writer.WriteEndObject();
        }
    }

    [TestMethod]
    public void GetEnumProperties_FindsAllEnums()
    {
        (var enums, var defaultEnums) = TestJsonHelper.GetEnumProperties(new DummyClassWithEnums());
        AssertMatches([ 
                nameof(DummyClassWithEnums.EnumWithValue),
                nameof(DummyClassWithEnums.EnumWithDefaultValue),
                nameof(DummyClassWithEnums.EnumWithInitialValue),
                nameof(DummyClassWithEnums.NullableValue),
                nameof(DummyClassWithEnums.NullableWithDefaultValue),
                nameof(DummyClassWithEnums.NullableWithInitialValue),
                nameof(DummyClassWithEnums.NullableWithNullValue),
                nameof(DummyClassWithEnums.NullableWithExplicitType),
            ], enums);
        AssertMatches([ 
                nameof(DummyClassWithEnums.EnumWithDefaultValue),
                nameof(DummyClassWithEnums.EnumWithInitialValue),
                nameof(DummyClassWithEnums.NullableWithDefaultValue),
                nameof(DummyClassWithEnums.NullableWithInitialValue),
                nameof(DummyClassWithEnums.NullableWithNullValue),
                nameof(DummyClassWithEnums.NullableWithExplicitType),
            ], defaultEnums);
    }

    private void AssertMatches(IEnumerable<string> expected, IEnumerable<string> actual)
    {
        var expectedSet = new HashSet<string>(expected);
        var actualSet = new HashSet<string>(actual);
        Assert.IsTrue(expectedSet.SetEquals(actualSet), $"Expected and actual sets do not match. Expected: {string.Join(", ", expectedSet)}; Actual: {string.Join(", ", actualSet)}");
    }

    private class DummyClassWithEnums
    {
        bool NotEnum { get; set; } = true;
        public ConsoleColor EnumWithValue { get; set; } = ConsoleColor.Red;
        public ConsoleColor EnumWithDefaultValue { get; set; } = default;
        public ConsoleColor EnumWithInitialValue { get; set; } = (ConsoleColor)0;
        public ConsoleColor? NullableValue { get; set; } = ConsoleColor.Red;
        public ConsoleColor? NullableWithDefaultValue { get; set; } = default;
        public ConsoleColor? NullableWithInitialValue { get; set; } = (ConsoleColor?)0;
        public ConsoleColor? NullableWithNullValue { get; set; } = null;
        public Nullable<ConsoleColor> NullableWithExplicitType { get; set; } = null;
    }
}