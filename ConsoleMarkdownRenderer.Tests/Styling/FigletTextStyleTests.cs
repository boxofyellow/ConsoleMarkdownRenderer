using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;
using BoxOfYellow.ConsoleMarkdownRenderer.Support;
using Spectre.Console;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Tests
{
    [TestClass]
    public class FigletTextStyleTests : TestBase
    {
        // Note: This gets placed here b/c this project has a dependency on the ConsoleMarkdownRenderer.Spectre.Tests
        public static readonly string BundledFontPath = Path.Combine(AppContext.BaseDirectory, "data", "fonts", "shadow.flf");

        [TestMethod]
        public void Defaults_Are_Null()
        {
            var style = FigletTextStyle.Create();
            Assert.IsNull(style.Justification);
            Assert.IsNull(style.Foreground);
            Assert.IsNull(style.FontPath);
            Assert.IsNull(style.Font);
        }

        [TestMethod]
        public void FigletTextStyle_Create_Preserves_Properties()
        {
            var created = FigletTextStyle.Create(
                justification: TextJustification.Right,
                foreground: TextColor.Green);

            TestUtilities.AssertTheseMatch(TextJustification.Right, created.Justification, shouldMatch: true);
            TestUtilities.AssertTheseMatch(TextColor.Green, created.Foreground, shouldMatch: true);
            Assert.IsNull(created.FontPath);
            Assert.IsNull(created.Font);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Same_Instances()
        {
            var style1 = FigletTextStyle.Create(TextJustification.Center, TextColor.Red);
            TestUtilities.AssertTheseMatch(style1, style1, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Equivalent_Instances()
        {
            var style1 = FigletTextStyle.Create(TextJustification.Center, TextColor.Red);
            var style2 = FigletTextStyle.Create(TextJustification.Center, TextColor.Red);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Equivalent_Instances_With_Paths()
        {
            var style1 = FigletTextStyle.Create(TextJustification.Center, TextColor.Red, "path");
            var style2 = FigletTextStyle.Create(TextJustification.Center, TextColor.Red, "path");
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        }

        [TestMethod]
        public void Equals_Returns_True_For_Empty_Instances() 
            => TestUtilities.AssertTheseMatch(FigletTextStyle.Create(), FigletTextStyle.Create(), shouldMatch: true);

        [TestMethod]
        public async Task Equals_Returns_True_For_Equivalent_Instances_Created_Differently()
        {
            var style1 = FigletTextStyle.Create(TextJustification.Center, TextColor.Red);
            var style2 = await FigletTextStyle.CreateAsync(fontPath: string.Empty, TextJustification.Center, TextColor.Red, default);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: true);
        }

        [TestMethod]
        [DataRow(TextJustification.Center, NamedColor.Red  , "font1.flf")]
        [DataRow(TextJustification.Left  , NamedColor.Red  , null)]
        [DataRow(null                    , NamedColor.Red  , null)]
        [DataRow(TextJustification.Center, NamedColor.Green, null)]
        [DataRow(TextJustification.Center, null            , null)]
        public void Equals_Returns_False_For_Difference(TextJustification? justification, NamedColor? foreground, string? fontPath)
        {
            TextColor? color =  foreground.HasValue ? TextColor.FromNamed(foreground.Value) : null;
            var style1 = FigletTextStyle.Create(TextJustification.Center, TextColor.Red);
            var style2 = FigletTextStyle.Create(justification, color, fontPath);
            TestUtilities.AssertTheseMatch(style1, style2, shouldMatch: false);
        }

        [TestMethod]
        public void Has_Hard_Coded_IHeaderStyle_Values()
        {
            // FigletText does not support a background color or text decoration so those
            // members on the IHeaderStyle interface are hard-coded.
            IHeaderStyle figlet = FigletTextStyle.Create(foreground: TextColor.Green);

            TestUtilities.AssertTheseMatch(TextColor.Green, figlet.Foreground, shouldMatch: true);
            Assert.IsNull(figlet.Background);
            TestUtilities.AssertTheseMatch(TextDecoration.None, figlet.Decoration, shouldMatch: true);
        }

        [TestMethod]
        public async Task CreateAsync_Loads_Font()
        {
            var style = await FigletTextStyle.CreateAsync(
                BundledFontPath,
                justification: TextJustification.Center,
                foreground: TextColor.Green);

            TestUtilities.AssertTheseMatch(BundledFontPath, style.FontPath, shouldMatch: true);
            TestUtilities.AssertTheseMatch(TextJustification.Center, style.Justification, shouldMatch: true);
            TestUtilities.AssertTheseMatch(TextColor.Green, style.Foreground, shouldMatch: true);
            Assert.IsNotNull(style.Font);
        }

        [TestMethod]
        public async Task CreateAsync_Invalid_Path_Throws()
        {
            // Loading happens at CreateAsync time so an invalid path surfaces there
            // (rather than later at render time).
            await Assert.ThrowsExactlyAsync<FileNotFoundException>(
                () => FigletTextStyle.CreateAsync(
                    Path.Combine(AppContext.BaseDirectory, "data", "fonts", "does-not-exist.flf")));
        }

        [TestMethod]
        public async Task EnsureFontLoadedAsync_Materializes_Font()
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
            var style = FigletTextStyle.Create();
            await style.EnsureFontLoadedAsync();
            Assert.IsNull(style.Font);
        }

        [TestMethod]
        public async Task EnsureFontLoadedAsync_Invalid_Path_Throws()
        {
            // The deferred load surfaces I/O failures on the first await of EnsureFontLoadedAsync.
            var style = FigletTextStyle.Create(
                justification: null,
                foreground: null,
                fontPath: Path.Combine(AppContext.BaseDirectory, "data", "fonts", "does-not-exist.flf"));

            await Assert.ThrowsExactlyAsync<FileNotFoundException>(() => style.EnsureFontLoadedAsync());
        }

        [TestMethod]
        [DataRow(TextJustification.Center, NamedColor.Red  , true)]
        [DataRow(TextJustification.Left  , NamedColor.Red  , false)]
        [DataRow(null                    , NamedColor.Red  , false)]
        [DataRow(TextJustification.Center, NamedColor.Green, false)]
        [DataRow(TextJustification.Center, null            , false)]
        public async Task Can_Round_Trip_FigletTextStyle(TextJustification? justification, NamedColor? foreground, bool includeFontPath)
        {
            var style = await FigletTextStyle.CreateAsync(
                includeFontPath ? BundledFontPath : string.Empty,
                justification, 
                foreground.HasValue ? TextColor.FromNamed(foreground.Value) : null);

            var spectreStyle = style.ToSpectreHeaderStyle();
            var back = spectreStyle.ToHeaderStyle();

            TestUtilities.AssertTheseMatch(style, back, shouldMatch: true);
        }

        [TestMethod]
        [DataRow(Justify.Center, nameof(Color.Red),   true)]
        [DataRow(Justify.Left,   nameof(Color.Red),   false)]
        [DataRow(null,           nameof(Color.Red),   false)]
        [DataRow(Justify.Center, nameof(Color.Green), false)]
        [DataRow(Justify.Center, null               , false)]
        public async Task Can_Round_Trip_SpectreFigletTextStyle(Justify? justification, string? foreground, bool includeFontPath)
        {
            var style = await SpectreFigletTextStyle.CreateAsync(
                includeFontPath ? BundledFontPath : string.Empty,
                justification, 
                string.IsNullOrEmpty(foreground) ? null : Color.FromName(foreground));

            var textStyle = style.ToHeaderStyle();
            var back = textStyle.ToSpectreHeaderStyle();

            TestUtilities.AssertTheseMatch(style, back, shouldMatch: true);
        }
    }
}
