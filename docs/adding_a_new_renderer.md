# Adding a New Console Renderer

This guide walks through all the steps needed to add rendering support for a new Markdig AST node type.

---

## Checklist

- [ ] [Implement the renderer class(es)](#1-implement-the-renderer-classes)
- [ ] [Register the renderer(s) in `ConsoleRenderer`](#2-register-the-renderers-in-consolerenderer)
- [ ] [Add `SpectreDisplayOptions` style properties](#3-add-SpectreDisplayOptions-style-properties)
- [ ] [Add test resource files (`.md` / `.txt` pair)](#4-add-test-resource-files-md--txt-pair)
- [ ] [Write unit tests](#5-write-unit-tests)
- [ ] [Update `bracketEscaping` resources](#6-update-bracketescaping-resources)
- [ ] [Update the example document](#7-update-the-example-document)
- [ ] [Update the changelog](#8-update-the-changelog)

---

## Step-by-step instructions

### 1. Implement the renderer class(es)

Add one `internal` class per AST node type that extends `ConsoleObjectRenderer<TObject>` (which in turn extends Markdig's `MarkdownObjectRenderer<ConsoleRenderer, TObject>`) and overrides the `Write` method.

**Guidelines:**
- Keep renderer classes `internal`.
- Use the fluent helpers on `ConsoleRenderer` (`NewFrame`, `PushStyle`/`PopStyle`, `WriteEscape`, `WriteChildrenChain`, `AddInLine`, `StartInline`/`EndInline`, `CompleteFrame`, etc.).
- Access display styles through `renderer.Options.<PropertyName>` (see step 3).
- **Where to put the class:** If the renderer's `Write` method is simple — typically a single fluent expression — add it to **`ObjectRenderers/ConsoleObjectRenderers.cs`** alongside the other compact renderers. If the implementation is more involved (e.g. branching logic based on delimiter characters, as in [`ConsoleEmphasisInlineRenderer`](../ConsoleMarkdownRenderer.Spectre/ObjectRenderers/ConsoleEmphasisInlineRenderer.cs)), give it its own dedicated file under `ObjectRenderers/`.

```csharp
[SpectreSourceFile]
internal class ConsoleFootnoteRenderer : ConsoleObjectRendererBase<Footnote>
{
    protected override void Write(ConsoleRenderer renderer, Footnote obj)
        => renderer
            .NewFrame()
            .StartInline()
            .AddInLine($"[{renderer.Options.Footnote.ToMarkup()}]")
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

### 3. Add `SpectreDisplayOptions` style properties

Open **`SpectreDisplayOptions.cs`** and add a `public TextStyle` property for each new style, with a sensible default.


```csharp
public TextStyle FootnoteLink { get; set; } = new(foreground: TextColor.Blue, decoration: TextDecoration.Underline);
```

There some places that also need to be updated
- `SpectreDisplayOptions.Clone()`
- `SpectreDisplayOptions.Equals(object? obj)`
- `SpectreDisplayOptions.GetHashCode()`
- `SpectreDisplayOptions.Empty()`
- `SpectreDisplayOptions.Deserializers`
- `SpectreDisplayOptions.Serializers`

Similarly `DisplayOptions.cs` needs to updated in the same way.  One difference is that the default values should be come from `c_defaultSpectreOptions` instead of being hard coded.

If you are adding a new type to `SpectreDisplayOptions`, make sure to SpectreDisplayOptions update `public static SpectreDisplayOptions Crazy` in `TestUtilities.cs` to include a case for that type to get it set to a non-default value.

---

### 4. Add test resource files (`.md` / `.txt` pair)

Create a new pair of embedded resources under **`ConsoleMarkdownRenderer.Spectre.Tests/resources/`**:

| File | Purpose |
|------|---------|
| `<name>.md` | Minimal Markdown snippet that exercises the new renderer(s). |
| `<name>.txt` | The exact console output expected when `IncludeDebug = true` (i.e. the rendered frame structure). |
| `<name>.json` | Optional: SpectreDisplayOptions JSON file to customize the rendering options. |

The test `RendererTests_TextValidation` automatically discovers every `.md`/`.txt` pair in that folder and asserts that rendering the markdown produces the expected text layout.

To generate the `.txt` file, run the renderer against the new `.md` with `IncludeDebug = true` and capture the output, or write a quick scratch test first and copy the output.

---

### 5. Write unit tests

Add tests the correct Test project as needed

1. Verify the **text content** is rendered (picked up automatically by `RendererTests_TextValidation`).
2. Verify the **styling** applied to notable spans using `AssertMarkdownYieldsFormat`.
   - Pass `useCrazy: false` to validate default styles.
   - Pass `useCrazy: true` to validate that custom styles from `TestUtilities.Crazy` are applied correctly.

```csharp
[TestMethod]
[DataRow(false)]
[DataRow(true)]
public void RendererTests_FootnoteLinkTest(bool useCrazy)
{
    // Forward reference markers sit outside the FootnoteGroup so they only carry the FootnoteLink style
    AssertMarkdownYieldsFormat("footnote", "[^1]", new Style(foreground: Color.Blue, decoration: Decoration.Underline), useCrazy);
    AssertMarkdownYieldsFormat("footnote", "[^2]", new Style(foreground: Color.Blue, decoration: Decoration.Underline), useCrazy);
}
```

---

### 6. Update `bracketEscaping` resources

Always update **`ConsoleMarkdownRenderer.Tests/resources/bracketEscaping.md`** and regenerate **`bracketEscaping.txt`** after adding a new renderer. The fixture verifies that bracket characters embedded in rendered output are properly escaped by `WriteEscape`, so it should cover the new element even if the renderer does not explicitly emit `[` or `]` in its own logic (the content it renders might still contain brackets).

---

### 7. Update the example document

Open **`ConsoleMarkdownRenderer.Example/data/example.md`** and add a new section demonstrating the new renderer(s) in action. This is both a smoke-test for the CI pipeline and a living reference for users.

**Example (PR #86):** A `## Footnotes` section was appended showing inline footnote references and footnote definitions with mixed formatting.

### 8. Update the changelog

Open **`docs/CHANGELOG.md`** and add a link to your PR.  Using the of the existing entries as examples.  New PRs should always be added under the `## Upcoming Changes` section.  If the that section contains the text
```
- None yet, but check back soon!
```
Remove that.  Add your change to a reasonable section.  Most likely these should go in the `### :art: Renderers :art:` sections.  If the appropriate sections does not exist yet under `## Upcoming Changes`, add it.  It should typically be the top section, second only to sections containing breaking changes. 

For changes with visual difference make sure to include the scaffolding for showing off what has changed.  It is ok to leave place holders for the before/after images
```
  - ```markdown
    Show the mark down that will display the new visual change
    ```
  - Rendered
    Show the mark down that will dimply the new visual change
  - Before
    <img alt="Image" src=" {{** url for before **}}" />
  - After
    <img alt="Image" src=" {{** url for after **}}" />
```
