using System.Text.Json;
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

        [TestMethod]
        public void FigletTextStyle_Create_WithFontPath_DefersFontLoad()
        {
            // Sync Create with a FontPath should not read the file synchronously; the font
            // is lazily loaded the first time it is awaited or rendered.
            var style = FigletTextStyle.Create(fontPath: BundledFontPath);

            Assert.AreEqual(BundledFontPath, style.FontPath);
            Assert.IsNull(style.Font, "Font should not be materialized before EnsureFontLoadedAsync");
        }

        [TestMethod]
        public async Task FigletTextStyle_EnsureFontLoadedAsync_MaterializesFont()
        {
            var style = FigletTextStyle.Create(fontPath: BundledFontPath);
            Assert.IsNull(style.Font);

            await style.EnsureFontLoadedAsync();

            Assert.IsNotNull(style.Font, "Font should be materialized after EnsureFontLoadedAsync");

            // Calling EnsureFontLoadedAsync again is a no-op and reuses the cached parse.
            var first = style.Font;
            await style.EnsureFontLoadedAsync();
            Assert.AreSame(first, style.Font);
        }

        [TestMethod]
        public async Task FigletTextStyle_EnsureFontLoadedAsync_NullPath_NoOp()
        {
            var style = FigletTextStyle.Create();
            await style.EnsureFontLoadedAsync();
            Assert.IsNull(style.Font);
        }

        [TestMethod]
        public async Task FigletTextStyle_EnsureFontLoadedAsync_InvalidPath_Throws()
        {
            // The lazy load surfaces I/O failures on the first await of EnsureFontLoadedAsync.
            var style = FigletTextStyle.Create(
                fontPath: Path.Combine(AppContext.BaseDirectory, "data", "fonts", "does-not-exist.flf"));

            await Assert.ThrowsExactlyAsync<FileNotFoundException>(() => style.EnsureFontLoadedAsync());
        }

        [TestMethod]
        public async Task FigletTextStyle_JsonRoundTrip_WithFontPath()
        {
            // Stock System.Text.Json should be able to deserialize a FigletTextStyle via its
            // public [JsonConstructor]. The font is then materialized lazily.
            var json = $$"""
                {
                    "justification": "Center",
                    "foreground": { "isRgb": false, "named": "Blue", "r": 0, "g": 0, "b": 0 },
                    "fontPath": {{JsonSerializer.Serialize(BundledFontPath)}}
                }
                """;

            var style = JsonSerializer.Deserialize<FigletTextStyle>(json, RendererTests.JsonOptions);

            Assert.IsNotNull(style);
            Assert.AreEqual(TextJustification.Center, style.Justification);
            Assert.AreEqual(TextColor.Blue, style.Foreground);
            Assert.AreEqual(BundledFontPath, style.FontPath);
            Assert.IsNull(style.Font, "Font should be loaded lazily after deserialization");

            await style.EnsureFontLoadedAsync();
            Assert.IsNotNull(style.Font);
        }
    }
}
