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

## See Also

- [ConsoleMarkdownRenderer README](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/README.md) — full library documentation
