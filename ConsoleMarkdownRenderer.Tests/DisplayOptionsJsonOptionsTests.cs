using System.Text.Json;
using System.Text.Json.Serialization;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests that <see cref="DisplayOptions.Serialize(JsonSerializerOptions?)"/> honours the
    /// caller-supplied <see cref="JsonSerializerOptions"/> when producing JSON — in
    /// particular <see cref="JsonSerializerOptions.PropertyNamingPolicy"/> and
    /// <see cref="JsonSerializerOptions.DefaultIgnoreCondition"/>. The library still adds
    /// the required converters (<see cref="HeaderStyleJsonConverter"/> and
    /// <see cref="TextColorJsonConverter"/>) to a copy of the caller's options so the
    /// caller's instance is not mutated.
    /// </summary>
    [TestClass]
    public class DisplayOptionsJsonOptionsTests
    {
        [TestMethod]
        public void Serialize_PropertyNamingPolicy_SnakeCaseLower_IsHonoured()
        {
            var options = new DisplayOptions
            {
                Headers =
                {
                    FigletTextStyle.Create(TextJustification.Left, TextColor.FromRgb(10, 20, 30)),
                    new TextStyle(TextDecoration.Bold, TextColor.Red),
                },
                Header = new TextStyle(TextDecoration.Italic),
            };

            var json = options.Serialize(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                Converters = { new JsonStringEnumConverter() },
            });

            // The converter-emitted property names (justification / foreground / font_path /
            // decoration / background / r / g / b) and the property names of DisplayOptions
            // itself (headers, header) all reflect the policy.
            StringAssert.Contains(json, "\"headers\":");
            StringAssert.Contains(json, "\"header\":");
            StringAssert.Contains(json, "\"justification\":");
            StringAssert.Contains(json, "\"foreground\":");
            StringAssert.Contains(json, "\"font_path\":");
            StringAssert.Contains(json, "\"decoration\":");
            // RGB channels still pass through the naming policy (single-letter names are
            // unchanged by SnakeCaseLower).
            StringAssert.Contains(json, "\"r\":");

            // The $type discriminator is a System.Text.Json convention and is emitted
            // literally regardless of the naming policy.
            StringAssert.Contains(json, $"\"$type\":\"{nameof(FigletTextStyle)}\"");
            StringAssert.Contains(json, $"\"$type\":\"{nameof(TextStyle)}\"");
        }

        [TestMethod]
        public void Serialize_PropertyNamingPolicy_OnlyTransformsKnownNames()
        {
            // PascalCase property names should not appear when CamelCase is requested.
            var options = new DisplayOptions
            {
                Headers = { FigletTextStyle.Create(TextJustification.Left, TextColor.Blue) },
            };

            var json = options.Serialize(new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() },
            });

            Assert.IsFalse(json.Contains("\"Justification\":"), $"CamelCase JSON should not contain PascalCase 'Justification': {json}");
            Assert.IsFalse(json.Contains("\"Foreground\":"), $"CamelCase JSON should not contain PascalCase 'Foreground': {json}");
            Assert.IsFalse(json.Contains("\"FontPath\":"), $"CamelCase JSON should not contain PascalCase 'FontPath': {json}");
        }

        [TestMethod]
        public void Serialize_DefaultIgnoreCondition_WhenWritingNull_SuppressesNulls()
        {
            // Build a DisplayOptions with a single TextStyle whose Foreground / Background
            // are explicitly null so we can verify they are skipped under WhenWritingNull.
            // Clear the defaulted Headers list so we only inspect what the test puts there.
            var options = new DisplayOptions { Headers = { } };
            options.Headers.Clear();
            options.Headers.Add(new TextStyle(TextDecoration.Bold, foreground: null, background: null));

            var json = options.Serialize(new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() },
            });

            using var doc = JsonDocument.Parse(json);
            var header = doc.RootElement.GetProperty("Headers")[0];
            Assert.IsFalse(header.TryGetProperty("Foreground", out _), $"Null Foreground should be omitted under WhenWritingNull: {json}");
            Assert.IsFalse(header.TryGetProperty("foreground", out _), $"Null Foreground should be omitted under WhenWritingNull: {json}");
            Assert.IsFalse(header.TryGetProperty("Background", out _), $"Null Background should be omitted under WhenWritingNull: {json}");
            Assert.IsFalse(header.TryGetProperty("background", out _), $"Null Background should be omitted under WhenWritingNull: {json}");
            // Decoration is a non-nullable enum, so it must still be emitted.
            Assert.IsTrue(header.TryGetProperty("Decoration", out _) || header.TryGetProperty("decoration", out _),
                $"Non-null Decoration should still be emitted: {json}");
        }

        [TestMethod]
        public void Serialize_DefaultIgnoreCondition_WhenWritingDefault_SuppressesDefaults()
        {
            // FigletTextStyle with null Justification, null FontPath, and a TextColor whose
            // Named is the default enum value — all three should be suppressed under
            // WhenWritingDefault.
            var options = new DisplayOptions { Headers = { } };
            options.Headers.Clear();
            options.Headers.Add(FigletTextStyle.Create(justification: null, foreground: TextColor.FromNamed(NamedColor.Default)));

            var json = options.Serialize(new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
                Converters = { new JsonStringEnumConverter() },
            });

            using var doc = JsonDocument.Parse(json);
            var header = doc.RootElement.GetProperty("Headers")[0];
            Assert.IsFalse(header.TryGetProperty("Justification", out _), $"Null Justification should be omitted under WhenWritingDefault: {json}");
            Assert.IsFalse(header.TryGetProperty("justification", out _), $"Null Justification should be omitted under WhenWritingDefault: {json}");
            Assert.IsFalse(header.TryGetProperty("FontPath", out _), $"Null FontPath should be omitted under WhenWritingDefault: {json}");
            Assert.IsFalse(header.TryGetProperty("fontPath", out _), $"Null FontPath should be omitted under WhenWritingDefault: {json}");

            // Foreground is non-null so it is emitted (the converter writes the literal
            // lowercase "foreground" when no naming policy is configured), but its inner
            // "named" property is the default enum value (0) and should itself be
            // suppressed.
            var foreground = header.GetProperty("foreground");
            Assert.IsFalse(foreground.TryGetProperty("Named", out _), $"Default NamedColor should be omitted under WhenWritingDefault: {json}");
            Assert.IsFalse(foreground.TryGetProperty("named", out _), $"Default NamedColor should be omitted under WhenWritingDefault: {json}");
        }

        [TestMethod]
        public void Serialize_DoesNotMutateCallerOptions()
        {
            var caller = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            var beforeConverters = caller.Converters.Count;
            var beforePolicy = caller.PropertyNamingPolicy;

            _ = new DisplayOptions().Serialize(caller);

            Assert.AreEqual(beforeConverters, caller.Converters.Count, "Caller's Converters list must not be mutated.");
            Assert.AreSame(beforePolicy, caller.PropertyNamingPolicy, "Caller's PropertyNamingPolicy must not be mutated.");
        }

        [TestMethod]
        public async Task Deserialize_PropertyNamingPolicy_DoesNotPreventReadingPascalOrCamelCase()
        {
            // The library's read side is intentionally lenient: regardless of the caller's
            // naming policy, both PascalCase and camelCase keys should round-trip.
            var pascalJson = $$"""
                {
                    "Headers": [
                        {
                            "$type": "{{nameof(TextStyle)}}",
                            "Decoration": "{{TextDecoration.Bold}}"
                        }
                    ]
                }
                """;
            var camelJson = $$"""
                {
                    "headers": [
                        {
                            "$type": "{{nameof(TextStyle)}}",
                            "decoration": "{{TextDecoration.Bold}}"
                        }
                    ]
                }
                """;

            var snakeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() },
            };

            var fromPascal = await DisplayOptions.DeserializeAsync(pascalJson, snakeOptions);
            var fromCamel = await DisplayOptions.DeserializeAsync(camelJson, snakeOptions);

            Assert.AreEqual(TextDecoration.Bold, ((TextStyle)fromPascal.Headers[0]).Decoration);
            Assert.AreEqual(TextDecoration.Bold, ((TextStyle)fromCamel.Headers[0]).Decoration);
        }
    }
}
