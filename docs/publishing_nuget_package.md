This repo is used to publish a few packages to NuGet:
- https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer/
- https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer.Spectre
- https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer.Fakes

Some note about when/who should cut a release

> [!Caution]
> - Bots (copilot or other AI) should **_NEVER_** cut a release.  This is a human only task.  Bots can help by updating [CHANGELOG.md](CHANGELOG.md) as they create PRs with PR level changes.
> - Bots can also prepare migration guides for breaking changes as part of their PR and link them within [CHANGELOG.md](CHANGELOG.md) as part of that PR.
> - Bots can even help prepare the new release changes to the [CHANGELOG.md](CHANGELOG.md) as described in this doc, but should **_ONLY_** do that at the direct request of a human.
> - In short, humans initiate the process

> [!Note]
> Some changes never require a new release since they don't get included in the NuGet packages; here are some examples:
> - Test-only changes; projects with `Tests` in their name are not included
> - Workflow content is never included, so **_most_** changes under `.github` don't contribute to a new release. One exception is the workflow that pushes the release to NuGet, which can directly impact release behavior
> - `.vscode` content is never included, so changes to that folder don't contribute to a new release
> - `scripts` content is never included, so changes to that folder don't contribute to a new release
> - **_most_** of the content in the `docs` directory is excluded; however [`README.md`](../README.md) (at the root) is included, and has a link to [example.png](example.png)
> - The Example project is never included; however its content is used to create [example.png](example.png)

> [!Note]
> Some examples of things that do get included in the nuget package.  Not every change to one of these warrants a new release
> - Source code changes to one of the projects included in a nuget package
> - Bumping nuget package dependencies in the project included in the packages we publish
> - the README.md files included with each project we publish

> [!Tip]
> When there is an API break it is generally a good idea to cut a new release right after that change, and if there is enough other content waiting to be published it might also be a good idea to cut a new release **_BEFORE_** the API break.
> In short a breaking change (even a minor one with minimal impact to consumers) should be kept to as small of a payload as reasonably possible

> [!Important]
> **_ALL_**  API breaks (regardless of how small they are) should be documented in the [CHANGELOG.md](CHANGELOG.md) and a migration guide should be created to help consumers of the nuget packages migrate to the new version.  These should never be included in a patch version.  And once we go to 1.0.0 they should always come with a major version bump.


Before cutting the release, make sure to update the [CHANGELOG.md](CHANGELOG.md) to prepare for a new release
1. Right under `## Upcoming Changes` insert a new section for the release you are about to make. Follow this pattern; assume {{** your new version **}} includes the version number with the `v` prefix
   ```
   ## [{{** your new version **}}](https://github.com/boxofyellow/ConsoleMarkdownRenderer/releases/tag/{{** your new version **}})

   **Full Changelog**: https://github.com/boxofyellow/ConsoleMarkdownRenderer/compare/{{** your previous version **}}...main
   ```
   Doing so should effectively empty the "Upcoming" section and create a new section for the new release whose content will be the old "Upcoming" content.
2. Update the last line of the old sections so that it no longer points to `main` but instead points to that old versions number.  Assuming {{** your previous version **}} include the version number with the `v` prefix
   ```
   **Full Changelog**: https://github.com/boxofyellow/ConsoleMarkdownRenderer/compare/{{** your previous version **}}...{{** your new version **}}
   ```

If substantial changes have been made to the visuals also update [example.png](example.png) to showcase how things look.  A tool like https://imagecombiner.com/ can be used to combine multiple screenshots into one image.

You can see [#233](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/233) for an example of a release prep PR.


The actual publishing of new nuget package versions is handled by [publish-to-nuget.yml](../.github/workflows/publish-to-nuget.yml).  This action will get kicked off whenever a tag matching `"v[0-9]+.[0-9]+.[0-9]+"` is created.

To do that create a release via the GitHub UI or CLI
`gh release create v0.4.0 -t v0.4.0`

Use the content of [CHANGELOG.md](CHANGELOG.md) for the release notes.  Take all content since the last successful nuget package release.
