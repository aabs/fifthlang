# Tasks: Roslyn Backend Migration (T‑series)

Feature: Roslyn Backend Migration
Spec: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/spec.md`
Plan: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/plan.md`
Data model: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/data-model.md`

Repository root (absolute): `/Users/aabs/dev/aabs/active/5th-related/fifthlang`

Guiding rules applied when generating tasks:
- Tests first (TDD): every implementation task references a preceeding test task where feasible.
- Setup tasks run before all others.
- Tasks that operate on different files are marked `[P]` (parallelizable).
- No destructive deletion of legacy emitter code will occur without explicit owner approval and confirmation of preservation inventory (gated task). See T021 (Cleanup gating).

Numbering, format and ordering follow the project template: T001..T0NN. Each task has: ID, Title, Owner (TBD), Estimate, File path(s) to create/change, Dependencies (task IDs), Description, Acceptance criteria, Example agent commands (fish shell) to start the work.

---

SECTION A — SETUP & TOOLING

T001 — Create POC branch and workspace
 - Owner: TBD
 - Estimate: 0.25d
 - Paths: N/A (repo-level)
 - Dependencies: none
 - Description: Create a short-lived work branch for the POC work and ensure local toolchain readiness instructions are present in quickstart.md.
 - Acceptance: Branch `006-roslyn-poc` exists and quickstart.md updated with how to run the POC locally.
 - Agent commands (fish):
   - git checkout -b 006-roslyn-poc  # create POC branch from current feature branch
   - printf '1. Install .NET 10-rc
2. Use dotnet tool to run POC
' > /Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/quickstart.md

T002 — Add Roslyn pin + C# language version to MSBuild (`Directory.Build.props`)
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/Directory.Build.props`
 - Dependencies: T001
 - Description: Add a `RoslynVersion` MSBuild property and a `LangVersion` override for generated compilation to C# 14. Provide conditional NuGet PackageReference injection in Release builds to use the pinned Roslyn package (avoid changing `global.json` which remains pinned at .NET 8 per constitution). This makes release builds reproducible while allowing local dev to use SDK-provided Roslyn.
 - Acceptance: `dotnet restore` and `dotnet build -c Release` succeed and the Release build uses the pinned Microsoft.CodeAnalysis package (verify `dotnet list package --include-transitive` shows pinned version when `Configuration=Release`).
 - Agent commands (fish):
   - git switch -c 006-roslyn-tooling
   - # Implement Directory.Build.props changes (developer edits)
   - dotnet restore -v minimal --configfile nuget.config
   - dotnet build -c Release

T003 — Add CI skeleton for .NET 8 + .NET 10-rc matrix (workflow file)
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/.github/workflows/roslyn-backend-validation.yml`
 - Dependencies: T002
 - Description: Create a GitHub Actions workflow that runs the baseline test matrix on an OS/SDK matrix: {ubuntu-latest, windows-latest, macos-latest} × {.NET 8.x (from global.json), .NET 10-rc}. Include a job step to run the Roslyn backend using `--backend=roslyn` or env var `BACKEND=roslyn`.
 - Acceptance: The workflow file is present and can be executed; a manual dispatch run completes the build+test steps for the sample POC branch without destructive changes.
 - Agent commands (fish):
   - git checkout -b 006-roslyn-ci
   - # Create workflow YAML file per path above
   - gh workflow run roslyn-backend-validation.yml -f ref=006-roslyn-poc

---

SECTION B — DATA MODEL (entity tasks) [P]
Note: Each entity in `data-model.md` becomes a model creation task and is parallelizable because they are different files.

T010 [P] — Create model: `LoweredASTModule`
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/Models/LoweredASTModule.cs`
 - Dependencies: T002
 - Description: Create a minimal POCO representing a module-level lowered AST wrapper used by the translator (module name, list of top-level lowered nodes, source mapping reference). Include XML doc comments.
 - Acceptance: Compiles in the compiler project and is used by a placeholder unit test.
 - Agent commands (fish):
   - git switch 006-roslyn-poc
   - printf '%s' "// create LoweredASTModule.cs skeleton" > /Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/Models/LoweredASTModule.cs

T011 [P] — Create model: `TranslationResult`
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/TranslationResult.cs`
 - Dependencies: T010
 - Description: Create `TranslationResult` carrying generated SyntaxTrees, MappingTable reference, and Diagnostics collection.
 - Acceptance: Unit test can construct a `TranslationResult` and read its fields.

T012 [P] — Create model: `MappingEntry`
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/MappingEntry.cs`
 - Dependencies: T010
 - Description: Implement a MappingEntry (Lowered node id → generated SyntaxNode id → SourceSpan). Include serialization helpers for test inspection.
 - Acceptance: Mapping entry unit test serializes/deserializes the entry and asserts equality.

T013 [P] — Create model: `PreservationCandidate`
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/Preservation/PreservationCandidate.cs`
 - Dependencies: T012
 - Description: Data structure representing a reflection/interop-sensitive member (member identity, reason, test references, recommended disposition).
 - Acceptance: The preservation inventory can be generated and mapped into `PreservationCandidate` instances.

T014 [P] — Create model: `DiagnosticRecord`
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/Diagnostics/DiagnosticRecord.cs`
 - Dependencies: T002
 - Description: A small record type representing a diagnostic with Id, Severity, SourceSpan, and optional structured data.
 - Acceptance: Parser diagnostic unit test can create and inspect a `DiagnosticRecord`.

---

SECTION C — TESTS FIRST (TDD tasks) [P]

T020 [P] — Add PDB SequencePoint verification tests (POC harness)
 - Owner: TBD
 - Estimate: 2d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/runtime-integration-tests/RoslynPdbVerificationTests.cs`
 - Dependencies: T010–T014
 - Description: Implement tests that construct a minimal lowered AST or a small sample `.5th` program, translate it to C# SyntaxTrees via a stub translator, compile in-memory using Microsoft.CodeAnalysis, emit assembly+pdb to MemoryStreams, and then use `System.Reflection.Metadata` to assert that SequencePoints exist and match expected line/column numbers. Keep the test isolated (no external runtime dependencies).
 - Acceptance: Test runs in CI in the POC job and asserts SequencePoints for at least one sample method.
 - Example test-run command (fish):
   - dotnet test /Users/aabs/dev/aabs/active/5th-related/fifthlang/test/runtime-integration-tests -f net8.0 --filter RoslynPdbVerificationTests

T021 [P] — Add Mapping unit tests (mapping invariants)
 - Owner: TBD
 - Estimate: 1d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/ast-tests/LoweredToRoslynMappingTests.cs`
 - Dependencies: T010–T014
 - Description: Unit tests that assert `MappingTable` contains expected entries for a small lowered AST input and that `TranslationResult` exposes SyntaxTrees and mapping info.
 - Acceptance: Tests pass locally and on CI when the model skeletons are in place.

T022 — Survey & inventory existing IL tests (automation)
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/preservation-inventory.md`
 - Dependencies: none
 - Description: Run a deterministic scan of repo test outputs and unit tests for references to `.il` outputs or IL metamodel types and produce `preservation-inventory.md` which lists candidate files, tests, and why they matter.
 - Acceptance: The `preservation-inventory.md` is created and includes at minimum the quick grep hits and proposed disposition columns.
 - Example agent commands (fish):
   - grep -R --line-number --exclude-dir=.git --include='*.il' '/Users/aabs/dev/aabs/active/5th-related/fifthlang' || true
   - grep -R --line-number --exclude-dir=.git --include='*.cs' "ILMetamodel" '/Users/aabs/dev/aabs/active/5th-related/fifthlang' || true

---

SECTION D — CORE IMPLEMENTATION (after tests) — sequential where same files affected

T030 — Implement `IBackendTranslator` and ParserManager integration
 - Owner: TBD
 - Estimate: 1d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/IBackendTranslator.cs` and `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/ParserManager.cs`
 - Dependencies: T010–T014, T020–T021
 - Description: Create the interface `IBackendTranslator` (Translate method) and add wiring in `ParserManager` to call the translator immediately after language lowering. The translator should return `TranslationResult`. Do not remove or modify existing legacy lowering/emitter logic — only add a new invocation path that can be enabled via CLI flag (T040).
 - Acceptance: `ParserManager` compiles and a unit test can invoke the translator path without disabling the legacy pipeline.

T031 — Implement minimal `LoweredAstToRoslynTranslator` (POC)
 - Owner: TBD
 - Estimate: 2d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredAstToRoslynTranslator.cs`
 - Dependencies: T030, T020, T021
 - Description: Implement translator skeleton that consumes `LoweredASTModule` and emits C# SyntaxTree(s) for a small subset of constructs (method, call, newobj, return). Include `#line` pragmas to map source and create a `TranslationResult` for tests to assert.
 - Acceptance: PDB SequencePoint test (T020) passes when translator is used in unit tests; no legacy emitter deletion performed.

T032 — Implement `TranslationResult` and `MappingTable` code (concrete)
 - Owner: TBD
 - Estimate: 1d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/TranslationResult.cs` and `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/MappingTable.cs`
 - Dependencies: T011–T013, T031
 - Description: Implement the concrete data structures used by translator and mapping tests.
 - Acceptance: Mapping unit tests (T021) pass.

T033 — Implement simple CompilationCache prototype
 - Owner: TBD
 - Estimate: 2d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/CompilationCache.cs`
 - Dependencies: T030, T031
 - Description: Implement file-level parse cache keyed by file content hash. Integrate with ParserManager to reuse parse result for unchanged files.
 - Acceptance: Demonstrable cache-hit on repeated parse+translate runs (add a unit test to assert cache usage).

---

SECTION E — INFRA (CI, Signing, Observability, Preservations & Gating)

T040 — Add `--backend=roslyn|legacy` CLI option and MSBuild property
 - Owner: TBD
 - Estimate: 0.5d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/CompilerOptions.cs` and `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/ParserManager.cs`
 - Dependencies: T030
 - Description: Introduce a CLI option `--backend` and corresponding CompilerOptions property; wire it so default is `legacy` and `roslyn` triggers the translator path. Ensure unit tests can set CompilerOptions programmatically.
 - Acceptance: Running `dotnet run --project src/compiler -- --backend=roslyn` selects the translator invocation path; running with `--backend=legacy` uses the legacy emitter. No deletions of legacy emitter code.

T041 — Add Signing task (P1-Signing)
 - Owner: TBD
 - Estimate: 1d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/signing.md` and `Directory.Build.props` updates
 - Dependencies: T002, T003
 - Description: Create a formal signing policy (where keys are stored, CI steps to sign release assemblies) and add MSBuild props to enable signing in Release builds. Add verification tests that signed assemblies contain expected public key tokens.
 - Acceptance: Release build artifacts are signed and CI can verify signatures using `sn` or equivalent tooling.

T042 — Observability & artifact identity (P1-Observability)
 - Owner: TBD
 - Estimate: 1d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/observability.md`
 - Dependencies: T003
 - Description: Define telemetry tokens for backend used per build (e.g., embed build-info resource with backend id) and CI artifact upload steps. Create a simple monitor script to compare artifact identity between legacy and Roslyn builds for the same sample input.
 - Acceptance: CI artifacts include a `backend.json` manifest that records backend id and Roslyn version; monitor script can compare manifests.

T043 — Preservation shims plan & implementation (blocked)
 - Owner: TBD
 - Estimate: 2d (after inventory)
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/fifthlang.system/PreservationShims.cs`
 - Dependencies: T022 (preservation inventory) and T012 (owner approval via T021)
 - Description: For each preservation candidate requiring exact IL or special behavior, implement a small runtime shim or wrapper in `fifthlang.system` that the translator can call. This is intentionally blocked until preservation inventory is approved.
 - Acceptance: For each shim, a unit or integration test demonstrates parity for the specific reflection/interop-sensitive scenario.

T044 — Final cleanup gating / deletion task (blocked, high-safeguard)
 - Owner: Project lead (must be explicit approver)
 - Estimate: 1d (execution after approvals)
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/code_generator/` and `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/ast-model/ILMetamodel.cs`
 - Dependencies: T041 (signing), T022 (inventory), T040 (backend flag), T003 (CI green), T021 (owner sign-off)
 - Description: Perform legacy emitter deletion only after all gating items are satisfied. The task includes: a) a non-destructive PR that moves legacy sources to `src/legacy-emitters/` for a canary interval, b) running full CI matrix on that PR, c) collecting acceptance sign-offs, d) final deletion PR after canary window.
 - Acceptance: All preconditions pass; final deletion is merged only with explicit owner approval and CI green on both SDKs/OS matrix.

---

SECTION F — POLISH & VALIDATION [P]

T050 [P] — Add property-based & unit tests for translator invariants
 - Owner: TBD
 - Estimate: 2d
 - Path: `test/property/TranslatorProperties.cs` and `test/ast-tests/*` updates
 - Dependencies: T031, T032
 - Description: Add property-based tests (e.g., FsCheck or Hypothesis-like approach in .NET) that validate mapping invariants: mapping counts, monotonicity, and idempotence of translation for sanitized inputs.
 - Acceptance: Property tests run locally and in CI; failures produce targeted counterexamples.

T051 — Performance benchmark harness (stabilization phase)
 - Owner: TBD
 - Estimate: 2d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/perf/roslyn-backend/` and scripts in `scripts/perf/`
 - Dependencies: T031, T032
 - Description: Add benchmark harness comparing compile-time and simple runtime characteristics between legacy and Roslyn pipelines for representative samples. The harness must not gate migration but must record baselines for stabilization phase.
 - Acceptance: Benchmarks run and produce CSV and HTML outputs in `BenchmarkDotNet.Artifacts/results/`.

T052 — Maintainer documentation & quickstart finalization
 - Owner: TBD
 - Estimate: 1d
 - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/maintainer-guide.md` and `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/quickstart.md`
 - Dependencies: T031, T032, T041
 - Description: Produce step-by-step maintainer documentation for running the translator, debugging PDB mapping, running the CI validation job, and the deprecation checklist. Include exact commands for fish shell.
 - Acceptance: Docs reviewed by one maintainer and incorporated into the spec folder.

---

PARALLEL GROUPS (examples & agent commands)
- Group P1: Model skeletons (T010, T011, T012, T013, T014) — safe to implement in parallel because different files
  - Example agent run (fish):
    - # Run three independent model creation tasks in background (example):
    - (apply_patch --some-script-to-add-file1 &); (apply_patch --some-script-to-add-file2 &); wait

- Group P2: Tests-first group (T020, T021, T022) — can be authored in parallel
  - Example: run unit test file creation and then run `dotnet test` in parallel per OS in CI matrix

AGENT COMMAND EXAMPLES (fish)
- Create branch and push:
  - git checkout -b 006-roslyn-poc; git push -u origin 006-roslyn-poc
- Run the PDB verification test locally (net8):
  - dotnet test /Users/aabs/dev/aabs/active/5th-related/fifthlang/test/runtime-integration-tests -f net8.0 --filter RoslynPdbVerificationTests
- Run the CI matrix locally (example using act or just run commands):
  - # For each SDK version run: dotnet build -f net8.0 && dotnet test -f net8.0

TASK NAMING / DEPENDENCY NOTES
- Use task ID prefixes when referencing a dependency (e.g., T031 depends on T030 and T020). Always resolve `T022` (preservation inventory) and `T021` (owner approval) before T044 (cleanup deletion).

---

If you want, I can now (pick one):
 - A) Convert these T‑series tasks into the repository `tasks.md` (this file is already updated) and open PRs for the top N implementation tasks (requires your approval and owner assignment).
 - B) Produce the precise apply_patch patches for the top N setup and test scaffold tasks (T002, T020, T021, T030, T031) so they can be applied immediately.
 - C) Keep this plan as-is and wait for owners/approvals before generating code-level patches.

