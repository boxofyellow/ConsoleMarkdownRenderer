const assert = require("node:assert/strict");
const fs = require("node:fs");
const path = require("node:path");
const test = require("node:test");

const workflow = fs.readFileSync(path.join(__dirname, "mirror-changelog.md"), "utf8");
const script = workflow.split("            script: |\n")[1].split("\n---")[0]
  .split("\n").map(line => line.slice(14)).join("\n");
const run = new (Object.getPrototypeOf(async function () {}).constructor)(
  "github", "context", "process", "core", script);
const owner = "boxofyellow";
const repo = "ConsoleMarkdownRenderer";
const pending = `[#aw_mirror](https://github.com/${owner}/${repo}/pull/#aw_mirror)`;
const link = `[#362](https://github.com/${owner}/${repo}/pull/362)`;
const changelog = `# Changelog\n\n## Upcoming Changes\n\n### :package: Dependencies :package:\n
| Dependency | From | To | Diff |
| --- | --- | --- | --- |
| Example | 1 | 2 | compare/1...2 |

- ${pending}: Mirror dependency updates
- [#360](https://github.com/${owner}/${repo}/pull/360): Existing entry

## v1.0.0

- Released entry
`;

function fixture(options = {}) {
  const updates = [];
  const reads = [];
  const pr = {
    state: "open",
    draft: true,
    head: { sha: "head-sha", ref: "new-mirror-branch", repo: { full_name: `${owner}/${repo}` } },
    base: { ref: "main" },
    ...options.pr
  };
  let text = options.text ?? changelog;
  const github = {
    rest: {
      pulls: { get: async args => {
        assert.deepEqual(args, { owner, repo, pull_number: 362 });
        return { data: pr };
      } },
      repos: {
        getContent: async args => {
          reads.push(args);
          if (options.readError) throw new Error("Read failed");
          return { data: options.file ?? {
            type: "file", encoding: "base64", sha: "file-sha",
            content: Buffer.from(options.verifyMismatch && updates.length ? "wrong" : text).toString("base64")
          } };
        },
        createOrUpdateFileContents: async args => {
          if (options.writeError) throw new Error("Write failed");
          updates.push(args);
          text = Buffer.from(args.content, "base64").toString("utf8");
        }
      }
    }
  };
  return {
    updates, reads,
    execute: () => run(github, { repo: { owner, repo } }, {
      env: { CREATED_PR_NUMBER: "362", SOURCE_PR_NUMBER: "356", ...options.env }
    }, { info: () => {} })
  };
}

test("resolves only the new PR link, preserving the dependency table and released entries", async () => {
  const f = fixture();
  await f.execute();
  assert.equal(f.updates.length, 1);
  assert.equal(Buffer.from(f.updates[0].content, "base64").toString("utf8"),
    changelog.replace(pending, link));
  assert.deepEqual(f.reads.map(read => read.ref), ["head-sha", "new-mirror-branch"]);
  assert.ok(f.reads.every(read => read.path === "docs/CHANGELOG.md"));
  assert.equal(f.updates[0].branch, "new-mirror-branch");
  assert.equal(f.updates[0].sha, "file-sha");
  assert.match(f.updates[0].message, /Co-authored-by: Copilot/);
});

test("rerunning finalization verifies an already completed link without another write", async () => {
  const f = fixture({ text: changelog.replace(pending, link) });
  await f.execute();
  assert.equal(f.updates.length, 0);
});

const invalidCases = [
  ["missing new PR", { env: { CREATED_PR_NUMBER: "" } }, /distinct/],
  ["invalid new PR", { env: { CREATED_PR_NUMBER: "bad" } }, /distinct/],
  ["nonpositive new PR", { env: { CREATED_PR_NUMBER: "0" } }, /distinct/],
  ["fractional new PR", { env: { CREATED_PR_NUMBER: "1.5" } }, /distinct/],
  ["unsafe integer new PR", { env: { CREATED_PR_NUMBER: "9007199254740992" } }, /distinct/],
  ["invalid source PR", { env: { SOURCE_PR_NUMBER: "bad" } }, /distinct/],
  ["nonpositive source PR", { env: { SOURCE_PR_NUMBER: "0" } }, /distinct/],
  ["source PR as target", { env: { CREATED_PR_NUMBER: "356" } }, /distinct/],
  ["closed PR", { pr: { state: "closed" } }, /draft branch/],
  ["non-draft PR", { pr: { draft: false } }, /draft branch/],
  ["fork branch", { pr: { head: { repo: { full_name: "other/repo" } } } }, /draft branch/],
  ["missing head repository", { pr: { head: { ref: "branch" } } }, /draft branch/],
  ["main as head", { pr: { base: { ref: "release" }, head: {
    ref: "main", repo: { full_name: `${owner}/${repo}` }
  } } }, /draft branch/],
  ["base as head", { pr: { base: { ref: "new-mirror-branch" } } }, /draft branch/],
  ["directory changelog", { file: [] }, /base64/],
  ["non-file changelog", { file: { type: "symlink" } }, /base64/],
  ["unsupported encoding", { file: { type: "file", encoding: "none" } }, /base64/],
  ["missing heading", { text: changelog.replace("## Upcoming Changes", "## Other") }, /section is missing/],
  ["missing pending link", { text: changelog.replace(pending, "no link") }, /exactly one/],
  ["duplicate pending link", { text: changelog.replace(pending, `${pending} ${pending}`) }, /exactly one/],
  ["pending link in released section", { text: changelog.replace(pending, "no link") + pending }, /exactly one/],
  ["duplicate in released section", { text: changelog + pending }, /exactly one/],
  ["existing new PR entry", { text: changelog.replace(pending, `${pending} ${link}`) }, /already has/],
  ["unresolved reference", { text: changelog + "\n#aw_mirror" }, /unresolved/],
  ["read failure", { readError: true }, /Read failed/],
  ["write failure", { writeError: true }, /Write failed/]
];

for (const [name, options, error] of invalidCases) {
  test(`rejects ${name} without writing`, async () => {
    const f = fixture(options);
    await assert.rejects(f.execute(), error);
    assert.equal(f.updates.length, 0);
  });
}

test("supports Upcoming Changes without any released sections", async () => {
  const text = changelog.split("\n## v1.0.0")[0];
  const f = fixture({ text });
  await f.execute();
  assert.equal(Buffer.from(f.updates[0].content, "base64").toString("utf8"), text.replace(pending, link));
});

test("reports a failed post-write verification", async () => {
  const f = fixture({ verifyMismatch: true });
  await assert.rejects(f.execute(), /verification failed/);
  assert.equal(f.updates.length, 1);
});
