using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class TableBorderJsonConverterTests
{
    private static readonly JsonSerializerOptions _options = new()
    {
        Converters = { new TableBorderJsonConverter() }
    };

    [TestMethod]
    public void Can_Deserialize_TableBorder_By_TypeDiscriminator()
        => TestJsonHelper.RoundTrip(
            $@"{{ ""{TableBorderJsonConverter.TypeDiscriminator}"": ""{TableBorder.Ascii.GetType().Name}"" }}",
            TableBorder.Ascii,
            _options,
            assertNoDefaultEnums: true);

    [TestMethod]
    public void Can_Deserialize_TableBorder_By_Name()
        => TestJsonHelper.RoundTrip(
            $@"{{ ""Named"": ""Double"" }}",
            TableBorder.Double,
            _options,
            assertNoDefaultEnums: true);

    [TestMethod]
    public void Cannot_Deserialize_TableBorder_With_Both_TypeDiscriminator_And_Name() 
        => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TableBorder>($@"{{ ""{TableBorderJsonConverter.TypeDiscriminator}"": ""{TableBorder.Ascii.GetType().Name}"", ""Named"": ""Double"" }}", _options));

    [TestMethod]
    public void Cannot_Deserialize_TableBorder_With_Neither_TypeDiscriminator_Nor_Name()
        => Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<TableBorder>(@"{ }", _options));

    [TestMethod]
    public void Can_Round_Trip_Parent()
        => TestJsonHelper.RoundTripClass(TableBorder.Ascii, _options);

    [TestMethod]
    public void Can_Round_Trip_Nullable() 
        => TestJsonHelper.RoundTripClass<TableBorder>(value: null, _options);
}
