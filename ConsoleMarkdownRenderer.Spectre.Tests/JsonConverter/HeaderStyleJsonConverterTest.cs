using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class HeaderStyleJsonConverterTest
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            Converters = 
            { 
                new SpectreHeaderStyleJsonConverter(),

                // needed for the inner types
                new ColorJsonConverter(),
                new BoxBorderJsonConverter(),
            }
        };

        private static readonly JsonSerializerOptions _optionsWithEnumConverter = new()
        {
            Converters =
            {
                new JsonStringEnumConverter(),

                new SpectreHeaderStyleJsonConverter(),

                // needed for the inner types
                new ColorJsonConverter(),
                new BoxBorderJsonConverter(),
            }
        };

        [TestMethod]
        public void Can_Deserialize_Empty_HeaderStyle_For_SpectreFigletTextStyle()
            => TestJsonHelper.RoundTrip<ISpectreHeaderStyle>(
                $@"{{ ""{SpectreHeaderStyleJsonConverter.TypeDiscriminator}"": ""{nameof(SpectreFigletTextStyle)}"" }}",
                SpectreFigletTextStyle.Create(),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Deserialize_HeaderStyle_For_SpectreFigletTextStyle()
            => TestJsonHelper.RoundTrip<ISpectreHeaderStyle>(
                $$"""
                { 
                    "{{SpectreHeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(SpectreFigletTextStyle)}}",
                    "{{nameof(SpectreFigletTextStyle.Justification)}}": {{(int)Justify.Center}},
                    "{{nameof(SpectreFigletTextStyle.Foreground)}}": { "Named": "{{nameof(Color.Red)}}" },
                    "{{nameof(SpectreFigletTextStyle.FontPath)}}": "path/to/font"
                }
                """,
                SpectreFigletTextStyle.Create(Justify.Center, Color.Red, "path/to/font"),
                _options,
                assertNoDefaultEnums: true);


        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Will_RespectOptions_When_Serializing_SpectreFigletTextStyle(bool useEnumConverter)
        {
            var options = useEnumConverter ? _optionsWithEnumConverter : _options;
            var figletStyle = SpectreFigletTextStyle.Create(Justify.Center);
            var json = JsonSerializer.Serialize<ISpectreHeaderStyle>(figletStyle, options);
            var expected = JsonSerializer.Serialize(Justify.Center, options);
            Assert.Contains(expected, json);
        }

        [TestMethod]
        public void Can_Deserialize_Empty_HeaderStyle_For_SpectreRuleHeaderStyle()
            => TestJsonHelper.RoundTrip<ISpectreHeaderStyle>(
                $@"{{ ""{SpectreHeaderStyleJsonConverter.TypeDiscriminator}"": ""{nameof(SpectreRuleHeaderStyle)}"" }}",
                new SpectreRuleHeaderStyle(),
                _options,
                assertNoDefaultEnums: false);


        [TestMethod]
        public void Can_Deserialize_HeaderStyle_For_SpectreRuleHeaderStyle()
            => TestJsonHelper.RoundTrip<ISpectreHeaderStyle>(
                $$"""
                { 
                    "{{SpectreHeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(SpectreRuleHeaderStyle)}}",
                    "{{nameof(SpectreRuleHeaderStyle.Justification)}}": {{(int)Justify.Center}},
                    "{{nameof(SpectreRuleHeaderStyle.Foreground)}}": { "Named": "{{nameof(Color.Red)}}" },
                    "{{nameof(SpectreRuleHeaderStyle.Border)}}": { "Named": "{{nameof(BoxBorder.Ascii)}}" }
                }
                """,
                new SpectreRuleHeaderStyle(Justify.Center, Color.Red, BoxBorder.Ascii),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Will_RespectOptions_When_Serializing_SpectreRuleHeaderStyle(bool useEnumConverter)
        {
            var options = useEnumConverter ? _optionsWithEnumConverter : _options;
            var ruleHeaderStyle = new SpectreRuleHeaderStyle(Justify.Center, Color.Red, BoxBorder.Ascii);
            var json = JsonSerializer.Serialize<ISpectreHeaderStyle>(ruleHeaderStyle, options);
            var expected = JsonSerializer.Serialize(Justify.Center, options);
            Assert.Contains(expected, json);
        }


        [TestMethod]
        public void Can_Deserialize_Empty_HeaderStyle_For_SpectreTextStyle()
            => TestJsonHelper.RoundTrip<ISpectreHeaderStyle>(
                $@"{{ ""{SpectreHeaderStyleJsonConverter.TypeDiscriminator}"": ""{nameof(SpectreTextStyle)}"" }}",
                new SpectreTextStyle(),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Deserialize_HeaderStyle_For_SpectreTextStyle()
            => TestJsonHelper.RoundTrip<ISpectreHeaderStyle>(
                $$"""
                { 
                    "{{nameof(SpectreTextStyle.Decoration)}}": {{(int)Decoration.Bold}},
                    "{{SpectreHeaderStyleJsonConverter.TypeDiscriminator}}": "{{nameof(SpectreTextStyle)}}",
                    "{{nameof(SpectreTextStyle.Foreground)}}": { "Named": "{{nameof(Color.Red)}}" },
                    "{{nameof(SpectreTextStyle.Background)}}": { "Named": "{{nameof(Color.Blue)}}" }
                }
                """,
                new SpectreTextStyle(Decoration.Bold, Color.Red, Color.Blue),
                new JsonSerializerOptions(_options) {UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow}, // exclude fields to avoid the default enum values being included in the JSON
                assertNoDefaultEnums: true);

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Will_RespectOptions_When_Serializing_SpectreTextStyle(bool useEnumConverter)
        {
            var options = useEnumConverter ? _optionsWithEnumConverter : _options;
            var textStyle = new SpectreTextStyle(Decoration.Bold, Color.Red, Color.Blue);
            var json = JsonSerializer.Serialize<ISpectreHeaderStyle>(textStyle, options);
            var expected = JsonSerializer.Serialize(Decoration.Bold, options);
            Assert.Contains(expected, json);
        }
    }
}