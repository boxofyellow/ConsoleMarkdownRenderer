using System.Net;
using ConsoleMarkdownRenderer.Fakes;
using Spectre.Console;
using Spectre.Console.Testing;

namespace ConsoleMarkdownRenderer.ExampleTests
{
    [TestClass]
    public class ValidatingFakeMarkdownDisplayerTests
    {
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
            Assert.IsFalse(fake.Calls[0].IsRecursive);
            Assert.IsNull(fake.Calls[0].ParentCall);
            Assert.IsNotNull(fake.Calls[0].Validation);
        }

        [TestMethod]
        public async Task AssertNoWarnings_OnCleanMarkdown_DoesNotThrow()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            await fake.DisplayMarkdownAsync(
                "# Title\n\nSome **bold** and *italic* text.",
                allowFollowingLinks: false);

            fake.AssertNoWarnings();
            Assert.IsFalse(fake.HasUnhandledTypes);
            Assert.IsFalse(fake.HasUnknownEmphasisDelimiters);
            Assert.IsFalse(fake.HasUnusableLinkWarnings);
        }

        [TestMethod]
        public async Task FollowableLinks_AreDetected_WhenAllowFollowingLinksTrue()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            await fake.DisplayMarkdownAsync(
                "See [docs](https://example.com/docs).",
                allowFollowingLinks: true);

            Assert.IsTrue(fake.HasUnusableLinkWarnings);
            Assert.AreEqual(1, fake.Calls[0].Validation.FollowableLinks.Count);
            Assert.AreEqual("https://example.com/docs", fake.Calls[0].Validation.FollowableLinks[0].Url);

            Assert.ThrowsExactly<MarkdownValidationException>(() => fake.AssertNoUnusableLinkWarnings());
            Assert.ThrowsExactly<MarkdownValidationException>(() => fake.AssertNoWarnings());
        }

        [TestMethod]
        public async Task FollowableLinks_DoNotWarn_WhenAllowFollowingLinksFalse()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            await fake.DisplayMarkdownAsync(
                "See [docs](https://example.com/docs).",
                allowFollowingLinks: false);

            Assert.IsFalse(fake.HasUnusableLinkWarnings);
            Assert.AreEqual(1, fake.Calls[0].Validation.FollowableLinks.Count, "Links are still recorded for inspection");
            fake.AssertNoUnusableLinkWarnings();
            fake.AssertNoWarnings();
        }

        [TestMethod]
        public async Task UnknownEmphasisDelimiters_AreNotProducedByStandardMarkdown()
        {
            var fake = new ValidatingFakeMarkdownDisplayer();

            // All standard emphasis variants are handled.
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

            await fake.DisplayMarkdownAsync("[link](https://example.com)", allowFollowingLinks: true);
            await fake.DisplayMarkdownAsync("# Clean", allowFollowingLinks: false);

            Assert.IsTrue(fake.HasUnusableLinkWarnings);

            var ex = Assert.ThrowsExactly<MarkdownValidationException>(() => fake.AssertNoWarnings());
            StringAssert.Contains(ex.Message, "https://example.com");
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

        [TestMethod]
        public async Task DisplayMarkdownAsync_RestoresAnsiConsoleAfterEachCall()
        {
            var sentinel = new TestConsole();
            var prev = AnsiConsole.Console;
            AnsiConsole.Console = sentinel;
            try
            {
                var fake = new ValidatingFakeMarkdownDisplayer();
                await fake.DisplayMarkdownAsync("# Hi", allowFollowingLinks: false);
                Assert.AreSame(sentinel, AnsiConsole.Console);
            }
            finally
            {
                AnsiConsole.Console = prev;
                sentinel.Dispose();
            }
        }

        [TestMethod]
        public async Task DisplayMarkdownAsync_WithUri_DelegatesViaIHttpClientFactory()
        {
            var factory = new StubHttpClientFactory(uri =>
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("# Remote\n\nClean content.", System.Text.Encoding.UTF8, "text/plain"),
                });

            var fake = new ValidatingFakeMarkdownDisplayer(factory);
            var uri = new Uri("https://example.com/readme.md");

            await fake.DisplayMarkdownAsync(uri, allowFollowingLinks: false);

            Assert.AreEqual(1, fake.Calls.Count);
            Assert.AreEqual(uri, fake.Calls[0].Uri);
            Assert.IsNotNull(fake.Calls[0].Validation);
            fake.AssertNoWarnings();
        }

        [TestMethod]
        public async Task Recursive_FollowsMarkdownLinks_AndAvoidsCycles()
        {
            // A -> B -> A (cycle), A -> C
            var responses = new Dictionary<string, string>
            {
                ["https://example.com/a.md"] = "Root [b](b.md) and [c](c.md)",
                ["https://example.com/b.md"] = "Back to [a](a.md)",
                ["https://example.com/c.md"] = "Leaf with **bold**.",
            };

            var factory = new StubHttpClientFactory(uri =>
            {
                if (responses.TryGetValue(uri.AbsoluteUri, out var body))
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain"),
                    };
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });

            var fake = new ValidatingFakeMarkdownDisplayer(factory, recursive: true);
            await fake.DisplayMarkdownAsync(new Uri("https://example.com/a.md"), allowFollowingLinks: false);

            // Three distinct documents, recorded once each (cycle to A is suppressed).
            Assert.AreEqual(3, fake.Calls.Count, "Expected exactly one call per distinct URI");
            CollectionAssert.AreEquivalent(
                new[]
                {
                    "https://example.com/a.md",
                    "https://example.com/b.md",
                    "https://example.com/c.md",
                },
                fake.Calls.Select(c => c.Uri!.AbsoluteUri).ToList());

            // Top-level call is the root; the others are recursive children.
            Assert.AreEqual(1, fake.Calls.Count(c => !c.IsRecursive));
            Assert.AreEqual(2, fake.Calls.Count(c => c.IsRecursive));
        }

        [TestMethod]
        public async Task Recursive_FromTextOverload_FollowsLinksRelativeToBaseUri()
        {
            var factory = new StubHttpClientFactory(uri => uri.AbsoluteUri switch
            {
                "https://example.com/child.md" => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("# Child", System.Text.Encoding.UTF8, "text/plain"),
                },
                _ => new HttpResponseMessage(HttpStatusCode.NotFound),
            });

            var fake = new ValidatingFakeMarkdownDisplayer(factory, recursive: true);
            await fake.DisplayMarkdownAsync(
                "Go to [child](child.md)",
                baseUri: new Uri("https://example.com/parent/"),
                allowFollowingLinks: false);

            Assert.AreEqual(2, fake.Calls.Count);
            Assert.IsNull(fake.Calls[0].Uri);                                   // text root
            Assert.AreEqual(new Uri("https://example.com/parent/child.md"), fake.Calls[1].Uri); // recursive child
            Assert.AreSame(fake.Calls[0], fake.Calls[1].ParentCall);
        }

        // ---- helpers ----

        private sealed class StubHttpClientFactory : IHttpClientFactory
        {
            private readonly Func<Uri, HttpResponseMessage> _responder;
            public StubHttpClientFactory(Func<Uri, HttpResponseMessage> responder) => _responder = responder;
            public HttpClient CreateClient(string name) => new(new StubHandler(_responder), disposeHandler: true);

            private sealed class StubHandler : HttpMessageHandler
            {
                private readonly Func<Uri, HttpResponseMessage> _responder;
                public StubHandler(Func<Uri, HttpResponseMessage> responder) => _responder = responder;
                protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
                    => Task.FromResult(_responder(request.RequestUri!));
            }
        }
    }
}
