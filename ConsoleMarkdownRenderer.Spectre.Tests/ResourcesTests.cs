namespace BoxOfYellow.ConsoleMarkdownRenderer.Spectre.Tests
{
    [TestClass]
    public class ResourcesTests : ConsoleTestBase
    {
        [TestMethod]
        public async Task RendererTests_TextValidation()
        {
            var markdowns = new HashSet<string>();
            var expected = new HashSet<string>();
            var optionResources = new HashSet<string>();

            foreach (var resourceName in GetType().Assembly.GetManifestResourceNames())
            {
                if (Path.GetDirectoryName(resourceName) == c_resources)
                {
                    var extension = Path.GetExtension(resourceName);
                    var name = Path.GetFileNameWithoutExtension(resourceName);
                    if (extension == ".md")
                    {
                        markdowns.Add(name);
                    }
                    else if (extension == ".txt")
                    {
                        expected.Add(name);
                    }
                    else if (extension == ".json")
                    {
                        optionResources.Add(name);
                    }
                }
            }

            var missing = markdowns.Where(x => !expected.Contains(x));
            if (missing.Any())
            {
                Assert.Fail($"Has markdowns but no expected {string.Join(", ", missing)}");
            }
            missing = expected.Where(x => !markdowns.Contains(x));
            if (missing.Any())
            {
                Assert.Fail($"Has expected but no markdown {string.Join(", ", missing)}");
            }
            // .json sidecars are optional but each one must pair with an .md
            var orphanOptions = optionResources.Where(x => !markdowns.Contains(x));
            if (orphanOptions.Any())
            {
                Assert.Fail($"Has options json but no markdown {string.Join(", ", orphanOptions)}");
            }

            var defaultOptions = new SpectreDisplayOptions { IncludeDebug = true };
            var jsonOptions = SpectreDisplayOptions.BuildEffectiveOptions(TestJsonHelper.EnumJsonOptions);

            var render = new MarkdownRenderer();

            foreach (var markdown in markdowns)
            {
                var markdownText = GetResourceContent(markdown, "md");
                var expectedText = GetResourceContent(markdown, "txt");

                SpectreDisplayOptions options;
                if (optionResources.Contains(markdown))
                {
                    try
                    {
                        options = await SpectreDisplayOptions.DeserializeAsync(GetResourceContent(markdown, "json"), jsonOptions);
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail($"Failed to deserialize options for {markdown} {ex} {GetResourceContent(markdown, "json")}");
                        throw;
                    }
                }
                else
                {
                    options = defaultOptions;
                }

                var result = render.Render(markdownText, options);
                Assert.IsNotNull(result.Root);
                NewConsole().Write(result.Root!);

                AssertCrossPlatStringMatch(
                    expectedText,
                    ConsoleUnderTest.Output,
                    $"""
                    {markdown} Did not get the expect result
                    {markdownText}
                    Yielded
                    {ConsoleUnderTest.Output}
                    Expected
                    {expectedText}
                    """);
            }
        }

        private string GetResourceContent(string name, string extension)
        {
            string path = Path.Combine(c_resources, Path.ChangeExtension(name, extension));
            using var steam = GetType().Assembly.GetManifestResourceStream(path);
            Assert.IsNotNull(steam, $"Failed to find resource for {path}");
            using var reader = new StreamReader(steam);
            return reader.ReadToEnd();
        }
        private const string c_resources = "resources";
    }
}