using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class SpectreRuleHeaderStyleTests
    {
        [TestMethod]
        public void Defaults_Are_Null()
        {
            var style = new SpectreRuleHeaderStyle();
            Assert.IsNull(style.Justification);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Border);
        }

        [TestMethod]
        public void SpectreRuleHeaderStyle_Create_PreservesProperties()
        {
            var created = new SpectreRuleHeaderStyle(
                justification: Justify.Right,
                foreground: Color.Green, 
                border: BoxBorder.Double);

            TestUtilities.AssertTheseMatch(Justify.Right, created.Justification, shouldMatch: true);
            TestUtilities.AssertTheseMatch(Color.Green, created.Foreground, shouldMatch: true);
            TestUtilities.AssertTheseMatch(BoxBorder.Double, created.Border, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Same_Instances()
        {
            var style1 = new SpectreRuleHeaderStyle(
                justification: Justify.Center,
                foreground: Color.Red,
                border: BoxBorder.Double);
            TestUtilities.AssertTheseMatch(style1, style1, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Equivalent_Instances()
        {
            var style1 = new SpectreRuleHeaderStyle(
                justification: Justify.Center,
                foreground: Color.Red,
                border: BoxBorder.Double);
            var style2 = new SpectreRuleHeaderStyle(
                justification: Justify.Center,
                foreground: Color.Red,
                border: BoxBorder.Double);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Empty_Instances()
            => TestUtilities.AssertTheseMatch(new SpectreRuleHeaderStyle(), new SpectreRuleHeaderStyle(), shouldMatch: true);

        [TestMethod]
        [DataRow(Justify.Center, nameof(Color.Red),   nameof(BoxBorder.Double))]
        [DataRow(Justify.Left,   nameof(Color.Red),   null)]
        [DataRow(null,           nameof(Color.Red),   null)]
        [DataRow(Justify.Center, nameof(Color.Green), null)]
        [DataRow(Justify.Center, null               , null)]
        public void Equals_Returns_False_For_Difference(Justify? justification, string? foreground, string? borderName)
        {
            Color? color =  string.IsNullOrEmpty(foreground) ? null : Color.FromName(foreground);
            BoxBorder? border = string.IsNullOrEmpty(borderName) ? null : Mappings.BoxBorders.NameMap.Forward.GetValueOrDefault(borderName);
            var style1 = new SpectreRuleHeaderStyle(Justify.Center, Color.Red);
            var style2 = new SpectreRuleHeaderStyle(justification, color, border);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: false);
        }

        [TestMethod]
        public void Has_Hard_Coded_ISpectreHeaderStyle_Values()
        {
            // SpectreRuleHeaderStyle does not support a background color or text decoration so those
            // members on the ISpectreHeaderStyle interface are hard-coded.
            ISpectreHeaderStyle rule = new SpectreRuleHeaderStyle(foreground: Color.Green);

            TestUtilities.AssertTheseMatch(Color.Green, rule.Foreground, shouldMatch: true);
            Assert.IsNull(rule.Background);
            TestUtilities.AssertTheseMatch(Decoration.None, rule.Decoration, shouldMatch: true);
        }
    }
}