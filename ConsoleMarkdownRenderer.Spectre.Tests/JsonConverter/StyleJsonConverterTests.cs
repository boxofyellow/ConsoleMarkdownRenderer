using System.Text.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.JsonConverter;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;

[TestClass]
public class StyleJsonConverterTests
{
    private static readonly JsonSerializerOptions _options = new()
    {
        Converters = { 
            new StyleJsonConverter(),
            new ColorJsonConverter(),
        } 
    };

    [TestMethod]
    public void Can_Deserialize_Empty_Style()
        => TestJsonHelper.RoundTrip(
            "{}",
            new Style(),
            _options,
            assertNoDefaultEnums: false);

    [TestMethod]
    public void Can_Deserialize_Color_By_Named()
        => TestJsonHelper.RoundTrip(
            $@"{{ 
                    ""{nameof(Style.Foreground)}"": {{ ""Named"": ""{nameof(Color.Red)}"" }},
                    ""{nameof(Style.Background)}"": {{ ""Named"": ""{nameof(Color.Blue)}"" }},
                    ""{nameof(Style.Decoration)}"": {(int)Decoration.Italic}
                }}",
            new Style(foreground: Color.Red, background: Color.Blue, decoration: Decoration.Italic),
            _options,
            assertNoDefaultEnums: false);


    [TestMethod]
    public void Can_Round_Trip_Parent()
        => TestJsonHelper.RoundTripStruct<Style>(new Style(foreground: Color.Red, background: Color.Blue, Decoration.Italic), _options);

    [TestMethod]
    public void Can_Round_Trip_Nullable() 
        => TestJsonHelper.RoundTripStruct<Style>(value: null, _options);
}