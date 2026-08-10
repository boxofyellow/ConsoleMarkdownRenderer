# The `COPILOT_GITHUB_TOKEN` Secret

This repository runs a set of [GitHub Agentic Workflows](https://github.github.com/gh-aw/)
(gh-aw) — see the `*.md` / `*.lock.yml` pairs under
[`.github/workflows/`](../.github/workflows/):

- `code-style-guide-bot`
- `dependency-feature-scout`
- `pr-subset-mirror`

Each of these compiled workflows authenticates the **GitHub Copilot CLI**
agent engine using the `COPILOT_GITHUB_TOKEN` repository secret. This document
describes what that secret is used for and what permissions it needs, so it can
be recreated correctly when the current token expires.

## What the token is used for

`COPILOT_GITHUB_TOKEN` is **only** used to authenticate the Copilot CLI engine
so it can make model/inference requests on behalf of the workflow. It is **not**
used to read or write repository contents, issues, or pull requests.

You can confirm this from the compiled lock files. In each
`*.lock.yml`, the secret appears exclusively in the agent-execution steps:

- **`Validate COPILOT_GITHUB_TOKEN secret`** — checks the secret is present and
  well-formed before the run starts.
- **`Check for OAuth tokens`** — validates the token type.
- **The Copilot agent invocation** (`copilot ... --prompt-file ...`) — where it
  is passed as `COPILOT_GITHUB_TOKEN: ${{ secrets.COPILOT_GITHUB_TOKEN }}` so
  the CLI can reach the Copilot models.
- **`Redact secrets in logs`** — ensures the value is scrubbed from logs.

Crucially, the token is deliberately **excluded from the sandboxed agent
environment** via `--exclude-env COPILOT_GITHUB_TOKEN` on the `awf` firewall
command. That means the model-authentication token is never exposed to the
tools, MCP servers, or any untrusted content the agent processes.

## What permissions it needs

`COPILOT_GITHUB_TOKEN` should be a **fine-grained Personal Access Token (PAT)**
whose sole capability is making Copilot model requests:

- **Account permission → "Copilot Requests": Read-only** (labelled *Send
  Copilot requests* in the fine-grained PAT UI). This is the only access level
  the UI offers for this permission, and it is what lets the token call the
  Copilot model APIs used by the CLI engine.

No repository, contents, issues, pull-request, or workflow permissions are
required for **this** token. Grant nothing beyond Copilot requests — the token
follows the principle of least privilege because repository operations are
handled by the separate tokens described below.

### Understanding the previously-created token

If you inspect the token currently stored in the secret you may notice a few
things that look surprising. They are all consistent with the above:

- **It has repository access and repository permissions (e.g. code: read,
  issues: read/write).** These are *repository* permissions and are **not**
  used by this secret. Copilot access is granted by a separate **account**
  permission, so the repository scopes are unnecessary and were most likely
  added by mistake (or intended for one of the other tokens, such as
  `GH_AW_GITHUB_TOKEN`, which does need issue write for the create-issue safe
  output). A least-privilege replacement should drop them entirely.
- **The PAT UI reports it has "never been used".** This is expected even though
  the workflows depend on it. A fine-grained PAT's *last used* timestamp only
  reflects calls to the github.com REST/GraphQL API; Copilot model/inference
  requests go to the separate Copilot API endpoint, which does not update that
  timestamp. The scheduled agentic workflow runs succeeding (most recently on
  2026-08-01) confirm the token is in fact working.

### Recommended settings

- **Type:** Fine-grained personal access token.
- **Resource owner:** the account/organization that owns (and is billed for)
  the Copilot subscription used by these workflows.
- **Repository access:** none required for Copilot requests; if the PAT UI
  forces a selection, "Public Repositories (read-only)" is sufficient.
- **Permissions:** only *Copilot Requests: Read-only*.
- **Expiration:** set a finite expiration and calendar a renewal reminder
  before it lapses, since an expired token causes the
  `Validate COPILOT_GITHUB_TOKEN secret` step to fail and every agentic
  workflow run to stop.
- The account issuing the token must have an active GitHub Copilot seat/plan.

## How to (re)create the secret

1. Create a fine-grained PAT with only the *Copilot Requests* permission as
   described above.
2. Store it as an **Actions repository secret** named `COPILOT_GITHUB_TOKEN`
   (Settings → Secrets and variables → Actions), for example:

   ```bash
   gh secret set COPILOT_GITHUB_TOKEN --repo boxofyellow/ConsoleMarkdownRenderer
   ```

3. Manually re-run one of the agentic workflows (for example
   *Code Style Guide Bot* via **Run workflow**) to confirm the
   `Validate COPILOT_GITHUB_TOKEN secret` step passes.

## Related tokens (for context)

The agentic workflows use several distinct tokens for different jobs. Only the
first is about Copilot model access; the rest handle GitHub API operations and
are **not** interchangeable with `COPILOT_GITHUB_TOKEN`:

| Secret | Purpose | Notes |
| --- | --- | --- |
| `COPILOT_GITHUB_TOKEN` | Authenticates the Copilot CLI engine for model/inference requests. | Needs only *Copilot Requests*. Excluded from the agent sandbox env. |
| `GITHUB_TOKEN` | Default token GitHub Actions injects automatically. | Used as a fallback for API/MCP operations; not manually created. |
| `GH_AW_GITHUB_TOKEN` | Optional PAT for gh-aw safe-output and Git operations (e.g. opening PRs/issues). | Falls back to `GITHUB_TOKEN` when unset. Needs repo/PR/issue scopes as required by the workflow. |
| `GH_AW_GITHUB_MCP_SERVER_TOKEN` | Token handed to the GitHub MCP server for read tools (`repos`, `pull_requests`, `issues`). | Falls back to `GH_AW_GITHUB_TOKEN`, then `GITHUB_TOKEN`. |
| `GH_AW_CI_TRIGGER_TOKEN` | Token used to trigger downstream CI on branches pushed by a workflow. | Only present in workflows that push branches. |

When renewing `COPILOT_GITHUB_TOKEN`, do **not** add repository scopes to it —
keep those responsibilities on the tokens above.
