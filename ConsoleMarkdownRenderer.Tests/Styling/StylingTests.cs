using System.Reflection;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

[TestClass]
public class StylingTests : TestBase
{
    [TestMethod]
    public void TextColor_FromRgb_SetsPropertiesCorrectly()
    {
        var color = TextColor.FromRgb(100, 150, 200);

        Assert.IsTrue(color.IsRgb);
        TestUtilities.AssertTheseMatch((byte)100, color.R, shouldMatch: true);
        TestUtilities.AssertTheseMatch((byte)150, color.G, shouldMatch: true);
        TestUtilities.AssertTheseMatch((byte)200, color.B, shouldMatch: true);
    }

    [TestMethod]
    public void TextColor_NamedColor_SetsPropertiesCorrectly()
    {
        var color = TextColor.Red;

        Assert.IsFalse(color.IsRgb);
        TestUtilities.AssertTheseMatch(NamedColor.Red, color.Named, shouldMatch: true);
    }

    [TestMethod]
    public void TextColor_Equals_SameRgb_AreEqual() 
        => TestUtilities.AssertTheseMatch(
            TextColor.FromRgb(10, 20, 30),
            TextColor.FromRgb(10, 20, 30),
            shouldMatch: true);

    [TestMethod]
    public void TextColor_Equals_DifferentRgb_AreNotEqual()
    {
        var color1 = TextColor.FromRgb(10, 20, 30);
        var color2 = TextColor.FromRgb(10, 20, 31);

        TestUtilities.AssertTheseMatch(color1, color2, shouldMatch: false);
    }

    [TestMethod]
    public void TextColor_Equals_SameNamed_AreEqual() 
        => TestUtilities.AssertTheseMatch(TextColor.Red, TextColor.Red, shouldMatch: true);

    [TestMethod]
    public void TextColor_Equals_DifferentNamed_AreNotEqual()
        => TestUtilities.AssertTheseMatch(TextColor.Red, TextColor.Blue, shouldMatch: false);

    [TestMethod]
    public void TextColor_Equals_RgbAndNamed_AreNotEqual()
    {
        var rgb = TextColor.FromRgb(255, 0, 0);
        var named = TextColor.Red;

        TestUtilities.AssertTheseMatch(rgb, named, shouldMatch: false);
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
        TestUtilities.AssertTheseMatch("rgb(10,20,30)", color.ToString(), shouldMatch: true);
    }

    [TestMethod]
    public void TextColor_ToString_Named_FormatsCorrectly()
    {
        TestUtilities.AssertTheseMatch("Red", TextColor.Red.ToString(), shouldMatch: true);
        TestUtilities.AssertTheseMatch("Default", TextColor.Default.ToString(), shouldMatch: true);
    }

    [TestMethod]
    public void TextColor_StaticProperties_ReturnExpectedNamedColors()
    {
        TestUtilities.AssertTheseMatch(NamedColor.Black, TextColor.Black.Named, shouldMatch: true);
        TestUtilities.AssertTheseMatch(NamedColor.Red, TextColor.Red.Named, shouldMatch: true);
        TestUtilities.AssertTheseMatch(NamedColor.Green, TextColor.Green.Named, shouldMatch: true);
        TestUtilities.AssertTheseMatch(NamedColor.Yellow, TextColor.Yellow.Named, shouldMatch: true);
        TestUtilities.AssertTheseMatch(NamedColor.Blue, TextColor.Blue.Named, shouldMatch: true);
        TestUtilities.AssertTheseMatch(NamedColor.Purple, TextColor.Purple.Named, shouldMatch: true);
        TestUtilities.AssertTheseMatch(NamedColor.Default, TextColor.Default.Named, shouldMatch: true);
    }

    [TestMethod]
    public void TextStyle_DefaultConstructor_CreatesPlainStyle()
    {
        var style = new TextStyle();

        TestUtilities.AssertTheseMatch(TextDecoration.None, style.Decoration, shouldMatch: true);
        Assert.IsNull(style.Foreground);
        Assert.IsNull(style.Background);
    }

    [TestMethod]
    public void TextStyle_Plain_IsPlainStyle()
    {
        var style = TextStyle.Plain;

        TestUtilities.AssertTheseMatch(TextDecoration.None, style.Decoration, shouldMatch: true);
        Assert.IsNull(style.Foreground);
        Assert.IsNull(style.Background);
    }

    [TestMethod]
    public void TextStyle_Equals_Null_ReturnsFalse()
        => Assert.IsFalse(new TextStyle().Equals(null));

    [TestMethod]
    public void TextStyle_Equals_DifferentType_ReturnsFalse()
        => Assert.IsFalse(new TextStyle().Equals("plain"));

    [TestMethod]
    public void TextStyle_ToString_Plain_ReturnsPlain()
        => TestUtilities.AssertTheseMatch("plain", new TextStyle().ToString(), shouldMatch: true);

    [TestMethod]
    public void TextStyle_ToString_WithDecoration_IncludesDecoration()
        => TestUtilities.AssertTheseMatch("Bold", new TextStyle(decoration: TextDecoration.Bold).ToString(), shouldMatch: true);

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

        TestUtilities.AssertTheseMatch(expectedDecoration, style.Decoration, shouldMatch: true);

        if (expectedFg == null)
        {
            Assert.IsNull(style.Foreground);
        }
        else
        {
            TextStyle fgStyle = expectedFg;
            TestUtilities.AssertTheseMatch(fgStyle.Foreground, style.Foreground, shouldMatch: true);
        }

        if (expectedBg == null)
        {
            Assert.IsNull(style.Background);
        }
        else
        {
            // Parse background using "on X" syntax
            TextStyle bgStyle = $"red on {expectedBg}";
            TestUtilities.AssertTheseMatch(bgStyle.Background, style.Background, shouldMatch: true);
        }
    }

    [TestMethod]
    public void TextStyle_FromMarkup_AllDecorations()
    {
        // Use reflection to test all decoration values
        foreach (TextDecoration decoration in Enum.GetValues<TextDecoration>())
        {
            if (decoration == TextDecoration.None)
            {
                continue;
            }

            var markup = decoration.ToString().ToLowerInvariant();
            TextStyle style = markup;
            TestUtilities.AssertTheseMatch(decoration, style.Decoration, shouldMatch: true, message: $"Decoration '{markup}' should map to {decoration}");
        }
    }

    [TestMethod]
    public void TextStyle_FromMarkup_AllNamedColors()
    {
        // Use reflection to test all named color values
        foreach (NamedColor namedColor in Enum.GetValues<NamedColor>())
        {
            var markup = namedColor.ToString().ToLowerInvariant();
            TextStyle style = markup;
            Assert.IsNotNull(style.Foreground, $"Foreground should be set for markup '{markup}'");
            TestUtilities.AssertTheseMatch(namedColor, style.Foreground.Named, shouldMatch: true, $"Color '{markup}' should map to {namedColor}");
        }
    }

    [TestMethod]
    public void TextStyle_ImplicitConversion_FromString()
    {
        TextStyle style = "bold red on blue";

        TestUtilities.AssertTheseMatch(TextDecoration.Bold, style.Decoration, shouldMatch: true);
        TestUtilities.AssertTheseMatch(TextColor.Red, style.Foreground, shouldMatch: true);
        TestUtilities.AssertTheseMatch(TextColor.Blue, style.Background, shouldMatch: true);
    }

    [TestMethod]
    public void ToSpectreStyle_PlainStyle_ReturnsDefaultSpectreStyle()
    {
        var textStyle = TextStyle.Plain;
        var spectreStyle = textStyle.ToSpectreStyle();

        TestUtilities.AssertTheseMatch(Decoration.None, spectreStyle.Decoration, shouldMatch: true);
        TestUtilities.AssertTheseMatch(Color.Default, spectreStyle.Foreground, shouldMatch: true);
        TestUtilities.AssertTheseMatch(Color.Default, spectreStyle.Background, shouldMatch: true);
    }

    [TestMethod]
    public void ToSpectreStyle_BoldDecoration_MapsToBold()
    {
        var textStyle = new TextStyle(decoration: TextDecoration.Bold);
        var spectreStyle = textStyle.ToSpectreStyle();

        TestUtilities.AssertTheseMatch(Decoration.Bold, spectreStyle.Decoration, shouldMatch: true);
    }

    [TestMethod]
    public void ToSpectreStyle_CombinedDecorations_MapsAllFlags()
    {
        TextDecoration decorations = default;
        // Use reflection to test all named color values
        foreach (TextDecoration decoration in Enum.GetValues<TextDecoration>())
        {
            decorations |= decoration;
        }

        var textStyle = new TextStyle(decoration: decorations);
        var spectreStyle = textStyle.ToSpectreStyle();

        foreach (Decoration decoration in Enum.GetValues<Decoration>())
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
        foreach (TextDecoration textDecoration in Enum.GetValues<TextDecoration>())
        {
            if (textDecoration == TextDecoration.None)
            {
                continue;
            }

            var textStyle = new TextStyle(decoration: textDecoration);
            var spectreStyle = textStyle.ToSpectreStyle();

            // Get the corresponding Spectre.Console.Decoration by parsing the enum value name
            var expectedSpectreDecoration = Enum.Parse<Decoration>(textDecoration.ToString());

            TestUtilities.AssertTheseMatch(expectedSpectreDecoration, spectreStyle.Decoration, shouldMatch: true, $"TextDecoration.{textDecoration} should map to Decoration.{textDecoration}");
        }
    }

    [TestMethod]
    public void ToSpectreStyle_NamedForeground_MapsToSpectreColor()
    {
        var textStyle = new TextStyle(foreground: TextColor.Red);
        var spectreStyle = textStyle.ToSpectreStyle();

        TestUtilities.AssertTheseMatch(Color.Red, spectreStyle.Foreground, shouldMatch: true);
    }

    [TestMethod]
    public void ToSpectreStyle_NamedBackground_MapsToSpectreColor()
    {
        var textStyle = new TextStyle(background: TextColor.Green);
        var spectreStyle = textStyle.ToSpectreStyle();

        TestUtilities.AssertTheseMatch(Color.Green, spectreStyle.Background, shouldMatch: true);
    }

    [TestMethod]
    public void ToSpectreStyle_AllNamedColors_MapCorrectly()
    {
        // Use reflection to test all named color values
        foreach (NamedColor namedColor in Enum.GetValues<NamedColor>())
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

            TestUtilities.AssertTheseMatch(expectedSpectreColor, spectreColor, shouldMatch: true, $"TextColor.{namedColor} should map to Color.{namedColor}");
        }
    }

    [TestMethod]
    public void ToSpectreStyle_RgbColor_MapsToSpectreRgb()
    {
        var textStyle = new TextStyle(foreground: TextColor.FromRgb(100, 150, 200));
        var spectreStyle = textStyle.ToSpectreStyle();

        TestUtilities.AssertTheseMatch(new Color(100, 150, 200), spectreStyle.Foreground, shouldMatch: true);
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
        TestUtilities.AssertTheseMatch(Color.Red, spectreStyle.Foreground, shouldMatch: true);
        TestUtilities.AssertTheseMatch(new Color(0, 0, 255), spectreStyle.Background, shouldMatch: true);
    }
}
