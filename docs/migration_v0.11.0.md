# Migration Guide: v0.10.x → v0.11.0

Version 0.11.0 introduces a new heading abstraction (`IHeaderStyle`) and a new default look for top-level (`#`) headings. The runtime behavior for every other Markdown element is unchanged.

| # | Change | Action required by consumers? |
|---|---|---|
| 1 | `DisplayOptions.Header` and `DisplayOptions.Headers` are now typed as `IHeaderStyle` (was `TextStyle`). `TextStyle` implements `IHeaderStyle`, so existing assignments still compile. | **Most consumers: none.** Code that *reads* `Header`/`Headers` back as a `TextStyle` will need a cast or interface-based access. |
| 2 | `DisplayOptions.EffectiveHeader(int)` is now `internal` (was `public`). | **Most consumers: none.** This helper is an implementation detail; if you were calling it, please open an issue describing the use case. |
| 3 | `DisplayOptions.Headers` now defaults to a single `FigletTextStyle` entry, so `#` (H1) headings render as large FIGlet ASCII art out of the box. | **Visual change.** See "Restoring the v0.10.x heading look" below for a one-line opt-out. |

---

## Breaking change #1 — `Header` and `Headers` are typed as `IHeaderStyle`

### Why

`Headers` used to be a `List<TextStyle>`, which made it impossible to assign a heading-only style (such as FIGlet ASCII art) without subclassing `TextStyle` and faking the parts it doesn't support. Introducing `IHeaderStyle` lets multiple peer implementations coexist:

- [`TextStyle`](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/Styling/TextStyle.cs) — inline styled markup (decoration, foreground, background), optionally wrapped with `#` characters via `WrapHeader`.
- [`FigletTextStyle`](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/Styling/FigletTextStyle.cs) — large ASCII art via Spectre.Console's `FigletText` widget, with optional `Justification` and `Foreground`. Custom FIGlet fonts (`.flf`) are loaded via the async `FigletTextStyle.CreateAsync(fontPath, …)` factory.

### What changed

```csharp
// Before (v0.10.x)
public List<TextStyle> Headers { get; set; }
public TextStyle Header { get; set; }

// After (v0.11.0)
public List<IHeaderStyle> Headers { get; set; }
public IHeaderStyle Header { get; set; }
```

`TextStyle` continues to implement `IHeaderStyle`, so **assigning a `TextStyle` keeps working unchanged**:

```csharp
var options = new DisplayOptions
{
    Header = new TextStyle(decoration: TextDecoration.Bold),         // still compiles
    Headers = { new TextStyle(foreground: TextColor.Cyan) },         // still compiles
};
```

### What you need to do

For nearly every consumer: **nothing**. Code that only *sets* `Header`/`Headers` continues to compile and behave as before.

Code that *reads* these properties back as a concrete `TextStyle` needs one of:

```csharp
// Before
TextStyle current = options.Header;

// After – option A: pattern-match on the runtime type
if (options.Header is TextStyle textStyle) { /* ... */ }

// After – option B: use the IHeaderStyle interface, which exposes
//                  Foreground / Background / Decoration
IHeaderStyle current = options.Header;
TextColor? fg = current.Foreground;
```

---

## Breaking change #2 — `DisplayOptions.EffectiveHeader(int)` is now `internal`

`EffectiveHeader` walks `Headers` and `Header` to pick the style used for a given heading level. It was always intended as plumbing between `DisplayOptions` and the built-in heading renderer; exposing it `public` in v0.10.x was an oversight.

For nearly every consumer: **nothing**. If you were relying on it, please open an issue describing the use case so we can consider exposing a stable replacement.

---

## Functional change — Default H1 now renders as FIGlet ASCII art

### Why

A prominent ASCII-art title makes the top of a document immediately distinguishable from body text in terminal output. Making it the H1 default surfaces the feature without configuration; existing callers can opt back into the previous behavior in one line.

### What changed

`DisplayOptions.Headers` used to default to an empty list, so `#` (H1) fell through to the styled `Header` like every other level. It now defaults to:

```csharp
Headers = new() { new FigletTextStyle(justification: TextJustification.Left) };
```

`H2`, `H3`, … are unchanged — they still fall through to `Header` and render as styled, `#`-wrapped markup.

#### Before / after for a simple document

Given this Markdown:

```markdown
# Hello

Some body text.

## Subsection
```

**Before (v0.10.x)** — H1 rendered as styled, `#`-wrapped markup:

```
 # Hello #

 Some body text.

 ## Subsection ##
```

**After (v0.11.0)** — H1 rendered as FIGlet ASCII art (H2 is unchanged):

```
   _   _          _   _
  | | | |   ___  | | | |   ___
  | |_| |  / _ \ | | | |  / _ \
  |  _  | |  __/ | | | | | (_) |
  |_| |_|  \___| |_| |_|  \___/


 Some body text.

 ## Subsection ##
```

### Restoring the v0.10.x heading look

If you prefer the previous behavior (every heading uses styled, `#`-wrapped markup), clear `Headers`. Every level — including H1 — will then fall through to the styled `Header`:

```csharp
var options = new DisplayOptions();
options.Headers.Clear(); // H1 (and every deeper level) renders as styled "#"-wrapped markup
```

### Opting deeper levels in to FIGlet rendering

`FigletTextStyle` works at any level. Append entries to `Headers` to make `##`, `###`, … use FIGlet too:

```csharp
var options = new DisplayOptions();
options.Headers.Add(new FigletTextStyle(
    justification: TextJustification.Center,
    foreground: TextColor.Blue));   // applies to '##' (H2)
```

See the [README](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/README.md#rendering-headings-as-figlet-ascii-art) for the full `IHeaderStyle` reference.
