# ConsoleMarkdownRenderer Code Style Guide

## Code Comments

- Comments in code should be used to explain *why* something is done, not *what* is being done. The code itself should be clear enough to convey the "what".  If the code is not clear, that is a sign that code should be changed to become clearer.
- For example header comments on methods and classes that only contain a summary of the method signature are not helpful.  If there are unexpected things that consumers need to know that would be a good thing to comment.
- Make sure to separate comments that belong in code, and comments that belong in PR description.  For example comments detail how the code is changing overtime belong in the PR description, not the code itself.
- Remember code comments can become out of date.  Typically the code is kept up to date with tests, but there is no forcing function to keep comments up to date.  So when making changes/additions to the code, review for comments that should be updated too.

## File and code placement

- Each non-trivial class (anything more than a one-liner) should live in its own file named after the class. One-liner classes may share a file (e.g., `ConsoleObjectRenderers.cs`), but as soon as a class grows beyond that, move it to a dedicated file in the same directory.
- New `ObjectRenderers` implementations go in the `ObjectRenderers/` folder.

## Test structure

- Prefer `[DataRow(...)]` over in-test arrays or repeated single-case test methods. Each distinct scenario should be its own `[DataRow(...)]` on a shared `[TestMethod]`.
- Column-align values across `[DataRow(...)]` attributes so that corresponding values can be compared across rows at a glance:
  ```csharp
  [DataRow(0, "one",   "one.md",   false)]
  [DataRow(1, "two",   "two.md",   false)]
  [DataRow(2, "three", "three.md", true )]
  ```
- When a test is meant to cover all values of an enum (e.g., "AllDecorations", "AllNamedColors"), use reflection (`Enum.GetValues()`) to iterate rather than maintaining a manual list of `[DataRow(...)]` entries. This ensures new enum members are automatically covered.
- Tests must actually validate the feature under test: a test that passes regardless of whether the feature is enabled or disabled is not a useful test. Verify that toggling the relevant option changes the output.
- When a collection is indexed in a test, add an assertion that the collection is large enough before accessing by index, so failures produce clear messages rather than thrown exceptions.

## Preferred kinds of tests

- New renderer code should be covered by **resource-based snapshot tests** — `.md` / `.txt` pairs under `ConsoleMarkdownRenderer.Tests/resources/` — so that the rendered output can be visually inspected and reviewed.
- Use `AssertCrossPlatStringMatch` (defined in the test project) for multi-line string comparisons instead of multiple individual `Assert.IsTrue` / `Assert.AreEqual` calls.
- Prefer unit tests that exercise the actual code path being introduced; do not rely solely on integration-level tests that might mask a broken implementation.
- Do not add standalone inline tests that solely duplicate text output already validated by a resource-based snapshot test. If a `.md`/`.txt` pair under `resources/` already covers the feature, a separate inline assertion of the same rendered output adds no signal and should be omitted.  An additional test that validates the styling can be added.

## Formatting & whitespace

- In renderer implementations, use the existing fluent chaining style (e.g., chain `.AddInLine(...)`, `.WriteEscape(...)`, `.AddInLine("[/]")`) rather than multi-statement imperative style.

## Reuse of existing patterns

- Before adding a new test-only helper or utility method, check whether an existing public API already provides the functionality (e.g., `TextStyle.FromMarkup` for parsing style strings). Avoid test-only wrappers around already-accessible functionality.
- Reuse existing shared test helpers (e.g., `AssertCrossPlatStringMatch`, `AssertTextStylesEqual`) rather than duplicating assertion logic.
- When all values of an enum must map to a corresponding value (e.g., a static lookup dictionary in `TextStyleExtensions.cs`), build the mapping reflectively using `Enum.GetValues<TEnum>()` rather than hardcoding each case. Pair the mapping with a reflection-based test that fails if any enum value is later added without a corresponding entry (follow the `s_tableBorderMap` and `ValidateEnumCoverage` patterns already established in this repo).

## References to in-repo guides

- When adding a new console renderer, follow `docs/adding_a_new_renderer.md`.

## Documentation comments

- Keep doc comments accurate when refactoring class hierarchies or interfaces. If a class relationship changes (e.g., a base class becomes an interface), update all affected comments in the same PR.

## Changelog

- **Every PR must add an entry to [`docs/CHANGELOG.md`](CHANGELOG.md) in the `Upcoming Changes` section.** If the appropriate subsection (e.g., `Renderers`, `Internal Improvements`, `Agentic Workflows`, `Documentation`, `Dependencies`) does not exist yet, create it following the existing examples. This applies to *all* PRs — including documentation, workflow, and dependency changes.
- The link in the entry must use the correct PR number — the number of the PR you are currently opening, not a related issue number or a placeholder. Verify the number before submitting.

## Audience of each document

- Documents in the `docs/` folder are written for **human contributors and coding agents** working on this repository. Keep language concrete and actionable.
- `README.md` is written for **external consumers** of the library. Avoid implementation-history language (e.g., "original behavior") and focus on current, correct usage.

## GitHub Actions

All GitHub Actions referenced via `uses:` in any workflow file under `.github/`
(including reusable/composite actions and generated `*.lock.yml` files
produced by `gh aw compile`) **must be pinned to a full-length 40-character
commit SHA**. Floating refs such as `@v1`, `@v9`, `@main`, or branch names are
not allowed.

Always include a trailing comment with the human-readable version next to the
SHA so the intent stays reviewable, for example:

```yaml
uses: actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v6.0.2
```

When regenerating gh-aw workflow lock files, pass `--action-tag <40-char-sha>`
to `gh aw compile` so the `github/gh-aw/actions/setup` reference is also
SHA-pinned. After regeneration, verify no unpinned `uses:` entries remain.
