using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Json;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Coverage-focused tests for the styling JSON converters
    /// (<see cref="HeaderStyleJsonConverter"/>, <see cref="TextColorJsonConverter"/>,
    /// <see cref="SpectreJsonWriteHelpers"/>) and the stream / <c>Header</c> branches of
    /// <see cref="DisplayOptions.DeserializeAsync(System.IO.Stream, JsonSerializerOptions?, System.Threading.CancellationToken)"/>.
    /// Each test exercises a path that the previous run of the code-coverage workflow flagged
    /// as unreached.
    /// </summary>
    [TestClass]
    public class JsonConverterCoverageTests
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
        public async Task HeaderStyleConverter_Read_NullToken_ReturnsNull()
        {
            // System.Text.Json short-circuits null tokens for reference-type targets, so
            // the converter is never invoked. We assert that DisplayOptions.Header round-
            // trips a JSON null regardless.
            var json = """{ "header": null }""";
            var options = await DisplayOptions.DeserializeAsync(json);
            Assert.IsNull(options.Header);
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
            StringAssert.Contains(ex.Message, "$type");
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
            StringAssert.Contains(ex.Message, "$type");
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
            StringAssert.Contains(ex.Message, "NotARealStyle");
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
                        "decoration": "{{TextDecoration.Bold}}",
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
            Assert.AreEqual(10, fg.R);
            Assert.AreEqual(20, fg.G);
            Assert.AreEqual(30, fg.B);
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
            Assert.AreEqual(42, fg.R);
            Assert.AreEqual(0, fg.G);
            Assert.AreEqual(0, fg.B);
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
                        "justification": "{{TextJustification.Center}}"
                    }
                }
                """;
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var options = await DisplayOptions.DeserializeAsync(stream);
            Assert.IsInstanceOfType(options.Header, typeof(FigletTextStyle));
            Assert.AreEqual(TextJustification.Center, ((FigletTextStyle)options.Header!).Justification);
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
            Assert.AreEqual(TextDecoration.Italic, ((TextStyle)options.Headers[0]).Decoration);
        }

        // ---- SpectreJsonWriteHelpers ----------------------------------------------------------

        [TestMethod]
        public void JsonWriteHelpers_ShouldIgnore_Always_ReturnsTrue()
        {
            // DefaultIgnoreCondition.Always cannot be set on JsonSerializerOptions directly
            // (the property setter rejects it), so ShouldIgnore is exercised directly to
            // cover the Always / Never arms.
            Assert.IsTrue(SpectreJsonWriteHelpers.ShouldIgnore("anything", JsonIgnoreCondition.Always));
            Assert.IsTrue(SpectreJsonWriteHelpers.ShouldIgnore<string?>(null, JsonIgnoreCondition.Always));
            Assert.IsFalse(SpectreJsonWriteHelpers.ShouldIgnore("anything", JsonIgnoreCondition.Never));
        }

        [TestMethod]
        public void JsonWriteHelpers_ShouldIgnore_UnexpectedCondition_Throws()
        {
            // An out-of-range enum value falls through to the default arm of the switch.
            var ex = Assert.ThrowsExactly<InvalidOperationException>(
                () => SpectreJsonWriteHelpers.ShouldIgnore("anything", (JsonIgnoreCondition)int.MaxValue));
            StringAssert.Contains(ex.Message, int.MaxValue.ToString());
        }
    }
}
