using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests;

[TestClass]
public class TextStyleTests : TestBase
{
    [TestMethod]
    public void Defaults_Are_Null()
    {
        var style = new TextStyle();
        TestUtilities.AssertTheseMatch(TextDecoration.None, style.Decoration, shouldMatch: true);
        Assert.IsNull(style.Foreground);
        Assert.IsNull(style.Background);
        ToStringRoundTrip(style);
    }

    [TestMethod]
    public void TextStyle_Create_PreservesProperties()
    {
        var created = new TextStyle(
            decoration: TextDecoration.Underline,
            foreground: TextColor.Green, 
            background: TextColor.Black);

        TestUtilities.AssertTheseMatch(TextDecoration.Underline, created.Decoration, shouldMatch: true);
        TestUtilities.AssertTheseMatch(TextColor.Green, created.Foreground, shouldMatch: true);
        TestUtilities.AssertTheseMatch(TextColor.Black, created.Background, shouldMatch: true);
        ToStringRoundTrip(created);
    }

    [TestMethod]
    public void Equals_Returns_True_For_Same_Instances()
    {
        var style = new TextStyle(
            decoration: TextDecoration.Underline,
            foreground: TextColor.Red,
            background: TextColor.Black);
        TestUtilities.AssertTheseMatch(style, style, shouldMatch: true);
        ToStringRoundTrip(style);
    }

    [TestMethod]
    public void Equals_Returns_True_For_Equivalent_Instances()
    {
        var style1 = new TextStyle(
            decoration: TextDecoration.Underline,
            foreground: TextColor.Red,
            background: TextColor.Black);
        var style2 = new TextStyle(
            decoration: TextDecoration.Underline,
            foreground: TextColor.Red,
            background: TextColor.Black);
        TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        ToStringRoundTrip(style1);
        ToStringRoundTrip(style2);
    }

    [TestMethod]
    public void Equals_Returns_True_For_Empty_Instances()
        => TestUtilities.AssertTheseMatch(new TextStyle(), new TextStyle(), shouldMatch: true);

    [TestMethod]
    [DataRow(TextDecoration.None     , NamedColor.Red  , NamedColor.Green, "red on green")]
    [DataRow(TextDecoration.Underline, NamedColor.Red  , NamedColor.Green, "underline red on green")]
    [DataRow(TextDecoration.Underline, NamedColor.Green, NamedColor.Black, "underline green on black")]
    [DataRow(TextDecoration.Bold     , NamedColor.Red  , NamedColor.Black, "bold red on black")]
    [DataRow(TextDecoration.Underline, NamedColor.Red  , null            , "underline red")]
    [DataRow(TextDecoration.Underline, null            , NamedColor.Black, "underline on black")]
    public void Equals_Returns_True_For_Equivalent_Instances_Created_Differently(TextDecoration decoration, NamedColor? foreground, NamedColor? background, string markup)
    {
        TextColor? fg =  foreground.HasValue ? TextColor.FromNamed(foreground.Value) : null;
        TextColor? bg = background.HasValue ? TextColor.FromNamed(background.Value) : null;
        var style1 = new TextStyle(decoration, fg, bg);
        var style2 = (TextStyle)markup;
        TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        ToStringRoundTrip(style1);
        ToStringRoundTrip(style2);
    }

    [TestMethod]
    [DataRow(TextDecoration.Underline, NamedColor.Red  , NamedColor.Green)]
    [DataRow(TextDecoration.Underline, NamedColor.Green, NamedColor.Black)]
    [DataRow(TextDecoration.Bold     , NamedColor.Red  , NamedColor.Black)]
    [DataRow(TextDecoration.Underline, NamedColor.Red  , null)]
    [DataRow(TextDecoration.Underline, null            , NamedColor.Black)]
    public void Equals_Returns_False_For_Difference(TextDecoration decoration, NamedColor? foreground, NamedColor? background)
    {
        TextColor? fg =  foreground.HasValue ? TextColor.FromNamed(foreground.Value) : null;
        TextColor? bg = background.HasValue ? TextColor.FromNamed(background.Value) : null;
        var style1 = new TextStyle(TextDecoration.Underline, TextColor.FromNamed(NamedColor.Red), TextColor.FromNamed(NamedColor.Black));
        var style2 = new TextStyle(decoration, fg, bg);
        TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: false);
        ToStringRoundTrip(style1);
        ToStringRoundTrip(style2);
    }

    [TestMethod]
    public void Has_Hard_Coded_ISpectreHeaderStyle_Values()
    {
        IHeaderStyle text = new TextStyle(TextDecoration.Conceal, TextColor.Green, TextColor.Red);

        TestUtilities.AssertTheseMatch(TextDecoration.Conceal, text.Decoration, shouldMatch: true);
        TestUtilities.AssertTheseMatch(TextColor.Green,        text.Foreground, shouldMatch: true);
        TestUtilities.AssertTheseMatch(TextColor.Red,          text.Background, shouldMatch: true);
        ToStringRoundTrip((TextStyle)text);
    }

    [TestMethod]
    [DataRow(TextDecoration.Underline, NamedColor.Red  , NamedColor.Green)]
    [DataRow(TextDecoration.Underline, NamedColor.Green, NamedColor.Black)]
    [DataRow(TextDecoration.Bold     , NamedColor.Red  , NamedColor.Black)]
    [DataRow(TextDecoration.Underline, NamedColor.Red  , null)]
    [DataRow(TextDecoration.Underline, null            , NamedColor.Black)]
    public void Can_Round_Trip_TextStyle(TextDecoration decoration, NamedColor? foreground, NamedColor? background)
    {
        var style = new TextStyle(
            decoration,
            foreground.HasValue ? TextColor.FromNamed(foreground.Value) : null,
            background.HasValue ? TextColor.FromNamed(background.Value) : null);
        
        var spectreHeaderStyle = style.ToSpectreHeaderStyle();
        var back = spectreHeaderStyle.ToHeaderStyle();

        TestUtilities.AssertTheseMatch(style, back, shouldMatch: true);

        var spectreStyle = style.ToSpectreStyle();
        var back2 = spectreStyle.ToTextStyle();

        // Style Can't have a null colors, they coalesce to Color.Default 
        var expected = new TextStyle(
            style.Decoration,
            style.Foreground ?? TextColor.Default,
            style.Background ?? TextColor.Default);

        TestUtilities.AssertTheseMatch(expected, back2, shouldMatch: true);

        ToStringRoundTrip(style);
        ToStringRoundTrip((TextStyle)back);
        ToStringRoundTrip(back2);
        ToStringRoundTrip(expected);
    }

    [TestMethod]
    [DataRow(Decoration.Underline, nameof(Color.Red)  , nameof(Color.Green))]
    [DataRow(Decoration.Underline, nameof(Color.Green), nameof(Color.Black))]
    [DataRow(Decoration.Bold     , nameof(Color.Red)  , nameof(Color.Black))]
    [DataRow(Decoration.Underline, nameof(Color.Red)  , null)]
    [DataRow(Decoration.Underline, null               , nameof(Color.Black))]
    public void Can_Round_Trip_SpectreStyle(Decoration decoration, string? foreground, string? background)
    {
        var style = new SpectreTextStyle(
            decoration,
            string.IsNullOrEmpty(foreground) ? null : Color.FromName(foreground),
            string.IsNullOrEmpty(background) ? null : Color.FromName(background));

        var headerStyle = style.ToHeaderStyle();
        var back = headerStyle.ToSpectreHeaderStyle();

        TestUtilities.AssertTheseMatch(style, back, shouldMatch: true);

        var spectreStyle = ((ISpectreHeaderStyle)style).ToSpectreStyle();
        var back2 = spectreStyle.ToTextStyle();

        // Style Can't have a null colors, they coalesce to Color.Default 
        var textStyle = new TextStyle(
            decoration: Extensions.ToTextDecoration(decoration),
            foreground: string.IsNullOrEmpty(foreground) ? TextColor.Default : DisplayMappings.Colors.Forward.GetValueOrDefault(foreground),
            background: string.IsNullOrEmpty(background) ? TextColor.Default : DisplayMappings.Colors.Forward.GetValueOrDefault(background));

        TestUtilities.AssertTheseMatch(textStyle, back2, shouldMatch: true);

        ToStringRoundTrip((TextStyle)headerStyle);
        ToStringRoundTrip(back2);
        ToStringRoundTrip(textStyle);
    }

    private static void ToStringRoundTrip(TextStyle style)
    {
        var markup = style.ToString();
        var roundTrip = (TextStyle)markup;
        TestUtilities.AssertTheseMatch(style, roundTrip, shouldMatch: true);

        var spectreStyle = (SpectreTextStyle)markup;
        TestUtilities.AssertTheseMatch(style.ToSpectreHeaderStyle(), spectreStyle, shouldMatch: true);
    }
}