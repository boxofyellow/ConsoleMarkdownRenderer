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
        public void TextColor_Equals_SameRgb_ReturnsTrue()
        {
            var color1 = TextColor.FromRgb(10, 20, 30);
            var color2 = TextColor.FromRgb(10, 20, 30);

            Assert.AreEqual(color1, color2);
        }

        [TestMethod]
        public void TextColor_Equals_DifferentRgb_ReturnsFalse()
        {
            var color1 = TextColor.FromRgb(10, 20, 30);
            var color2 = TextColor.FromRgb(10, 20, 31);

            Assert.AreNotEqual(color1, color2);
        }

        [TestMethod]
        public void TextColor_Equals_SameNamed_ReturnsTrue()
        {
            Assert.AreEqual(TextColor.Red, TextColor.Red);
        }

        [TestMethod]
        public void TextColor_Equals_DifferentNamed_ReturnsFalse()
        {
            Assert.AreNotEqual(TextColor.Red, TextColor.Blue);
        }

        [TestMethod]
        public void TextColor_Equals_RgbVsNamed_ReturnsFalse()
        {
            var rgb = TextColor.FromRgb(255, 0, 0);
            var named = TextColor.Red;

            Assert.AreNotEqual(rgb, named);
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
        public void TextColor_GetHashCode_SameRgb_AreEqual()
        {
            var color1 = TextColor.FromRgb(10, 20, 30);
            var color2 = TextColor.FromRgb(10, 20, 30);

            Assert.AreEqual(color1.GetHashCode(), color2.GetHashCode());
        }

        [TestMethod]
        public void TextColor_GetHashCode_SameNamed_AreEqual()
        {
            Assert.AreEqual(TextColor.Green.GetHashCode(), TextColor.Green.GetHashCode());
        }

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
        public void TextStyle_Equals_SameValues_ReturnsTrue()
        {
            var style1 = new TextStyle(TextDecoration.Bold, TextColor.Red, TextColor.Blue);
            var style2 = new TextStyle(TextDecoration.Bold, TextColor.Red, TextColor.Blue);

            Assert.AreEqual(style1, style2);
        }

        [TestMethod]
        public void TextStyle_Equals_DifferentDecoration_ReturnsFalse()
        {
            var style1 = new TextStyle(TextDecoration.Bold);
            var style2 = new TextStyle(TextDecoration.Italic);

            Assert.AreNotEqual(style1, style2);
        }

        [TestMethod]
        public void TextStyle_Equals_DifferentForeground_ReturnsFalse()
        {
            var style1 = new TextStyle(foreground: TextColor.Red);
            var style2 = new TextStyle(foreground: TextColor.Blue);

            Assert.AreNotEqual(style1, style2);
        }

        [TestMethod]
        public void TextStyle_Equals_DifferentBackground_ReturnsFalse()
        {
            var style1 = new TextStyle(background: TextColor.Red);
            var style2 = new TextStyle(background: TextColor.Blue);

            Assert.AreNotEqual(style1, style2);
        }

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
        public void TextStyle_GetHashCode_SameValues_AreEqual()
        {
            var style1 = new TextStyle(TextDecoration.Bold, TextColor.Red);
            var style2 = new TextStyle(TextDecoration.Bold, TextColor.Red);

            Assert.AreEqual(style1.GetHashCode(), style2.GetHashCode());
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
        public void TextStyle_FromMarkup_AllDecorations()
        {
            TextStyle style = "dim";
            Assert.AreEqual(TextDecoration.Dim, style.Decoration);

            style = "underline";
            Assert.AreEqual(TextDecoration.Underline, style.Decoration);

            style = "strikethrough";
            Assert.AreEqual(TextDecoration.Strikethrough, style.Decoration);

            style = "invert";
            Assert.AreEqual(TextDecoration.Invert, style.Decoration);

            style = "conceal";
            Assert.AreEqual(TextDecoration.Conceal, style.Decoration);

            style = "slowblink";
            Assert.AreEqual(TextDecoration.SlowBlink, style.Decoration);

            style = "rapidblink";
            Assert.AreEqual(TextDecoration.RapidBlink, style.Decoration);
        }

        [TestMethod]
        public void TextStyle_FromMarkup_AllNamedColors()
        {
            TextStyle style = "black";
            Assert.AreEqual(TextColor.Black, style.Foreground);

            style = "green";
            Assert.AreEqual(TextColor.Green, style.Foreground);

            style = "yellow";
            Assert.AreEqual(TextColor.Yellow, style.Foreground);

            style = "purple";
            Assert.AreEqual(TextColor.Purple, style.Foreground);

            style = "default";
            Assert.AreEqual(TextColor.Default, style.Foreground);
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
        public void ToSpectreStyle_AllDecorations_MapCorrectly()
        {
            var textStyle = new TextStyle(decoration: TextDecoration.Dim);
            Assert.AreEqual(Decoration.Dim, textStyle.ToSpectreStyle().Decoration);

            textStyle = new TextStyle(decoration: TextDecoration.SlowBlink);
            Assert.AreEqual(Decoration.SlowBlink, textStyle.ToSpectreStyle().Decoration);

            textStyle = new TextStyle(decoration: TextDecoration.RapidBlink);
            Assert.AreEqual(Decoration.RapidBlink, textStyle.ToSpectreStyle().Decoration);

            textStyle = new TextStyle(decoration: TextDecoration.Invert);
            Assert.AreEqual(Decoration.Invert, textStyle.ToSpectreStyle().Decoration);

            textStyle = new TextStyle(decoration: TextDecoration.Conceal);
            Assert.AreEqual(Decoration.Conceal, textStyle.ToSpectreStyle().Decoration);

            textStyle = new TextStyle(decoration: TextDecoration.Strikethrough);
            Assert.AreEqual(Decoration.Strikethrough, textStyle.ToSpectreStyle().Decoration);
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
            Assert.AreEqual(Color.Black, new TextStyle(foreground: TextColor.Black).ToSpectreStyle().Foreground);
            Assert.AreEqual(Color.Red, new TextStyle(foreground: TextColor.Red).ToSpectreStyle().Foreground);
            Assert.AreEqual(Color.Green, new TextStyle(foreground: TextColor.Green).ToSpectreStyle().Foreground);
            Assert.AreEqual(Color.Yellow, new TextStyle(foreground: TextColor.Yellow).ToSpectreStyle().Foreground);
            Assert.AreEqual(Color.Blue, new TextStyle(foreground: TextColor.Blue).ToSpectreStyle().Foreground);
            Assert.AreEqual(Color.Purple, new TextStyle(foreground: TextColor.Purple).ToSpectreStyle().Foreground);
            Assert.AreEqual(Color.Default, new TextStyle(foreground: TextColor.Default).ToSpectreStyle().Foreground);
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
