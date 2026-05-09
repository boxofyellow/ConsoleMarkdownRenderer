using System.Reflection;
using ConsoleMarkdownRenderer.Styling;
using Spectre.Console;

namespace ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="TextColor"/>, <see cref="TextStyle"/>, and <see cref="TextStyleExtensions"/>.
    /// </summary>
    [TestClass]
    public class StylingTests
    {
        #region Helper Methods

        /// <summary>
        /// Asserts that two TextStyles are equal and also validates their hash codes match.
        /// </summary>
        private static void AssertTextStylesEqual(TextStyle style1, TextStyle style2)
        {
            Assert.AreEqual(style1, style2, "Styles should be equal");
            Assert.AreEqual(style1.GetHashCode(), style2.GetHashCode(), "Equal styles must have equal hash codes");
        }

        /// <summary>
        /// Asserts that two TextColors are equal and also validates their hash codes match.
        /// </summary>
        private static void AssertTextColorsEqual(TextColor? color1, TextColor? color2)
        {
            Assert.AreEqual(color1, color2, "Colors should be equal");
            if (color1 is not null && color2 is not null)
            {
                Assert.AreEqual(color1.GetHashCode(), color2.GetHashCode(), "Equal colors must have equal hash codes");
            }
            else if (color1 is null != color2 is null)
            {
                // This is not really needed Assert.AreEqual is check this but just to be explicit
                Assert.Fail("Both colors should be null or both should be non-null");
            } 
        }

        #endregion

        #region TextColor Tests

        [TestMethod]
        public void TextColor_FromRgb_SetsPropertiesCorrectly()
        {
            var color = TextColor.FromRgb(100, 150, 200);

            Assert.IsTrue(color.IsRgb);
            Assert.AreEqual((byte)100, color.R);
            Assert.AreEqual((byte)150, color.G);
            Assert.AreEqual((byte)200, color.B);
        }

        [TestMethod]
        public void TextColor_NamedColor_SetsPropertiesCorrectly()
        {
            var color = TextColor.Red;

            Assert.IsFalse(color.IsRgb);
            Assert.AreEqual(NamedColor.Red, color.Named);
        }

        [TestMethod]
        public void TextColor_Equals_SameRgb_AreEqual() 
            => AssertTextColorsEqual(
                TextColor.FromRgb(10, 20, 30),
                TextColor.FromRgb(10, 20, 30));

        [TestMethod]
        public void TextColor_Equals_DifferentRgb_AreNotEqual()
        {
            var color1 = TextColor.FromRgb(10, 20, 30);
            var color2 = TextColor.FromRgb(10, 20, 31);

            Assert.AreNotEqual(color1, color2);
        }

        [TestMethod]
        public void TextColor_Equals_SameNamed_AreEqual() 
            => AssertTextColorsEqual(TextColor.Red, TextColor.Red);

        [TestMethod]
        public void TextColor_Equals_DifferentNamed_AreNotEqual()
            => Assert.AreNotEqual(TextColor.Red, TextColor.Blue);

        [TestMethod]
        public void TextColor_Equals_RgbAndNamed_AreNotEqual()
        {
            var rgb = TextColor.FromRgb(255, 0, 0);
            var named = TextColor.Red;

            Assert.AreNotEqual(rgb, named);
        }

        [TestMethod]
        public void TextColor_Equals_Null_ReturnsFalse() 
            => Assert.IsFalse(TextColor.Red.Equals(null));

        [TestMethod]
        public void TextColor_Equals_DifferentType_ReturnsFalse() 
            => Assert.IsFalse(TextColor.Red.Equals("Red"));

        [TestMethod]
        public void TextColor_ToString_Rgb_FormatsCorrectly()
        {
            var color = TextColor.FromRgb(10, 20, 30);
            Assert.AreEqual("rgb(10,20,30)", color.ToString());
        }

        [TestMethod]
        public void TextColor_ToString_Named_FormatsCorrectly()
        {
            Assert.AreEqual("Red", TextColor.Red.ToString());
            Assert.AreEqual("Default", TextColor.Default.ToString());
        }

        [TestMethod]
        public void TextColor_StaticProperties_ReturnExpectedNamedColors()
        {
            Assert.AreEqual(NamedColor.Black, TextColor.Black.Named);
            Assert.AreEqual(NamedColor.Red, TextColor.Red.Named);
            Assert.AreEqual(NamedColor.Green, TextColor.Green.Named);
            Assert.AreEqual(NamedColor.Yellow, TextColor.Yellow.Named);
            Assert.AreEqual(NamedColor.Blue, TextColor.Blue.Named);
            Assert.AreEqual(NamedColor.Purple, TextColor.Purple.Named);
            Assert.AreEqual(NamedColor.Default, TextColor.Default.Named);
        }

        #endregion

        #region TextStyle Tests

        [TestMethod]
        public void TextStyle_DefaultConstructor_CreatesPlainStyle()
        {
            var style = new TextStyle();

            Assert.AreEqual(TextDecoration.None, style.Decoration);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Background);
        }

        [TestMethod]
        public void TextStyle_Plain_IsPlainStyle()
        {
            var style = TextStyle.Plain;

            Assert.AreEqual(TextDecoration.None, style.Decoration);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Background);
        }

        [TestMethod]
        public void TextStyle_WithDecoration_SetsDecoration()
        {
            var style = new TextStyle(decoration: TextDecoration.Bold);

            Assert.AreEqual(TextDecoration.Bold, style.Decoration);
        }

        [TestMethod]
        public void TextStyle_WithColors_SetsColors()
        {
            var style = new TextStyle(foreground: TextColor.Red, background: TextColor.Blue);

            Assert.AreEqual(TextColor.Red, style.Foreground);
            Assert.AreEqual(TextColor.Blue, style.Background);
        }

        [TestMethod]
        [DataRow("bold red on blue", "bold red on blue",   true)]
        [DataRow("bold red on blue", "italic red on blue", false)]
        [DataRow("red on blue",      "blue on blue",       false)]
        [DataRow("red on blue",      "red on red",         false)]
        public void TextStyle_Equals_ReturnsExpected(string markup1, string markup2, bool expectedEqual)
        {
            TextStyle style1 = markup1;
            TextStyle style2 = markup2;

            if (expectedEqual)
            {
                AssertTextStylesEqual(style1, style2);
            }
            else
            {
                Assert.AreNotEqual(style1, style2);
            }
        }

        [TestMethod]
        public void TextStyle_Equals_Null_ReturnsFalse()
            => Assert.IsFalse(new TextStyle().Equals(null));

        [TestMethod]
        public void TextStyle_Equals_DifferentType_ReturnsFalse()
            => Assert.IsFalse(new TextStyle().Equals("plain"));

        [TestMethod]
        public void TextStyle_ToString_Plain_ReturnsPlain()
            => Assert.AreEqual("plain", new TextStyle().ToString());

        [TestMethod]
        public void TextStyle_ToString_WithDecoration_IncludesDecoration()
            => Assert.Contains("Bold", new TextStyle(decoration: TextDecoration.Bold).ToString());

        [TestMethod]
        public void TextStyle_ToString_WithColors_IncludesColors()
        {
            var style = new TextStyle(foreground: TextColor.Red, background: TextColor.Blue);
            var result = style.ToString();

            Assert.Contains("fg:Red", result);
            Assert.Contains("bg:Blue", result);
        }

        [TestMethod]
        [DataRow("bold",              TextDecoration.Bold,                         null,   null)]
        [DataRow("bold italic",       TextDecoration.Bold | TextDecoration.Italic, null,   null)]
        [DataRow("red",               TextDecoration.None,                         "red",  null)]
        [DataRow("red on blue",       TextDecoration.None,                         "red",  "blue")]
        [DataRow("bold red on green", TextDecoration.Bold,                         "red",  "green")]
        public void TextStyle_FromMarkup_ParsesCorrectly(string markup, TextDecoration expectedDecoration, string expectedFg, string expectedBg)
        {
            TextStyle style = markup;

            Assert.AreEqual(expectedDecoration, style.Decoration);

            if (expectedFg == null)
            {
                Assert.IsNull(style.Foreground);
            }
            else
            {
                TextStyle fgStyle = expectedFg;
                AssertTextColorsEqual(fgStyle.Foreground, style.Foreground);
            }

            if (expectedBg == null)
            {
                Assert.IsNull(style.Background);
            }
            else
            {
                // Parse background using "on X" syntax
                TextStyle bgStyle = $"red on {expectedBg}";
                AssertTextColorsEqual(bgStyle.Background, style.Background);
            }
        }

        [TestMethod]
        public void TextStyle_FromMarkup_AllDecorations()
        {
            // Use reflection to test all decoration values
            foreach (TextDecoration decoration in Enum.GetValues(typeof(TextDecoration)))
            {
                if (decoration == TextDecoration.None)
                {
                    continue;
                }

                var markup = decoration.ToString().ToLowerInvariant();
                TextStyle style = markup;
                Assert.AreEqual(decoration, style.Decoration, $"Decoration '{markup}' should map to {decoration}");
            }
        }

        [TestMethod]
        public void TextStyle_FromMarkup_AllNamedColors()
        {
            // Use reflection to test all named color values
            foreach (NamedColor namedColor in Enum.GetValues(typeof(NamedColor)))
            {
                var markup = namedColor.ToString().ToLowerInvariant();
                TextStyle style = markup;
                Assert.IsNotNull(style.Foreground, $"Foreground should be set for markup '{markup}'");
                Assert.AreEqual(namedColor, style.Foreground.Named, $"Color '{markup}' should map to {namedColor}");
            }
        }

        [TestMethod]
        public void TextStyle_ImplicitConversion_FromString()
        {
            TextStyle style = "bold red on blue";

            Assert.AreEqual(TextDecoration.Bold, style.Decoration);
            Assert.AreEqual(TextColor.Red, style.Foreground);
            Assert.AreEqual(TextColor.Blue, style.Background);
        }

        #endregion

        #region TextStyleExtensions Tests

        [TestMethod]
        public void ToSpectreStyle_PlainStyle_ReturnsDefaultSpectreStyle()
        {
            var textStyle = TextStyle.Plain;
            var spectreStyle = textStyle.ToSpectreStyle();

            Assert.AreEqual(Decoration.None, spectreStyle.Decoration);
            Assert.AreEqual(Color.Default, spectreStyle.Foreground);
            Assert.AreEqual(Color.Default, spectreStyle.Background);
        }

        [TestMethod]
        public void ToSpectreStyle_BoldDecoration_MapsToBold()
        {
            var textStyle = new TextStyle(decoration: TextDecoration.Bold);
            var spectreStyle = textStyle.ToSpectreStyle();

            Assert.AreEqual(Decoration.Bold, spectreStyle.Decoration);
        }

        [TestMethod]
        public void ToSpectreStyle_CombinedDecorations_MapsAllFlags()
        {
            TextDecoration decorations = default;
            // Use reflection to test all named color values
            foreach (TextDecoration decoration in Enum.GetValues(typeof(TextDecoration)))
            {
                decorations |= decoration;
            }

            var textStyle = new TextStyle(decoration: decorations);
            var spectreStyle = textStyle.ToSpectreStyle();

            foreach (Decoration decoration in Enum.GetValues(typeof(Decoration)))
            {
                if (decoration == Decoration.None)
                {
                    continue;
                }
                Assert.IsTrue(spectreStyle.Decoration.HasFlag(decoration));
            }
        }

        [TestMethod]
        public void ToSpectreStyle_AllDecorations_MapCorrectly()
        {
            // Use reflection to test all decoration values
            foreach (TextDecoration textDecoration in Enum.GetValues(typeof(TextDecoration)))
            {
                if (textDecoration == TextDecoration.None)
                {
                    continue;
                }

                var textStyle = new TextStyle(decoration: textDecoration);
                var spectreStyle = textStyle.ToSpectreStyle();

                // Get the corresponding Spectre.Console.Decoration by parsing the enum value name
                var expectedSpectreDecoration = Enum.Parse<Decoration>(textDecoration.ToString());

                Assert.AreEqual(expectedSpectreDecoration, spectreStyle.Decoration, $"TextDecoration.{textDecoration} should map to Decoration.{textDecoration}");
            }
        }

        [TestMethod]
        public void ToSpectreStyle_NamedForeground_MapsToSpectreColor()
        {
            var textStyle = new TextStyle(foreground: TextColor.Red);
            var spectreStyle = textStyle.ToSpectreStyle();

            Assert.AreEqual(Color.Red, spectreStyle.Foreground);
        }

        [TestMethod]
        public void ToSpectreStyle_NamedBackground_MapsToSpectreColor()
        {
            var textStyle = new TextStyle(background: TextColor.Green);
            var spectreStyle = textStyle.ToSpectreStyle();

            Assert.AreEqual(Color.Green, spectreStyle.Background);
        }

        [TestMethod]
        public void ToSpectreStyle_AllNamedColors_MapCorrectly()
        {
            // Use reflection to test all named color values
            foreach (NamedColor namedColor in Enum.GetValues(typeof(NamedColor)))
            {
                // Get the TextColor static property for this named color
                var textColorProp = typeof(TextColor).GetProperty(namedColor.ToString(), BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(textColorProp, $"TextColor should have static property '{namedColor}'");
                var textColor = (TextColor)textColorProp.GetValue(null)!;

                var textStyle = new TextStyle(foreground: textColor);
                var spectreColor = textStyle.ToSpectreStyle().Foreground;

                // Get the corresponding Spectre.Console.Color using reflection
                var spectreColorProp = typeof(Color).GetProperty(namedColor.ToString(), BindingFlags.Public | BindingFlags.Static);
                Assert.IsNotNull(spectreColorProp, $"Spectre.Console.Color should have property '{namedColor}'");
                var expectedSpectreColor = (Color)spectreColorProp.GetValue(null)!;

                Assert.AreEqual(expectedSpectreColor, spectreColor, $"TextColor.{namedColor} should map to Color.{namedColor}");
            }
        }

        [TestMethod]
        public void ToSpectreStyle_RgbColor_MapsToSpectreRgb()
        {
            var textStyle = new TextStyle(foreground: TextColor.FromRgb(100, 150, 200));
            var spectreStyle = textStyle.ToSpectreStyle();

            Assert.AreEqual(new Color(100, 150, 200), spectreStyle.Foreground);
        }

        [TestMethod]
        public void ToSpectreStyle_FullStyle_MapsAllComponents()
        {
            var textStyle = new TextStyle(
                decoration: TextDecoration.Bold | TextDecoration.Italic,
                foreground: TextColor.Red,
                background: TextColor.FromRgb(0, 0, 255));
            var spectreStyle = textStyle.ToSpectreStyle();

            Assert.IsTrue(spectreStyle.Decoration.HasFlag(Decoration.Bold));
            Assert.IsTrue(spectreStyle.Decoration.HasFlag(Decoration.Italic));
            Assert.AreEqual(Color.Red, spectreStyle.Foreground);
            Assert.AreEqual(new Color(0, 0, 255), spectreStyle.Background);
        }

        #endregion
    }
}
