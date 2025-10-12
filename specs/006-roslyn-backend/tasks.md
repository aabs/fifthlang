# Phase 2: Task List — Roslyn Backend Migration

This file contains the actionable tasks derived from Phase 0/1 artifacts and the user's implementation directives (target .NET 10, C# 14, remove legacy backend). Tasks are ordered with a TDD-first approach where feasible. Each task includes: id, title, owner (TBD), estimate (rough), dependencies, and acceptance criteria.

Repository absolute paths referenced below are rooted at `/Users/aabs/dev/aabs/active/5th-related/fifthlang`.

## Tasks

1. [P1-POC] Create Roslyn POC — LoweredAstToRoslynTranslator skeleton
   - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredAstToRoslynTranslator.cs` (create new file)
   - Owner: TBD
   - Estimate: 2d
   - Dependencies: none
   - Description: Implement a minimal translator that accepts the current Lowered AST inputs used by `AstToIlTransformationVisitor` and emits C# syntax trees for a tiny set of constructs: method declarations, calls, newobj, return, entrypoint. Emit `#line` directives and EmbeddedText for PDB mapping.
   - Acceptance: Produces a compilable CSharpCompilation that emits a portable PDB with SequencePoints for the translated constructs. Add a unit test under `test/ast-tests/` that uses the translator to emit code for a small sample and asserts the resulting assembly loads and PDB sequence points exist.

2. [P1-Mapping] Implement MappingTable and TranslationResult types
   - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/LoweredToRoslyn/MappingTable.cs` and `TranslationResult.cs`
   - Owner: TBD
   - Estimate: 1d
   - Description: Define `TranslationResult` (contains SyntaxTrees, MappingTable, Diagnostics) and `MappingTable` (maps LoweredAst node -> generated Syntax node -> SourceSpan). Use this in the POC for testable mapping verification.
   - Acceptance: Unit tests asserting mapping entries for a sample method and statements.

3. [P1-PDB] PDB SequencePoint verification harness
   - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/test/runtime-integration-tests/RoslynPdbVerificationTests.cs`
   - Owner: TBD
   - Estimate: 2d
   - Description: Create tests that compile SyntaxTrees via Roslyn in-memory, emit assembly+PDB streams, load PDB data (using `System.Reflection.Metadata`) and assert SequencePoints match expected source locations from the Lowered AST (line/column fidelity).
   - Acceptance: At least one test validates full line-and-column SequencePoints for a sample program.

4. [P1-TestsConversion] Survey and convert low-level IL tests
   - Path: `test/**` (affected tests will be enumerated and listed) — output a conversion plan file `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/preservation-inventory.md`
   - Owner: TBD
   - Estimate: 3d
   - Description: Identify all tests that assert `.il` output or rely on ILMetamodel shapes (`test/**/*build_debug_il/*.il` and unit tests referencing `ILMetamodel`). For each test, either: 1) convert it to a behavioral test that validates runtime behavior, 2) add it to the preservation inventory requiring a shim, or 3) drop it with documented rationale.
   - Acceptance: `preservation-inventory.md` created listing conversion outcome for each test (Converted / PreservationCandidate / Dropped), and at least 50% of conversions completed for high-priority samples.

5. [P1-CI] Add CI job matrix for Roslyn backend validation on .NET 8 and .NET 10
   - Path: `.github/workflows/roslyn-backend-validation.yml` (new workflow)
   - Owner: TBD
   - Estimate: 1d
   - Description: Create CI workflow that runs the full baseline test matrix (parser, ast, runtime-integration-tests, kg-smoke-tests) on both .NET 8 and .NET 10 SDKs using the Roslyn backend. The workflow must produce artifacts for inspection (assemblies, PDBs, test results) and report any regressions.
   - Acceptance: Workflow runs successfully in CI (manual run) and produces the test matrix; fails if behavioral regressions are detected.

6. [P1-Tooling] Add Directory.Build.props properties for Roslyn pin and C# language version
   - Path: `Directory.Build.props`
   - Owner: TBD
   - Estimate: 0.5d
   - Description: Add `RoslynVersion` property and set `LangVersion` to `14` (or `latest` with explicit 14 pin) for generated projects. Add MSBuild conditions for release builds to use pinned Roslyn package references.
   - Acceptance: `dotnet build` with pinned Roslyn reproduces the same compiler version for release builds; developers may continue to use SDK-provided Roslyn locally.

7. [P1-TranslationAPI] Add a Translator interface and plug into ParserManager
   - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/IBackendTranslator.cs` and edit `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/ParserManager.cs`
   - Owner: TBD
   - Estimate: 1d
   - Description: Define `IBackendTranslator` with a method `Translate(LoweredAst ast, CompilerOptions options) -> TranslationResult` and register the Roslyn translator in `ParserManager` immediately after the last language-lowering visitor.
   - Acceptance: `ParserManager` can invoke either a translator implementation or (temporarily) the existing IL lowering pipeline. For this task, add the Roslyn implementation path only; deletion of legacy pipeline is deferred until owner approval.

8. [P1-Diagnostics] Implement a unified Diagnostic model & initial migration for parser
   - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/Diagnostics/Diagnostic.cs` and update `Parser` and `AstBuilderVisitor` to emit structured diagnostics instead of throwing
   - Owner: TBD
   - Estimate: 3d
   - Description: Introduce a `Diagnostic` record with `Id`, `Severity`, `Message`, and `SourceSpan`. Migrate parser error handling to accumulate diagnostics and construct ErrorNode placeholders in the AST instead of throwing exceptions. Update transformation visitors to accept diagnostics and skip processing `ErrorNode` instances.
   - Acceptance: Parser tests that previously caused throws instead return diagnostics and partial ASTs; existing parser test harness updated accordingly.

9. [P1-Incremental] File-level parsing cache & compilation cache prototype
   - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/compiler/CompilationCache.cs`
   - Owner: TBD
   - Estimate: 3d
   - Description: Implement a file-level caching of parsed ASTs keyed by content hash and timestamp. Integrate with ParserManager to reuse parsed ASTs when unchanged. Provide a compile cache for generated SyntaxTrees to allow incremental Roslyn compilation experiments.
   - Acceptance: Running parse+translate twice with unchanged inputs reuses cached parse results and reduces CPU time for parsing; tests demonstrate cache hits.

10. [P1-LSP] LSP skeleton and workspace services (diagnostics + document service)
    - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/language-server/` (new project)
    - Owner: TBD
    - Estimate: 5d
    - Description: Create a starting Language Server project that provides basic document management and diagnostics support. Integrate the incremental parser and unified diagnostics to provide document-level diagnostics over LSP. This is a separate, incremental deliverable intended for later PRs.
    - Acceptance: LSP server can accept document text, parse with the incremental parser, and return diagnostics via LSP protocol handlers.

11. [P1-Preservation] Create Preservation Inventory and narrow legacy shims plan
    - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/preservation-inventory.md`
    - Owner: TBD
    - Estimate: 2d
    - Description: Record reflection/interop-sensitive members and tests that require IL-preserving behavior. For each, define whether to implement a shim, keep the legacy emitter for just that case, or re-specify the behavior with test changes.
    - Acceptance: Inventory completed and approved by migration owner; each preservation candidate has an assigned disposition (shim / legacy-emit / test-change).

12. [P1-Deprecation] Constitutional deviation & owner approval workflow
    - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/specs/006-roslyn-backend/constitutional-deviation.md`
    - Owner: Project lead (TBD)
    - Estimate: 1d
    - Description: Document the deviation from the constitution (immediate removal request) and a checklist required before deletion: owner approval, preservation inventory completed, CI validation passing on both SDKs, and a canary/rollout plan.
    - Acceptance: Deviation document signed off by project lead and a board (or designated approver) recorded in the spec.

13. [P1-Cleanup] Remove IL-level generated files from codegen only after approval (deferred)
    - Path: `/Users/aabs/dev/aabs/active/5th-related/fifthlang/src/code_generator/` and `src/ast-model/ILMetamodel.cs` (deletion to be gated)
    - Owner: TBD
    - Estimate: 2d (execution after approval)
    - Description: After constitution deviation approval and successful Roslyn backend validation, delete IL lowering/PEEmitter code and update `src/ast-model` and `src/ast-generated` accordingly (regeneration if needed). Ensure tests referencing IL files are converted first.
    - Acceptance: Legacy emitter source files removed in a single gated PR with passing CI and documented rollback steps.

## Ordering & Prioritization
- Start with POC tasks (1-3) to validate feasibility (P1-POC, P1-Mapping, P1-PDB).
- Parallelize test conversion (4), CI work (5), and toolchain pins (6).
- Work on diagnostics (8) and incremental caching (9) early as they enable LSP and incremental work.
- LSP (10) can be prepared in parallel but is lower priority than core translator validation.
- Constitutional deviation (12) must be completed and approved before executing cleanup (13).

## Next Steps (for /tasks command to execute)
- Mark Task 1 (POC) as in-progress and create the initial PR branch `006-roslyn-poс-poc` (or continue in existing feature branch as preferred).
- Create unit tests and a minimal sample under `test/ast-tests/CodeSamples` to exercise the POC translator.
- Run CI workflow for Roslyn backend validation and iterate until the POC's PDB mapping tests pass.
