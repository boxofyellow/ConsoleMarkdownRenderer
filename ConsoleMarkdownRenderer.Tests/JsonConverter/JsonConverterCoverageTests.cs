using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

[TestClass]
public class JsonConverterCoverageTests : TestBase
{
    // A user-defined IHeaderStyle that the library's HeaderStyleJsonConverter must reject
    // on the write side (it only knows TextStyle and FigletTextStyle).
    private sealed class UnknownHeaderStyle : IHeaderStyle
    {
        public TextColor? Foreground => null;
        public TextColor? Background => null;
        public TextDecoration Decoration => TextDecoration.None;
    }

    // ---- HeaderStyleJsonConverter --------------------------------------------------

    [TestMethod]
    public async Task HeaderStyleConverter_Read_NullToken_ReturnsPlain()
    {
        var json = """{ "header": null }""";
        var options = await DisplayOptions.DeserializeAsync(json);
        Assert.IsNotNull(options.Header);
        TestUtilities.AssertTheseMatch(TextStyle.Plain, options.Header, shouldMatch: true);
    }

    [TestMethod]
    public async Task HeaderStyleConverter_Read_MissingDiscriminator_Throws()
    {
        var json = $$"""
            {
                "header": { "decoration": "{{TextDecoration.Bold}}" }
            }
            """;
        var ex = await Assert.ThrowsExactlyAsync<JsonException>(
            () => DisplayOptions.DeserializeAsync(json));
        Assert.Contains("$type", ex.Message);
    }

    [TestMethod]
    public async Task HeaderStyleConverter_Read_NullDiscriminator_Throws()
    {
        // Discriminator key is present but its value is JSON null — exercises the
        // ValueKind == Null branch and the IsNullOrEmpty(typeName) throw.
        var json = """
            {
                "header": { "$type": null }
            }
            """;
        var ex = await Assert.ThrowsExactlyAsync<JsonException>(
            () => DisplayOptions.DeserializeAsync(json));
        Assert.Contains("$type", ex.Message);
    }

    [TestMethod]
    public async Task HeaderStyleConverter_Read_UnknownDiscriminator_Throws()
    {
        var json = """
            {
                "header": { "$type": "NotARealStyle" }
            }
            """;
        var ex = await Assert.ThrowsExactlyAsync<JsonException>(
            () => DisplayOptions.DeserializeAsync(json));
        Assert.Contains("NotARealStyle", ex.Message);
    }

    [TestMethod]
    public void HeaderStyleConverter_Write_UnknownImplementation_Throws()
    {
        var options = new DisplayOptions();
        options.Headers.Clear();
        options.Headers.Add(new UnknownHeaderStyle());

        var ex = Assert.ThrowsExactly<JsonException>(() => options.Serialize());
        StringAssert.Contains(ex.Message, nameof(UnknownHeaderStyle));
    }

    // ---- TextColorJsonConverter ----------------------------------------------------

    [TestMethod]
    public async Task TextColorConverter_Read_NullToken_ReturnsNull()
    {
        // STJ short-circuits null tokens for reference-type targets, so the converter
        // is never invoked. We assert TextStyle.Foreground round-trips a JSON null.
        var json = $$"""
            {
                "header": {
                    "$type": "{{nameof(TextStyle)}}",
                    "decoration": {{(int)TextDecoration.Bold}},
                    "foreground": null
                }
            }
            """;
        var options = await DisplayOptions.DeserializeAsync(json);
        var header = (TextStyle)options.Header!;
        Assert.IsNull(header.Foreground);
    }

    [TestMethod]
    public async Task TextColorConverter_Read_RgbChannels_AllPresent()
    {
        var json = $$"""
            {
                "header": {
                    "$type": "{{nameof(TextStyle)}}",
                    "foreground": { "r": 10, "g": 20, "b": 30 }
                }
            }
            """;
        var options = await DisplayOptions.DeserializeAsync(json);
        var fg = ((TextStyle)options.Header!).Foreground!;
        Assert.IsTrue(fg.IsRgb);
        TestUtilities.AssertTheseMatch(10, fg.R, shouldMatch: true);
        TestUtilities.AssertTheseMatch(20, fg.G, shouldMatch: true);
        TestUtilities.AssertTheseMatch(30, fg.B, shouldMatch: true);
    }

    [TestMethod]
    public async Task TextColorConverter_Read_RgbChannels_MissingDefaultToZero()
    {
        // Only "r" provided — exercises both the r-case-only branch and the
        // FromRgb(r, g ?? 0, b ?? 0) "any RGB field implies RGB" fallback.
        var json = $$"""
            {
                "header": {
                    "$type": "{{nameof(TextStyle)}}",
                    "foreground": { "r": 42 }
                }
            }
            """;
        var options = await DisplayOptions.DeserializeAsync(json);
        var fg = ((TextStyle)options.Header!).Foreground!;
        Assert.IsTrue(fg.IsRgb);
        TestUtilities.AssertTheseMatch(42, fg.R, shouldMatch: true);
        TestUtilities.AssertTheseMatch(0, fg.G, shouldMatch: true);
        TestUtilities.AssertTheseMatch(0, fg.B, shouldMatch: true);
    }

    // ---- DisplayOptions.DeserializeAsync(Stream) -----------------------------------

    [TestMethod]
    public async Task DeserializeAsync_Stream_RoundTripsHeader()
    {
        // Exercises the stream overload and the "options.Header is FigletTextStyle"
        // branch of EnsureHeaderFontsLoadedAsync (since the deserialized Header is a
        // FigletTextStyle without a FontPath — EnsureFontLoadedAsync becomes a no-op
        // but the branch is taken).
        var json = $$"""
            {
                "header": {
                    "$type": "{{nameof(FigletTextStyle)}}",
                    "justification": {{(int)TextJustification.Center}}
                }
            }
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var options = await DisplayOptions.DeserializeAsync(stream);
        Assert.IsInstanceOfType(options.Header, typeof(FigletTextStyle));
        TestUtilities.AssertTheseMatch(TextJustification.Center, ((FigletTextStyle)options.Header!).Justification, shouldMatch: true);
    }

    [TestMethod]
    public async Task DeserializeAsync_Stream_WithCallerOptions()
    {
        // Same overload, exercising the caller-options parameter as well.
        var json = $$"""
            {
                "headers": [
                    {
                        "$type": "{{nameof(TextStyle)}}",
                        "decoration": "{{TextDecoration.Italic}}"
                    }
                ]
            }
            """;
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var caller = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() },
        };
        var options = await DisplayOptions.DeserializeAsync(stream, caller);
        TestUtilities.AssertTheseMatch(TextDecoration.Italic, ((TextStyle)options.Headers[0]).Decoration, shouldMatch: true);
    }
}
