# ConsoleMarkdownRenderer.Fakes

Test fakes for [ConsoleMarkdownRenderer](https://github.com/boxofyellow/ConsoleMarkdownRenderer), enabling dependency injection and unit testing of code that displays markdown.

## Installation

```shell
dotnet add package BoxOfYellow.ConsoleMarkdownRenderer.Fakes
```

## Usage

`FakeMarkdownDisplayer` implements `IMarkdownDisplayer` and records all calls for assertion:

```csharp
using ConsoleMarkdownRenderer;
using ConsoleMarkdownRenderer.Fakes;

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

`ValidatingFakeMarkdownDisplayer` exposes the same `IMarkdownDisplayer` surface and recording behavior as `FakeMarkdownDisplayer`, but additionally inspects each text-based call for the warning conditions that `MarkdownDisplayer` would surface at runtime:

- **Unhandled markdown object types** — markdown elements no `ConsoleObjectRenderer` knows how to render (the `IncludeDebug` "Unhandled `<Name>`" warning).
- **Unusable links** — followable links present together with `allowFollowingLinks: true`, which would trigger the "Non-interactive terminal detected. The following links are available but cannot be followed interactively" warning in CI/non-interactive terminals.
- **Unknown emphasis delimiters** — emphasis inlines that fall into the catch-all branch in `ConsoleEmphasisInlineRenderer` (rendered as e.g. `(!1)`).

```csharp
using ConsoleMarkdownRenderer;
using ConsoleMarkdownRenderer.Fakes;

var fake = new ValidatingFakeMarkdownDisplayer();

await fake.DisplayMarkdownAsync("# Title\n\nSome **bold** text.", allowFollowingLinks: false);

// Throws MarkdownValidationException if any warning condition is detected.
fake.AssertNoWarnings();

// Or assert each category individually:
fake.AssertNoUnhandledTypes();
fake.AssertNoUnknownEmphasisDelimiters();
fake.AssertNoUnusableLinkWarnings();
```

You can also inspect the per-call validation results — useful for learning exactly what tripped the unknown-delimiter catch-all:

```csharp
foreach (var call in fake.Calls)
{
    var v = call.Validation;
    if (v == null) continue; // URI overload; no validation performed
    foreach (var d in v.UnknownEmphasisDelimiters)
    {
        Console.WriteLine($"Unknown emphasis delimiter: {d.DelimiterChar} x{d.DelimiterCount}");
    }
    foreach (var t in v.UnhandledTypes)
    {
        Console.WriteLine($"Unhandled markdown type: {t.Name}");
    }
    foreach (var link in v.FollowableLinks)
    {
        Console.WriteLine($"Followable link: {link.Url}");
    }
}
```

> Note: validation only runs for the text overload of `DisplayMarkdownAsync`. The URI overload records the call but does not perform any network I/O, so `ValidatedDisplayCall.Validation` is `null` for those calls.

## See Also

- [ConsoleMarkdownRenderer README](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/README.md) — full library documentation