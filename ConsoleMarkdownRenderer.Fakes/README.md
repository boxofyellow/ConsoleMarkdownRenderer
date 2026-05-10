# ConsoleMarkdownRenderer.Fakes

Test fakes for [ConsoleMarkdownRenderer](https://github.com/boxofyellow/ConsoleMarkdownRenderer), enabling dependency injection and unit testing of code that displays markdown.

## Installation

```shell
dotnet add package BoxOfYellow.ConsoleMarkdownRenderer.Fakes
```

## Usage

`FakeMarkdownDisplayer` implements `IMarkdownDisplayer` and records all calls for assertion:

```csharp
using BoxOfYellow.ConsoleMarkdownRenderer;
using BoxOfYellow.ConsoleMarkdownRenderer.Fakes;

var fake = new FakeMarkdownDisplayer();

// Pass the fake wherever IMarkdownDisplayer is expected
await fake.DisplayMarkdownAsync(new Uri("https://example.com/readme.md"));

// Assert calls were made as expected
Assert.AreEqual(1, fake.Calls.Count);
Assert.AreEqual("https://example.com/readme.md", fake.Calls[0].Uri?.ToString());
```

Both overloads (URI-based and text-based) are recorded:

```csharp
await fake.DisplayMarkdownAsync("# Hello", new Uri("file:///base/"));
Assert.AreEqual("# Hello", fake.Calls[0].Text);
```

## Asserting no warnings with `ValidatingFakeMarkdownDisplayer`

`ValidatingFakeMarkdownDisplayer` implements `IMarkdownDisplayer` like `FakeMarkdownDisplayer`, but under the covers each call is delegated to a real `MarkdownDisplayer` wired up against an isolated `Spectre.Console.Testing.TestConsole`. That means:

- The validation logic stays in lock-step with the real renderer/displayer — there is no parallel "what counts as a warning" implementation to drift.
- Nothing is written to the surrounding test console; the previous `AnsiConsole.Console` is restored on exit (even on exception).
- The displayer is forced into the non-interactive code path so it never tries to prompt during validation.

For each call, the fake captures structured warning data from the renderer:

- **Unhandled markdown object types** — markdown elements no `ConsoleObjectRenderer` knows how to render (the `IncludeDebug` "Unhandled `<Name>`" warning).
- **Unusable links** — followable links present together with `allowFollowingLinks: true`, which would trigger the "Non-interactive terminal detected. The following links are available but cannot be followed interactively" warning in CI/non-interactive terminals.
- **Unknown emphasis delimiters** — emphasis inlines that fall into the catch-all branch in `ConsoleEmphasisInlineRenderer` (rendered as e.g. `(!1)`).

```csharp
using BoxOfYellow.ConsoleMarkdownRenderer;
using BoxOfYellow.ConsoleMarkdownRenderer.Fakes;

var fake = new ValidatingFakeMarkdownDisplayer();

await fake.DisplayMarkdownAsync("# Title\n\nSome **bold** text.", allowFollowingLinks: true);

// Throws MarkdownValidationException if any warning condition is detected.
fake.AssertNoWarnings();

// Or assert each category individually:
fake.AssertNoUnhandledTypes();
fake.AssertNoUnknownEmphasisDelimiters();
fake.AssertNoUnusableLinkWarnings();
```

You can also inspect the per-call validation results — useful for learning exactly what tripped the unknown-emphasis catch-all:

```csharp
foreach (var call in fake.Calls)
{
    foreach (var d in call.Validation.UnknownEmphasisDelimiters)
        Console.WriteLine($"Unknown emphasis delimiter: {d.DelimiterChar} x{d.DelimiterCount}");
    foreach (var t in call.Validation.UnhandledTypes)
        Console.WriteLine($"Unhandled markdown type: {t.Name}");
    foreach (var link in call.Validation.FollowableLinks)
        Console.WriteLine($"Followable link: {link.Url}");
}
```

### Recursive validation

Set `recursive: true` in the constructor and the fake will follow every markdown link discovered in each rendered document, validate it the same way, and record a child call. Visited absolute URIs are tracked per top-level call to avoid cycles. Recursion is bounded by `maxDepth` (default `10`) and `maxFiles` (default `100`) guardrails — `AssertNoWarnings()` will also fail if either was hit, and you can inspect `MaxDepthReached`/`FilesProcessed`/`ExceededMaxDepth`/`ExceededMaxFiles` directly.

```csharp
var fake = new ValidatingFakeMarkdownDisplayer(httpClientFactory, recursive: true);
await fake.DisplayMarkdownAsync(new Uri("https://example.com/index.md"), allowFollowingLinks: true);

fake.AssertNoWarnings(); // covers the root document AND every linked .md
foreach (var call in fake.Calls.Where(c => c.IsRecursive))
    Console.WriteLine($"Recursively validated (depth {call.Depth}): {call.Uri}");
```

### Supplying an `IHttpClientFactory`

Because each call is delegated to a real `MarkdownDisplayer`, the URI overload (and recursive link-following) will perform real HTTP requests against any web URI. Hand the fake an `IHttpClientFactory` to stub those requests out:

```csharp
var fake = new ValidatingFakeMarkdownDisplayer(myFactory);
```

A few easy ways to build a factory that maps URIs to canned responses:

- **[`RichardSzalay.MockHttp`](https://github.com/richardszalay/mockhttp)** — `mockHttp.When("https://example.com/*.md").Respond("text/plain", "# stub");`, then wrap the resulting `HttpClient` in a one-line `IHttpClientFactory`.
- **A small hand-rolled `DelegatingHandler`** when you don't want a new dependency:

  ```csharp
  sealed class StubHttpClientFactory : IHttpClientFactory
  {
      readonly Func<Uri, HttpResponseMessage> _responder;
      public StubHttpClientFactory(Func<Uri, HttpResponseMessage> responder) => _responder = responder;
      public HttpClient CreateClient(string name) => new(new Handler(_responder));

      sealed class Handler(Func<Uri, HttpResponseMessage> responder) : HttpMessageHandler
      {
          protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
              => Task.FromResult(responder(req.RequestUri!));
      }
  }
  ```

- **Moq + a custom `HttpMessageHandler`** if you already use Moq in your test suite.

> **Thread safety:** `ValidatingFakeMarkdownDisplayer` temporarily swaps `AnsiConsole.Console` on every call. Like other things that touch `AnsiConsole.Console`, do not run multiple validations concurrently in the same process.

## See Also

- [ConsoleMarkdownRenderer README](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/README.md) — full library documentation