# Migration Guide: v0.9.x → v0.10.0

Version 0.10.0 contains **two breaking changes**.

| # | Change | Action required by consumers? |
|---|---|---|
| 1 | The renderer building blocks under `ConsoleMarkdownRenderer.ObjectRenderers` are now `internal`. | **Most consumers: none.** They were never intended to be part of the public API. |
| 2 | All namespaces shipped by the NuGet packages are now prefixed with `BoxOfYellow.`. | **Yes — small `using` updates.** |

The published NuGet packages are unchanged:

- [`BoxOfYellow.ConsoleMarkdownRenderer`](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer)
- [`BoxOfYellow.ConsoleMarkdownRenderer.Fakes`](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer.Fakes)

---

## Breaking change #1 — `ObjectRenderers` types are now `internal`

### Why

Everything under `BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers` (`ConsoleRenderer`, `ConsoleRendererBase<T>`, `ConsoleObjectRenderer<T>`, the per-element renderers like `ConsoleLinkInlineRenderer`, the frame helpers, `TempFileManager`, `Utilities`, etc.) is implementation detail of `MarkdownDisplayer`/`Displayer`. They were declared `public` by mistake; nothing in the public API surface (`IMarkdownDisplayer`, `MarkdownDisplayer`, `Displayer`, `DisplayOptions`, `LinkItem`, the `Styling` types, `UnknownEmphasisDelimiter`, `Fakes`) requires consumers to construct or subclass them.

This change is being **documented as breaking** so anyone who happened to take a dependency on those types is aware that hiding them was a deliberate correction rather than an accident.

### What changed

Every `class`, `interface`, `enum`, `struct`, and `record` declared in the `BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers` namespace is now `internal`.

### What you need to do

For nearly every consumer: **nothing**. If you were relying on one of these types directly, please open an issue describing the use case so we can consider exposing a stable replacement on the public API.

---

## Breaking change #2 — Namespaces are now prefixed with `BoxOfYellow.`

### Why

The NuGet package IDs already include the `BoxOfYellow.` owner prefix, but the namespaces inside the assemblies did not. Aligning the namespaces with the package IDs prevents collisions with similarly-named libraries and makes the source of a type unambiguous at a glance.

### What changed

| Before (v0.9.x) | After (v0.10.0) |
|---|---|
| `ConsoleMarkdownRenderer` | `BoxOfYellow.ConsoleMarkdownRenderer` |
| `ConsoleMarkdownRenderer.Styling` | `BoxOfYellow.ConsoleMarkdownRenderer.Styling` |
| `ConsoleMarkdownRenderer.Fakes` | `BoxOfYellow.ConsoleMarkdownRenderer.Fakes` |

(The `ObjectRenderers` namespace was renamed to `BoxOfYellow.ConsoleMarkdownRenderer.ObjectRenderers` for consistency, but per breaking change #1 it has no publicly visible types.)

The **assembly names** were also renamed to match the package IDs:

| Before (v0.9.x) | After (v0.10.0) |
|---|---|
| `ConsoleMarkdownRenderer.dll` | `BoxOfYellow.ConsoleMarkdownRenderer.dll` |
| `ConsoleMarkdownRenderer.Fakes.dll` | `BoxOfYellow.ConsoleMarkdownRenderer.Fakes.dll` |

Standard `<PackageReference>` consumers don't need to do anything for this — only code that loads the assembly by name (e.g. `Assembly.Load("ConsoleMarkdownRenderer")`) needs to use the new name.

### What you need to do

Update your `using` directives. Find-and-replace handles every case:

| Find | Replace |
|---|---|
| `using ConsoleMarkdownRenderer;` | `using BoxOfYellow.ConsoleMarkdownRenderer;` |
| `using ConsoleMarkdownRenderer.Styling;` | `using BoxOfYellow.ConsoleMarkdownRenderer.Styling;` |
| `using ConsoleMarkdownRenderer.Fakes;` | `using BoxOfYellow.ConsoleMarkdownRenderer.Fakes;` |

Any fully-qualified type references should be updated similarly, e.g. `ConsoleMarkdownRenderer.Displayer.DisplayMarkdownAsync(...)` becomes `BoxOfYellow.ConsoleMarkdownRenderer.Displayer.DisplayMarkdownAsync(...)`.

#### Examples

**Before (v0.9.x)**

```csharp
using ConsoleMarkdownRenderer;
using ConsoleMarkdownRenderer.Styling;

var options = new DisplayOptions
{
    Bold = new TextStyle(decoration: TextDecoration.Bold),
};

await Displayer.DisplayMarkdownAsync(uri, options);
```

**After (v0.10.0)**

```csharp
using BoxOfYellow.ConsoleMarkdownRenderer;
using BoxOfYellow.ConsoleMarkdownRenderer.Styling;

var options = new DisplayOptions
{
    Bold = new TextStyle(decoration: TextDecoration.Bold),
};

await Displayer.DisplayMarkdownAsync(uri, options);
```

**Before (v0.9.x) — Fakes**

```csharp
using ConsoleMarkdownRenderer;
using ConsoleMarkdownRenderer.Fakes;

var fake = new FakeMarkdownDisplayer();
await fake.DisplayMarkdownAsync(new Uri("https://example.com/readme.md"));
```

**After (v0.10.0) — Fakes**

```csharp
using BoxOfYellow.ConsoleMarkdownRenderer;
using BoxOfYellow.ConsoleMarkdownRenderer.Fakes;

var fake = new FakeMarkdownDisplayer();
await fake.DisplayMarkdownAsync(new Uri("https://example.com/readme.md"));
```

The runtime behavior, type names, member signatures, and DI patterns are otherwise unchanged from v0.9.0.
