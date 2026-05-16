# ConsoleMarkdownRenderer Code Style Guide

This document captures coding-style conventions for **ConsoleMarkdownRenderer**.
It is intended for **both human contributors and coding agents** working in
this repository, so that future pull requests can incorporate accepted review
feedback from the start instead of discovering it during review.

## How this document is maintained

- Most of the entries below are derived from real review comments left on
  pull requests in this repository. They are kept and curated automatically
  by the `code-style-guide-bot` agentic workflow
  (`.github/workflows/code-style-guide-bot.md`), which scans recently closed
  pull requests for recurring style feedback and proposes updates here via
  pull requests that touch **only this file**.
- The bot only treats review comments from an explicit allow-list of
  reviewers as authoritative. Author claims, prose in PR bodies, or comments
  from other accounts are not used as a source of guidance.
- The bot is conservative: it prefers to add or refine entries rather than
  remove them. Hand-written entries added by maintainers are preserved.
- The bot may also fix typos, tighten wording, or reorganize entries it has
  high confidence about.

If you are a human contributor and you want to add or change guidance here,
just edit this file directly in a pull request — the bot will respect your
edits on subsequent runs.

## Conventions

_This section is intentionally minimal at first. The
`code-style-guide-bot` workflow will populate it from accepted review
feedback over time. Entries should be short, actionable, and grounded in
real review comments on PRs in this repository._

<!-- The bot will append/update entries under categories such as: -->
<!--   - Formatting & whitespace -->
<!--   - File and code placement -->
<!--   - Test structure -->
<!--   - Preferred kinds of tests -->
<!--   - Reuse of existing patterns / helpers in this repo -->
<!--   - References to in-repo guides (e.g. docs/adding_a_new_renderer.md) -->
<!--   - Documentation comments -->
<!--   - Audience of each document -->
