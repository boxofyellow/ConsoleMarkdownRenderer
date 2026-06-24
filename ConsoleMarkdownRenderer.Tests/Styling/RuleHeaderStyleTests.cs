using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    [TestClass]
    public class RuleHeaderStyleTests : TestBase
    {
        [TestMethod]
        public void Defaults_Are_Null()
        {
            var style = new RuleHeaderStyle();
            Assert.IsNull(style.Justification);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Border);
        }

        [TestMethod]
        public void RuleHeaderStyle_Create_PreservesProperties()
        {
            var created = new RuleHeaderStyle(
                justification: TextJustification.Right,
                foreground: TextColor.Green, 
                border: RuleBorder.Double);

            TestUtilities.AssertTheseMatch(TextJustification.Right, created.Justification, shouldMatch: true);
            TestUtilities.AssertTheseMatch(TextColor.Green, created.Foreground, shouldMatch: true);
            TestUtilities.AssertTheseMatch(RuleBorder.Double, created.Border, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Same_Instances()
        {
            var style1 = new RuleHeaderStyle(
                justification: TextJustification.Center,
                foreground: TextColor.Red,
                border: RuleBorder.Double);
            TestUtilities.AssertTheseMatch(style1, style1, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Equivalent_Instances()
        {
            var style1 = new RuleHeaderStyle(
                justification: TextJustification.Center,
                foreground: TextColor.Red,
                border: RuleBorder.Double);
            var style2 = new RuleHeaderStyle(
                justification: TextJustification.Center,
                foreground: TextColor.Red,
                border: RuleBorder.Double);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Empty_Instances()
            => TestUtilities.AssertTheseMatch(new RuleHeaderStyle(), new RuleHeaderStyle(), shouldMatch: true);

        [TestMethod]
        [DataRow(TextJustification.Center, NamedColor.Red  , RuleBorder.Double)]
        [DataRow(TextJustification.Left  , NamedColor.Red  , null)]
        [DataRow(null,                     NamedColor.Red  , null)]
        [DataRow(TextJustification.Center, NamedColor.Green, null)]
        [DataRow(TextJustification.Center, null            , null)]
        public void Equals_Returns_False_For_Difference(TextJustification? justification, NamedColor? foreground, RuleBorder? border)
        {
            TextColor? color =  foreground.HasValue ? TextColor.FromNamed(foreground.Value) : null;
            var style1 = new RuleHeaderStyle(TextJustification.Center, TextColor.Red);
            var style2 = new RuleHeaderStyle(justification, color, border);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: false);
        }

        [TestMethod]
        public void Has_Hard_Coded_IHeaderStyle_Values()
        {
            // RuleHeaderStyle does not support a background color or text decoration so those
            // members on the IHeaderStyle interface are hard-coded.
            IHeaderStyle rule = new RuleHeaderStyle(foreground: TextColor.Green);

            TestUtilities.AssertTheseMatch(TextColor.Green, rule.Foreground, shouldMatch: true);
            Assert.IsNull(rule.Background);
            TestUtilities.AssertTheseMatch(TextDecoration.None, rule.Decoration, shouldMatch: true);
        }

        [TestMethod]
        [DataRow(TextJustification.Center, NamedColor.Red  , RuleBorder.Double)]
        [DataRow(TextJustification.Left  , NamedColor.Red  , null)]
        [DataRow(null,                     NamedColor.Red  , null)]
        [DataRow(TextJustification.Center, NamedColor.Green, null)]
        [DataRow(TextJustification.Center, null            , null)]
        public void Can_Round_Trip_RuleHeaderStyle(TextJustification? justification, NamedColor? foreground, RuleBorder? border)
        {
            var style = new RuleHeaderStyle(
                justification,
                foreground.HasValue ? TextColor.FromNamed(foreground.Value) : null,
                border);
            
            var spectreStyle = style.ToSpectreHeaderStyle();
            var back = spectreStyle.ToHeaderStyle();

            TestUtilities.AssertTheseMatch(style, back, shouldMatch: true);
        }

        [TestMethod]
        [DataRow(Justify.Center, nameof(Color.Red),   nameof(BoxBorder.Double))]
        [DataRow(Justify.Left,   nameof(Color.Red),   null)]
        [DataRow(null,           nameof(Color.Red),   null)]
        [DataRow(Justify.Center, nameof(Color.Green), null)]
        [DataRow(Justify.Center, null               , null)]
        public void Can_Round_Trip_SpectreRuleHeaderStyle(Justify? justification, string? foreground, string? borderName)
        {
            var style = new SpectreRuleHeaderStyle(
                justification,
                string.IsNullOrEmpty(foreground) ? null : Color.FromName(foreground),
                string.IsNullOrEmpty(borderName) ? null : Mappings.BoxBorders.NameMap.Forward.GetValueOrDefault(borderName));

            var headerStyle = style.ToHeaderStyle();
            var back = headerStyle.ToSpectreHeaderStyle();

            TestUtilities.AssertTheseMatch(style, back, shouldMatch: true);
        }
    }
}
