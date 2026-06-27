using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    [TestClass]
    public class TextColorJsonConverterTests : TestBase
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            Converters = { new TextColorJsonConverter() }
        };

        private static readonly JsonSerializerOptions _spectreOptions = new()
        {
            Converters = { new ColorJsonConverter() }
        };

        [TestMethod]
        public void Can_Deserialize_Default_Color()
            => AllTheRoundTrips(
                $@"{{ ""{TextColorJsonConverter.IsDefaultDiscriminator}"": true }}",
                TextColor.Default,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Deserialize_Default_Set_False_Throws()
            => Assert.Throws<JsonException>(
                () => JsonSerializer.Deserialize<TextColor>(
                        $@"{{ ""{TextColorJsonConverter.IsDefaultDiscriminator}"": false }}",
                        _options));

        [TestMethod]
        [DataRow($@"{{ }}")]
        [DataRow($@"{{ ""R"": 0, ""G"": 0, ""B"": 0 }}")]
        public void Can_Deserialize_Empty_Color(string json)
            => AllTheRoundTrips(
                json,
                TextColor.FromRgb(0, 0, 0),
                assertNoDefaultEnums: false);


        [TestMethod]
        [DataRow(nameof(TextColor.Red))]
        [DataRow("red")]
        [DataRow("RED")]
        public void Can_Deserialize_Color_By_Named(string redText)
            => AllTheRoundTrips(
                $@"{{ ""{TextColorJsonConverter.NamedDiscriminator}"": ""{redText}"" }}",
                TextColor.Red,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Deserialize_Color_By_ConsoleColor()
            => AllTheRoundTrips(
                $@"{{ ""{TextColorJsonConverter.ConsoleColorDiscriminator}"": {(int)ConsoleColor.Blue} }}",
                TextColor.Blue,
                assertNoDefaultEnums: true);

        [TestMethod]
        [DataRow(10, 20, 30)]
        [DataRow(11, 19, 31)]
        public void Can_Deserialize_Color_By_RGB(int r, int g, int b)
            => AllTheRoundTrips(
                $@"{{ ""R"": {r}, ""G"": {g}, ""B"": {b} }}",
                TextColor.FromRgb((byte)r, (byte)g, (byte)b),
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Round_Trip_Parent()
            => TestJsonHelper.RoundTripClass(TextColor.Green, _options);

        [TestMethod]
        public void Can_Round_Trip_Nullable() 
            => TestJsonHelper.RoundTripClass<TextColor>(value: null, _options);

        [TestMethod]
        [DataRow(false, false, null          , null            , 0, 0, 0)]
        [DataRow(false, true , null          , null            , 0, 0, 0)]
        [DataRow(true , true , NamedColor.Red, null            , 0, 0, 0)]
        [DataRow(true , true , null          , ConsoleColor.Red, 0, 0, 0)]
        [DataRow(true , true , null          , null            , 1, 0, 0)]
        [DataRow(true , true , null          , null            , 0, 1, 0)]
        [DataRow(true , true , null          , null            , 0, 0, 1)]
        [DataRow(false, false, NamedColor.Red, null            , 0, 0, 0)]
        [DataRow(true , false, NamedColor.Red, ConsoleColor.Red, 0, 0, 0)]
        [DataRow(true , false, NamedColor.Red, null            , 1, 0, 0)]
        [DataRow(true , false, NamedColor.Red, null            , 0, 1, 0)]
        [DataRow(true , false, NamedColor.Red, null            , 0, 0, 1)]
        [DataRow(false, false, null          , ConsoleColor.Red, 0, 0, 0)]
        [DataRow(true , false, null          , ConsoleColor.Red, 1, 0, 0)]
        [DataRow(true , false, null          , ConsoleColor.Red, 0, 1, 0)]
        [DataRow(true , false, null          , ConsoleColor.Red, 0, 0, 1)]
        [DataRow(false, false, null          , null            , 1, 0, 0)]
        [DataRow(false, false, null          , null            , 0, 1, 0)]
        [DataRow(false, false, null          , null            , 0, 0, 1)]
        public void Mix_Properties_Throw(bool shouldThrow, bool isDefault, NamedColor? name, ConsoleColor? consoleColor, int r, int g, int b)
        {
            var json = new Dictionary<string, object?>();
            TextColor color = TextColor.FromRgb((byte)r, (byte)g, (byte)b);
            if (isDefault)
            {
                json[TextColorJsonConverter.IsDefaultDiscriminator] = true;
                color = TextColor.Default;
            }
            if (name.HasValue)
            {
                json[TextColorJsonConverter.NamedDiscriminator] = name.ToString();
                color = TextColor.FromNamed(name.Value);
            }
            if (consoleColor.HasValue)
            {
                json[TextColorJsonConverter.ConsoleColorDiscriminator] = consoleColor.Value;
                color = Color.FromConsoleColor(consoleColor.Value).ToTextColor();
            }
            if (r > 0)
            {
                json[nameof(TextColor.R)] = r;
            }
            if (g > 0)
            {
                json[nameof(TextColor.G)] = g;
            }
            if (b > 0)
            {
                json[nameof(TextColor.B)] = b;
            }

            var jsonString = JsonSerializer.Serialize(json);

            if (shouldThrow)
            {
                Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TextColor>(jsonString, _options));
            }
            else
            {
                AllTheRoundTrips(jsonString, color, assertNoDefaultEnums: false);
            }
        }

        private static void AllTheRoundTrips(string json, TextColor color, bool assertNoDefaultEnums)
        {
            TestJsonHelper.RoundTrip(json, color, _options, assertNoDefaultEnums);

            var spectreColor = color.ToSpectreColor();
            TestJsonHelper.RoundTrip(json, spectreColor, _spectreOptions, assertNoDefaultEnums);
        }
    }
}