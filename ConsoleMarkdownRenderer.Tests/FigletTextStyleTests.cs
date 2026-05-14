using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    /// <summary>
    /// Tests for <see cref="FigletTextStyle"/> and the shared <see cref="IHeaderStyle"/> contract.
    /// </summary>
    [TestClass]
    public class FigletTextStyleTests : TestWithFileCleanupBase
    {
        [TestMethod]
        public void FigletTextStyle_DefaultsAreNull()
        {
            var style = FigletTextStyle.Create();
            Assert.IsNull(style.Justification);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.FontPath);
            Assert.IsNull(style.Font);
        }

        [TestMethod]
        public void FigletTextStyle_PropertiesArePreserved()
        {
            var style = FigletTextStyle.Create(
                justification: TextJustification.Center,
                foreground: TextColor.Red);

            Assert.AreEqual(TextJustification.Center, style.Justification);
            Assert.AreEqual(TextColor.Red, style.Foreground);
        }

        [TestMethod]
        public void FigletTextStyle_Create_PreservesProperties()
        {
            var created = FigletTextStyle.Create(
                justification: TextJustification.Right,
                foreground: TextColor.Green);

            Assert.AreEqual(TextJustification.Right, created.Justification);
            Assert.AreEqual(TextColor.Green, created.Foreground);
            Assert.IsNull(created.FontPath);
            Assert.IsNull(created.Font);
            Assert.AreEqual(FigletTextStyle.Create(TextJustification.Right, TextColor.Green), created);
        }

        [TestMethod]
        public void FigletTextStyle_Equality_SameValues()
        {
            var a = FigletTextStyle.Create(justification: TextJustification.Right, foreground: TextColor.Green);
            var b = FigletTextStyle.Create(justification: TextJustification.Right, foreground: TextColor.Green);

            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [TestMethod]
        public void FigletTextStyle_Equality_DifferentJustification()
        {
            var a = FigletTextStyle.Create(justification: TextJustification.Left);
            var b = FigletTextStyle.Create(justification: TextJustification.Right);

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void FigletTextStyle_Equality_DifferentForeground()
        {
            var a = FigletTextStyle.Create(foreground: TextColor.Red);
            var b = FigletTextStyle.Create(foreground: TextColor.Blue);

            Assert.AreNotEqual(a, b);
        }

        [TestMethod]
        public void FigletTextStyle_ImplementsIHeaderStyle()
        {
            // Both FigletTextStyle and the existing TextStyle satisfy the shared IHeaderStyle
            // contract so they can be assigned to DisplayOptions.Header / Headers interchangeably.
            IHeaderStyle figlet = FigletTextStyle.Create(foreground: TextColor.Green);
            IHeaderStyle plain  = new TextStyle(decoration: TextDecoration.Bold);

            Assert.AreEqual(TextColor.Green, figlet.Foreground);
            Assert.AreEqual(TextDecoration.Bold, plain.Decoration);
        }

        [TestMethod]
        public void TextStyle_ExposesForegroundBackgroundDecorationViaIHeaderStyle()
        {
            // Foreground/Background/Decoration on TextStyle round-trip through the
            // IHeaderStyle interface unchanged.
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
            IHeaderStyle figlet = FigletTextStyle.Create(foreground: TextColor.Green);

            Assert.AreEqual(TextColor.Green,        figlet.Foreground);
            Assert.IsNull(figlet.Background);
            Assert.AreEqual(TextDecoration.None,    figlet.Decoration);
        }

        private static string BundledFontPath
            => Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");

        [TestMethod]
        public async Task FigletTextStyle_CreateAsync_LoadsFont()
        {
            var style = await FigletTextStyle.CreateAsync(
                BundledFontPath,
                justification: TextJustification.Center,
                foreground: TextColor.Green);

            Assert.AreEqual(BundledFontPath, style.FontPath);
            Assert.AreEqual(TextJustification.Center, style.Justification);
            Assert.AreEqual(TextColor.Green, style.Foreground);
            Assert.IsNotNull(style.Font);
        }

        [TestMethod]
        public async Task FigletTextStyle_CreateAsync_InvalidPath_Throws()
        {
            // Loading happens at CreateAsync time so an invalid path surfaces there
            // (rather than later at render time).
            await Assert.ThrowsExactlyAsync<FileNotFoundException>(
                () => FigletTextStyle.CreateAsync(
                    Path.Combine(AppContext.BaseDirectory, "data", "fonts", "does-not-exist.flf")));
        }

        [TestMethod]
        public async Task FigletTextStyle_Equality_DifferentFontPath()
        {
            // Copy the bundled font to a temp location so equality has two distinct, valid
            // .flf paths to compare. TempFileManager cleans up the copy at test teardown.
            var tempPath = TempFiles.GetTempFile();
            File.Copy(BundledFontPath, tempPath, overwrite: true);

            var a = await FigletTextStyle.CreateAsync(BundledFontPath);
            var b = await FigletTextStyle.CreateAsync(tempPath);

            Assert.AreNotEqual(a, b);
        }
    }
}
