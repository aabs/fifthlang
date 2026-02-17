Per-runner Baselines for Guard Validation

Overview

This repository stores canonical guard-validation benchmark baselines in a per-runner-family fashion.
Each baseline is a JSON file containing a mapping of benchmark names to median timings (in nanoseconds) and a small metadata block describing the environment where the baseline was produced.

Baseline file locations

- Generic baseline (fallback):
  - test/perf/baselines/guard_validation_baseline.json
- Per-runner-family baselines (preferred for strict comparisons):
  - test/perf/baselines/guard_validation_baseline.<family>.json

Where <family> is a short, sanitized string describing the runner family, derived from OS/distro and CPU model, e.g.:

  ubuntu24-epyc-7763
  macos-m1

Family naming rules

- The family string is computed in the Benchmarks workflow by combining the OS major version and a short architecture token (e.g. amd64 or arm64). Examples:

  - Ubuntu 24.04 on x86_64 -> "linux24-amd64"
  - macOS 14 on arm64 -> "macos14-arm64"

- This naming is intentionally stable: it avoids encoding exact CPU microarchitecture names (which vary a lot) and instead focuses on OS major version + architecture so baselines are more reusable and less noisy.

Why per-runner baselines?

Micro-benchmarks are sensitive to hardware, CPU topology, OS version, and underlying virtualization. Comparing numbers across different runner families (for example, local dev Mac vs GitHub-hosted Ubuntu EPYC) leads to false regressions. Using per-runner baselines reduces noise and increases signal when evaluating perf regressions.

Workflow behavior (how baselines are promoted)

- The Benchmarks GitHub Action runs on a runner and computes a family string for that runner.
- The compare step attempts to use a per-family baseline file (guard_validation_baseline.<family>.json). If that file exists it will be used.
- If the per-family baseline does not exist, the workflow falls back to the generic baseline (guard_validation_baseline.json) if present.
- To propose a new baseline for the current runner family, run the Benchmarks workflow with the workflow_dispatch input `updateBaseline=true`.
  - The job will generate guard_validation_current_baseline.<family>.json and open an automated PR that replaces/creates test/perf/baselines/guard_validation_baseline.<family>.json with the current results.
  - A human reviewer should inspect the PR (and the baseline metadata) before merging.

Policy for running strict perf assertions

- Strict perf assertions (the `perf-assertions` project) require a family-specific baseline in order to run on GitHub-hosted runners. The policy is conservative because hosted runners may change over time and we prefer to avoid flaky CI failures.
- If a family baseline exists, perf assertions run normally.
- If no family baseline exists:
  - If updateBaseline=true, the workflow writes the current baseline and opens a PR for promotion.
  - Otherwise, the workflow will skip running the strict perf assertions on hosted runners (to avoid noisy failures). The workflow will still generate artifacts and a compare summary.
- Self-hosted runners (or controlled perf runners) are allowed to run strict perf assertions even if no family baseline exists. If you want to run assertions on a self-hosted runner with no baseline, create or adjust the appropriate family baseline or set the `updateBaseline` flag to produce a candidate baseline PR.

Inspecting and reviewing baseline PRs

- Baseline PRs contain the new baseline JSON file and the metadata (OS, CPU, dotnet version, commit) that produced the baseline. Reviewers should ensure the environment is the intended one (for example, an authorized perf runner) and that the numbers look reasonable before merging.

Notes

- If you want to adopt a stricter or looser policy (for example, auto-merge baselines created on a protected runner or require multiple runs before promoting), ask and we can update the workflow to implement that policy (for example, add protected-branch rules, a reviewer requirement, or a second-run verification).

Interim note: CI enforcement of the macrobench "no measurable regression" gate is deferred; run the macrobench locally before pushing and include results in your PR description.
