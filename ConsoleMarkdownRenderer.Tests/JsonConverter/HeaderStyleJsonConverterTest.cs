using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    [TestClass]
    public class HeaderStyleJsonConverterTest : TestBase
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            Converters = 
            { 
                new HeaderStyleJsonConverter(),

                // needed for the inner types
                new TextColorJsonConverter(),
            }
        };

        private static readonly JsonSerializerOptions _optionsWithEnumConverter = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(),

                new HeaderStyleJsonConverter(),

                // needed for the inner types
                new TextColorJsonConverter(),
            }
        };

        [TestMethod]
        public void Can_Deserialize_Empty_HeaderStyle_For_FigletTextStyle()
            => TestJsonHelper.RoundTrip<IHeaderStyle>(
                $@"{{ ""{HeaderStyleJsonConverter.TypeDiscriminator}"": ""{nameof(FigletTextStyle)}"" }}",
                FigletTextStyle.Create(),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Deserialize_HeaderStyle_For_FigletTextStyle()
            => TestJsonHelper.RoundTrip<IHeaderStyle>(
                $$"""
                { 
                    "{{HeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(FigletTextStyle)}}",
                    "{{nameof(FigletTextStyle.Justification)}}": {{(int)TextJustification.Center}},
                    "{{nameof(FigletTextStyle.Foreground)}}": { "Named": "{{nameof(TextColor.Red)}}" },
                    "{{nameof(FigletTextStyle.FontPath)}}": "path/to/font"
                }
                """,
                FigletTextStyle.Create(TextJustification.Center, TextColor.Red, "path/to/font"),
                _options,
                assertNoDefaultEnums: true);


        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Will_RespectOptions_When_Serializing_FigletTextStyle(bool useEnumConverter)
        {
            var options = useEnumConverter ? _optionsWithEnumConverter : _options;
            var figletStyle = FigletTextStyle.Create(TextJustification.Center);
            var json = JsonSerializer.Serialize<IHeaderStyle>(figletStyle, options);
            var expected = JsonSerializer.Serialize(TextJustification.Center, options);
            Assert.Contains(expected, json);
        }

        [TestMethod]
        public void Can_Deserialize_Empty_HeaderStyle_ForRuleHeaderStyle()
            => TestJsonHelper.RoundTrip<IHeaderStyle>(
                $@"{{ ""{HeaderStyleJsonConverter.TypeDiscriminator}"": ""{nameof(RuleHeaderStyle)}"" }}",
                new RuleHeaderStyle(),
                _options,
                assertNoDefaultEnums: false);


        [TestMethod]
        public void Can_Deserialize_HeaderStyle_ForRuleHeaderStyle()
            => TestJsonHelper.RoundTrip<IHeaderStyle>(
                $$"""
                { 
                    "{{HeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(RuleHeaderStyle)}}",
                    "{{nameof(RuleHeaderStyle.Justification)}}": {{(int)TextJustification.Center}},
                    "{{nameof(RuleHeaderStyle.Foreground)}}": { "Named": "{{nameof(TextColor.Red)}}" },
                    "{{nameof(RuleHeaderStyle.Border)}}": {{(int)RuleBorder.Ascii}}
                }
                """,
                new RuleHeaderStyle(TextJustification.Center, TextColor.Red, RuleBorder.Ascii),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Will_RespectOptions_When_SerializingRuleHeaderStyle(bool useEnumConverter)
        {
            var options = useEnumConverter ? _optionsWithEnumConverter : _options;
            var ruleHeaderStyle = new RuleHeaderStyle(TextJustification.Center, TextColor.Red, RuleBorder.Ascii);
            var json = JsonSerializer.Serialize<IHeaderStyle>(ruleHeaderStyle, options);
            var expected = JsonSerializer.Serialize(TextJustification.Center, options);
            Assert.Contains(expected, json);
            expected = JsonSerializer.Serialize(RuleBorder.Ascii, options);
            Assert.Contains(expected, json);
        }

        [TestMethod]
        public void Can_Deserialize_Empty_HeaderStyle_ForTextStyle()
            => TestJsonHelper.RoundTrip<IHeaderStyle>(
                $@"{{ ""{HeaderStyleJsonConverter.TypeDiscriminator}"": ""{nameof(TextStyle)}"" }}",
                new TextStyle(),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Deserialize_HeaderStyle_ForTextStyle()
            => TestJsonHelper.RoundTrip<IHeaderStyle>(
                $$"""
                { 
                    "{{nameof(TextStyle.Decoration)}}": {{(int)TextDecoration.Bold}},
                    "{{HeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(TextStyle)}}",
                    "{{nameof(TextStyle.Foreground)}}": { "Named": "{{nameof(TextColor.Red)}}" },
                    "{{nameof(TextStyle.Background)}}": { "Named": "{{nameof(TextColor.Blue)}}" }
                }
                """,
                new TextStyle(TextDecoration.Bold, TextColor.Red, TextColor.Blue),
                new JsonSerializerOptions(_options) {UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow}, // exclude fields to avoid the default enum values being included in the JSON
                assertNoDefaultEnums: true);

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Will_RespectOptions_When_SerializingTextStyle(bool useEnumConverter)
        {
            var options = useEnumConverter ? _optionsWithEnumConverter : _options;
            var textStyle = new TextStyle(TextDecoration.Bold, TextColor.Red, TextColor.Blue);
            var json = JsonSerializer.Serialize<IHeaderStyle>(textStyle, options);
            var expected = JsonSerializer.Serialize(TextDecoration.Bold, options);
            Assert.Contains(expected, json);
        }
    }
}