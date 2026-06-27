# BoxOfYellow.ConsoleMarkdownRenderer.Spectre

[![nuget](https://img.shields.io/nuget/v/BoxOfYellow.ConsoleMarkdownRenderer.Spectre.svg?label=BoxOfYellow.ConsoleMarkdownRenderer.Spectre)](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer.Spectre)

Low-level **markdown text → Spectre.Console `IRenderable`** package.

For consumers who are already building their own [Spectre.Console](https://spectreconsole.net/) document and want to splice rendered markdown into it — without the interactive prompt loop or HTTP downloading of the main [`BoxOfYellow.ConsoleMarkdownRenderer`](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer) package.

See the [main README](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/README.md) for the full project overview.

---

## Quick start

```csharp
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;

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

---

## Key types

| Type | Description |
|---|---|
| `SpectreMarkdownRenderer` | Entry point. `Render(string)` returns a fresh `MarkdownRenderResult` per call. |
| `MarkdownRenderResult` | `Root` (`IRenderable?`), `Links`, `UnhandledTypes`, `UnknownEmphasisDelimiters`. |
| `SpectreDisplayOptions` | All style properties use Spectre.Console types directly (`Style`, `TableBorder`, etc.). Supports JSON serialization. |
| `ISpectreHeaderStyle` | Interface for heading styles; three built-in implementations below. |
| `SpectreFigletHeaderStyle` | Renders headings as large ASCII art via Spectre.Console `FigletText`. |
| `SpectreRuleHeaderStyle` | Renders headings as a titled horizontal rule (e.g. `──── Overview ────`). |
| `SpectreStyleHeaderStyle` | Renders headings as inline styled markup. |
| `LinkItem` | A hyperlink found during rendering: `Url`, `Content`, `IsImage`. |
| `UnknownEmphasisDelimiter` | An emphasis delimiter that fell into the catch-all rendering branch. |

> [!Note]
> The types within the Support namespace are not intended for public consumption. They are used internally to implement the rendering logic and may change without notice.
> If you find yourself needing to use them, please open an issue and we can discuss whether they should be promoted to public API.

---

## Relationship to the main package

`BoxOfYellow.ConsoleMarkdownRenderer` depends on this package and layers the interactive prompt loop, HTTP downloading, image inlining, and on top of it.

If you need the full experience (follow links, display images, etc.), use the main package.
If you only need to render markdown to an `IRenderable`, use this package directly.