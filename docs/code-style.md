# ConsoleMarkdownRenderer Code Style Guide

## File and code placement

## Test structure

## Preferred kinds of tests

## Formatting & whitespace

## Reuse of existing patterns

## References to in-repo guides

## Documentation comments

## Audience of each document

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
