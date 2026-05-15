# Changelog

## Upcoming Changes

None yet, but check back soon!

---

**Full Changelog**: https://github.com/boxofyellow/ConsoleMarkdownRenderer/compare/v0.11.0...main

## [v0.11.0](https://github.com/boxofyellow/ConsoleMarkdownRenderer/releases/tag/v0.11.0)

### :warning: Minor API Cleanup :warning:
- [#117](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/117): Seal some classes

### :art: Renderers :art:
- [#108](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/108): Add ConsoleObjectRenderer support for Markdig DefinitionList, DefinitionItem, and DefinitionTerm
- [#112](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/112): Render Markdig EmojiInline nodes with DisplayOptions.Emojis gate
- [#113](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/113): Render headings via Spectre.Console FigletText (IHeaderStyle, default H1)

### :writing_hand: Documentation :writing_hand:
- [#89](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/89): Fix Downloads badge link in README.md
- [#90](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/90): Fix the readme one last time
- [#103](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/103): docs: add guide for adding a new console renderer
- [#118](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/118): docs: add CHANGELOG with Upcoming Changes section

### :copilot: Agentic Workflows :copilot:
- [#91](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/91): Add dependency-feature-scout agentic workflow
- [#92](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/92): Fix dependency-feature-scout workflow by SHA-pinning all actions
- [#93](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/93): Initialize repository for GitHub Agentic Workflows
- [#94](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/94): Downgrade gh-aw extension version to v0.71.1
- [#95](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/95): Fix Dependency Feature Scout: move gh-aw actions-source checkout under `$GITHUB_WORKSPACE`
- [#96](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/96): Fix Dependency Feature Scout: preserve gh-aw actions-source across secondary checkouts
- [#97](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/97): Recompile dependency-feature-scout to fix missing clean.sh (exit 127) and SHA-pin all actions
- [#104](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/104): dependency-feature-scout: cap open issues at 7, never close existing, link to renderer guide
- [#110](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/110): Harden dependency-feature-scout workflow against prompt injection from untrusted content

### :package: Dependencies :package:
- [#114](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/114): Update dependabot configuration for NuGet and GitHub Actions
- [#115](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/115): Bump the nuget-dependencies group with 3 updates
- [#116](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/116): Apply Dependabot #111: bump codeql-action, gh-aw-actions/setup-cli, gh-aw
- [#121](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/121): Bump the nuget-dependencies group with 5 updates
- [#120](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/120): Bump github/gh-aw-actions from 0.74.0 to 0.74.2 in the github-actions group across 1 directory
- [#126](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/126): Bump gh-aw to v0.74.2 in dependency-feature-scout lock file

---

**Full Changelog**: https://github.com/boxofyellow/ConsoleMarkdownRenderer/compare/v0.10.1...v0.11.0

## Past Changes

See the [this project's releases](https://github.com/boxofyellow/ConsoleMarkdownRenderer/releases) but some notable changes

- [v0.10.0](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/docs/migration_v0.10.0.md)
- [v0.9.0](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/docs/migration_v0.9.0.md)
- [v0.8.0](https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/docs/migration_v0.8.0.md)
