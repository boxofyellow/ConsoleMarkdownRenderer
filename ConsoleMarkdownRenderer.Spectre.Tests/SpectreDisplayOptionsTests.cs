using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Spectre.Console;
using STJSJsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class SpectreDisplayOptionsTests
{
    private static readonly JsonSerializerOptions _options
        = SpectreDisplayOptions.BuildEffectiveOptions(null);

    private static readonly JsonSerializerOptions _optionsWithEnumConverterOnToEmpty 
        = SpectreDisplayOptions.BuildEffectiveOptions(TestJsonHelper.EnumJsonOptions, createObject: SpectreDisplayOptions.Empty);

    [TestMethod]
    public void New_Equals_New()
        => TestUtilities.AssertTheseMatch(
            new SpectreDisplayOptions(),
            new SpectreDisplayOptions(),
            shouldMatch: true);

    [TestMethod]
    public void Empty_Equals_Empty()
        => TestUtilities.AssertTheseMatch(
            SpectreDisplayOptions.Empty(),
            SpectreDisplayOptions.Empty(),
            shouldMatch: true);

    [TestMethod]
    public void Equals_Different_Types()
        => Assert.IsFalse(new SpectreDisplayOptions().Equals(new object()), "SpectreDisplayOptions should not equal a different type");

    [TestMethod]
    public void Json_Options_Are_Not_Replaced_When_Not_Needed()
    {
        var options = SpectreDisplayOptions.BuildEffectiveOptions(caller: null);
        var options2 = SpectreDisplayOptions.BuildEffectiveOptions(options);
        Assert.AreSame(options, options2, "Options should not be replaced when not needed");
    }

    [TestMethod]
    public void Json_Options_Are_Replaced_When_Needed()
    {
        var options = SpectreDisplayOptions.BuildEffectiveOptions(caller: null);
        Assert.IsNotEmpty(options.Converters, "Options should have converters");
        var options2 = new JsonSerializerOptions(options);
        options2.Converters.Clear();
        options2 = SpectreDisplayOptions.BuildEffectiveOptions(options2);
        Assert.AreNotSame(options, options2, "Options should be replaced");
        Assert.HasCount(options.Converters.Count, options2.Converters, "Options should have the same number of converters");
    }

    [TestMethod]
    public void Json_Options_Throw_On_Incompatible_Converters()
    {
        var options = SpectreDisplayOptions.BuildEffectiveOptions(caller: null);
        Assert.Throws<InvalidOperationException>(() => SpectreDisplayOptions.BuildEffectiveOptions(options, createObject: SpectreDisplayOptions.Empty), "Options should throw on incompatible converters");
    }

    private class BadConverter : JsonConverter<SpectreDisplayOptions>
    {
        public override SpectreDisplayOptions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, SpectreDisplayOptions value, JsonSerializerOptions options) { }
    }

    [TestMethod]
    public void Json_Options_Throw_On_Bad_Converter()
    {
        var options = new JsonSerializerOptions()
        {
            Converters =
            {
                new BadConverter()
            }
        };
        Assert.Throws<InvalidOperationException>(() => SpectreDisplayOptions.BuildEffectiveOptions(options, createObject: SpectreDisplayOptions.Empty), "Options should throw on bad converter");
    }

    private class OkHeaderConverter : JsonConverter<ISpectreHeaderStyle>
    {
        public override ISpectreHeaderStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, ISpectreHeaderStyle value, JsonSerializerOptions options) { }
    }

    private class OkColorConverter : JsonConverter<Color>
    {
        public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options) { }
    }

    private class OkBoxConverter : JsonConverter<BoxBorder>
    {
        public override BoxBorder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, BoxBorder value, JsonSerializerOptions options) { }
    }

    private class OkTableConverter : JsonConverter<TableBorder>
    {
        public override TableBorder Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, TableBorder value, JsonSerializerOptions options) { }
    }

    private class OkStyleConverter : JsonConverter<Style>
    {
        public override Style Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
        public override void Write(Utf8JsonWriter writer, Style value, JsonSerializerOptions options) { }
    }

    [TestMethod]
    public void Json_Options_Do_Not_Throw_On_Ok_Converters()
    {
        var list = new List<STJSJsonConverter>
        {
            new OkHeaderConverter(),
            new OkColorConverter(),
            new OkBoxConverter(),
            new OkTableConverter(),
            new OkStyleConverter()
        };
        Assert.HasCount(_options.Converters.Count - 1, list, "Options should have all but SpectreDisplayOptionsConverter");

        foreach (var converter in list)
        {
            var oneConverterOptions = new JsonSerializerOptions()
            {
                Converters =
                {
                    converter
                }
            };
            var newOptions = SpectreDisplayOptions.BuildEffectiveOptions(oneConverterOptions, createObject: SpectreDisplayOptions.Empty);
            Assert.HasCount(_options.Converters.Count, newOptions.Converters, "Options should have the same number of converters");

            bool found = false;
            foreach (var c in newOptions.Converters)
            {
                if (c.GetType() == converter.GetType())
                {
                    Assert.AreSame(converter, c, $"Options should contain the same instance of the converter of type {converter.GetType().Name}");
                    found = true;
                    break;
                }
            }
            Assert.IsTrue(found, $"Options should contain the converter of type {converter.GetType().Name}");
        }
    }


    [TestMethod]
    public void All_Properties_Are_Handled()
    {
        Assert.HasCount(Mappings.SpectreDisplayOptionsProperties.Count, SpectreDisplayOptions.Deserializers);
        Assert.HasCount(Mappings.SpectreDisplayOptionsProperties.Count, SpectreDisplayOptions.Serializers);
        
        foreach (var propertyName in Mappings.SpectreDisplayOptionsProperties.Keys)
        {
            Assert.IsTrue(SpectreDisplayOptions.Deserializers.ContainsKey(propertyName), $"Missing deserializer for {propertyName}");
        }
    }

    [TestMethod]
    public void Equals_Clone_Ooo_My()
    {
        var original = new SpectreDisplayOptions();

        foreach (var property in Mappings.SpectreDisplayOptionsProperties)
        {
            Logger.LogMessage($"Testing property {property.Key}\n");
            var clone = original.Clone();
            TestUtilities.AssertTheseMatch(original, clone, shouldMatch: true, message: $"Failed clone equals {property.Key}");
            
            var type = property.Value.Type;
            if (type == typeof(Style))
            {
                var val = (Style)property.Value.Getter(original);
                if (val.Decoration == Decoration.None)
                {
                    val = new Style(val.Foreground, val.Background, Decoration.Bold);
                }
                else
                {
                    val = new Style(val.Foreground, val.Background, Decoration.None);
                }
                property.Value.Setter(original, val);
            }
            else if (type == typeof(int))
            {
                var val = (int)property.Value.Getter(original);
                property.Value.Setter(original, val + 1);
            }
            else if (type == typeof(ISpectreHeaderStyle))
            {
                ISpectreHeaderStyle val;
                if (property.Value.Getter(original) is SpectreTextStyle)
                {
                    val = new SpectreRuleHeaderStyle();
                }
                else
                {
                    val = new SpectreTextStyle();
                }
                property.Value.Setter(original, val);
            }
            else if (type == typeof(bool))
            {
                var val = (bool)property.Value.Getter(original);
                property.Value.Setter(original, !val);
            }
            else if (type == typeof(List<ISpectreHeaderStyle>))
            {
                var val = (List<ISpectreHeaderStyle>)property.Value.Getter(original);
                val.Add(new SpectreTextStyle());
            }
            else if (type == typeof(string))
            {
                var val = (string)property.Value.Getter(original);
                property.Value.Setter(original, val + "a");
            }
            else if (type == typeof(TableBorder))
            {
                var val = (TableBorder)property.Value.Getter(original);
                property.Value.Setter(original, val == TableBorder.Ascii ? TableBorder.Double : TableBorder.Ascii);
            }
            else if (type == typeof(BoxBorder))
            {
                var val = (BoxBorder)property.Value.Getter(original);
                property.Value.Setter(original, val == BoxBorder.Ascii ? BoxBorder.Double : BoxBorder.Ascii);
            }
            else
            {
                throw new InvalidOperationException($"Unexpected type {type} for property {property.Key}");
            }

            TestUtilities.AssertTheseMatch(original, clone, shouldMatch: false, message: $"Failed to be different after changing {property.Key}");

            var changedProperty =  TestUtilities.FindFirstDifferenceSpectreDisplayOptions(original, clone, details: false);
            TestUtilities.AssertTheseMatch(property.Key, changedProperty, shouldMatch: true);

            var json = original.Serialize();
            TestJsonHelper.RoundTrip(
                json,
                original,
                _optionsWithEnumConverterOnToEmpty,
                assertNoDefaultEnums: false);
        }
    }


    [TestMethod]
    public void Can_Deserialize_Empty()
        => TestJsonHelper.RoundTrip(
            "{}",
            new SpectreDisplayOptions(),
            _options,
            assertNoDefaultEnums: false);

    [TestMethod]
    public void Can_Deserialize_Empty_For_Really_Reals()
        => TestJsonHelper.RoundTrip(
            "{}",
            SpectreDisplayOptions.Empty(),
            SpectreDisplayOptions.BuildEffectiveOptions(caller: null, createObject: SpectreDisplayOptions.Empty),
            assertNoDefaultEnums: false);

    [TestMethod]
    public async Task RendererTests_DisplayOptionsJson_RoundTrip()
    {
        var fontPath = Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");
        var json = $$"""
            {
                "{{nameof(SpectreDisplayOptions.ShowFencedCodeBlockInfo)}}": true,
                "{{nameof(SpectreDisplayOptions.Bold)}}": { "{{nameof(Style.Decoration)}}": "{{Decoration.Bold}}" },
                "{{nameof(SpectreDisplayOptions.Headers)}}": [
                    {
                        "{{SpectreHeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(SpectreFigletTextStyle)}}",
                        "{{nameof(SpectreFigletTextStyle.Justification)}}": "{{Justify.Left}}",
                        "{{nameof(SpectreFigletTextStyle.Foreground)}}": { "{{ColorJsonConverter.NamedDiscriminator}}": "{{nameof(Color.Green)}}" },
                        "{{nameof(SpectreFigletTextStyle.FontPath)}}": {{JsonSerializer.Serialize(fontPath)}}
                    },
                    {
                        "{{SpectreHeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(SpectreTextStyle)}}",
                        "{{nameof(SpectreTextStyle.Decoration)}}": "{{Decoration.Bold | Decoration.Underline}}"
                    }
                ],
                "{{nameof(SpectreDisplayOptions.Header)}}": {
                    "{{SpectreHeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(SpectreTextStyle)}}",
                    "{{nameof(SpectreTextStyle.Decoration)}}": "{{Decoration.Italic}}"
                }
            }
            """;

        var options = await SpectreDisplayOptions.DeserializeAsync(json, TestJsonHelper.EnumJsonOptions).ConfigureAwait(false);
        var expected = new SpectreDisplayOptions
        {
            ShowFencedCodeBlockInfo = true,
            Bold = new Style(decoration: Decoration.Bold),
            Headers = new List<ISpectreHeaderStyle>
            {
                await SpectreFigletTextStyle.CreateAsync(fontPath, Justify.Left, Color.Green),
                new SpectreTextStyle(decoration: Decoration.Bold | Decoration.Underline)
            },
            Header = new SpectreTextStyle(decoration: Decoration.Italic)
        };

        TestUtilities.AssertTheseMatch(expected, options, shouldMatch: true);
    }


    [TestMethod]
    public async Task Ensure_Header()
    {
        var fontPath = Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");
        var json = $$"""
            {
                "{{nameof(SpectreDisplayOptions.Header)}}": {
                        "{{SpectreHeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(SpectreFigletTextStyle)}}",
                        "{{nameof(SpectreFigletTextStyle.Justification)}}": "{{Justify.Left}}",
                        "{{nameof(SpectreFigletTextStyle.Foreground)}}": { "{{ColorJsonConverter.NamedDiscriminator}}": "{{nameof(Color.Green)}}" },
                        "{{nameof(SpectreFigletTextStyle.FontPath)}}": {{JsonSerializer.Serialize(fontPath)}}
                }
            }
            """;

        var options = await SpectreDisplayOptions.DeserializeAsync(json, TestJsonHelper.EnumJsonOptions).ConfigureAwait(false);
        var expected = new SpectreDisplayOptions
        {
            Header = await SpectreFigletTextStyle.CreateAsync(fontPath, Justify.Left, Color.Green)
        };

        TestUtilities.AssertTheseMatch(expected, options, shouldMatch: true);
    }


    [TestMethod]
    public async Task Serialize_Does_Not_Mutate_Caller_Options()
    {
        var caller = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            { 
                new JsonStringEnumConverter(),
                new ColorJsonConverter(),
            },
        };
        var beforeConverters = caller.Converters.Count;
        var beforePolicy = caller.PropertyNamingPolicy;

        var json = new SpectreDisplayOptions().Serialize(caller);

        Assert.HasCount(beforeConverters, caller.Converters, "Caller's Converters list must not be mutated.");
        Assert.AreSame(beforePolicy, caller.PropertyNamingPolicy, "Caller's PropertyNamingPolicy must not be mutated.");

        await SpectreDisplayOptions.DeserializeAsync(json, caller);

        Assert.HasCount(beforeConverters, caller.Converters, "Caller's Converters list must not be mutated.");
        Assert.AreSame(beforePolicy, caller.PropertyNamingPolicy, "Caller's PropertyNamingPolicy must not be mutated.");
    }

    [TestMethod]
    public async Task Can_Merge_In_Values()
    {
        var options = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        var original = SpectreDisplayOptions.Empty();
        // What we really need is not-Empty, and crazy is fairly close, except the bools
        var crazy = TestUtilities.Crazy;
        foreach (var property in Mappings.SpectreDisplayOptionsProperties.Values.Where(p => p.Type == typeof(bool)))
        {
            property.Setter(crazy, !(bool)property.Getter(original));
        }

        var json = original.Serialize(options);
        TestUtilities.AssertTheseMatch("{}", json, shouldMatch: true, message: "Empty options should serialize to an empty JSON object when using WhenWritingDefault.");

        foreach (var property in Mappings.SpectreDisplayOptionsProperties)
        {
            Logger.LogMessage($"Testing property {property.Key}\n");
            var mergeContent = SpectreDisplayOptions.Empty();

            TestUtilities.AssertTheseMatch(property.Value.Getter(original), property.Value.Getter(mergeContent), shouldMatch: true, message: $"Empty values should match {property.Key}");
            property.Value.Setter(mergeContent, property.Value.Getter(crazy));
            TestUtilities.AssertTheseMatch(property.Value.Getter(original), property.Value.Getter(mergeContent), shouldMatch: false, message: $"Failed change {property.Key}");

            var mergeJson = mergeContent.Serialize(options);
            TestUtilities.AssertTheseMatch(true, mergeJson.Contains(property.Key), shouldMatch: true, message: "The new property should be somewhere in the json");

            var clone = original.Clone();
            TestUtilities.AssertTheseMatch(original, clone, shouldMatch: true, message: $"Failed clone equals before merge {property.Key}");
            var mergedObject = await SpectreDisplayOptions.DeserializeAsync(mergeJson, createObject: () => clone).ConfigureAwait(false);
            // The end result here is now mergedObject should reference the same object as the clone, and that object should be updated.
            TestUtilities.AssertTheseMatch(clone, mergedObject, shouldMatch: true, message: $"Merged object should be the same reference as the clone for {property.Key}"); 
            Assert.AreSame(clone, mergedObject, $"Merged object should be the same reference as the clone for {property.Key}");
            TestUtilities.AssertTheseMatch(property.Value.Getter(mergeContent), property.Value.Getter(mergedObject), shouldMatch: true, message: $"merged object should match for {property.Key}");

            TestUtilities.AssertTheseMatch(original, mergedObject, shouldMatch: false, message: $"Failed change with merge {property.Key}");
            var changedProperty = TestUtilities.FindFirstDifferenceSpectreDisplayOptions(original, mergedObject, details: false);
            TestUtilities.AssertTheseMatch(property.Key, changedProperty, shouldMatch: true, message: $"Failed change with merge {property.Key}");

            var mergedJson = mergedObject.Serialize(options);
            TestUtilities.AssertTheseMatch(true, mergedJson.Contains(property.Key), shouldMatch: true, message: "The new property should be somewhere in the merged json");

            TestJsonHelper.RoundTrip(
                mergedJson,
                mergedObject,
                SpectreDisplayOptions.BuildEffectiveOptions(options, createObject: () => SpectreDisplayOptions.Empty()),
                assertNoDefaultEnums: false);

            original = mergedObject; // Set original to the merged object for the next iteration, so we keep merging in changes
        }

        TestUtilities.AssertTheseMatch(crazy, original, shouldMatch: true, message: "After merging in each property one by one we should be back to the crazy options");

        TestJsonHelper.RoundTrip(
            crazy.Serialize(options),
            original,
            _options,
            assertNoDefaultEnums: true);
    }

    [TestMethod]
    public async Task Can_Deserialize_From_Stream()
    {
        var json = new SpectreDisplayOptions().Serialize();
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
        var options = await SpectreDisplayOptions.DeserializeAsync(stream).ConfigureAwait(false);
        TestUtilities.AssertTheseMatch(new SpectreDisplayOptions(), options, shouldMatch: true);
    } 
}