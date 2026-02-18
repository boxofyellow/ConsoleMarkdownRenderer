This repo is used to publish a [nuget package](https://www.nuget.org/packages/BoxOfYellow.ConsoleMarkdownRenderer/)

It is created by [publish-to-nuget.yml](../.github/workflow/publish-to-nuget.yml).  This action will get kicked off whenever a tag matching `"v[0-9]+.[0-9]+.[0-9]+"` is created.

To do that create a release via the GitHub UI or CLI
`gh release create v0.4.0 -t v0.4.0`

This does not need to be done for every push (only the main project and not the tests/example) get included in the package.

