using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    [TestClass]
    public class TextColorTest : TestBase
    {
        [TestMethod]
        [DataRow(false, false, null          , 0  , 0 , 0 , null)]
        [DataRow(false, false, null          , 0  , 0 , 0 , "")]
        [DataRow(false, false, null          , 0  , 0 , 0 , "NotAColor")]
        [DataRow(true , true , null          , 0  , 0 , 0 , "default")]
        [DataRow(true , false, NamedColor.Red, 0  , 0 , 0 , "Red")]
        [DataRow(true , false, null          , 255, 0 , 0 , "rgb(255,0,0)")]
        [DataRow(true , false, null          , 0  , 0 , 0 , "#000000")]
        [DataRow(true , false, null          , 1  , 0 , 0 , "#010000")]
        [DataRow(true , false, null          , 0  , 1 , 0 , "#000100")]
        [DataRow(true , false, null          , 0  , 0 , 1 , "#000001")]
        [DataRow(true , false, null          , 17 , 0 , 0 , "#100")]  // hex shorthand for #110000
        [DataRow(true , false, null          , 0  , 17, 0 , "#010")]
        [DataRow(true , false, null          , 0  , 0 , 17, "#001")]
        [DataRow(true , false, null          , 0  , 0 , 0 , "rgb(0,0,0)")]
        [DataRow(true , false, null          , 1  , 0 , 0 , "rgb(1,0,0)")]
        [DataRow(true , false, null          , 0  , 1 , 0 , "rgb(0,1,0)")]
        [DataRow(true , false, null          , 0  , 0 , 1 , "rgb(0,0,1)")]
        public void Validate_TryParseColor(bool isAColor, bool isDefault, NamedColor? name, int r, int g, int b, string? text)
        {
            TextColor color = TextColor.FromRgb((byte)r, (byte)g, (byte)b);
            if (isDefault)
            {
                color = TextColor.Default;
            }
            else if (name.HasValue)
            {
                color = TextColor.FromNamed(name.Value);
            }

            var result = TextColor.TryParseColor(text, out var parsedColor);
            Assert.AreEqual(isAColor, result, $"Expected {isAColor} for '{text}' but got {result}");
            if (isAColor)
            {
                Assert.IsNotNull(parsedColor);
                TestUtilities.AssertTheseMatch(color, parsedColor, shouldMatch: true, $"Expected {color} for '{text}' but got {parsedColor}");
            }
            else
            {
                Assert.IsNull(parsedColor);
            }

            var upperText = text?.ToUpperInvariant();
            result = TextColor.TryParseColor(upperText, out parsedColor);
            Assert.AreEqual(isAColor, result, $"Expected {isAColor} for '{upperText}' but got {result}");
            if (isAColor)
            {
                Assert.IsNotNull(parsedColor);
                TestUtilities.AssertTheseMatch(color, parsedColor, shouldMatch: true, $"Expected {color} for '{upperText}' but got {parsedColor}");
            }
            else
            {
                Assert.IsNull(parsedColor);
            }

            if (isAColor)
            {
                Assert.IsNotNull(parsedColor);
                var spectreColor = parsedColor.ToSpectreColor();
                if (Utilities.TryParseColor(text, out var fromTextColor))
                {
                    TestUtilities.AssertTheseMatch(spectreColor, fromTextColor, shouldMatch: true, $"Expected {spectreColor} for '{text}' but got {fromTextColor}");
                }
                else
                {
                    Assert.Fail($"Utilities.TryParseColor failed for '{text}'");
                }

                if (TextColor.TryParseColor(parsedColor.ToString(), out var fromTextColor2))
                {
                    TestUtilities.AssertTheseMatch(color, fromTextColor2, shouldMatch: true, $"Expected {color} for '{parsedColor}' but got {fromTextColor2}");
                }
                else
                {
                    Assert.Fail($"TextColor.TryParseColor failed for '{parsedColor}'");
                }
            }
        }
    }
}