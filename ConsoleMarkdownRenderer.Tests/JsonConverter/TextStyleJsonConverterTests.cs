using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.JsonConverter;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

[TestClass]
public class TextStyleJsonConverterTests : TestBase

{
    private static readonly JsonSerializerOptions _options = new()
    {
        Converters = { 
            new TextStyleJsonConverter(),
            new TextColorJsonConverter(),
        } 
    };

    [TestMethod]
    public void Can_Deserialize_Empty_Style()
        => TestJsonHelper.RoundTrip(
            "{}",
            new TextStyle(),
            _options,
            assertNoDefaultEnums: false);

    [TestMethod]
    public void Can_Deserialize_Color_By_Named()
        => TestJsonHelper.RoundTrip(
            $@"{{ 
                    ""{nameof(TextStyle.Foreground)}"": {{ ""Named"": ""{nameof(TextColor.Red)}"" }},
                    ""{nameof(TextStyle.Background)}"": {{ ""Named"": ""{nameof(TextColor.Blue)}"" }},
                    ""{nameof(TextStyle.Decoration)}"": {(int)TextDecoration.Italic}
                }}",
            new TextStyle(foreground: TextColor.Red, background: TextColor.Blue, decoration: TextDecoration.Italic),
            _options,
            assertNoDefaultEnums: false);


    [TestMethod]
    public void Can_Round_Trip_Parent()
        => TestJsonHelper.RoundTripClass(new TextStyle(foreground: TextColor.Red, background: TextColor.Blue, decoration: TextDecoration.Italic), _options);

    [TestMethod]
    public void Can_Round_Trip_Nullable() 
        => TestJsonHelper.RoundTripClass<TextStyle>(value: null, _options);
}