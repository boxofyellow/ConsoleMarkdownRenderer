# Rebasing :copilot: PRs

This will work for other PRs too, but it comes up a lot when we have multiple :copilot: PRs open at the same time.  Since ever PR is supposed to up CHANGELOG.md, it is easy to get conflicts there.

:copilot: can easily rebase the PR and address these merge conflicts, but it lacks the permissions to force push to a branch.

So instead ask copilot to create a new branch starting with main, then ask it merge the current PR on top of that resolving the conflicts as it goes.

A prompt like this has worked in the past:
> @copilot This PR need to be rebased (not just having master merged on top). However you will not have permission to forcepush changes.
> 
> Please checkout the main branch, create a new branch and then replay this PRs commits onto top of that new branch while addressing conflicts.
> 
> When you are done please let me know the name of the new branch you created.

At this point you should have to branches
1. The original PR branch with the conflicts -- pr-branch
2. A new branch with the same commits but rebased on main -- tmp-branch

One done local do the following
1. `git checkout main`
1. `git pull` 
1. `git checkout pr-branch`
1. `git pull`
1. Take a quick looks to make sure things make sense
1. `git checkout tmp-branch`
1. Take a quick look to make sure things make sense
1. `git rev-parse origin/pr-branch` -- note the commit hash
1. `git rev-parse origin/tmp-branch` -- note the commit hash
1. These shas will be different
1. `git log --left-right --graph --oneline origin/pr-branch...origin/tmp-branch`
    ```
    > 143bd2d (origin/copilot/thematic-break-style) Update docs/CHANGELOG.md
    > 3e810ac Inline AddThematicBreakImplementation to a single expression-bodied line
    > cd6af06 Drop ThematicBreakTitle in favor of in-document headings
    > dd1e5f7 Expose Spectre.Console Rule style and title for thematic breaks
    > 1a62860 Initial plan
    > 0b36942 (HEAD -> main, origin/main, origin/HEAD) Honor Markdown pipe-table column alignment in ConsoleTableRenderer (#143)
    > 20f15e5 Render Markdig Figure and FigureCaption blocks (#142)
    > 42efafd Render Markdig AbbreviationInline nodes with their expansion title (#141)
    > 8f3862a Skip docs/ and README.md changes in CI workflows (#146)
    < 8f5dbb8 (origin/copilot/expose-rule-widget-style-title) Update docs/CHANGELOG.md
    < 1bce3b0 Inline AddThematicBreakImplementation to a single expression-bodied line
    < 9d5f5bc Drop ThematicBreakTitle in favor of in-document headings
    < 7ff31bc Expose Spectre.Console Rule style and title for thematic breaks
    < 2fd779b Initial plan
    ```
    In this example `copilot/thematic-break-style` is the tmp-branch and `copilot/expose-rule-widget-style-title` is the pr-branch.  You can see that the commits are the same but the shas are different because of the rebase.
    You can also see that commits from main that are present in the tmp-branch but not the pr-branch.
1. `git push --force-with-lease origin origin/tmp-branch:pr-branch`
1. This should updated the PRs branch to be at the same commit, and PR should reflect that change.
1. `git rev-parse origin/pr-branch` -- note the commit hash
1. `git rev-parse origin/tmp-branch` -- note the commit hash
1. These shas should now be the same and match the commits in from tmp-branch from before.


Once done verified the PR, and then tmp-branch can be deleted.