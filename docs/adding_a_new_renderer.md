# Adding a New Console Renderer

This guide walks through all the steps needed to add rendering support for a new Markdig AST node type. [PR #86](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/86) — which added renderers for Markdig's `FootnoteLink`, `Footnote`, and `FootnoteGroup` types — is used as the running example throughout.

---

## Checklist

- [ ] [Implement the renderer class(es)](#1-implement-the-renderer-classes)
- [ ] [Register the renderer(s) in `ConsoleRenderer`](#2-register-the-renderers-in-consolerenderer)
- [ ] [Add `DisplayOptions` style properties](#3-add-displayoptions-style-properties)
- [ ] [Update `DisplayOptions.Clone()`](#4-update-displayoptionsclone)
- [ ] [Add test resource files (`.md` / `.txt` pair)](#5-add-test-resource-files-md--txt-pair)
- [ ] [Write unit tests](#6-write-unit-tests)
- [ ] [Update `m_crazyOptions` in `RendererTests`](#7-update-m_crazyoptions-in-renderertests)
- [ ] [Update `bracketEscaping` resources](#8-update-bracketescaping-resources)
- [ ] [Update the example document](#9-update-the-example-document)

---

## Step-by-step instructions

### 1. Implement the renderer class(es)

Add one `internal` class per AST node type that extends `ConsoleObjectRenderer<TObject>` (which in turn extends Markdig's `MarkdownObjectRenderer<ConsoleRenderer, TObject>`) and overrides the `Write` method.

**Guidelines:**
- Keep renderer classes `internal` (this was enforced as of v0.10.0).
- Use the fluent helpers on `ConsoleRenderer` (`NewFrame`, `PushStyle`/`PopStyle`, `WriteEscape`, `WriteChildrenChain`, `AddInLine`, `StartInline`/`EndInline`, `CompleteFrame`, etc.).
- Access display styles through `renderer.Options.<PropertyName>` (see step 3).
- **Where to put the class:** If the renderer's `Write` method is simple — typically a single fluent expression — add it to **`ObjectRenderers/ConsoleObjectRenderers.cs`** alongside the other compact renderers. If the implementation is more involved (e.g. branching logic based on delimiter characters, as in [`ConsoleEmphasisInlineRenderer`](../ObjectRenderers/ConsoleEmphasisInlineRenderer.cs)), give it its own dedicated file under `ObjectRenderers/`.

**Example (PR #86):** Three new renderer classes were added — `ConsoleFootnoteLinkRenderer`, `ConsoleFootnoteRenderer`, and `ConsoleFootnoteGroupRenderer`:

```csharp
internal class ConsoleFootnoteRenderer : ConsoleObjectRenderer<Footnote>
{
    protected override void Write(ConsoleRenderer renderer, Footnote obj)
        => renderer
            .NewFrame()
            .StartInline()
            .AddInLine($"[{renderer.Options.Footnote.ToSpectreStyle().ToMarkup()}]")
            .WriteEscape($"[{obj.Label}]:")
            .AddInLine("[/]")
            .EndInline()
            .WriteChildrenChain(obj)
            .CompleteFrame();
}
```

---

### 2. Register the renderer(s) in `ConsoleRenderer`

Open **`ObjectRenderers/ConsoleRenderer.cs`** and add `new YourRenderer()` to the `ObjectRenderers.AddRange([...])` call inside the constructor.

**Important:** The list order matters. Markdig picks the *first* renderer whose accepted type is assignable from the object being rendered. More-specific types must appear *before* less-specific base types. In particular, `ConsoleContainerInlineRenderer` (which handles `ContainerInline`) must remain last.

**Example (PR #86):** Three entries were added in alphabetical order among the existing registrations:

```csharp
new ConsoleFootnoteGroupRenderer(),
new ConsoleFootnoteLinkRenderer(),
new ConsoleFootnoteRenderer(),
```

---

### 3. Add `DisplayOptions` style properties

Open **`DisplayOptions.cs`** and add a `public TextStyle` property for each new style, with a sensible default. Include XML-doc comments that reference the corresponding Markdig type.

**Example (PR #86):**

```csharp
/// <summary>
/// Style applied to a <see cref="Markdig.Extensions.Footnotes.FootnoteLink"/> marker.
/// </summary>
public TextStyle FootnoteLink { get; set; } = new(foreground: TextColor.Blue, decoration: TextDecoration.Underline);

public TextStyle Footnote { get; set; } = new(decoration: TextDecoration.Bold);

public TextStyle FootnoteGroup { get; set; } = new(decoration: TextDecoration.Italic);
```

---

### 4. Update `DisplayOptions.Clone()`

Still in **`DisplayOptions.cs`**, copy the new properties inside the `Clone()` method so that cloned options include them.

**Example (PR #86):**

```csharp
Footnote = this.Footnote,
FootnoteGroup = this.FootnoteGroup,
FootnoteLink = this.FootnoteLink,
```

---

### 5. Add test resource files (`.md` / `.txt` pair)

Create a new pair of embedded resources under **`ConsoleMarkdownRenderer.Tests/resources/`**:

| File | Purpose |
|------|---------|
| `<name>.md` | Minimal Markdown snippet that exercises the new renderer(s). |
| `<name>.txt` | The exact console output expected when `IncludeDebug = true` (i.e. the rendered frame structure). |

The test `RendererTests_TextValidation` automatically discovers every `.md`/`.txt` pair in that folder and asserts that rendering the markdown produces the expected text layout.

To generate the `.txt` file, run the renderer against the new `.md` with `IncludeDebug = true` and capture the output, or write a quick scratch test first and copy the output.

**Example (PR #86):** `footnote.md` and `footnote.txt` were added.

---

### 6. Write unit tests

Open **`ConsoleMarkdownRenderer.Tests/RendererTests.cs`** and add one or more `[TestMethod]` tests that:

1. Verify the **text content** is rendered (picked up automatically by `RendererTests_TextValidation`).
2. Verify the **styling** applied to notable spans using `AssertMarkdownYieldsFormat`.
   - Pass `useCrazy: false` to validate default styles.
   - Pass `useCrazy: true` to validate that custom styles from `m_crazyOptions` are applied correctly.

**Example (PR #86):** Three test methods were added:

```csharp
[TestMethod]
[DataRow(false)]
[DataRow(true)]
public void RendererTests_FootnoteLinkTest(bool useCrazy)
{
    AssertMarkdownYieldsFormat("footnote", "[^1]", new Style(foreground: Color.Blue, decoration: Decoration.Underline), useCrazy);
    AssertMarkdownYieldsFormat("footnote", "[^2]", new Style(foreground: Color.Blue, decoration: Decoration.Underline), useCrazy);
}
```

---

### 7. Update `m_crazyOptions` in `RendererTests`

Still in **`ConsoleMarkdownRenderer.Tests/RendererTests.cs`**, add each new `DisplayOptions` property to the `m_crazyOptions` static field, setting it to `c_crazyFormat` (`"red on purple"`). This ensures the "crazy" style path is covered in tests.

**Example (PR #86):**

```csharp
Footnote = c_crazyFormat,
FootnoteGroup = c_crazyFormat,
FootnoteLink = c_crazyFormat,
```

---

### 8. Update `bracketEscaping` resources

Always update **`ConsoleMarkdownRenderer.Tests/resources/bracketEscaping.md`** and regenerate **`bracketEscaping.txt`** after adding a new renderer. The fixture verifies that bracket characters embedded in rendered output are properly escaped by `WriteEscape`, so it should cover the new element even if the renderer does not explicitly emit `[` or `]` in its own logic (the content it renders might still contain brackets).

**Example (PR #86):** A footnote reference and a footnote definition containing `[testN]` markers were added to `bracketEscaping.md`, and `bracketEscaping.txt` was regenerated.

---

### 9. Update the example document

Open **`ConsoleMarkdownRenderer.Example/data/example.md`** and add a new section demonstrating the new renderer(s) in action. This is both a smoke-test for the CI pipeline and a living reference for users.

**Example (PR #86):** A `## Footnotes` section was appended showing inline footnote references and footnote definitions with mixed formatting.

---

## Files changed summary

The table below lists every file touched by a typical "add a new renderer" change (based on [PR #86](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/86)):

| File | Kind of change |
|------|----------------|
| `ObjectRenderers/ConsoleObjectRenderers.cs` | Add one `internal` renderer class per new AST node type |
| `ObjectRenderers/ConsoleRenderer.cs` | Register the new renderer(s) in the constructor |
| `DisplayOptions.cs` | Add `TextStyle` property per renderer; update `Clone()` |
| `ConsoleMarkdownRenderer.Tests/RendererTests.cs` | Add test methods; extend `m_crazyOptions` |
| `ConsoleMarkdownRenderer.Tests/resources/<name>.md` | New minimal Markdown fixture |
| `ConsoleMarkdownRenderer.Tests/resources/<name>.txt` | Expected text-layout output for the fixture |
| `ConsoleMarkdownRenderer.Tests/resources/bracketEscaping.md` | Extend with new element; regenerate expected output |
| `ConsoleMarkdownRenderer.Tests/resources/bracketEscaping.txt` | Regenerate expected output |
| `ConsoleMarkdownRenderer.Example/data/example.md` | Add a demo section for the new element |
