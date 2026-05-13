using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="FigletTextStyle"/> and the shared <see cref="IHeaderStyle"/> contract.
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
        }

        [TestMethod]
        public void FigletTextStyle_PropertiesArePreserved()
        {
            var style = new FigletTextStyle(
                justification: TextJustification.Center,
                foreground: TextColor.Red);

            Assert.AreEqual(TextJustification.Center, style.Justification);
            Assert.AreEqual(TextColor.Red, style.Foreground);
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
        public void FigletTextStyle_Equality_DifferentForeground()
        {
            var a = new FigletTextStyle(foreground: TextColor.Red);
            var b = new FigletTextStyle(foreground: TextColor.Blue);

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void FigletTextStyle_ImplementsIHeaderStyle()
        {
            // Both FigletTextStyle and the existing TextStyle satisfy the shared IHeaderStyle
            // contract so they can be assigned to DisplayOptions.Header / Headers interchangeably.
            IHeaderStyle figlet = new FigletTextStyle(justification: TextJustification.Center);
            IHeaderStyle plain  = new TextStyle(decoration: TextDecoration.Bold);

            Assert.AreEqual(TextJustification.Center, figlet.Justification);
            Assert.IsNull(plain.Justification);
        }

        [TestMethod]
        public void TextStyle_Justification_IsNullViaIHeaderStyle()
        {
            // The Justification property on TextStyle is provided exclusively through the
            // IHeaderStyle interface (explicit implementation) and must always return null.
            IHeaderStyle plain = new TextStyle(decoration: TextDecoration.Bold, foreground: TextColor.Red);
            Assert.IsNull(plain.Justification);
        }

        [TestMethod]
        public void TextStyle_FontPath_IsNullViaIHeaderStyle()
        {
            // FontPath is explicitly implemented on TextStyle and must always return null --
            // custom FIGlet fonts are only meaningful for FigletTextStyle.
            IHeaderStyle plain = new TextStyle();
            Assert.IsNull(plain.FontPath);
        }

        [TestMethod]
        public void TextStyle_ExposesForegroundBackgroundDecorationViaIHeaderStyle()
        {
            // Foreground/Background/Decoration on TextStyle are implicit interface members --
            // they round-trip through the IHeaderStyle interface unchanged.
            IHeaderStyle plain = new TextStyle(
                decoration: TextDecoration.Italic,
                foreground: TextColor.Red,
                background: TextColor.Blue);

            Assert.AreEqual(TextDecoration.Italic, plain.Decoration);
            Assert.AreEqual(TextColor.Red,         plain.Foreground);
            Assert.AreEqual(TextColor.Blue,        plain.Background);
        }

        [TestMethod]
        public void FigletTextStyle_BackgroundAndDecoration_HardCodedViaIHeaderStyle()
        {
            // FigletText does not support a background color or text decoration so those
            // members on the IHeaderStyle interface are hard-coded.
            IHeaderStyle figlet = new FigletTextStyle(foreground: TextColor.Green);

            Assert.AreEqual(TextColor.Green,        figlet.Foreground);
            Assert.IsNull(figlet.Background);
            Assert.AreEqual(TextDecoration.None,    figlet.Decoration);
        }

        [TestMethod]
        public void FigletTextStyle_FontPath_Preserved()
        {
            var style = new FigletTextStyle(fontPath: "/some/path/font.flf");

            Assert.AreEqual("/some/path/font.flf", style.FontPath);
            Assert.AreEqual("/some/path/font.flf", ((IHeaderStyle)style).FontPath);
        }

        [TestMethod]
        public void FigletTextStyle_Equality_DifferentFontPath()
        {
            var a = new FigletTextStyle(fontPath: "/a.flf");
            var b = new FigletTextStyle(fontPath: "/b.flf");

            Assert.AreNotEqual(a, b);
        }
    }
}
