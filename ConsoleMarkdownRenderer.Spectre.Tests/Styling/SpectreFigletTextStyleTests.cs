using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class SpectreFigletTextStyleTests
    {
        public static readonly string BundledFontPath = Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");

        [TestMethod]
        public void Defaults_Are_Null()
        {
            var style = SpectreFigletTextStyle.Create();
            Assert.IsNull(style.Justification);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.FontPath);
            Assert.IsNull(style.Font);
        }

        [TestMethod]
        public void FigletTextStyle_Create_Preserves_Properties()
        {
            var created = SpectreFigletTextStyle.Create(
                justification: Justify.Right,
                foreground: Color.Green);

            TestUtilities.AssertTheseMatch(Justify.Right, created.Justification, shouldMatch: true);
            TestUtilities.AssertTheseMatch(Color.Green, created.Foreground, shouldMatch: true);
            Assert.IsNull(created.FontPath);
            Assert.IsNull(created.Font);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Same_Instances()
        {
            var style1 = SpectreFigletTextStyle.Create(Justify.Center, Color.Red);
            TestUtilities.AssertTheseMatch(style1, style1, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Equivalent_Instances()
        {
            var style1 = SpectreFigletTextStyle.Create(Justify.Center, Color.Red);
            var style2 = SpectreFigletTextStyle.Create(Justify.Center, Color.Red);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Equivalent_Instances_With_Paths()
        {
            var style1 = SpectreFigletTextStyle.Create(Justify.Center, Color.Red, "path");
            var style2 = SpectreFigletTextStyle.Create(Justify.Center, Color.Red, "path");
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Empty_Instances() 
            => TestUtilities.AssertTheseMatch(SpectreFigletTextStyle.Create(), SpectreFigletTextStyle.Create(), shouldMatch: true);

        [TestMethod]
        public async Task Equals_Returns_True_For_Equivalent_Instances_Created_Differently()
        {
            var style1 = SpectreFigletTextStyle.Create(Justify.Center, Color.Red);
            var style2 = await SpectreFigletTextStyle.CreateAsync(fontPath: string.Empty, Justify.Center, Color.Red, default);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        }

        [TestMethod]
        [DataRow(Justify.Center, nameof(Color.Red),   "font1.flf")]
        [DataRow(Justify.Left,   nameof(Color.Red),   null)]
        [DataRow(null,           nameof(Color.Red),   null)]
        [DataRow(Justify.Center, nameof(Color.Green), null)]
        [DataRow(Justify.Center, null               , null)]
        public void Equals_Returns_False_For_Difference(Justify? justification, string? foreground, string? fontPath)
        {
            Color? color =  string.IsNullOrEmpty(foreground) ? null : Color.FromName(foreground);
            var style1 = SpectreFigletTextStyle.Create(Justify.Center, Color.Red);
            var style2 = SpectreFigletTextStyle.Create(justification, color, fontPath);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: false);
        }

        [TestMethod]
        public void Has_Hard_Coded_ISpectreHeaderStyle_Values()
        {
            // SpectreFigletText does not support a background color or text decoration so those
            // members on the ISpectreHeaderStyle interface are hard-coded.
            ISpectreHeaderStyle figlet = SpectreFigletTextStyle.Create(foreground: Color.Green);

            TestUtilities.AssertTheseMatch(Color.Green, figlet.Foreground, shouldMatch: true);
            Assert.IsNull(figlet.Background);
            TestUtilities.AssertTheseMatch(Decoration.None, figlet.Decoration, shouldMatch: true);
        }

        [TestMethod]
        public async Task CreateAsync_Loads_Font()
        {
            var style = await SpectreFigletTextStyle.CreateAsync(
                BundledFontPath,
                justification: Justify.Center,
                foreground: Color.Green);

            TestUtilities.AssertTheseMatch(BundledFontPath, style.FontPath, shouldMatch: true);
            TestUtilities.AssertTheseMatch(Justify.Center, style.Justification, shouldMatch: true);
            TestUtilities.AssertTheseMatch(Color.Green, style.Foreground, shouldMatch: true);
            Assert.IsNotNull(style.Font);
        }

        [TestMethod]
        public async Task CreateAsync_Invalid_Path_Throws()
        {
            // Loading happens at CreateAsync time so an invalid path surfaces there
            // (rather than later at render time).
            await Assert.ThrowsExactlyAsync<FileNotFoundException>(
                () => SpectreFigletTextStyle.CreateAsync(
                    Path.Combine(AppContext.BaseDirectory, "data", "fonts", "does-not-exist.flf")));
        }

        [TestMethod]
        public async Task EnsureFontLoadedAsync_Materializes_Font()
        {
            // The internal factory mirrors what HeaderStyleJsonConverter does: build a
            // FigletTextStyle with a FontPath but defer the load. EnsureFontLoadedAsync
            // then materializes the font.
            var style = SpectreFigletTextStyle.Create(
                justification: null,
                foreground: null,
                fontPath: BundledFontPath);

            // Reading Font before the load completes is a programming error.
            Assert.ThrowsExactly<InvalidOperationException>(() => _ = style.Font);

            await style.EnsureFontLoadedAsync(CancellationToken.None);

            Assert.IsNotNull(style.Font, "Font should be materialized after EnsureFontLoadedAsync");

            // Calling EnsureFontLoadedAsync again is a no-op and reuses the cached parse.
            var first = style.Font;
            await style.EnsureFontLoadedAsync();
            Assert.AreSame(first, style.Font);
        }

        [TestMethod]
        public async Task EnsureFontLoadedAsync_No_Ops_Null_Path()
        {
            var style = SpectreFigletTextStyle.Create();
            await style.EnsureFontLoadedAsync();
            Assert.IsNull(style.Font);
        }

        [TestMethod]
        public async Task EnsureFontLoadedAsync_Invalid_Path_Throws()
        {
            // The deferred load surfaces I/O failures on the first await of EnsureFontLoadedAsync.
            var style = SpectreFigletTextStyle.Create(
                justification: null,
                foreground: null,
                fontPath: Path.Combine(AppContext.BaseDirectory, "data", "fonts", "does-not-exist.flf"));

            await Assert.ThrowsExactlyAsync<FileNotFoundException>(() => style.EnsureFontLoadedAsync());
        }
    }
}