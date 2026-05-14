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

        private static string FontPath
            => Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");

        private static string OtherFontFile { get; } = MakeOtherFontFile();

        private static string MakeOtherFontFile()
        {
            // Produce a second valid font file (a copy of shadow.flf) so equality tests have
            // two distinct, loadable paths to compare. Loading is eager so both paths must
            // refer to real, parseable .flf files.
            var src = Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");
            var dst = Path.Combine(Path.GetDirectoryName(src)!, "shadow-copy.flf");
            if (File.Exists(src) && !File.Exists(dst))
            {
                File.Copy(src, dst);
            }
            return dst;
        }

        [TestMethod]
        public void FigletTextStyle_FontPath_Preserved()
        {
            var style = new FigletTextStyle(fontPath: FontPath);

            Assert.AreEqual(FontPath, style.FontPath);
            Assert.AreEqual(FontPath, ((IHeaderStyle)style).FontPath);
            // The font should be loaded eagerly and cached on the style.
            Assert.IsNotNull(style.Font);
        }

        [TestMethod]
        public void FigletTextStyle_Equality_DifferentFontPath()
        {
            var a = new FigletTextStyle(fontPath: FontPath);
            var b = new FigletTextStyle(fontPath: OtherFontFile);

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void FigletTextStyle_InvalidFontPath_ThrowsAtConstruction()
        {
            // Loading happens eagerly at construction so an invalid path surfaces here
            // (rather than later at render time).
            Assert.ThrowsExactly<FileNotFoundException>(
                () => new FigletTextStyle(fontPath: Path.Combine(AppContext.BaseDirectory, "data", "fonts", "does-not-exist.flf")));
        }

        [TestMethod]
        public async Task FigletTextStyle_LoadAsync_LoadsFont()
        {
            var style = await FigletTextStyle.LoadAsync(
                FontPath,
                justification: TextJustification.Center,
                foreground: TextColor.Green);

            Assert.AreEqual(FontPath, style.FontPath);
            Assert.AreEqual(TextJustification.Center, style.Justification);
            Assert.AreEqual(TextColor.Green, style.Foreground);
            Assert.IsNotNull(style.Font);
        }
    }
}
