using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class BoxBorderJsonConverterTests
{
    private static readonly JsonSerializerOptions _options = new()
    {
        Converters = { new BoxBorderJsonConverter() }
    };

    [TestMethod]
    [DataRow(null)]
    [DataRow(nameof(AsciiBoxBorder))]
    [DataRow("ASCIIBOXBORDER")]
    public void Can_Deserialize_BoxBorder_By_TypeDiscriminator(string? asciiTypeText)
        => TestJsonHelper.RoundTrip(
            $@"{{ ""{BoxBorderJsonConverter.TypeDiscriminator}"": ""{asciiTypeText ?? BoxBorder.Ascii.GetType().Name}"" }}",
            BoxBorder.Ascii,
            _options,
            assertNoDefaultEnums: true);

    [TestMethod]
    [DataRow(nameof(BoxBorder.Double))]
    [DataRow("DOUBLE")]
    public void Can_Deserialize_BoxBorder_By_Name(string doubleText)
        => TestJsonHelper.RoundTrip(
            $@"{{ ""{BoxBorderJsonConverter.NamedDiscriminator}"": ""{doubleText}"" }}",
            BoxBorder.Double,
            _options,
            assertNoDefaultEnums: true);

    [TestMethod]
    public void Cannot_Deserialize_BoxBorder_With_Both_TypeDiscriminator_And_Name() 
        => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BoxBorder>($@"{{ ""{BoxBorderJsonConverter.TypeDiscriminator}"": ""{BoxBorder.Ascii.GetType().Name}"", ""Named"": ""Double"" }}", _options));

    [TestMethod]
    public void Cannot_Deserialize_BoxBorder_With_Neither_TypeDiscriminator_Nor_Name()
        => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<BoxBorder>(@"{ }", _options));

    [TestMethod]
    public void Can_Round_Trip_Parent()
        => TestJsonHelper.RoundTripClass(BoxBorder.Ascii, _options);

    [TestMethod]
    public void Can_Round_Trip_Nullable() 
        => TestJsonHelper.RoundTripClass<BoxBorder>(value: null, _options);

    [TestMethod]
    public void Identifies_None_As_Default()
    {
        var converter = new BoxBorderJsonConverter();

        TestUtilities.AssertTheseMatch(true, converter.IsDefault(BoxBorder.None), shouldMatch: true);
        TestUtilities.AssertTheseMatch(false, converter.IsDefault(BoxBorder.Rounded), shouldMatch: true);
    }
}
