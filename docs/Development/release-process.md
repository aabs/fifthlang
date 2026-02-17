# Fifth Language Release Process

This document explains how we create official Fifth Compiler releases, what triggers the Release Packaging workflow, and how to keep the process predictable. It reflects the current automation defined in `.github/workflows/release.yml`.

## Overview

All release artifacts (platform-specific archives, metadata bundles, and checksum manifests) are produced by the **Release Packaging** GitHub Actions workflow. The workflow builds across Linux, macOS, and Windows with both .NET 8.0 and .NET 10.0 targets, validates the bundles via smoke tests, assembles metadata, and finally publishes a GitHub release with the generated archives and `SHA256SUMS` file.

## Triggers

| Trigger | When to use | Effect |
| --- | --- | --- |
| **Tag push** | Push a tag matching `vX.Y.Z` or `vX.Y.Z-qualifier` (e.g., `v1.4.0`, `v1.4.1-beta1`) to any branch | Starts Release Packaging using that commit. This is the normal production release path. |
| **Manual dispatch** | Use the *Run workflow* button in GitHub, supply `version` and optional `dry_run` | Builds artifacts for the provided version. If `dry_run=true`, packages and checksums are produced but the GitHub release step is skipped. Useful for rehearsals. |

## Prerequisites

1. **Clean repository**: `scripts/release/version-info.sh` aborts when uncommitted changes are present unless `--allow-dirty` is passed. Keep the tree clean before tagging.
2. **Passing CI**: Ensure `dotnet build fifthlang.sln` and the full `dotnet test fifthlang.sln` suite complete successfully on the target branch.
3. **Version agreement**: Decide the semantic version (e.g., `1.5.0` or `1.5.0-rc1`). The workflow infers the version from the pushed tag, so double-check spelling before publishing.
4. **Repository permissions**: Only maintainers with push access to tags can trigger production releases. GitHub automatically supplies a `GITHUB_TOKEN` with `contents: write`, enabling `softprops/action-gh-release` to publish.

## Standard Release Procedure

1. **Stabilize the branch**
   - Merge the desired changes into `master` (or a dedicated release branch) and make sure CI passes.
   - Run `just build-all` (or `dotnet build fifthlang.sln`) locally for a final sanity check.

2. **Create and push the tag**
   - Choose the version identifier and create an annotated tag: `git tag -a v1.5.0 -m "Fifth 1.5.0"`.
   - Push the tag (and any supporting branch): `git push origin v1.5.0`.
   - Tag pushes automatically enqueue the Release Packaging workflow.

3. **Monitor the workflow**
   - Track the workflow run in GitHub Actions. Key stages:
     - **Build matrix**: Each OS/runtime combination produces an archive and metadata JSON.
     - **Verification**: `scripts/test/smoke-test.sh` validates the packaged compiler.
     - **Publish job**: Aggregates metadata, verifies coverage (6 net8 + 6 net10 packages expected), generates `SHA256SUMS`, composes release notes, and runs `softprops/action-gh-release`.

4. **Validate the published release**
   - Confirm the GitHub release page lists all `.tar.gz`/`.zip` artifacts and `SHA256SUMS`.
   - Optionally download a package and verify `shasum -c SHA256SUMS` to ensure integrity.

5. **Announce** (optional)
   - Update documentation, changelogs, or blog posts referencing the new version.

## Manual Dispatch Workflow

When running the workflow manually (e.g., for a rehearsal):

1. Navigate to *Actions → Release Packaging → Run workflow*.
2. Supply the `version` input (e.g., `1.5.0-rc1`) and toggle `dry_run` if you want to skip publishing.
3. The workflow builds exactly like the tag-triggered path but relies on the provided version string.
4. Review the artifacts in the run summary or download them from the workflow’s artifacts section.

## Failure Recovery

- **Package Build Failures**: Fix the underlying issue (missing SDK, smoke test failure, etc.), push a new commit, re-tag (e.g., delete and recreate `v1.5.0`), and force-push the tag.
- **Publish Failures**: Most publication issues stem from missing permissions or invalid release notes. After fixing the workflow, re-push the tag to rerun publishing (GitHub will overwrite the existing draft release).
- **Checksum Issues**: If `SHA256SUMS` generation fails, inspect `scripts/build/generate-checksums.sh`. Once fixed, rerun the publish job by deleting/recreating the tag.

## Addendum: Opportunities to Streamline Releases

1. **`just` Helpers**
   - Add recipes such as `just prepare-release VERSION=1.5.0` to run tests, check cleanliness, create the tag, and push it. This reduces manual steps and encodes best practices.
   - Another recipe (`just dry-run-release VERSION=1.5.0-rc1`) could trigger the workflow dispatch via GitHub CLI with `dry_run=true` for rehearsals.

2. **GitFlow-style Branching**
   - Adopt `release/x.y.z` branches to stage fixes separate from `master`. When ready, finish the branch by tagging `vX.Y.Z` and merging back into both `master` and `develop`. This keeps stabilization work isolated and makes it easier to produce hotfixes.
   - GitFlow also clarifies responsibility: only changes merged into the release branch flow into the release, minimizing accidental scope creep.

3. **Automated Tag Validation**
   - Extend the Justfile or a pre-push hook to validate that `git status` is clean and that `scripts/release/version-info.sh --format text` matches the intended version before allowing the tag push.
   - Optionally script a `just release VERSION=1.5.0` command that wraps: calculcating changelog entries, creating the tag, pushing branch/tag, and opening the release page for verification.

Implementing one or more of these ideas will reduce human error, keep the process repeatable, and free maintainers from rote tagging chores.
