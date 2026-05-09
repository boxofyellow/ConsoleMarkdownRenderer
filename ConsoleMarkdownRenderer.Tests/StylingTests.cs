using System;
using ConsoleMarkdownRenderer.Styling;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        /// Parses a string into a TextColor. Supports formats:
        /// - "rgb:R,G,B" for RGB colors (e.g., "rgb:10,20,30")
        /// - "named:ColorName" for named colors (e.g., "named:Red")
        /// </summary>
        private static TextColor ParseTextColor(string colorSpec)
        {
            if (colorSpec.StartsWith("rgb:"))
            {
                var parts = colorSpec[4..].Split(',');
                return TextColor.FromRgb(
                    byte.Parse(parts[0]),
                    byte.Parse(parts[1]),
                    byte.Parse(parts[2]));
            }
            else if (colorSpec.StartsWith("named:"))
            {
                var namedColor = Enum.Parse<NamedColor>(colorSpec[6..]);
                return namedColor switch
                {
                    NamedColor.Black   => TextColor.Black,
                    NamedColor.Red     => TextColor.Red,
                    NamedColor.Green   => TextColor.Green,
                    NamedColor.Yellow  => TextColor.Yellow,
                    NamedColor.Blue    => TextColor.Blue,
                    NamedColor.Purple  => TextColor.Purple,
                    NamedColor.Default => TextColor.Default,
                    _ => throw new ArgumentException($"Unknown named color: {colorSpec}")
                };
            }
            throw new ArgumentException($"Invalid color spec: {colorSpec}");
        }

        /// <summary>
        /// Asserts that two TextColors are equal and also validates their hash codes match.
        /// </summary>
        private static void AssertTextColorsEqual(TextColor color1, TextColor color2)
        {
            Assert.AreEqual(color1, color2, "Colors should be equal");
            Assert.AreEqual(color1.GetHashCode(), color2.GetHashCode(), "Equal colors must have equal hash codes");
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
        [DataRow("rgb:10,20,30",  "rgb:10,20,30",  true)]
        [DataRow("rgb:10,20,30",  "rgb:10,20,31",  false)]
        [DataRow("named:Red",     "named:Red",     true)]
        [DataRow("named:Red",     "named:Blue",    false)]
        [DataRow("rgb:255,0,0",   "named:Red",     false)]
        public void TextColor_Equals_ReturnsExpected(string color1Spec, string color2Spec, bool expectedEqual)
        {
            var color1 = ParseTextColor(color1Spec);
            var color2 = ParseTextColor(color2Spec);

            if (expectedEqual)
            {
                AssertTextColorsEqual(color1, color2);
            }
            else
            {
                Assert.AreNotEqual(color1, color2);
            }
        }

        [TestMethod]
        public void TextColor_Equals_Null_ReturnsFalse()
        {
            Assert.IsFalse(TextColor.Red.Equals(null));
        }

        [TestMethod]
        public void TextColor_Equals_DifferentType_ReturnsFalse()
        {
            Assert.IsFalse(TextColor.Red.Equals("Red"));
        }

        [TestMethod]
        [DataRow("rgb:10,20,30",  "rgb(10,20,30)")]
        [DataRow("named:Red",     "Red")]
        [DataRow("named:Default", "Default")]
        public void TextColor_ToString_FormatsCorrectly(string colorSpec, string expected)
        {
            var color = ParseTextColor(colorSpec);
            Assert.AreEqual(expected, color.ToString());
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
        [DataRow(TextDecoration.Bold,   NamedColor.Red,  NamedColor.Blue, TextDecoration.Bold,   NamedColor.Red,  NamedColor.Blue, true)]
        [DataRow(TextDecoration.Bold,   NamedColor.Red,  NamedColor.Blue, TextDecoration.Italic, NamedColor.Red,  NamedColor.Blue, false)]
        [DataRow(TextDecoration.None,   NamedColor.Red,  NamedColor.Blue, TextDecoration.None,   NamedColor.Blue, NamedColor.Blue, false)]
        [DataRow(TextDecoration.None,   NamedColor.Red,  NamedColor.Blue, TextDecoration.None,   NamedColor.Red,  NamedColor.Red,  false)]
        public void TextStyle_Equals_ReturnsExpected(
            TextDecoration decoration1, NamedColor fg1, NamedColor bg1,
            TextDecoration decoration2, NamedColor fg2, NamedColor bg2,
            bool expectedEqual)
        {
            var style1 = new TextStyle(decoration1, GetNamedTextColor(fg1), GetNamedTextColor(bg1));
            var style2 = new TextStyle(decoration2, GetNamedTextColor(fg2), GetNamedTextColor(bg2));

            if (expectedEqual)
            {
                Assert.AreEqual(style1, style2, "Styles should be equal");
                Assert.AreEqual(style1.GetHashCode(), style2.GetHashCode(), "Equal styles must have equal hash codes");
            }
            else
            {
                Assert.AreNotEqual(style1, style2);
            }
        }

        /// <summary>
        /// Returns the static TextColor property for a given NamedColor.
        /// </summary>
        private static TextColor GetNamedTextColor(NamedColor named) => named switch
        {
            NamedColor.Black   => TextColor.Black,
            NamedColor.Red     => TextColor.Red,
            NamedColor.Green   => TextColor.Green,
            NamedColor.Yellow  => TextColor.Yellow,
            NamedColor.Blue    => TextColor.Blue,
            NamedColor.Purple  => TextColor.Purple,
            NamedColor.Default => TextColor.Default,
            _ => throw new ArgumentException($"Unknown named color: {named}")
        };

        [TestMethod]
        public void TextStyle_Equals_Null_ReturnsFalse()
        {
            Assert.IsFalse(new TextStyle().Equals(null));
        }

        [TestMethod]
        public void TextStyle_Equals_DifferentType_ReturnsFalse()
        {
            Assert.IsFalse(new TextStyle().Equals("plain"));
        }

        [TestMethod]
        public void TextStyle_ToString_Plain_ReturnsPlain()
        {
            Assert.AreEqual("plain", new TextStyle().ToString());
        }

        [TestMethod]
        public void TextStyle_ToString_WithDecoration_IncludesDecoration()
        {
            var style = new TextStyle(decoration: TextDecoration.Bold);

            Assert.IsTrue(style.ToString().Contains("Bold"));
        }

        [TestMethod]
        public void TextStyle_ToString_WithColors_IncludesColors()
        {
            var style = new TextStyle(foreground: TextColor.Red, background: TextColor.Blue);
            var result = style.ToString();

            Assert.IsTrue(result.Contains("fg:Red"));
            Assert.IsTrue(result.Contains("bg:Blue"));
        }

        [TestMethod]
        public void TextStyle_FromMarkup_SingleDecoration()
        {
            TextStyle style = "bold";

            Assert.AreEqual(TextDecoration.Bold, style.Decoration);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Background);
        }

        [TestMethod]
        public void TextStyle_FromMarkup_MultipleDecorations()
        {
            TextStyle style = "bold italic";

            Assert.IsTrue(style.Decoration.HasFlag(TextDecoration.Bold));
            Assert.IsTrue(style.Decoration.HasFlag(TextDecoration.Italic));
        }

        [TestMethod]
        public void TextStyle_FromMarkup_ForegroundColor()
        {
            TextStyle style = "red";

            Assert.AreEqual(TextColor.Red, style.Foreground);
            Assert.IsNull(style.Background);
        }

        [TestMethod]
        public void TextStyle_FromMarkup_ForegroundOnBackground()
        {
            TextStyle style = "red on blue";

            Assert.AreEqual(TextColor.Red, style.Foreground);
            Assert.AreEqual(TextColor.Blue, style.Background);
        }

        [TestMethod]
        public void TextStyle_FromMarkup_DecorationAndColors()
        {
            TextStyle style = "bold red on green";

            Assert.AreEqual(TextDecoration.Bold, style.Decoration);
            Assert.AreEqual(TextColor.Red, style.Foreground);
            Assert.AreEqual(TextColor.Green, style.Background);
        }

        [TestMethod]
        [DataRow("dim",           TextDecoration.Dim)]
        [DataRow("underline",     TextDecoration.Underline)]
        [DataRow("strikethrough", TextDecoration.Strikethrough)]
        [DataRow("invert",        TextDecoration.Invert)]
        [DataRow("conceal",       TextDecoration.Conceal)]
        [DataRow("slowblink",     TextDecoration.SlowBlink)]
        [DataRow("rapidblink",    TextDecoration.RapidBlink)]
        public void TextStyle_FromMarkup_AllDecorations(string markup, TextDecoration expectedDecoration)
        {
            TextStyle style = markup;
            Assert.AreEqual(expectedDecoration, style.Decoration);
        }

        [TestMethod]
        [DataRow("black",   NamedColor.Black)]
        [DataRow("green",   NamedColor.Green)]
        [DataRow("yellow",  NamedColor.Yellow)]
        [DataRow("purple",  NamedColor.Purple)]
        [DataRow("default", NamedColor.Default)]
        public void TextStyle_FromMarkup_AllNamedColors(string markup, NamedColor expectedNamedColor)
        {
            TextStyle style = markup;
            Assert.AreEqual(expectedNamedColor, style.Foreground?.Named);
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
            var textStyle = new TextStyle(decoration: TextDecoration.Bold | TextDecoration.Italic | TextDecoration.Underline);
            var spectreStyle = textStyle.ToSpectreStyle();

            Assert.IsTrue(spectreStyle.Decoration.HasFlag(Decoration.Bold));
            Assert.IsTrue(spectreStyle.Decoration.HasFlag(Decoration.Italic));
            Assert.IsTrue(spectreStyle.Decoration.HasFlag(Decoration.Underline));
        }

        [TestMethod]
        [DataRow(TextDecoration.Dim,           Decoration.Dim)]
        [DataRow(TextDecoration.SlowBlink,     Decoration.SlowBlink)]
        [DataRow(TextDecoration.RapidBlink,    Decoration.RapidBlink)]
        [DataRow(TextDecoration.Invert,        Decoration.Invert)]
        [DataRow(TextDecoration.Conceal,       Decoration.Conceal)]
        [DataRow(TextDecoration.Strikethrough, Decoration.Strikethrough)]
        public void ToSpectreStyle_AllDecorations_MapCorrectly(TextDecoration textDecoration, Decoration expectedSpectreDecoration)
        {
            var textStyle = new TextStyle(decoration: textDecoration);
            Assert.AreEqual(expectedSpectreDecoration, textStyle.ToSpectreStyle().Decoration);
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
        [DataRow(NamedColor.Black,   "black")]
        [DataRow(NamedColor.Red,     "red")]
        [DataRow(NamedColor.Green,   "green")]
        [DataRow(NamedColor.Yellow,  "yellow")]
        [DataRow(NamedColor.Blue,    "blue")]
        [DataRow(NamedColor.Purple,  "purple")]
        [DataRow(NamedColor.Default, "default")]
        public void ToSpectreStyle_AllNamedColors_MapCorrectly(NamedColor namedColor, string expectedSpectreColorName)
        {
            var textStyle = new TextStyle(foreground: GetNamedTextColor(namedColor));
            var spectreColor = textStyle.ToSpectreStyle().Foreground;
            // Compare by name since Spectre.Console.Color is a struct and can't be used as DataRow parameter
            Assert.AreEqual(expectedSpectreColorName, spectreColor.ToString());
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
