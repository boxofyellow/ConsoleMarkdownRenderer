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
        public async Task FigletTextStyle_EnsureFontLoadedAsync_MaterializesFont()
        {
            // The internal factory mirrors what HeaderStyleJsonConverter does: build a
            // FigletTextStyle with a FontPath but defer the load. EnsureFontLoadedAsync
            // then materializes the font.
            var style = FigletTextStyle.Create(
                justification: null,
                foreground: null,
                fontPath: BundledFontPath);

            // Reading Font before the load completes is a programming error.
            Assert.ThrowsExactly<InvalidOperationException>(() => _ = style.Font);

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
            // The deferred load surfaces I/O failures on the first await of EnsureFontLoadedAsync.
            var style = FigletTextStyle.Create(
                justification: null,
                foreground: null,
                fontPath: Path.Combine(AppContext.BaseDirectory, "data", "fonts", "does-not-exist.flf"));

            await Assert.ThrowsExactlyAsync<FileNotFoundException>(() => style.EnsureFontLoadedAsync());
        }

        [TestMethod]
        public async Task FigletTextStyle_JsonRoundTrip_WithFontPath_ViaDisplayOptions()
        {
            // FigletTextStyle is not directly JSON-deserializable on its own (its constructor
            // is private). The supported path is through DisplayOptions.DeserializeAsync,
            // which uses the internal HeaderStyleJsonConverter and finalizes the font load
            // before returning.
            var json = $$"""
                {
                    "Headers": [
                        {
                            "Kind": "figlet",
                            "Justification": "Center",
                            "Foreground": { "IsRgb": false, "Named": "Blue", "R": 0, "G": 0, "B": 0 },
                            "FontPath": {{JsonSerializer.Serialize(BundledFontPath)}}
                        }
                    ]
                }
                """;

            var options = await DisplayOptions.DeserializeAsync(json);
            var style = (FigletTextStyle)options.Headers[0];

            Assert.AreEqual(TextJustification.Center, style.Justification);
            Assert.AreEqual(TextColor.Blue, style.Foreground);
            Assert.AreEqual(BundledFontPath, style.FontPath);
            Assert.IsNotNull(style.Font);
        }
    }
}
