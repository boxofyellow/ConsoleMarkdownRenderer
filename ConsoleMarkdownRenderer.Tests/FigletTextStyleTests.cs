using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="FigletTextStyle"/>.
    /// </summary>
    [TestClass]
    public class FigletTextStyleTests
    {
        [TestMethod]
        public void FigletTextStyle_DefaultsAreNull()
        {
            var style = new FigletTextStyle();
            Assert.IsNull(style.Justification);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.Background);
            Assert.AreEqual(TextDecoration.None, style.Decoration);
        }

        [TestMethod]
        public void FigletTextStyle_PropertiesArePreserved()
        {
            var style = new FigletTextStyle(
                justification: TextJustification.Center,
                foreground: TextColor.Red,
                decoration: TextDecoration.Bold,
                background: TextColor.Blue);

            Assert.AreEqual(TextJustification.Center, style.Justification);
            Assert.AreEqual(TextColor.Red, style.Foreground);
            Assert.AreEqual(TextColor.Blue, style.Background);
            Assert.AreEqual(TextDecoration.Bold, style.Decoration);
        }

        [TestMethod]
        public void FigletTextStyle_Equality_SameValues()
        {
            var a = new FigletTextStyle(justification: TextJustification.Right, foreground: TextColor.Green);
            var b = new FigletTextStyle(justification: TextJustification.Right, foreground: TextColor.Green);

            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [TestMethod]
        public void FigletTextStyle_Equality_DifferentJustification()
        {
            var a = new FigletTextStyle(justification: TextJustification.Left);
            var b = new FigletTextStyle(justification: TextJustification.Right);

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void FigletTextStyle_IsNotEqualToBaseTextStyleWithSameCoreValues()
        {
            // A FigletTextStyle should never compare equal to a plain TextStyle because they have
            // different rendering semantics, even if their decoration / colors happen to match.
            var plain = new TextStyle(foreground: TextColor.Red);
            var figlet = new FigletTextStyle(foreground: TextColor.Red);

            Assert.AreNotEqual<TextStyle>(plain, figlet);
            Assert.AreNotEqual<TextStyle>(figlet, plain);
        }

        [TestMethod]
        public void FigletTextStyle_CanBeAssignedAsTextStyle()
        {
            TextStyle style = new FigletTextStyle(justification: TextJustification.Center);
            Assert.IsInstanceOfType<FigletTextStyle>(style);
        }
    }
}
