using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class SpectreTextStyleTests
    {
        [TestMethod]
        public void Defaults_Are_Null()
        {
            var style = new SpectreTextStyle();
            TestUtilities.AssertTheseMatch(Decoration.None, style.Decoration, shouldMatch: true);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Background);
            ToStringRoundTrip(style);
        }

        [TestMethod]
        public void SpectreTextStyle_Create_PreservesProperties()
        {
            var created = new SpectreTextStyle(
                decoration: Decoration.Underline,
                foreground: Color.Green, 
                background: Color.Black);

            TestUtilities.AssertTheseMatch(Decoration.Underline, created.Decoration, shouldMatch: true);
            TestUtilities.AssertTheseMatch(Color.Green, created.Foreground, shouldMatch: true);
            TestUtilities.AssertTheseMatch(Color.Black, created.Background, shouldMatch: true);
            ToStringRoundTrip(created);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Same_Instances()
        {
            var style = new SpectreTextStyle(
                decoration: Decoration.Underline,
                foreground: Color.Red,
                background: Color.Black);
            TestUtilities.AssertTheseMatch(style, style, shouldMatch: true);
            ToStringRoundTrip(style);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Equivalent_Instances()
        {
            var style1 = new SpectreTextStyle(
                decoration: Decoration.Underline,
                foreground: Color.Red,
                background: Color.Black);
            var style2 = new SpectreTextStyle(
                decoration: Decoration.Underline,
                foreground: Color.Red,
                background: Color.Black);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
            ToStringRoundTrip(style1);
            ToStringRoundTrip(style2);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Empty_Instances()
            => TestUtilities.AssertTheseMatch(new SpectreTextStyle(), new SpectreTextStyle(), shouldMatch: true);

        [TestMethod]
        public void Equals_Different_Types()
            => Assert.IsFalse(new SpectreTextStyle().Equals(new object()), "SpectreTextStyle should not equal a different type");

        [TestMethod]
        [DataRow(Decoration.None,      nameof(Color.Red),   nameof(Color.Green), "red on green")]
        [DataRow(Decoration.Underline, nameof(Color.Red),   nameof(Color.Green), "underline red on green")]
        [DataRow(Decoration.Underline, nameof(Color.Green), nameof(Color.Black), "underline green on black")]
        [DataRow(Decoration.Bold,      nameof(Color.Red),   nameof(Color.Black), "bold red on black")]
        [DataRow(Decoration.Underline, nameof(Color.Red),   null,                "underline red")]
        [DataRow(Decoration.Underline, null,                nameof(Color.Black), "underline on black")]
        public void Equals_Returns_True_For_Equivalent_Instances_Created_Differently(Decoration decoration, string? foreground, string? background, string markup)
        {
            Color? fg =  string.IsNullOrEmpty(foreground) ? null : Color.FromName(foreground);
            Color? bg = string.IsNullOrEmpty(background) ? null : Color.FromName(background);
            var style1 = new SpectreTextStyle(decoration, fg, bg);
            var style2 = (SpectreTextStyle)markup;
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
            ToStringRoundTrip(style1);
            ToStringRoundTrip(style2);
        }

        [TestMethod]
        [DataRow(Decoration.Underline, nameof(Color.Red),   nameof(Color.Green))]
        [DataRow(Decoration.Underline, nameof(Color.Green), nameof(Color.Black))]
        [DataRow(Decoration.Bold,      nameof(Color.Red),   nameof(Color.Black))]
        [DataRow(Decoration.Underline, nameof(Color.Red),   null)]
        [DataRow(Decoration.Underline, null,                nameof(Color.Black))]
        public void Equals_Returns_False_For_Difference(Decoration decoration, string? foreground, string? background)
        {
            Color? fg =  string.IsNullOrEmpty(foreground) ? null : Color.FromName(foreground);
            Color? bg = string.IsNullOrEmpty(background) ? null : Color.FromName(background);
            var style1 = new SpectreTextStyle(Decoration.Underline, Color.Red, Color.Black);
            var style2 = new SpectreTextStyle(decoration, fg, bg);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: false);
            ToStringRoundTrip(style1);
            ToStringRoundTrip(style2);
        }

        [TestMethod]
        public void Has_Hard_Coded_ISpectreHeaderStyle_Values()
        {
            ISpectreHeaderStyle text = new SpectreTextStyle(Decoration.Conceal, Color.Green, Color.Red);

            TestUtilities.AssertTheseMatch(Decoration.Conceal, text.Decoration, shouldMatch: true);
            TestUtilities.AssertTheseMatch(Color.Green,        text.Foreground, shouldMatch: true);
            TestUtilities.AssertTheseMatch(Color.Red,          text.Background, shouldMatch: true);
            ToStringRoundTrip((SpectreTextStyle)text);
        }

        private static void ToStringRoundTrip(SpectreTextStyle style)
        {
            var markup = style.ToString();
            var roundTrip = (SpectreTextStyle)markup;
            TestUtilities.AssertTheseMatch(style, roundTrip, shouldMatch: true);
        }
    }
}