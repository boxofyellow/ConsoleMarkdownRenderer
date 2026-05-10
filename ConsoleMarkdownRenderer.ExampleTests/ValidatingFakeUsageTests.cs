using ConsoleMarkdownRenderer.Fakes;

namespace ConsoleMarkdownRenderer.ExampleTests
{
    [TestClass]
    public class ValidatingFakeMarkdownDisplayerTests
    {
        [TestMethod]
        public async Task DisplayMarkdownAsync_WithUri_RecordsCallWithoutValidation()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();
            var uri = new Uri("https://example.com/readme.md");

            await fake.DisplayMarkdownAsync(uri);

            Assert.AreEqual(1, fake.Calls.Count);
            Assert.AreEqual(uri, fake.Calls[0].Uri);
            Assert.IsNull(fake.Calls[0].Text);
            Assert.IsTrue(fake.Calls[0].AllowFollowingLinks);
            Assert.IsNull(fake.Calls[0].Validation, "URI overload should not perform validation");
        }

        [TestMethod]
        public async Task DisplayMarkdownAsync_WithText_RecordsCallAndValidates()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();
            var baseUri = new Uri("https://example.com/");

            await fake.DisplayMarkdownAsync("# Hello", baseUri, allowFollowingLinks: false);

            Assert.AreEqual(1, fake.Calls.Count);
            Assert.AreEqual("# Hello", fake.Calls[0].Text);
            Assert.AreEqual(baseUri, fake.Calls[0].BaseUri);
            Assert.IsFalse(fake.Calls[0].AllowFollowingLinks);
            Assert.IsNotNull(fake.Calls[0].Validation);
        }

        [TestMethod]
        public async Task AssertNoWarnings_OnCleanMarkdown_DoesNotThrow()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            await fake.DisplayMarkdownAsync("# Title\n\nSome **bold** and *italic* text.");

            fake.AssertNoWarnings();
            Assert.IsFalse(fake.HasUnhandledTypes);
            Assert.IsFalse(fake.HasUnknownEmphasisDelimiters);
            Assert.IsFalse(fake.HasUnusableLinkWarnings);
        }

        [TestMethod]
        public async Task FollowableLinks_AreDetected_WhenAllowFollowingLinksTrue()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            await fake.DisplayMarkdownAsync("See [docs](https://example.com/docs).", allowFollowingLinks: true);

            Assert.IsTrue(fake.HasUnusableLinkWarnings);
            Assert.AreEqual(1, fake.Calls[0].Validation!.FollowableLinks.Count);
            Assert.AreEqual("https://example.com/docs", fake.Calls[0].Validation!.FollowableLinks[0].Url);

            Assert.ThrowsExactly<MarkdownValidationException>(() => fake.AssertNoUnusableLinkWarnings());
            Assert.ThrowsExactly<MarkdownValidationException>(() => fake.AssertNoWarnings());
        }

        [TestMethod]
        public async Task FollowableLinks_DoNotWarn_WhenAllowFollowingLinksFalse()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            await fake.DisplayMarkdownAsync("See [docs](https://example.com/docs).", allowFollowingLinks: false);

            Assert.IsFalse(fake.HasUnusableLinkWarnings);
            Assert.AreEqual(1, fake.Calls[0].Validation!.FollowableLinks.Count, "Links are still recorded for inspection");
            fake.AssertNoUnusableLinkWarnings();
            fake.AssertNoWarnings();
        }

        [TestMethod]
        public async Task UnknownEmphasisDelimiters_AreNotProducedByStandardMarkdown()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            // All standard emphasis variants are handled (bold/italic/strike/sub/super/inserted/marked).
            await fake.DisplayMarkdownAsync(
                "*i* **b** ~s~ ~~ss~~ ^sup^ ++ins++ ==mark==",
                allowFollowingLinks: false);

            Assert.IsFalse(fake.HasUnknownEmphasisDelimiters);
            fake.AssertNoUnknownEmphasisDelimiters();
        }

        [TestMethod]
        public async Task AssertNoWarnings_AggregatesAcrossCalls()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            await fake.DisplayMarkdownAsync("# Clean", allowFollowingLinks: false);
            await fake.DisplayMarkdownAsync("[link](https://example.com)", allowFollowingLinks: true);

            Assert.IsTrue(fake.HasUnusableLinkWarnings);

            try
            {
                fake.AssertNoWarnings();
                Assert.Fail("Expected MarkdownValidationException");
            }
            catch (MarkdownValidationException ex)
            {
                StringAssert.Contains(ex.Message, "https://example.com");
            }
        }

        [TestMethod]
        public async Task DisplayMarkdownAsync_DoesNotMutateCallerOptions()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();
            var options = new DisplayOptions { IncludeDebug = false };

            await fake.DisplayMarkdownAsync("# Hi", options: options, allowFollowingLinks: false);

            Assert.IsFalse(options.IncludeDebug, "Validation must clone options before forcing IncludeDebug=true");
            Assert.AreSame(options, fake.Calls[0].Options);
        }
    }
}
