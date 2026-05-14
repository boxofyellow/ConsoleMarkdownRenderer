# Changelog

## Upcoming Changes

### Agentic Workflows
- [#91](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/91): Add dependency-feature-scout agentic workflow
- [#92](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/92): Fix dependency-feature-scout workflow by SHA-pinning all actions
- [#93](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/93): Initialize repository for GitHub Agentic Workflows
- [#94](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/94): Downgrade gh-aw extension version to v0.71.1
- [#95](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/95): Fix Dependency Feature Scout: move gh-aw actions-source checkout under `$GITHUB_WORKSPACE`
- [#96](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/96): Fix Dependency Feature Scout: preserve gh-aw actions-source across secondary checkouts
- [#97](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/97): Recompile dependency-feature-scout to fix missing clean.sh (exit 127) and SHA-pin all actions
- [#104](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/104): dependency-feature-scout: cap open issues at 7, never close existing, link to renderer guide
- [#110](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/110): Harden dependency-feature-scout workflow against prompt injection from untrusted content

### Renderers
- [#108](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/108): Add ConsoleObjectRenderer support for Markdig DefinitionList, DefinitionItem, and DefinitionTerm
- [#112](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/112): Render Markdig EmojiInline nodes with DisplayOptions.Emojis gate

### Documentation
- [#89](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/89): Fix Downloads badge link in README.md
- [#90](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/90): Fix the readme one last time
- [#103](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/103): docs: add guide for adding a new console renderer
- This PR: Add `docs/CHANGELOG.md` with an "Upcoming Changes" section

### Dependencies
- [#114](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/114): Update dependabot configuration for NuGet and GitHub Actions
- [#115](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/115): Bump the nuget-dependencies group with 3 updates
- [#116](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/116): Apply Dependabot #111: bump codeql-action, gh-aw-actions/setup-cli, gh-aw
