# Migration Guide: v0.7.x → v0.8.0

Version 0.8.0 introduces library-owned styling abstractions (`TextStyle`, `TextColor`, `TextDecoration`) that replace direct use of Spectre.Console's `Style`, `Color`, and `Decoration` types in the public API. This is a **breaking change**.

## Why

Previously, consumers of `DisplayOptions` needed a transitive dependency on Spectre.Console just to configure styles. The new types decouple the public API surface from the rendering library.

## What Changed

| Before (v0.7.x) | After (v0.8.0) |
|---|---|
| `using Spectre.Console;` | `using ConsoleMarkdownRenderer.Styling;` |
| `Style` | `TextStyle` |
| `Color` | `TextColor` |
| `Decoration` | `TextDecoration` |

## Examples

### Setting a decoration

```csharp
// Before
options.Bold = new Style(decoration: Decoration.Bold);

// After
options.Bold = new TextStyle(decoration: TextDecoration.Bold);
```

### Setting foreground and background colors

```csharp
// Before
options.CodeBlock = new Style(foreground: Color.Yellow, background: Color.Blue);

// After
options.CodeBlock = new TextStyle(foreground: TextColor.Yellow, background: TextColor.Blue);
```

### Combining decorations (flags)

```csharp
// Before
options.Header = new Style(decoration: Decoration.Bold | Decoration.Underline | Decoration.Invert);

// After
options.Header = new TextStyle(decoration: TextDecoration.Bold | TextDecoration.Underline | TextDecoration.Invert);
```

### Using markup strings (unchanged)

The implicit string-to-`TextStyle` conversion continues to work as before:

```csharp
options.Bold = "red on purple";
options.Headers.Add("blue on green");
```

## Available Colors

`TextColor` provides static properties for common colors:

- `TextColor.Black`
- `TextColor.Red`
- `TextColor.Green`
- `TextColor.Yellow`
- `TextColor.Blue`
- `TextColor.Purple`
- `TextColor.Default`

For custom RGB colors:

```csharp
var custom = TextColor.FromRgb(128, 0, 255);
options.Bold = new TextStyle(foreground: custom);
```

## Available Decorations

`TextDecoration` is a `[Flags]` enum with the following values:

- `None`
- `Bold`
- `Dim`
- `Italic`
- `Underline`
- `SlowBlink`
- `RapidBlink`
- `Invert`
- `Conceal`
- `Strikethrough`
