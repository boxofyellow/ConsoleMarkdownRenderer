# Migration Guide: v0.11.x → v0.12.0

Version 0.12.0 introduces a new `BoxOfYellow.ConsoleMarkdownRenderer.Spectre` NuGet package and moves two public types into it. All rendering output is unchanged.

| # | Change | Action required by consumers? |
|---|---|---|
| 1 | Removed `BoxOfYellow.ConsoleMarkdownRenderer.DisplayOptions.PrettyPrintJson` | This can be replaced with whatever `System.Text.Json.JsonSerializerOptions` you like. |
| 2 | The two `BoxOfYellow.ConsoleMarkdownRenderer.DisplayOptions.DeserializeAsync` methods have been given an optional parameter that can be used provide a `DisplayOptions` instead of using the default values | It is optional, so no actions should be required |
| 3 | `LinkItem` moved from `BoxOfYellow.ConsoleMarkdownRenderer` (main package) to `BoxOfYellow.ConsoleMarkdownRenderer.Spectre` (new package). | **Consumers using `LinkItem`**: add a reference to the new package and update the `using` directive. |
| 4 | `UnknownEmphasisDelimiter` moved from `BoxOfYellow.ConsoleMarkdownRenderer` (main package) to `BoxOfYellow.ConsoleMarkdownRenderer.Spectre` (new package). | **Consumers using `UnknownEmphasisDelimiter`**: add a reference to the new package and update the `using` directive. |

---

## Breaking change #1 — `BoxOfYellow.ConsoleMarkdownRenderer.DisplayOptions.PrettyPrintJson` removal

### Why

This property was not used.

### What you need to do

If you were using this property, replace it with whatever `System.Text.Json.JsonSerializerOptions` you like, for example:

```csharp
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true,
    Converters = { new JsonStringEnumConverter() },
};
```

## Breaking change #2 — `BoxOfYellow.ConsoleMarkdownRenderer.DisplayOptions.DeserializeAsync` additional optional parameter

### Why

The two `DeserializeAsync` methods were using the default values from `new DisplayOptions()` when deserializing. This was not always desirable, so an optional parameter was added to allow callers to provide their own `DisplayOptions` instance.  The new factory allows more flexibility

For example if you don't want any default values to be applied and only want values that are specified in the JSON, you can pass `DisplayOptions.Empty()` as the factory.

```csharp
var options = await DisplayOptions.DeserializeAsync(json, createObject: DisplayOptions.Empty);
```

Or if you want to combine the result from multiple JSON files, you can pass a pre-existing object.

```csharp
var options = await DisplayOptions.DeserializeAsync(json);

...

var options = await DisplayOptions.DeserializeAsync(json2, createObject: () => options);
```

### What you need to do

The old behavior is preserved by providing `null` for the new factory. 

## Breaking change #3 — `LinkItem` is now in `BoxOfYellow.ConsoleMarkdownRenderer.Spectre`

### Why

The new `BoxOfYellow.ConsoleMarkdownRenderer.Spectre` package provides a stateless, low-level `SpectreMarkdownRenderer` API for callers who are already managing a Spectre.Console document. `LinkItem` is part of its `MarkdownRenderResult` return type, so it belongs in the same package.

### What changed

```csharp
// Before (v0.11.x) — in BoxOfYellow.ConsoleMarkdownRenderer
using BoxOfYellow.ConsoleMarkdownRenderer;
LinkItem link = result.Links[0];

// After (v0.12.0) — in BoxOfYellow.ConsoleMarkdownRenderer.Spectre
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
LinkItem link = result.Links[0];
```

### What you need to do

1. Add a NuGet reference to `BoxOfYellow.ConsoleMarkdownRenderer.Spectre`:

   ```xml
   <PackageReference Include="BoxOfYellow.ConsoleMarkdownRenderer.Spectre" Version="0.12.0" />
   ```

2. Replace `using BoxOfYellow.ConsoleMarkdownRenderer;` with `using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;` wherever `LinkItem` is referenced.

---

## Breaking change #4 — `UnknownEmphasisDelimiter` is now in `BoxOfYellow.ConsoleMarkdownRenderer.Spectre`

### Why

`UnknownEmphasisDelimiter` is returned by `MarkdownRenderResult.UnknownEmphasisDelimiters`, which is part of the Spectre package's public API.

### What changed

```csharp
// Before (v0.11.x)
using BoxOfYellow.ConsoleMarkdownRenderer;
UnknownEmphasisDelimiter d = result.UnknownEmphasisDelimiters[0];

// After (v0.12.0)
using BoxOfYellow.ConsoleMarkdownRenderer.Spectre;
UnknownEmphasisDelimiter d = result.UnknownEmphasisDelimiters[0];
```

### What you need to do

Same as for `LinkItem` above — add a reference to `BoxOfYellow.ConsoleMarkdownRenderer.Spectre` and update the `using` directive.

---

## New package — `BoxOfYellow.ConsoleMarkdownRenderer.Spectre`

The new package exposes `SpectreMarkdownRenderer`, which renders Markdown to a Spectre.Console `IRenderable` tree without any interactive prompts, HTTP downloading, or JSON-serializable option types.

```csharp
var renderer = new SpectreMarkdownRenderer(new SpectreDisplayOptions
{
    CodeBlock = new Style(Color.Grey85, Color.Grey15),
    TableBorder = TableBorder.Rounded,
});

MarkdownRenderResult result = renderer.Render(File.ReadAllText("notes.md"));

var grid = new Grid().AddColumn().AddColumn();
grid.AddRow(new Panel("Sidebar"), result.Root ?? Text.Empty);
AnsiConsole.Write(grid);

foreach (LinkItem link in result.Links)
{
    Console.WriteLine($"  {link.Content} → {link.Url}");
}
```

The existing `BoxOfYellow.ConsoleMarkdownRenderer` package continues to work exactly as before; it now depends on the Spectre package transitively, so installing it automatically pulls in `BoxOfYellow.ConsoleMarkdownRenderer.Spectre`.