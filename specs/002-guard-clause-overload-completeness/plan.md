# Implementation Plan: Guarded Overload Completeness Validation & Destructuring Bonus Fix

**Branch**: `002-guard-clause-overload-completeness` | **Date**: 2025-09-17 | **Spec**: specs/002-guard-clause-overload-completeness/spec.md
**Input**: Feature specification from `/specs/002-guard-clause-overload-completeness/spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path ✔
2. Fill Technical Context (scan for NEEDS CLARIFICATION) – none remaining; spec fully enumerates FR-001..070 & AC-001..038
3. Constitution Check (initial) – PASS (single solution, no excess layering)
4. Execute Phase 0 → research.md (produced below)
5. Execute Phase 1 → data-model.md, contracts/, quickstart.md (produced below)
6. Re-evaluate Constitution Check – PASS
7. Plan Phase 2 (task generation approach) – documented
8. STOP
```

## Summary
Introduce a compiler validation phase after symbol/type binding to enforce guarded function overload completeness, reachability, and structural guard quality. Emit deterministic diagnostics (E1001, W1002, E1004, E1005, W1101, W1102) with dual highlighting and secondary notes. Provide guard normalization (interval / equality reasoning, UNKNOWN classification) and fix the `destructuring_example_ShouldReturn6000` exit code discrepancy. Performance target: ≤5% compile time overhead, complexity O(n^2 + n*k).

## Technical Context
**Language/Version**: C# (.NET 8.0) custom compiler for Fifth language
**Primary Dependencies**: Internal AST model, parser (ANTLR 4.8 runtime), diagnostic infrastructure, code generator
**Storage**: N/A (in-memory compilation)
**Testing**: TUnit + FluentAssertions integration/runtime tests
**Target Platform**: .NET 8 toolchain (macOS/Linux CI)
**Project Type**: Single compiler/runtime solution (Option 1 structure)
**Performance Goals**: Validator adds <5% wall clock; O(n^2 + n*k) per group; minimal allocations (reuse lists)
**Constraints**: Must not mutate existing AST shapes; no manual edits to generated code; no added external dependencies
**Scale/Scope**: Typical function overload groups <10; edge scenarios tested up to ≥33 for warnings

## Constitution Check
**Simplicity**:
- Projects: 1 feature phase added inside existing compiler (no new project)
- Framework wrappers: None added
- Single data model extension: OverloadInfo / PredicateDescriptor (internal only)
- Avoided patterns: No repository/UoW; direct AST traversal

**Architecture**:
- New phase implemented as internal service class invoked from pipeline
- Libraries: not adding new assemblies
- CLI unaffected (compiler command remains same)

**Testing (NON-NEGOTIABLE)**:
- TDD for validator: add failing tests for each diagnostic first
- Integration focus: .5th programs as inputs; minimal unit helpers where needed
- Real dependencies: parser + existing type binder used

**Observability**:
- Diagnostics provide trace of decision points; optional verbose flag hook (future)

**Versioning**:
- Diagnostic codes stable (FR-047) – no version bump logic required here

Result: PASS – no violations requiring Complexity Tracking.

## Project Structure
Follows existing single-project layout (Option 1). New files under `src/compiler/Validation/` (folder) + tests in `test/runtime-integration-tests/TestPrograms/Functions/Guards/`.

## Phase 0: Outline & Research (research.md)
Key Unknowns resolved in spec: boolean exhaustiveness, interval handling, duplicate detection precedence, explosion metrics, dispatch halt semantics. No external tech research required beyond internal design trade‑offs.

Created `research.md` summarizing decisions.

## Phase 1: Design & Contracts
Although traditional API contracts not applicable (compiler internal), we define internal contracts:
- Data model (OverloadInfo, PredicateDescriptor, AtomicConstraint, Interval)
- Validator interface (IGuardedOverloadValidator or internal static orchestrator)
- Diagnostic contract (GuardDiagnosticEmitter: CreateIncomplete, CreateUnreachable, etc.)
Generated `data-model.md`, `quickstart.md`, and `contracts/validator-contract.md` documenting public surface (internal to assembly).

## Phase 2: Task Planning Approach
**Task Generation Strategy**:
- Derive tasks from FR groupings: collection, normalization, diagnostics, performance, test sets, doc updates
- Each diagnostic scenario receives: failing test → implementation commit → refactor
- Parallelizable tasks: individual diagnostic scenario tests after core normalization utilities built

**Ordering Strategy**:
1. Phase skeleton & pipeline hook
2. Data structures + normalization utilities
3. Base/duplicate/empty detection
4. Coverage + completeness (E1001)
5. Unreachable + ordering (W1002/E1004/E1005)
6. Explosion & count (W1101/W1102)
7. Destructuring test fix
8. Performance polish
9. Documentation & spec status update

**Estimated Output**: ~25 tasks (grouped by diagnostic & infrastructure). Will enumerate in `tasks.md` (next command outside /plan scope).

## Complexity Tracking
| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| (none) | | |

## Progress Tracking
**Phase Status**:
- [x] Phase 0: Research complete (/plan)
- [x] Phase 1: Design complete (/plan)
- [x] Phase 2: Task planning complete (approach defined)
- [ ] Phase 3: Tasks generated (/tasks command)
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented (none required)

---
*Based on Constitution v2.1.1 - See `/specs/.specify/memory/constitution.md`*
