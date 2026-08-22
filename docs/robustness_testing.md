# Markdown robustness testing

The `Scheduled Markdown Robustness` workflow runs the bounded, deterministic
`PropertyBasedMarkdownSuiteTests` once each week at 04:23 UTC on Sunday and
can also be started with `workflow_dispatch`.

It runs only on `ubuntu-latest`, because the normal CI matrix already covers
the supported operating systems. The suite also remains part of normal CI's
full `ConsoleMarkdownRenderer.Spectre.Tests` run. This scheduled workflow is
additive: it gives the fixed generator a focused, reproducible diagnostic run
without duplicating the normal cross-platform matrix. It runs the focused suite
on both `net8.0` and `net10.0`:

```shell
dotnet test --configuration Release --framework <net8.0|net10.0> \
  --filter "FullyQualifiedName~PropertyBasedMarkdownSuiteTests" \
  ConsoleMarkdownRenderer.Spectre.Tests
```

The generated cases use the suite's fixed seeds, bounded document size and
nesting, and bounded shrinking. This keeps the scheduled job reproducible and
does not expand it into unbounded fuzzing.

Each matrix job has a 10-minute GitHub Actions timeout and an 8-minute command
timeout. A command-timeout error is labelled separately in the workflow log.
An MSTest `[Timeout]` is instead a test failure and is identified in the test
output. MSTest does not forcibly terminate a timed-out render thread, so this
workflow is a bounded diagnostic job rather than zombie-thread containment.

When a scheduled run reports an MSTest timeout, triage or fix that case first.
Treat any other failures from the same run as suspect until the focused suite
is rerun successfully. The failure output includes the fixed seed/case label,
render width, and bounded shrunk replay input. Failed jobs upload their TRX
test results, retaining those diagnostics after the runner is cleaned up.

GitHub runs scheduled workflows from the default branch. Therefore, the
workflow begins scheduling only after the property-based suite and this
workflow are merged to `main`.
