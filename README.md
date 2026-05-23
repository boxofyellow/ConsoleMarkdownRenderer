| Area | Badges |
|---|---|
| Health | [![Build Status](https://github.com/boxofyellow/ConsoleMarkdownRenderer/actions/workflows/ci.yml/badge.svg)](https://github.com/boxofyellow/ConsoleMarkdownRenderer/actions/workflows/ci.yml) [![API Compat](https://github.com/boxofyellow/ConsoleMarkdownRenderer/actions/workflows/api-compat.yml/badge.svg)](https://github.com/boxofyellow/ConsoleMarkdownRenderer/actions/workflows/api-compat.yml) [![codecov](https://codecov.io/gh/boxofyellow/ConsoleMarkdownRenderer/branch/main/graph/badge.svg?token=2VSOFO21BN)](https://codecov.io/gh/boxofyellow/ConsoleMarkdownRenderer) |
| Release | [![License](https://img.shields.io/github/license/boxofyellow/ConsoleMarkdownRenderer)](LICENSE) [![GitHub Release Date](https://img.shields.io/github/release-date/boxofyellow/ConsoleMarkdownRenderer?label=released)](https://github.com/boxofyellow/ConsoleMarkdownRenderer/releases) [![GitHub commits since latest release (by date)](https://img.shields.io/github/commits-since/boxofyellow/ConsoleMarkdownRenderer/latest?label=new+commits)](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/docs/CHANGELOG.md#upcoming-changes) |
| nuget | [![nuget BoxOfYellow.ConsoleMarkdownRenderer](https://img.shields.io/nuget/v/BoxOfYellow.ConsoleMarkdownRenderer.svg?label=BoxOfYellow.ConsoleMarkdownRenderer)](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer) [![Downloads](https://img.shields.io/nuget/dt/BoxOfYellow.ConsoleMarkdownRenderer.svg?label=Downloads)](https://www.nuget.org/stats/packages/BoxOfYellow.ConsoleMarkdownRenderer?groupby=Version) [![nuget BoxOfYellow.ConsoleMarkdownRenderer.Fakes](https://img.shields.io/nuget/v/BoxOfYellow.ConsoleMarkdownRenderer.svg?label=BoxOfYellow.ConsoleMarkdownRenderer.Fakes)](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer.Fakes) [![nuget BoxOfYellow.ConsoleMarkdownRenderer.Spectre](https://img.shields.io/nuget/v/BoxOfYellow.ConsoleMarkdownRenderer.Spectre.svg?label=BoxOfYellow.ConsoleMarkdownRenderer.Spectre)](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer.Spectre) |

*I* have markdown files, *you* have markdown files, we *all* have markdown files...

---

We create them to document various parts of projects.  Sometimes that documentation would be helpful _while_ folks are using those projects.  And that's where this library comes in.  This library provides support for displaying markdown within the console and provides a simple navigation list of links and images within the document.  When items from the list are selected their content will be shown inline when possible (aka it's another markdown file, or it's an image and the console appears to be using [iTerm2]((https://iterm2.com/)))

I will totally admit `README.md` files and response that is displayed with `--help` are not 100% interchangeable, but there is a lot of overlap :slightly_smiling_face:

## Using it is simple

### Option 1: Static API

Just call the one public method from the static [Displayer.cs](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/Displayer.cs) class called `DisplayMarkdownAsync` it accepts the following parameters

| name | type | description | required/default |
| - | - | - | - |
| `uri` | `Uri` | The [Uri](https://en.wikipedia.org/wiki/Uniform_Resource_Identifier) that is either a file containing your markdown, or the web address where said content can be downloaded | Yes |
| `options` | `DisplayOptions` | Properties and styles to apply to the Markdown elements | no / `null` |
| `allowFollowingLinks` | `bool` | A flag, when set to true, the list of links will be provided, when false the list is omitted | no / `true` |

It has a second overload

| name | type | description | required/default |
| - | - | - | - |
| `text` | `string` | the text to display | Yes |
| `uriBase` | `Uri` | The [Uri](https://en.wikipedia.org/wiki/Uniform_Resource_Identifier) base for all links | no / the current working directory |
| `options` | `DisplayOptions` | Properties and styles to apply to the Markdown elements | no / `null` |
| `allowFollowingLinks` | `bool` | A flag, when set to true, the list of links will be provided, when false the list is omitted | no / `true` |

### Option 2: Injectable API (`IMarkdownDisplayer`)

For dependency injection and testability, the library provides an `IMarkdownDisplayer` interface with the same display methods.
`MarkdownDisplayer` implements `IDisposable`; use a `using` declaration or let your DI container manage the lifetime:

```csharp
// Short-lived / direct use
using IMarkdownDisplayer displayer = new MarkdownDisplayer();

// Display from a URI
await displayer.DisplayMarkdownAsync(uri, options, allowFollowingLinks: true);

// Display from text
await displayer.DisplayMarkdownAsync(markdownText, baseUri, options);
```

For DI registration without an `IHttpClientFactory` (the displayer manages its own `HttpClient`):

```csharp
// Register as scoped so the container disposes it automatically
services.AddScoped<IMarkdownDisplayer, MarkdownDisplayer>();
```

To pipe the container's `IHttpClientFactory` through to `MarkdownDisplayer`, use a factory delegate:

```csharp
// services.AddHttpClient() registers IHttpClientFactory
services.AddHttpClient();
services.AddScoped<IMarkdownDisplayer>(sp =>
    new MarkdownDisplayer(sp.GetRequiredService<IHttpClientFactory>()));

// Or with a named client:
services.AddHttpClient("myClient", client => { /* configure */ });
services.AddScoped<IMarkdownDisplayer>(sp =>
    new MarkdownDisplayer(sp.GetRequiredService<IHttpClientFactory>(), httpClientName: "myClient"));
```

#### Supplying a custom `IHttpClientFactory`

If your application already registers an `IHttpClientFactory` (e.g. in an ASP.NET Core or `IHostBuilder` setup),
you can pass it to `MarkdownDisplayer` so that all HTTP requests go through your configured factory.
The factory — and the clients it produces — is owned and managed by the caller.

```csharp
// Using the default (unnamed) client from the factory:
using IMarkdownDisplayer displayer = new MarkdownDisplayer(httpClientFactory);

// Using a named client registered in your DI container:
using IMarkdownDisplayer displayer = new MarkdownDisplayer(httpClientFactory, httpClientName: "myClient");
```

When no factory is supplied, `MarkdownDisplayer` creates and reuses its own internal `HttpClient` with a
`SocketsHttpHandler` configured with a 15-minute pooled connection lifetime.

### Testing with Fakes

The `ConsoleMarkdownRenderer.Fakes` package provides an out-of-the-box test double:

```csharp
// Install: BoxOfYellow.ConsoleMarkdownRenderer.Fakes

var fakeDisplayer = new FakeMarkdownDisplayer();
await fakeDisplayer.DisplayMarkdownAsync(new Uri("https://example.com/readme.md"));

// Assert on recorded calls
Assert.AreEqual(1, fakeDisplayer.Calls.Count);
Assert.AreEqual("https://example.com/readme.md", fakeDisplayer.Calls[0].Uri?.ToString());
```

See [ConsoleMarkdownRenderer.ExampleTests](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/ConsoleMarkdownRenderer.ExampleTests) for more examples.

### Embedding markdown in your own Spectre.Console document

If you are already building a Spectre.Console document and want to splice rendered markdown into it — without the interactive prompt loop, HTTP downloading, or JSON-serializable options from the main package — use the `BoxOfYellow.ConsoleMarkdownRenderer.Spectre` package directly.

```csharp
// Install: BoxOfYellow.ConsoleMarkdownRenderer.Spectre

var md = new SpectreMarkdownRenderer(new SpectreDisplayOptions
{
    CodeBlock = new Style(Color.Grey85, Color.Grey15),
    TableBorder = TableBorder.Rounded,
});

var result = md.Render(File.ReadAllText("notes.md"));

// Splice into your own document
var grid = new Grid().AddColumn().AddColumn();
grid.AddRow(new Panel("Sidebar"), result.Root ?? Text.Empty);
AnsiConsole.Write(grid);

// Process extracted links however you like
foreach (var link in result.Links)
{
    AnsiConsole.MarkupLine($"[blue]{link.Url}[/]");
}
```

Each call to `Render` is stateless — no link accumulation or mutable state leaks between calls.

---

Checkout [ConsoleMarkdownRenderer.Example](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/ConsoleMarkdownRenderer.Example) to see it in use
![](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/docs/example.png)

## Default Styling

The defaults for the styling for the Markdown elements can be seen in the examples listed above.  The details for that style can be changed by creating an instance of [DisplayOptions](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/DisplayOptions.cs) and overwriting any that you see fit.

This object is more or less a bag of styles to use for the various parts of your markdown document.  There are a few exceptions

| name | type | description | default
| - | - | - | - |
| `Headers` | `List<IHeaderStyle>` | Used as overrides of `Header`, an order lists of styles to use for different level of headers | by default contains a single `FigletTextStyle` for H1; deeper levels fall back to `Header` |
| `WrapHeader` | `bool` | When `true`, will wrap Headers with `#`'s to denote the level (only applies to plain `TextStyle` heading entries; ignored for `FigletTextStyle`) | yes / `true` |
| `IncludeDebug` | `bool` | When `true` will display all content within in boxes to help visualize how the content is being interpreted by the tool | off / `false` |
| `ShowFencedCodeBlockInfo` | `bool` | When `true`, displays the info field (e.g., language identifier) from fenced code blocks | off / `false` |

### Rendering headings as FIGlet ASCII art

Each entry in `Headers` (and the `Header` fallback) is an [`IHeaderStyle`](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/Styling/IHeaderStyle.cs).  Two implementations ship with the library:

- [`TextStyle`](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/Styling/TextStyle.cs) — inline styled markup, optionally wrapped with `#` characters via `WrapHeader`.
- [`FigletTextStyle`](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/Styling/FigletTextStyle.cs) — renders the heading text as large ASCII art through Spectre.Console's [`FigletText`](https://spectreconsole.net/widgets/figlet) widget.  Exposes an optional `Justification` (`TextJustification.Left`, `Right`, or `Center`) and an optional `Foreground` color.  Instances are created via the static factory methods: `FigletTextStyle.Create(…)` for the default FIGlet font, and the async `FigletTextStyle.CreateAsync(fontPath, …)` to load a custom FIGlet font (`.flf`).  Because `FigletText` does not support decoration or background, `FigletTextStyle` is a peer of `TextStyle` (both implement `IHeaderStyle`) rather than a subclass.

**Default behavior.** By default the first entry in `Headers` is a `FigletTextStyle`, so top-level (`#`) headings render as FIGlet ASCII art out of the box.  Deeper levels (`##`, `###`, …) fall through to `Header` and continue to render as styled, `#`-wrapped markup.  To opt H1 into the inline styled markup behavior, clear `Headers` so every level falls through to `Header`:

```csharp
var options = new DisplayOptions();
options.Headers.Clear(); // every level (including H1) uses the styled-markup `Header` fallback
```

To opt a different level (or the `Header` fallback) in to FIGlet rendering, assign a `FigletTextStyle`:

```csharp
var options = new DisplayOptions();
options.Headers.Add(FigletTextStyle.Create(
    justification: TextJustification.Center,
    foreground: TextColor.Blue));    // applies to '##' (H2)
```

## Supporting packages 

It's also important to give credit where credit is due, this library is really just glue for the following packages
- [Markdig](https://www.nuget.org/packages/Markdig/) for parsing the markdown
- [Spectre.Console](https://www.nuget.org/packages/Spectre.Console/) for display rich formatting within the console
- [RomanNumeral](https://www.nuget.org/packages/RomanNumeral/) for minimal roman numeral processing

## Contributing

Contributions are welcome, please see [CONTRIBUTING.md](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/CODE_OF_CONDUCT.md)

## Credits

The `shadow.flf` FIGlet font included in [ConsoleMarkdownRenderer.Tests/data/fonts](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/ConsoleMarkdownRenderer.Tests/data/fonts/shadow.flf) is "Shadow" by Glenn Chappell (6/93), distributed with the [cmatsuoka/figlet](https://github.com/cmatsuoka/figlet) project, and is used here only as a test resource.
