# Tasks: Roslyn Backend Migration (Feature 006)

**Input**: Design artifacts in `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/`
**Prerequisites**: `plan.md` (required), `research.md`, `data-model.md`, `quickstart.md`, `inventory-il.md`

> NOTE: All file paths in task descriptions are absolute and rooted at the repository root: `/Users/aabs/dev/aabs/active/5th-related/fifthlang`.

## Execution & Ordering Rules
- Setup tasks run first.
- Tests-first (TDD): Write failing tests before implementation whenever possible.
- Models before services; services before endpoints; core before integration; everything before polish.
- Mark `[P]` on tasks that can safely run in parallel (different files, no shared write conflicts).
- If two tasks edit the same file, do NOT mark them `[P]` and run them sequentially.

## Shortcuts for agent executors
- Branch naming convention: `006-roslyn-T{NNN}-{short-slug}` (e.g. `006-roslyn-T004-roslyn-pdb-test`).
- Per-task agent sequence (example):
  1. `git checkout -b 006-roslyn-T{NNN}-{slug}`
  2. Apply changes to the files listed in the task (use repo's apply_patch mechanism or edit files directly)
  3. `dotnet restore && dotnet build` (fail fast if build breaks)
  4. `dotnet test --filter FullyQualifiedName~{TestName}` for the specific tests added/changed
  5. `git add ... && git commit -m "T{NNN}: {short description}" && git push --set-upstream origin 006-roslyn-T{NNN}-{slug}`
  6. Open a PR with description and link to this task in the PR body

## Tasks (TDD-first, numbered)

### Phase 1 — Setup & Tooling

T001 - [ ] (setup) Add Roslyn pin and C# LangVersion to `Directory.Build.props`
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/Directory.Build.props`
- Owner: @aabs
- Estimate: 0.5d
- Description: Add a `RoslynVersion` MSBuild property (pin candidate: `4.1.0`) and set `LangVersion` to `14` for generated projects. Ensure an MSBuild `Condition` exists so release builds use the pinned Roslyn package, and developers may still use SDK-provided compiler locally.
- Dependencies: none
- Acceptance: `Directory.Build.props` contains `<RoslynVersion>4.1.0</RoslynVersion>` and `<LangVersion>14</LangVersion>`, `dotnet build` succeeds on the solution.
- Agent commands (example):
  - `git checkout -b 006-roslyn-T001-tooling`
  - Edit `/Users/aabs/.../Directory.Build.props` to add the properties under a top-level `<PropertyGroup>` (or update existing ones)
  - `dotnet build` to validate

T002 - [P] (setup) Add Roslyn & Metadata test dependencies to runtime integration project
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/runtime-integration-tests/runtime-integration-tests.csproj`
- Owner: @aabs
- Estimate: 0.25d
- Description: Add PackageReference entries:
  - `Microsoft.CodeAnalysis.CSharp` -> pinned version `4.1.0`
  - `System.Reflection.Metadata` -> pinned version `8.0.1`
  - Preserve existing test SDK/runner references
- Dependencies: T001 (preferred, but not strictly required to edit csproj)
- Acceptance: `dotnet restore` completes; test project can reference Roslyn APIs without compile errors.
- Agent commands:
  - `git checkout -b 006-roslyn-T002-add-roslyn-deps`
  - Modify the `.csproj` to add the package references and commit
  - `dotnet restore && dotnet build test/runtime-integration-tests/runtime-integration-tests.csproj`

T003 - [ ] (setup) Add a small POC CodeSample used by Roslyn PDB tests
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/ast-tests/CodeSamples/roslyn-poc-simple.5th`
- Owner: @aabs
- Estimate: 0.25d
- Description: Create a simple `.5th` sample that exercises functions, a method call and a return value. This sample will be used by both mapping tests and PDB verification harness.
- Dependencies: none
- Acceptance: File exists with a minimal program that the translator POC can translate and for which the PDB mapping expectations can be described precisely (line/column positions documented in the test expectations).
- Agent commands: create the file with a minimal sample and reference it in the mapping/PDB tests added in Phase 2.

### Phase 2 — Tests (TDD) — Write failing tests first

T004 - [P] (test) Roslyn PDB verification harness (failing test to drive POC)
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/runtime-integration-tests/RoslynPdbVerificationTests.cs`
- Owner: @aabs
- Estimate: 1d
- Description: Add/extend tests that compile generated C# SyntaxTrees using Roslyn and assert that the produced Portable PDB contains:
  - Document entries mapping to the expected generated source file path(s)
  - MethodDebugInformation entries with SequencePoints for statements (including start-line/start-column and end-line/end-column)
  - Coverage: Add a test that asserts at least one SequencePoint corresponds to the original `.5th` sample's known line/column position
- Dependencies: T003 (POC sample exists)
- Acceptance: Test compiles (may fail initially) and asserts the PDB shape; tests should be written to fail initially until the translator emits the expected mapping.
- Agent commands:
  - `git checkout -b 006-roslyn-T004-roslyn-pdb-test`
  - Add/extend `RoslynPdbVerificationTests.cs` with the described assertions
  - `dotnet test test/runtime-integration-tests --filter FullyQualifiedName~RoslynPdbVerificationTests`

T005 - [P] (test) Mapping unit tests for MappingTable and TranslationResult
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/ast-tests/LoweredToRoslynMappingTests.cs`
- Owner: @aabs
- Estimate: 0.5d
- Description: Add failing tests that create a miniature Lowered AST (or a mocked representation), pass it to the translator (or its mapping helper), and assert that `MappingTable` contains specific `MappingEntry` rows with expected `NodeId` -> SourceIndex/line/column mappings.
- Dependencies: T003 (POC sample)
- Acceptance: Tests express the exact mapping expectations and fail until the mapping code is implemented.
- Agent commands:
  - `git checkout -b 006-roslyn-T005-mapping-tests`
  - Extend `LoweredToRoslynMappingTests.cs` to include a failing mapping assertion for at least one node in `roslyn-poc-simple.5th`
  - `dotnet test test/ast-tests --filter FullyQualifiedName~LoweredToRoslynMappingTests`

T006 - [P] (test) TranslationResult & API contract tests
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/ast-tests/TranslationResultTests.cs`
- Owner: @aabs
- Estimate: 0.5d
- Description: Add unit tests asserting `TranslationResult` shape, that `Sources` contains expected entries after a minimal translation, and that `Diagnostics` are surfaced when translation fails.
- Dependencies: T004, T005
- Acceptance: Small unit tests that fail until `TranslationResult` is filled by POC translator.
- Agent commands:
  - `git checkout -b 006-roslyn-T006-translation-result-tests`
  - Add `TranslationResultTests.cs` and run the unit test project to confirm failures when translator is absent.

### Phase 3 — Models & Core Implementation (models before implementation)

T007 - [P] (model) Create `LoweredAstModule` model
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/LoweredAstModule.cs`
- Owner: @aabs
- Estimate: 0.5d
- Description: Define a lightweight representation of the lowered assembly/module used by the translator (Module Name, list of lowered types, list of lowered methods, source mapping origin information). Keep the model small and testable.
- Dependencies: T005 (mapping tests reference shape), T003 (sample)
- Acceptance: Model compiles and mapping tests can construct a `LoweredAstModule` instance to feed into translator tests.
- Agent commands:
  - `git checkout -b 006-roslyn-T007-create-lowered-ast-module`
  - Create file `LoweredAstModule.cs` with record type and basic fields and commit
  - `dotnet build`

T008 - [P] (model) Create `TranslationResult` type & refine existing file if necessary
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/TranslationResult.cs`
- Owner: @aabs
- Estimate: 0.25d
- Description: Ensure `TranslationResult` exposes `IReadOnlyList<string> Sources`, a `MappingTable` and `IReadOnlyList<Diagnostic> Diagnostics`. Add convenience factory helpers for test construction.
- Dependencies: T006
- Acceptance: Unit tests in T006 can construct `TranslationResult` fixtures successfully.
- Agent commands: `git checkout -b 006-roslyn-T008-translationresult && edit file && dotnet build`

T009 - [P] (model) Create `MappingEntry`/`MappingTable` (if not already complete)
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/MappingTable.cs`
- Owner: @aabs
- Estimate: 0.25d
- Description: Confirm `MappingEntry` (NodeId, SourceIndex, StartLine, StartColumn, EndLine, EndColumn) and ensure `MappingTable` exposes a query API that tests can use to assert mapping for a specific `NodeId`.
- Dependencies: T005, T007
- Acceptance: Mapping unit tests reference a stable API and compile.
- Agent commands: `git checkout -b 006-roslyn-T009-mapping-table && small edits && dotnet test`

T010 - [P] (model) Create `PreservationCandidate` model & initial inventory
- Path(s):
  - Model: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/PreservationCandidate.cs`
  - Inventory: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/preservation-inventory.md`
- Owner: @aabs
- Estimate: 1d
- Description: Define `PreservationCandidate` (identifier, reason, test-reference, recommended disposition) and populate `preservation-inventory.md` by scanning tests and known IL hotspots (see `inventory-il.md`).
- Dependencies: `inventory-il.md` (present in spec dir)
 - Acceptance: Inventory file contains an initial pass listing top preservation candidates and their recommended disposition (shim / keep legacy emitter / test-change). For each top-priority candidate include a `Representative-Sample-Path` and a `Required-Acceptance-Test` entry. At least one high-priority candidate must include either a passing acceptance test or an implemented shim before deletion gating (see FR-009).
- Agent commands:
  - `git checkout -b 006-roslyn-T010-preservation-inventory`
  - Create `PreservationCandidate.cs` and `preservation-inventory.md` and commit

T011 - [P] (model) Create `DiagnosticRecord` and align existing Diagnostic type
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/Diagnostics/DiagnosticRecord.cs` (or refine `CompilationResult.cs`)
- Owner: @aabs
- Estimate: 0.5d
- Description: Provide a durable `DiagnosticRecord` (Id, Severity, Message, SourceSpan) used across parser, translator and compiler phases. If `CompilationResult.Diagnostic` already exists adapt usage or create an alias type for the translator surface.
- Dependencies: T008
- Acceptance: Tests and translator diagnostics compile and the translator can return diagnostics into `TranslationResult`.
- Agent commands: `git checkout -b 006-roslyn-T011-diagnostic-record && add file && dotnet build`

T012 - [ ] (core) Implement `LoweredAstToRoslynTranslator` skeleton
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredAstToRoslynTranslator.cs`
- Owner: @aabs
- Estimate: 2d
- Description: Implement the translator skeleton so that it:
  - Accepts `LoweredAstModule` / `AssemblyDef` and produces `TranslationResult` with at least one generated source file for the POC sample
  - Populates `MappingTable` entries for at least one AST node
  - Returns diagnostics if translation cannot proceed for certain nodes
- Dependencies: T005-T011 (tests and models)
- Acceptance: `RoslynPdbVerificationTests` (T004) and mapping tests (T005) now pass for the minimal sample.
- Agent commands:
  - `git checkout -b 006-roslyn-T012-implement-translator`
  - Edit `LoweredAstToRoslynTranslator.cs` to produce a small SyntaxTree via Roslyn APIs or to return equivalent generated sources expected by the test harness
  - `dotnet test --filter FullyQualifiedName~RoslynPdbVerificationTests`

### Phase 4 — Conversion, Preservation & Shims

T013 - [ ] (analysis) Survey low-level IL tests and produce conversion plan
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/preservation-inventory.md`
- Owner: @aabs
- Estimate: 3d
- Description: Enumerate tests and artifacts that assert `.il` output or directly exercise `ILMetamodel` and produce an actionable conversion plan: convert to behavioral tests, implement shims, or keep under legacy emitter. Mark which tests must be executed unchanged and which can be converted.
- Dependencies: T010 (preservation inventory)
- Acceptance: `preservation-inventory.md` contains per-test disposition and a short conversion checklist for each test marked for conversion.
- Agent commands:
  - `git checkout -b 006-roslyn-T013-survey-il-tests`
  - Add entries to `preservation-inventory.md` with links to the test files and suggested disposition

T014 - [P] (conversion) Convert top-priority IL tests to behavioral tests
- Path: varies; examples under `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/**` (each converted test must list its new test file path)
- Owner: @aabs
- Estimate: 3d (first tranche)
- Description: For the highest-priority tests from the inventory, implement behavioral tests that validate runtime behavior rather than textual IL output. If conversion is impossible without preserving IL layout, mark test as preservation candidate.
- Dependencies: T013
- Acceptance: Converted tests pass against both legacy and Roslyn backends, or are documented preserved cases.
- Agent commands: standard per-test patch / branch sequence.

T015 - [ ] (preservation) Implement narrow runtime shims for preservation candidates (if necessary)
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/fifthlang.system/` or a dedicated runtime shim library path
- Owner: @aabs
- Estimate: 2d per candidate
- Description: For cases where IL semantics cannot be reproduced in C# directly but the observable runtime behavior must be preserved, implement small runtime shims (helper methods) that can be referenced by generated C# code.
- Dependencies: T013, T014
- Acceptance: Preservation candidate tests pass when the translator uses the shim interface.
- Agent commands: create shim stubs, wire translator to emit calls to shim helpers, run conversion tests.

### Phase 5 — CI, Flags & Canary

T016 - [ ] (ci) Create Roslyn backend validation CI workflow (SDK matrix)
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/.github/workflows/roslyn-backend-validation.yml`
- Owner: @aabs
- Estimate: 1d
- Description: New GitHub Actions workflow that runs critical test suites (parser, ast, runtime-integration, kg-smoke) on both .NET 8 and .NET 10-rc; produce artifacts (assemblies+pdbs) for inspection and enable optional manual gating for cut-over.
- Dependencies: T001, T002, T012 (POC tests should compile in CI)
- Acceptance: Workflow executes successfully on a sample PR and artifacts uploaded for inspection.
- Agent commands: add workflow file and push branch; run or request a workflow dispatch in CI.
  - Additional constraints (toolchain policy - Option A): The workflow MUST validate both SDKs and produce artifacts for both. The workflow should also include a lightweight PR guard that flags changes to `global.json` and requires a constitution amendment or explicit owner sign-off before allowing a change to the canonical pinned SDK. Explicitly: do NOT change `global.json` as part of this migration unless a constitution amendment is performed and recorded.

T017 - [ ] (feature) Add compiler backend selection flag and wiring (non-destructive)
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/CompilerOptions.cs` and `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/ParserManager.cs`
- Owner: @aabs
- Estimate: 0.5d
- Description: Add a `--backend` option (`legacy|roslyn`) and ensure `ParserManager` can instantiate `IBackendTranslator` implementations without deleting legacy emitter. Default behavior remains legacy until canary is approved.
- Dependencies: T012
- Acceptance: Local `dotnet run -- --backend=roslyn` triggers the Roslyn translator path for the POC.
- Agent commands:
  - `git checkout -b 006-roslyn-T017-backend-flag`
  - Modify `CompilerOptions` and `ParserManager` to read the flag and select translator
  - `dotnet build && dotnet run -- --backend=roslyn` against small sample

T018 - [ ] (ci) Canary: CI-overlay of legacy emitters for non-destructive validation (no repo copy)
- Path(s): CI job step and helper script: `scripts/ci-overlay-legacy.sh` (CI-only overlay of `src/code_generator/` into `src/legacy-emitters/`)
- Owner: @aabs
- Estimate: 1d
- Description: Instead of committing a copy of legacy emitters into the repository, implement a CI-only overlay that places a copy of the legacy emitter sources into the CI workspace at `src/legacy-emitters/` at job runtime. The overlay is created from a trusted source (for example `origin/master` or a dedicated archival branch) by checking out that ref into a separate path (e.g., `legacy-src`) and copying just the required sub-tree. This lets reviewers and CI validate the Roslyn backend side-by-side with the legacy emitter behavior without polluting the repository with throwaway code.

  Implementation sketch (CI job):
  1. Checkout the PR/feature branch as usual (`actions/checkout` default).
  2. Also checkout the authoritative legacy branch into a separate path:
     - `uses: actions/checkout@v4` with `ref: 'master'` and `path: 'legacy-src'` (or target archival branch or tag)
  3. Run the overlay helper: `scripts/ci-overlay-legacy.sh legacy-src src/code_generator src/legacy-emitters`
     - The script copies the necessary subtree from `legacy-src` into `src/legacy-emitters/` in the CI workspace.
  4. Build and run the Roslyn backend (`--backend=roslyn`) in CI; optionally, run the legacy backend in a separate job to gather comparison artifacts. Upload artifacts for inspection.

  Local reproduction (developer machine):
  - `git fetch origin master && mkdir -p legacy-src && git --work-tree=legacy-src checkout origin/master -- src/code_generator`
  - `./scripts/ci-overlay-legacy.sh legacy-src src/code_generator src/legacy-emitters`
  - `dotnet run -- --backend=roslyn` (or use the feature flag to select backend)

- Dependencies: T016, T017
- Acceptance: The CI canary run produces artifacts for review (Roslyn-built assemblies + PDBs) while the same PR contains no committed `src/legacy-emitters/` changes. The PR must not include a committed copy of legacy emitters. The overlay script and CI job must be documented and included in the canary run. Artifacts are uploaded and accessible for reviewers to validate equivalence and PDB fidelity.
- Agent commands:
  - `git checkout -b 006-roslyn-T018-ci-overlay`
  - Add `scripts/ci-overlay-legacy.sh` and update CI dispatch or `roslyn-backend-validation.yml` to include an overlay step gated behind a 'canary' label or a workflow input
  - Push branch and request a canary run (label PR with `canary` or use workflow dispatch)

T019 - [ ] (governance) Complete constitutional deviation sign-off checklist
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/constitutional-deviation.md`
- Owner: Project lead (TBD)
- Estimate: 0.5d
- Description: Ensure preservation inventory, CI green (on matrix), PDB mapping tests passing, and obtain owner approval recorded in the deviation document.
- Dependencies: T010, T012, T016
- Acceptance: Signed checklist and an approval comment recorded in the spec file.
- Agent commands: add approvals as comments and push a signed PR to the deviation file.

T026 - [ ] (governance) Codify toolchain policy (Option A) and add CI enforcement
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/spec.md` and `.github/workflows/roslyn-backend-validation.yml`
- Owner: Project lead (TBD)
- Estimate: 0.25d
- Description: Record the decision to keep the repository canonical SDK pinned to .NET 8 in `global.json` while treating .NET 10 as the development focus. Update `spec.md` (this file) and `plan.md` to reflect the policy (done). Add CI guard(s) to the T016 workflow that detect changes to `global.json` and require a special label/approval and a constitution amendment before allowing such PRs to proceed. Document the policy in the top-level README or `docs/` as appropriate.
- Dependencies: T016
- Acceptance: `spec.md` and `plan.md` include the Option A policy; CI workflow (T016) includes a check that flags PRs that modify `global.json` unless a special approver label is present; `global.json` remains unchanged in the feature branch.
- Agent commands:
  - `git checkout -b 006-roslyn-T026-toolchain-policy`
  - Ensure `spec.md` and `plan.md` are updated to include the Option A policy (this is already performed)
  - Add a small CI job in `roslyn-backend-validation.yml` that runs `git diff --name-only ${{ github.event.before }} ${{ github.sha }}` and fails or annotates the PR if `global.json` is modified without the required label/approval
  - Commit and push; request a workflow dispatch

T027 - [ ] (research) Resolve outstanding clarifications and close [NEEDS CLARIFICATION]
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/clarifications.md`
- Owner: TBD
- Estimate: 2d
- Description: Collect and resolve every `[NEEDS CLARIFICATION]` in `spec.md` and record each decision in `clarifications.md`. For each clarification record: question, chosen decision, rationale, owner, and date. Examples to resolve: canonical list of IL preservation candidates, signing requirements, named approvers for deletion, Roslyn pining policy for release builds, and performance measurement definitions.
- Dependencies: none
- Acceptance: `clarifications.md` is complete (one entry per prior marker), `spec.md` contains no unresolved `[NEEDS CLARIFICATION]` markers, and owners are assigned for each decision. Completion of T027 is REQUIRED before any deletion PR (T020) is merged.
- Agent commands:
  - `git checkout -b 006-roslyn-T027-clarifications`
  - Populate `clarifications.md` with a row per open question and commit
  - Update `spec.md` to remove resolved `[NEEDS CLARIFICATION]` markers (one commit per resolved group)
  - `dotnet build` to validate no spec-driven tests reference unresolved markers

 T020 - [ ] (cleanup) Prepare gated deletion PR for legacy emitters (deferred until sign-off)
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/code_generator/` and `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/ast-model/ILMetamodel.cs` and tests referencing IL
- Owner: @aabs
- Estimate: 2d (execute only after approvals)
- Description: After owner sign-off and canary period completion, remove legacy emitter source files in a single, well-documented PR referencing the constitutional-deviation checklist and the preservation inventory.
 - Dependencies: T019, T027
- Acceptance: Deletion PR merges only after CI passes and sign-off file is present in PR description.
- Agent commands: create a PR branch, delete files, run full CI matrix.

### Phase 6 — Diagnostics, Incremental, LSP & Polish

T021 - [ ] (diagnostics) Introduce a unified Diagnostic system and migrate parser to return diagnostics
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/Diagnostics/` and `src/parser/AstBuilderVisitor.cs`
- Owner: @aabs
- Estimate: 3d
- Description: Provide a stable diagnostic codeset and a parser flow that records diagnostics (instead of throwing) and returns partial ASTs. Update tests to assert diagnostics where previously they threw.
- Dependencies: T011
- Acceptance: Parser tests pass and newly created diagnostic tests validate handling of error nodes.

T022 - [ ] (incremental) File-level parse and compilation cache prototype
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/CompilationCache.cs`
- Owner: @aabs
- Estimate: 3d
- Description: Implement a content-hash keyed parse cache and a compilation cache for generated SyntaxTrees to speed iteration.
- Dependencies: T012
- Acceptance: Demonstrable cache hits on repeat runs of the same inputs; unit test showing cache reuse.

T023 - [ ] (lsp) LSP skeleton and document services
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/language-server/`
- Owner: @aabs
- Estimate: 5d
- Description: Create a minimal Language Server project (separate process) to host diagnostics and document parsing services (document open/close, quick diagnostics). Use incremental parser from T022.
- Dependencies: T021, T022
- Acceptance: LSP server accepts a document, parses it and returns diagnostics for a malformed sample.

T024 - [ ] (perf & polish) Add unit tests, docs and perf harness entries
- Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/perf/` and `docs/` and `README.md`
- Owner: @aabs
- Estimate: 2d
- Description: Add unit tests for mapping, PDB verification, update `docs/` with developer guidance (how to run the Roslyn backend locally, how to debug generated sources), and add a perf scenario to `test/perf/` for compile-time measurement.
- Dependencies: T012, T016
- Acceptance: Docs updated and perf scenario present in `test/perf/`.

T025 - [ ] (polish) Final regression & integration runs, sign-off and merge
- Path: N/A (process)
- Owner: Project lead
- Estimate: 1d
- Description: Run the full test suite (parser, ast, runtime-integration, kg-smoke) on the release pipeline and obtain final sign-off before removal of legacy emitters.
- Dependencies: T019, T020, T024
- Acceptance: Final sign-off documented and canary period completed without critical regressions.

---

## Parallel Execution Examples

1) Run model creation tasks (safe parallel group): `T007`, `T008`, `T009`, `T010`, `T011`
- Agent pattern to run in parallel (multiple agents/worker processes recommended):
  - For each task `T###`:
    - `git checkout -b 006-roslyn-T###-<slug>`
    - Apply the model file changes described in the task
    - `dotnet build`
    - `git commit` & push & open PR
  - If you must run in a single shell sequentially, do one-by-one.

2) Run test-writing tasks together (safe parallel group): `T004`, `T005`, `T006`
- Example local validation commands (fish shell):
  - `dotnet test /Users/aabs/dev/aabs/active/5th-related/fifthlang/test/runtime-integration-tests/runtime-integration-tests.csproj --filter FullyQualifiedName~RoslynPdbVerificationTests &`
  - `dotnet test /Users/aabs/dev/aabs/active/5th-related/fifthlang/test/ast-tests/ast-tests.csproj --filter FullyQualifiedName~LoweredToRoslynMappingTests &`
  - `wait`  # wait for the background tests to finish

3) CI parallelization guidance:
- Configure GitHub Actions matrix entries to run distinct test suites in parallel jobs: `parser-tests`, `ast-tests`, `runtime-integration-tests`, `kg-smoke-tests` across SDKs (`dotnet-8`, `dotnet-10-rc`).

## Task Validation Checklist (Gates)
- [ ] Setup tasks completed: T001, T002, T003
- [ ] Core POC tests added and failing (T004-T006)
- [ ] Basic models created (T007-T011)
- [ ] Translator skeleton implemented and POC tests pass (T012)
- [ ] Preservation inventory created and top conversions planned (T010, T013)
- [ ] CI matrix added and green for POC (T016)
- [ ] Constitutional-deviation checklist signed (T019)
 - [ ] All [NEEDS CLARIFICATION] resolved (T027)

---

If you want I can: 1) run the prerequisite script and confirm available docs (already done), 2) create or update `tasks.md` on disk (done by this change), and 3) start the first task (T001) by creating the branch and applying the Directory.Build.props change. Which one should I do next?
