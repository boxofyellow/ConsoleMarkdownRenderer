using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using STJSJsonConverter = System.Text.Json.Serialization.JsonConverter;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    [TestClass]
    public class DisplayOptionsTests : TestBase
    {
        private static readonly JsonSerializerOptions _options
            = DisplayOptions.BuildEffectiveOptions(null);

        private static readonly JsonSerializerOptions _optionsWithEnumConverterOnToEmpty 
            = DisplayOptions.BuildEffectiveOptions(TestJsonHelper.EnumJsonOptions, createObject: DisplayOptions.Empty);

        [TestMethod]
        public void New_Equals_New()
            => TestUtilities.AssertTheseMatch(
                new DisplayOptions(),
                new DisplayOptions(),
                shouldMatch: true);

        [TestMethod]
        public void Empty_Equals_Empty()
            => TestUtilities.AssertTheseMatch(
                DisplayOptions.Empty(),
                DisplayOptions.Empty(),
                shouldMatch: true);

        [TestMethod]
        public void Equals_Different_Types()
            => Assert.IsFalse(new DisplayOptions().Equals(new object()), "DisplayOptions should not equal a different type");

        [TestMethod]
        public void ToSpectreOptions_On_New_Matches()
            => TestUtilities.AssertTheseMatch(
                new DisplayOptions().ToSpectreOptions(),
                new SpectreDisplayOptions(),
                shouldMatch: true);

        [TestMethod]
        public void ToSpectreOptions_On_Empty_Matches()
            => TestUtilities.AssertTheseMatch(
                DisplayOptions.Empty().ToSpectreOptions(),
                SpectreDisplayOptions.Empty(),
                shouldMatch: true);


        [TestMethod]
        public void FromSpectreOptions_On_New_Matches()
            => TestUtilities.AssertTheseMatch(
                DisplayOptions.FromSpectreOptions(new SpectreDisplayOptions(), preferNullColors: true),
                new DisplayOptions(),
                shouldMatch: true);

        [TestMethod]
        public void FromSpectreOptions_On_Empty_Matches()
            => TestUtilities.AssertTheseMatch(
                DisplayOptions.FromSpectreOptions(SpectreDisplayOptions.Empty(), preferNullColors: true),
                DisplayOptions.Empty(),
                shouldMatch: true);

        [TestMethod]
        public void Can_Round_Trip_Crazy()
            => TestUtilities.AssertTheseMatch(
                DisplayOptions.FromSpectreOptions(TestUtilities.Crazy).ToSpectreOptions(),
                TestUtilities.Crazy,
                shouldMatch: true);

        [TestMethod]
        public void Json_Options_Are_Not_Replaced_When_Not_Needed()
        {
            var options = DisplayOptions.BuildEffectiveOptions(caller: null);
            var options2 = DisplayOptions.BuildEffectiveOptions(options);
            Assert.AreSame(options, options2, "Options should not be replaced when not needed");
        }

        [TestMethod]
        public void Json_Options_Are_Replaced_When_Needed()
        {
            var options = DisplayOptions.BuildEffectiveOptions(caller: null);
            Assert.IsNotEmpty(options.Converters, "Options should have converters");
            var options2 = new JsonSerializerOptions(options);
            options2.Converters.Clear();
            options2 = DisplayOptions.BuildEffectiveOptions(options2);
            Assert.AreNotSame(options, options2, "Options should be replaced");
            Assert.HasCount(options.Converters.Count, options2.Converters, "Options should have the same number of converters");
        }

        [TestMethod]
        public void Json_Options_Throw_On_Incompatible_Converters()
        {
            var options = DisplayOptions.BuildEffectiveOptions(caller: null);
            Assert.Throws<InvalidOperationException>(() => DisplayOptions.BuildEffectiveOptions(options, createObject: DisplayOptions.Empty), "Options should throw on incompatible converters");
        }

        private class BadConverter : JsonConverter<DisplayOptions>
        {
            public override DisplayOptions Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
            public override void Write(Utf8JsonWriter writer, DisplayOptions value, JsonSerializerOptions options) { }
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
            Assert.Throws<InvalidOperationException>(() => DisplayOptions.BuildEffectiveOptions(options, createObject: DisplayOptions.Empty), "Options should throw on bad converter");
        }

        private class OkHeaderConverter : JsonConverter<IHeaderStyle>
        {
            public override IHeaderStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
            public override void Write(Utf8JsonWriter writer, IHeaderStyle value, JsonSerializerOptions options) { }
        }

        private class OkColorConverter : JsonConverter<TextColor>
        {
            public override TextColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
            public override void Write(Utf8JsonWriter writer, TextColor value, JsonSerializerOptions options) { }
        }

        private class OkStyleConverter : JsonConverter<TextStyle>
        {
            public override TextStyle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) => throw new NotImplementedException();
            public override void Write(Utf8JsonWriter writer, TextStyle value, JsonSerializerOptions options) { }
        }

        [TestMethod]
        public void Json_Options_Do_Not_Throw_On_Ok_Converters()
        {
            var list = new List<STJSJsonConverter>
            {
                new OkHeaderConverter(),
                new OkColorConverter(),
                new OkStyleConverter()
            };
            Assert.HasCount(_options.Converters.Count - 1, list, "Options should have all but DisplayOptionsConverter");

            foreach (var converter in list)
            {
                var oneConverterOptions = new JsonSerializerOptions()
                {
                    Converters =
                    {
                        converter
                    }
                };
                var newOptions = DisplayOptions.BuildEffectiveOptions(oneConverterOptions, createObject: DisplayOptions.Empty);
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
            Assert.HasCount(DisplayMappings.DisplayOptionsProperties.Count, DisplayOptions.Deserializers);
            Assert.HasCount(DisplayMappings.DisplayOptionsProperties.Count, DisplayOptions.Serializers);
            
            foreach (var propertyName in DisplayMappings.DisplayOptionsProperties.Keys)
            {
                Assert.IsTrue(DisplayOptions.Deserializers.ContainsKey(propertyName), $"Missing deserializer for {propertyName}");
            }
        }

        [TestMethod]
        public void Equals_Clone_Ooo_My()
        {
            var original = new DisplayOptions();

            foreach (var property in DisplayMappings.DisplayOptionsProperties)
            {
                Logger.LogMessage($"Testing property {property.Key}\n");
                var clone = original.Clone();
                TestUtilities.AssertTheseMatch(original, clone, shouldMatch: true, message: $"Failed clone equals {property.Key}");
                
                var type = property.Value.Type;
                if (type == typeof(TextStyle))
                {
                    var val = (TextStyle)property.Value.Getter(original);
                    if (val.Decoration == TextDecoration.None)
                    {
                        val = new TextStyle(TextDecoration.Bold, val.Foreground, val.Background);
                    }
                    else
                    {
                        val = new TextStyle(TextDecoration.None, val.Foreground, val.Background);
                    }
                    property.Value.Setter(original, val);
                }
                else if (type == typeof(int))
                {
                    var val = (int)property.Value.Getter(original);
                    property.Value.Setter(original, val + 1);
                }
                else if (type == typeof(IHeaderStyle))
                {
                    IHeaderStyle val;
                    if (property.Value.Getter(original) is TextStyle)
                    {
                        val = new RuleHeaderStyle();
                    }
                    else
                    {
                        val = new TextStyle();
                    }
                    property.Value.Setter(original, val);
                }
                else if (type == typeof(bool))
                {
                    var val = (bool)property.Value.Getter(original);
                    property.Value.Setter(original, !val);
                }
                else if (type == typeof(List<IHeaderStyle>))
                {
                    var val = (List<IHeaderStyle>)property.Value.Getter(original);
                    val.Add(new TextStyle());
                }
                else if (type == typeof(string))
                {
                    var val = (string)property.Value.Getter(original);
                    property.Value.Setter(original, val + "a");
                }
                else if (type == typeof(TextTableBorder))
                {
                    var val = (TextTableBorder)property.Value.Getter(original);
                    property.Value.Setter(original, val == TextTableBorder.Ascii ? TextTableBorder.Double : TextTableBorder.Ascii);
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected type {type} for property {property.Key}");
                }

                TestUtilities.AssertTheseMatch(original, clone, shouldMatch: false, message: $"Failed to be different after changing {property.Key}");

                var changedProperty = FindFirstDifferenceDisplayOptions(original, clone, details: false);
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
                new DisplayOptions(),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Deserialize_Empty_For_Really_Reals()
            => TestJsonHelper.RoundTrip(
                "{}",
                DisplayOptions.Empty(),
                DisplayOptions.BuildEffectiveOptions(caller: null, createObject: DisplayOptions.Empty),
                assertNoDefaultEnums: false);

        [TestMethod]
        public async Task RendererTests_DisplayOptionsJson_RoundTrip()
        {
            var fontPath = Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");
            var json = $$"""
                {
                    "{{nameof(DisplayOptions.ShowFencedCodeBlockInfo)}}": true,
                    "{{nameof(DisplayOptions.Bold)}}": { "{{nameof(TextStyle.Decoration)}}": "{{TextDecoration.Bold}}" },
                    "{{nameof(DisplayOptions.Headers)}}": [
                        {
                            "{{HeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(FigletTextStyle)}}",
                            "{{nameof(FigletTextStyle.Justification)}}": "{{TextJustification.Left}}",
                            "{{nameof(FigletTextStyle.Foreground)}}": { "{{TextColorJsonConverter.NamedDiscriminator}}": "{{nameof(TextColor.Green)}}" },
                            "{{nameof(FigletTextStyle.FontPath)}}": {{JsonSerializer.Serialize(fontPath)}}
                        },
                        {
                            "{{HeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(TextStyle)}}",
                            "{{nameof(TextStyle.Decoration)}}": "{{TextDecoration.Bold | TextDecoration.Underline}}"
                        }
                    ],
                    "{{nameof(DisplayOptions.Header)}}": {
                        "{{HeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(TextStyle)}}",
                        "{{nameof(TextStyle.Decoration)}}": "{{TextDecoration.Italic}}"
                    }
                }
                """;

            var options = await DisplayOptions.DeserializeAsync(json, TestJsonHelper.EnumJsonOptions).ConfigureAwait(false);
            var expected = new DisplayOptions
            {
                ShowFencedCodeBlockInfo = true,
                Bold = new TextStyle(decoration: TextDecoration.Bold),
                Headers = new List<IHeaderStyle>
                {
                    await FigletTextStyle.CreateAsync(fontPath, TextJustification.Left, TextColor.Green),
                    new TextStyle(decoration: TextDecoration.Bold | TextDecoration.Underline)
                },
                Header = new TextStyle(decoration: TextDecoration.Italic)
            };

            TestUtilities.AssertTheseMatch(expected, options, shouldMatch: true);
        }

        [TestMethod]
        public async Task Ensure_Header()
        {
            var fontPath = Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");
            var json = $$"""
                {
                    "{{nameof(DisplayOptions.Header)}}": {
                            "{{HeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(FigletTextStyle)}}",
                            "{{nameof(FigletTextStyle.Justification)}}": "{{TextJustification.Left}}",
                            "{{nameof(FigletTextStyle.Foreground)}}": { "{{TextColorJsonConverter.NamedDiscriminator}}": "{{nameof(TextColor.Green)}}" },
                            "{{nameof(FigletTextStyle.FontPath)}}": {{JsonSerializer.Serialize(fontPath)}}
                    }
                }
                """;
            var options = await DisplayOptions.DeserializeAsync(json, TestJsonHelper.EnumJsonOptions).ConfigureAwait(false);
            var expected = new DisplayOptions
            {
                Header = await FigletTextStyle.CreateAsync(fontPath, TextJustification.Left, TextColor.Green)
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
                    new TextColorJsonConverter(),
                },
            };
            var beforeConverters = caller.Converters.Count;
            var beforePolicy = caller.PropertyNamingPolicy;

            var json = new DisplayOptions().Serialize(caller);

            Assert.HasCount(beforeConverters, caller.Converters, "Caller's Converters list must not be mutated.");
            Assert.AreSame(beforePolicy, caller.PropertyNamingPolicy, "Caller's PropertyNamingPolicy must not be mutated.");

            await DisplayOptions.DeserializeAsync(json, caller);

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

            var original = DisplayOptions.Empty();
            // What we really need is not-Empty, and crazy is fairly close, except the bools
            var crazy = DisplayOptions.FromSpectreOptions(TestUtilities.Crazy);
            foreach (var property in DisplayMappings.DisplayOptionsProperties.Values.Where(p => p.Type == typeof(bool)))
            {
                property.Setter(crazy, !(bool)property.Getter(original));
            }

            var json = original.Serialize(options);
            TestUtilities.AssertTheseMatch("{}", json, shouldMatch: true, message: "Empty options should serialize to an empty JSON object when using WhenWritingDefault.");

            foreach (var property in DisplayMappings.DisplayOptionsProperties)
            {
                Logger.LogMessage($"Testing property {property.Key}\n");
                var mergeContent = DisplayOptions.Empty();

                TestUtilities.AssertTheseMatch(property.Value.Getter(original), property.Value.Getter(mergeContent), shouldMatch: true, message: $"Empty values should match {property.Key}");
                property.Value.Setter(mergeContent, property.Value.Getter(crazy));
                TestUtilities.AssertTheseMatch(property.Value.Getter(original), property.Value.Getter(mergeContent), shouldMatch: false, message: $"Failed change {property.Key}");

                var mergeJson = mergeContent.Serialize(options);
                TestUtilities.AssertTheseMatch(true, mergeJson.Contains(property.Key), shouldMatch: true, message: "The new property should be somewhere in the json");

                var clone = original.Clone();
                TestUtilities.AssertTheseMatch(original, clone, shouldMatch: true, message: $"Failed clone equals before merge {property.Key}");
                var mergedObject = await DisplayOptions.DeserializeAsync(mergeJson, createObject: () => clone).ConfigureAwait(false);
                // The end result here is now mergedObject should reference the same object as the clone, and that object should be updated.
                TestUtilities.AssertTheseMatch(clone, mergedObject, shouldMatch: true, message: $"Merged object should be the same reference as the clone for {property.Key}"); 
                Assert.AreSame(clone, mergedObject, $"Merged object should be the same reference as the clone for {property.Key}");
                TestUtilities.AssertTheseMatch(property.Value.Getter(mergeContent), property.Value.Getter(mergedObject), shouldMatch: true, message: $"merged object should match for {property.Key}");

                TestUtilities.AssertTheseMatch(original, mergedObject, shouldMatch: false, message: $"Failed change with merge {property.Key}");
                var changedProperty = FindFirstDifferenceDisplayOptions(original, mergedObject, details: false);
                TestUtilities.AssertTheseMatch(property.Key, changedProperty, shouldMatch: true, message: $"Failed change with merge {property.Key}");

                var toSpectreAndBack = DisplayOptions.FromSpectreOptions(mergedObject.ToSpectreOptions(), preferNullColors: true);
                TestUtilities.AssertTheseMatch(mergedObject, toSpectreAndBack, shouldMatch: true, message: $"Merging {property.Key} should not change the SpectreOptions conversion");
                TestUtilities.AssertTheseMatch(original, toSpectreAndBack, shouldMatch: false, message: $"Merging {property.Key} should change the SpectreOptions conversion");

                var mergedJson = mergedObject.Serialize(options);
                TestUtilities.AssertTheseMatch(true, mergedJson.Contains(property.Key), shouldMatch: true, message: "The new property should be somewhere in the merged json");

                TestJsonHelper.RoundTrip(
                    mergedJson,
                    mergedObject,
                    DisplayOptions.BuildEffectiveOptions(options, createObject: () => DisplayOptions.Empty()),
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
            var json = new DisplayOptions().Serialize();
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            var options = await DisplayOptions.DeserializeAsync(stream).ConfigureAwait(false);
            TestUtilities.AssertTheseMatch(new DisplayOptions(), options, shouldMatch: true);
        }
    }
}
