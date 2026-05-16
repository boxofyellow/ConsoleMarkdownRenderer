# Changelog

## Upcoming Changes

### :art: Renderers :art:
- [#132](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/132): Render Markdig CustomContainer admonition blocks
  - ```markdown
    :::note
    This is a *note* admonition with **bold** content.
    :::
    ```
  - Rendered
    :::note
    This is a *note* admonition with **bold** content.
    :::
  - Before
    <img width="314" height="22" alt="Image" src="https://github.com/user-attachments/assets/17f529a2-17c3-4a18-bd47-145befff5acb" />
  - After
    <img width="338" height="58" alt="Image" src="https://github.com/user-attachments/assets/6b66b0fd-9cfa-4b40-8733-236ed5ab4b39" />
- [#133](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/133): Emit OSC 8 terminal hyperlinks from WriteLink via Spectre Markup

### :wrench: Internal Improvements :wrench:
- [#129](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/129): Use ConfigureAwait(false) on awaits in published library code
- [#131](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/131): Use raw string literals for multi-line AssertCrossPlatStringMatch arguments
- [#146](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/146): Skip CI workflows for `docs/**` and `README.md` changes; restrict API Compatibility Check to `*.cs`/`*.csproj` changes

### :copilot: Agentic Workflows :copilot:
- [#130](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/130): Restrict dependency-feature-scout push trigger to main branch
- [#135](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/135): Add code-style-guide-bot agentic workflow
- [#137](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/137): Fix unpinned actions/github-script@v9 in agentic workflow lock; document SHA-pin rule

### :writing_hand: Documentation :writing_hand:
- [#134](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/134): Enhance documentation for renderer addition process
- [#139](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/139): [code-style-guide-bot] Update code-style.md with recurring review feedback (last 6 months)
- [#140](https://github.com/boxofyellow/ConsoleMarkdownRenderer/issues/140): Add links to coding style guide

---

**Full Changelog**: https://github.com/boxofyellow/ConsoleMarkdownRenderer/compare/v0.11.0...main

## [v0.11.0](https://github.com/boxofyellow/ConsoleMarkdownRenderer/releases/tag/v0.11.0)

> [!WARNING]
> This change includes minor breaking changes
> https://github.com/boxofyellow/ConsoleMarkdownRenderer/blob/main/docs/migration_v0.11.0.md

### :warning: Minor API Cleanup :warning:
- [#117](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/117): Seal some classes

### :art: Renderers :art:
- [#108](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/108): Add ConsoleObjectRenderer support for Markdig DefinitionList, DefinitionItem, and DefinitionTerm
  - ```markdown
    Apple
    :   A fruit that is red or green.
    
    Orange
    :   A citrus fruit.
    :   Also a color.
    ```
  - Rendered
    Apple
    :   A fruit that is red or green.
    
    Orange
    :   A citrus fruit.
    :   Also a color.
  - Before
    <img width="211" height="51" alt="Image" src="https://github.com/user-attachments/assets/10a0117d-c9b5-4d70-b9b3-b48ae2f38c46" />
  - After
    <img width="254" height="115" alt="Image" src="https://github.com/user-attachments/assets/5f0cb9db-56c7-49c4-a159-30ee60981c51" />
- [#112](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/112): Render Markdig EmojiInline nodes with DisplayOptions.Emojis gate
  - ```markdown
    :smile: :-) 😅
    ```
  - Rendered
    :smile: :-) 😅
  - Before
    <img width="110" height="20" alt="Image" src="https://github.com/user-attachments/assets/76a60e22-6351-4d19-9118-606f7c8df781" />
  - After
    <img width="67" height="19" alt="Image" src="https://github.com/user-attachments/assets/a21a9aa7-0cad-4cd5-9cdf-fe2e74a649fa" />
- [#113](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/113): Render headings via Spectre.Console FigletText (IHeaderStyle, default H1)
   - ```markdown
     # Header 1
     text 1
     ## Header 2
     text 2
     ```
   - Rendered
     # Header 1
     text 1
     ## Header 2
     text 2
   - Before
     <img width="110" height="124" alt="Image" src="https://github.com/user-attachments/assets/363b188b-af98-4cd1-ab46-011d37433f6d" />
   - After
     <img width="370" height="186" alt="Image" src="https://github.com/user-attachments/assets/29af19fd-a50b-4b7d-abea-22e65ef4e13a" />

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
