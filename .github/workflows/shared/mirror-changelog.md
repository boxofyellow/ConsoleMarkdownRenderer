---
safe-outputs:
  create-pull-request:
    require-temporary-id: true
  jobs:
    finalize-mirror-changelog:
      description: "Resolve and verify the new mirror PR's own changelog link after PR creation."
      needs: [safe_outputs]
      runs-on: ubuntu-latest
      permissions:
        contents: write
        pull-requests: read
      output: "Verified the new mirror PR's changelog link."
      steps:
        - name: Complete the new PR's changelog link
          uses: actions/github-script@3a2844b7e9c422d3c10d287c895573f7108da1b3 # v9.0.0
          env:
            CREATED_PR_NUMBER: ${{ needs.safe_outputs.outputs.created_pr_number }}
            SOURCE_PR_NUMBER: ${{ inputs.source_pr }}
          with:
            script: |
              const number = Number(process.env.CREATED_PR_NUMBER);
              const source = Number(process.env.SOURCE_PR_NUMBER);
              if (!Number.isSafeInteger(number) || number <= 0 ||
                  !Number.isSafeInteger(source) || source <= 0 || number === source) {
                throw new Error("A distinct, successfully created mirror PR is required.");
              }
              const { owner, repo } = context.repo;
              const { data: pr } = await github.rest.pulls.get({ owner, repo, pull_number: number });
              if (pr.state !== "open" || !pr.draft || pr.head.repo?.full_name !== `${owner}/${repo}` ||
                  pr.head.ref === "main" || pr.head.ref === pr.base.ref) {
                throw new Error("Only the newly created same-repository draft branch may be updated.");
              }
              const path = "docs/CHANGELOG.md";
              const read = async ref => {
                const { data } = await github.rest.repos.getContent({ owner, repo, path, ref });
                if (Array.isArray(data) || data.type !== "file" || data.encoding !== "base64") {
                  throw new Error("Expected a base64-encoded changelog file.");
                }
                return { text: Buffer.from(data.content, "base64").toString("utf8"), sha: data.sha };
              };
              const file = await read(pr.head.sha);
              const heading = "## Upcoming Changes";
              const start = file.text.indexOf(`${heading}\n`);
              if (start < 0) throw new Error("Upcoming Changes section is missing.");
              const next = file.text.indexOf("\n## ", start + heading.length);
              const end = next < 0 ? file.text.length : next;
              const upcoming = file.text.slice(start, end);
              const pending = `[#aw_mirror](https://github.com/${owner}/${repo}/pull/#aw_mirror)`;
              const link = `[#${number}](https://github.com/${owner}/${repo}/pull/${number})`;
              const count = (text, value) => text.split(value).length - 1;
              if (!file.text.includes("#aw_mirror") && count(upcoming, link) === 1 &&
                  count(file.text, link) === 1) {
                core.info("The new PR's changelog link is already complete.");
                return;
              }
              if (count(upcoming, pending) !== 1 || count(file.text, pending) !== 1) {
                throw new Error("Expected exactly one pending mirror PR entry in Upcoming Changes.");
              }
              if (count(upcoming, link) !== 0) {
                throw new Error("The new PR already has a changelog entry.");
              }
              const content = file.text.slice(0, start) +
                upcoming.replace(pending, link) + file.text.slice(end);
              if (content.includes("#aw_mirror")) {
                throw new Error("An unresolved mirror reference remains in the changelog.");
              }
              await github.rest.repos.createOrUpdateFileContents({
                owner, repo, path, branch: pr.head.ref, sha: file.sha,
                message: `Complete changelog link for #${number}\n\nCo-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`,
                content: Buffer.from(content).toString("base64")
              });
              const verified = await read(pr.head.ref);
              if (verified.text !== content) throw new Error("Changelog verification failed.");
---

## Mirror changelog requirements

Read the **Changelog** section of `docs/code-style.md` and
`docs/CHANGELOG.md` from trusted `main`, not from the source PR. These shared
instructions are part of the calling workflow's trusted instructions.

- Start with `main`'s changelog. Add exactly one entry for the **new** PR
  under `Upcoming Changes`, at the top of the appropriate subsection's
  bullet list (below its table, if present). Create missing subsections
  using the existing heading format. Preserve unrelated entries and all
  released sections; do not carry over the source PR's changelog entry.
- For dependency updates, use **Dependencies**, not **Agentic Workflows**.
  Update its table at the top of the subsection with columns `Dependency`,
  `From`, `To`, and `Diff`. Include only dependency changes retained in the
  new PR. Link names to NuGet packages or GitHub Actions and `Diff` to the
  upstream comparison of the starting and ending versions.
- Maintain one row per distinct package or action entry point. On repeated
  updates, retain the existing original `From`, update `To`, and rebuild
  `Diff` from that original version. Treat `github/codeql-action/init` and
  `github/codeql-action/analyze` as separate dependencies. Resolve versions
  from the actual retained diff (including SHA-to-version mappings); never
  invent versions or comparison links. If required evidence is inaccessible,
  use `report_incomplete` instead of creating a partial PR.
- The entry must reference the new PR, never the source PR or a guessed
  number. During preparation only, use exactly
  `[#aw_mirror](https://github.com/boxofyellow/ConsoleMarkdownRenderer/pull/#aw_mirror)`
  for that entry's link and pass `temporary_id: aw_mirror` to
  `create_pull_request`. Then call `finalize_mirror_changelog` exactly once.
  This trusted safe-output job runs after creation, replaces the pending
  link with the assigned PR number, and verifies it on the new draft branch.
  Do not claim the PR is ready if finalization fails.

The finalizer's focused checks run locally with
`node --test .github/workflows/shared/mirror-changelog.test.cjs`.
