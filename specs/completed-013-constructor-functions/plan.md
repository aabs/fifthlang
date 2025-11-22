ios/ or android/
# Implementation Plan: Constructor Functions

**Branch**: `001-constructor-functions` | **Date**: 2025-11-19 | **Spec**: `specs/001-constructor-functions/spec.md`
**Input**: Feature specification for constructor functions

## Summary
Add explicit class-named constructors (non-generic, overloadable) with base chaining, definite assignment enforcement, deterministic lowering, and structured diagnostics (CTOR001–CTOR008) while keeping performance overhead <5% and memory growth <2%. Implementation spans grammar recognition, AST additions, overload resolution, data-flow assignment analysis, inheritance validation, lowering to allocation-init sequence, code generation, and test/documentation coverage.

## Technical Context
**Language/Version**: C# 14 / .NET 8.0 (host), Fifth language surface (compiler target)  
**Primary Dependencies**: Antlr4.Runtime.Standard, RazorLight (for AST gen templates), xUnit + FluentAssertions, dunet, Vogen  
**Storage**: N/A (in-memory AST + type tables)  
**Testing**: xUnit (parser, AST, integration), runtime-integration-tests suite  
**Target Platform**: .NET 8.0 managed execution (cross-platform)  
**Project Type**: Multi-project compiler/toolchain (no new projects required)  
**Performance Goals**: Constructor resolution adds <5% compile time; diagnostics emit <1ms median; memory +2% max for medium codebases  
**Constraints**: No hand edits to `src/ast-generated/`; maintain deterministic ordering; no cancellation of build/test; must pass end-to-end runtime tests  
**Scale/Scope**: Applies across all user classes; expected typical projects with 10–50 constructors per assembly; worst-case synthetic tests up to 200 constructors for perf validation  

No clarifications pending; all parameters defined in spec.  

## Constitution Check
Principles adhered:
- Library-first: Uses existing `compiler`, `parser`, `ast-model`; no new library.
- Generator-as-source-of-truth: Adds AST metamodel entries only (if needed); regenerate.
- Test-first: Will introduce failing tests before implementation in `syntax-parser-tests`, `runtime-integration-tests` and possibly new targeted assignment tests.
- Multi-pass pipeline: Adds visitors/rewriters consistent with transformation philosophy (ConstructorLoweringRewriter).
- Determinism & Diagnostics: Structured codes CTOR001–CTOR008 follow existing pattern.

Gates: All preconditions satisfied; no violations to justify. Re-check after Phase 1 ensuring metamodel + grammar modifications accompanied by tests.

## Project Structure (Feature Impact)
Documentation additions under `specs/001-constructor-functions/` as per template; code impacts limited to existing directories:

```text
specs/001-constructor-functions/
├── spec.md
├── plan.md
├── research.md        # Phase 0
├── data-model.md      # Phase 1
├── quickstart.md      # Phase 1
├── contracts/         # Phase 1
└── (tasks.md later via /speckit.tasks)

src/
├── ast-model/                # Possible ConstructorDef additions (metamodel)
├── parser/grammar/           # Add constructor recognition (class-name method rule)
├── parser/AstBuilderVisitor.cs
├── compiler/LanguageTransformations/
│   ├── ClassCtorInserter.cs (existing - may adjust synthesis logic)
│   └── ConstructorLoweringRewriter.cs (new)
└── compiler/ParserManager.cs (pipeline sequencing adjustments)

test/
├── syntax-parser-tests/            # New parsing tests for constructor forms
├── runtime-integration-tests/      # End-to-end object creation, inheritance
└── ast-tests/                      # Field assignment analysis tests
```

**Structure Decision**: Reuse existing multi-project layout; add only new source files within existing projects (no complexity increase).

## Complexity Tracking
No constitution violations; table not required.

## Phase 0: Research & Decisions
Output file: `research.md`
Focus areas: overload resolution ranking, data-flow definite assignment strategy, diagnostic shape consistency, inheritance cycle detection, perf instrumentation strategy. All decisions to document rationale + alternatives.

## Phase 1: Design & Contracts
Artifacts: `data-model.md`, `contracts/diagnostics.md`, `contracts/lowering.md`, `quickstart.md`.
Data model enumerates: ConstructorDef, BaseConstructorCall, InstantiationExpression, RequiredFieldSet tracking structure.
Contracts define public (internal-to-compiler) interfaces: IConstructorResolver, IAssignmentAnalyzer, IConstructorLoweringService.
Quickstart shows steps: add constructor → regenerate → build → run tests.

## Phase 2: (Planning Boundary)
Placeholder: Detailed task decomposition, developer task tickets, sequencing with existing transformation passes, and performance benchmarking scripts will be enumerated via `/speckit.tasks`. No actions required at this stage.

## Performance Strategy
- Pre-filter overloads by arity & first parameter type hash.
- Cache successful resolutions keyed by (ClassTypeId, ParamSigHash).
- Single-pass forward data-flow for definite assignment (CFG built from statements; treat conditionals conservatively).

## Diagnostics Mapping
Codes CTOR001–CTOR008 with structured JSON emission fields: { code, message, class, signature, location { file, line, column }, hint }.

## Risks & Mitigations (Expanded)
- Overload explosion: Mitigate via indexing by arity & leading type.
- Assignment false negatives: Document conservative behavior; allow future refinement pass.
- Inheritance cycles rare: Depth guard + visited set.
- Performance regression: Benchmark before & after enabling caching.

## Test Plan (High-Level)
- Parser suite: valid/invalid constructor syntax (base initializer, disallowed return type, async keyword).
- Overload suite: exact, convertible, ambiguous.
- Assignment suite: single path, branching, early return, base chain effects.
- Inheritance suite: missing base call, cycle detection.
- Generic class suite: parameter substitution correctness.
- Negative diagnostics: one test per code.
- Perf suite: synthetic 200-constructor class compile time measurement.

## Tooling & Automation
Ensure CI parser-check includes new constructor samples; include sample `.5th` files under `src/parser/grammar/test_samples/`.

## Documentation Targets
Add section “Constructor Functions” to developer guide; update `learn5thInYMinutes.md` and link from spec.

## Exit Criteria for Phases 0–1
Phase 0: research.md finalized, no open questions.
Phase 1: data model & contracts stable, quickstart instructive, constitution re-check passes.

