using System.Net;

namespace BoxOfYellow.ConsoleMarkdownRenderer.Fakes.Tests;

[TestClass]
public class ValidatingFakeMarkdownDisplayerTests
{
    [TestMethod]
    public async Task AssertNoUnhandledTypes_Throws_WhenUnhandledTypePresent()
    {
        // Drive the unhandled-type path the same way DisplayTests_UnhandledTypesDisplayedAsync
        // does: omit the AutolinkInline renderer so a plain <https://example.com> autolink
        // falls through and is surfaced in UnhandledTypes via the inspector hook.
        var fake = new ValidatingFakeMarkdownDisplayer
        {
            OmitAutolinkInlineRendererForTesting = true,
        };
        await fake.DisplayMarkdownAsync(
            "<https://example.com>",
            allowFollowingLinks: false);

        Assert.IsTrue(fake.HasUnhandledTypes);
        CollectionAssert.Contains(
            fake.Calls[0].Result.UnhandledTypes.Select(t => t.Name).ToList(),
            "AutolinkInline");

        var ex = Assert.ThrowsExactly<MarkdownValidationException>(fake.AssertNoUnhandledTypes);
        Assert.Contains("unhandled markdown object types", ex.Message);

        // AssertNoWarnings aggregates and surfaces the same failure.
        Assert.ThrowsExactly<MarkdownValidationException>(fake.AssertNoWarnings);
    }

    [TestMethod]
    public async Task AssertWithinRecursionLimits_Throws_WhenMaxDepthExceeded()
    {
        var responses = new Dictionary<string, string>
        {
            ["https://example.com/a.md"] = "Go [b](b.md)",
            ["https://example.com/b.md"] = "leaf",
        };
        var factory = new StubHttpClientFactory(uri =>
            responses.TryGetValue(uri.AbsoluteUri, out var body)
                ? new HttpResponseMessage(HttpStatusCode.OK)
                  {
                      Content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain"),
                  }
                : new HttpResponseMessage(HttpStatusCode.NotFound));

        // maxDepth=0 means "root only" — recursing to b.md (depth 1) is over the limit.
        var fake = new ValidatingFakeMarkdownDisplayer(factory, recursive: true, maxDepth: 0);
        await fake.DisplayMarkdownAsync(new Uri("https://example.com/a.md"), allowFollowingLinks: false);

        Assert.IsTrue(fake.ExceededMaxDepth);
        Assert.AreEqual(1, fake.Calls.Count, "Recursion past the depth limit must be skipped");

        var ex = Assert.ThrowsExactly<MarkdownValidationException>(fake.AssertWithinRecursionLimits);
        Assert.Contains("MaxDepth", ex.Message);
        Assert.ThrowsExactly<MarkdownValidationException>(fake.AssertNoWarnings);
    }

    [TestMethod]
    public async Task AssertWithinRecursionLimits_Throws_WhenMaxFilesExceeded()
    {
        var responses = new Dictionary<string, string>
        {
            ["https://example.com/a.md"] = "Go [b](b.md) and [c](c.md)",
            ["https://example.com/b.md"] = "leaf b",
            ["https://example.com/c.md"] = "leaf c",
        };
        var factory = new StubHttpClientFactory(uri =>
            responses.TryGetValue(uri.AbsoluteUri, out var body)
                ? new HttpResponseMessage(HttpStatusCode.OK)
                  {
                      Content = new StringContent(body, System.Text.Encoding.UTF8, "text/plain"),
                  }
                : new HttpResponseMessage(HttpStatusCode.NotFound));

        // maxFiles=1 means only the root counts; both children push us over the limit.
        var fake = new ValidatingFakeMarkdownDisplayer(factory, recursive: true, maxFiles: 1);
        await fake.DisplayMarkdownAsync(new Uri("https://example.com/a.md"), allowFollowingLinks: false);

        Assert.IsTrue(fake.ExceededMaxFiles);
        Assert.AreEqual(1, fake.FilesProcessed);

        var ex = Assert.ThrowsExactly<MarkdownValidationException>(fake.AssertWithinRecursionLimits);
        Assert.Contains("MaxFiles", ex.Message);
        Assert.ThrowsExactly<MarkdownValidationException>(fake.AssertNoWarnings);
    }

    [TestMethod]
    public void TestMethod1()
    {
    }

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