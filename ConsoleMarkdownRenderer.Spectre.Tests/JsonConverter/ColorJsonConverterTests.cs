using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class ColorJsonConverterTests
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            Converters = { new ColorJsonConverter() }
        };

        [TestMethod]
        public void Can_Deserialize_Color_By_IsDefault()
            => TestJsonHelper.RoundTrip(
                $@"{{ ""{ColorJsonConverter.IsDefaultDiscriminator}"": true }}",
                Color.Default,
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        [DataRow($@"{{ }}")]
        [DataRow($@"{{ ""R"": 0, ""G"": 0, ""B"": 0 }}")]
        public void Can_Deserialize_Empty_Color(string json)
            => TestJsonHelper.RoundTrip(
                json,
                new Color(),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        [DataRow(nameof(Color.Red))]
        [DataRow("red")]
        [DataRow("RED")]
        public void Can_Deserialize_Color_By_Named(string redText)
            => TestJsonHelper.RoundTrip(
                $@"{{ ""{ColorJsonConverter.NamedDiscriminator}"": ""{redText}"" }}",
                Color.Red,
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Deserialize_Color_By_ConsoleColor()
            => TestJsonHelper.RoundTrip(
                $@"{{ ""{ColorJsonConverter.ConsoleColorDiscriminator}"": {(int)ConsoleColor.Blue} }}",
                Color.FromConsoleColor(ConsoleColor.Blue),
                _options,
                assertNoDefaultEnums: true);

        [TestMethod]
        [DataRow(10, 20, 30)]
        [DataRow(11, 19, 31)]
        public void Can_Deserialize_Color_By_RGB(int r, int g, int b)
            => TestJsonHelper.RoundTrip(
                $@"{{ ""R"": {r}, ""G"": {g}, ""B"": {b} }}",
                new Color((byte)r, (byte)g, (byte)b),
                _options,
                assertNoDefaultEnums: false);

        [TestMethod]
        public void Can_Round_Trip_Parent()
            => TestJsonHelper.RoundTripStruct<Color>(Color.Green, _options);

        [TestMethod]
        public void Can_Round_Trip_Nullable() 
            => TestJsonHelper.RoundTripStruct<Color>(value: null, _options);

        [TestMethod]
        [DataRow(false, false, ""               , null            , 0, 0, 0)]
        [DataRow(false, true , ""               , null            , 0, 0, 0)]
        [DataRow(true , true , nameof(Color.Red), null            , 0, 0, 0)]
        [DataRow(true , true , ""               , ConsoleColor.Red, 0, 0, 0)]
        [DataRow(true , true , ""               , null            , 1, 0, 0)]
        [DataRow(true , true , ""               , null            , 0, 1, 0)]
        [DataRow(true , true , ""               , null            , 0, 0, 1)]
        [DataRow(false, false, nameof(Color.Red), null            , 0, 0, 0)]
        [DataRow(true , false, nameof(Color.Red), ConsoleColor.Red, 0, 0, 0)]
        [DataRow(true , false, nameof(Color.Red), null            , 1, 0, 0)]
        [DataRow(true , false, nameof(Color.Red), null            , 0, 1, 0)]
        [DataRow(true , false, nameof(Color.Red), null            , 0, 0, 1)]
        [DataRow(false, false, ""               , ConsoleColor.Red, 0, 0, 0)]
        [DataRow(true , false, ""               , ConsoleColor.Red, 1, 0, 0)]
        [DataRow(true , false, ""               , ConsoleColor.Red, 0, 1, 0)]
        [DataRow(true , false, ""               , ConsoleColor.Red, 0, 0, 1)]
        [DataRow(false, false, ""               , null            , 1, 0, 0)]
        [DataRow(false, false, ""               , null            , 0, 1, 0)]
        [DataRow(false, false, ""               , null            , 0, 0, 1)]
        public void Mix_Properties_Throw(bool shouldThrow, bool isDefault, string? name, ConsoleColor? consoleColor, int r, int g = 0, int b = 0)
        {
            var json = new Dictionary<string, object?>();
            Color color = new Color((byte)r, (byte)g, (byte)b);
            if (isDefault)
            {
                json[ColorJsonConverter.IsDefaultDiscriminator] = true;
                color = Color.Default;
            }
            if (!string.IsNullOrEmpty(name))
            {
                json[ColorJsonConverter.NamedDiscriminator] = name;
                var namedColor = Color.FromName(name);
                Assert.IsTrue(namedColor.HasValue, $"Test setup failure: '{name}' is not a valid color name.");
                color = namedColor.Value;
            }
            if (consoleColor.HasValue)
            {
                json[ColorJsonConverter.ConsoleColorDiscriminator] = consoleColor.Value;
                color = Color.FromConsoleColor(consoleColor.Value);
            }
            if (r > 0)
            {
                json[nameof(Color.R)] = r;
            }
            if (g > 0)
            {
                json[nameof(Color.G)] = g;
            }
            if (b > 0)
            {
                json[nameof(Color.B)] = b;
            }

            var jsonString = JsonSerializer.Serialize(json);

            if (shouldThrow)
            {
                Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Color>(jsonString, _options));
            }
            else
            {
                TestJsonHelper.RoundTrip(
                    jsonString,
                    color,
                    _options,
                    assertNoDefaultEnums: false);
            }
        }
    }
}