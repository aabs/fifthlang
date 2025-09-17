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

## Phase 2 Addendum (Enhanced Directives)
The following implementation directives were added after initial plan finalization to reduce architectural drift and technical debt risk. These refine memory policy, layering, diagnostic ordering, traceability, and deferred unrelated bug handling.

### Namespace & Folder Boundaries
- All guard validation code resides under `src/compiler/Validation/GuardValidation/`.
- Subfolders (suggested): `Infrastructure/`, `Collection/`, `Normalization/`, `Analysis/`, `Diagnostics/`, `Instrumentation/`.
- Only the phase entry point class (`GuardOverloadValidationPhase`) (and optional facade interface `IGuardOverloadValidator`) may be `public`.
- A reflection-based boundary test will fail the build if additional public types appear in this namespace.
- No other compiler subsystems may reference internal classes from GuardValidation (one‑way dependency only: GuardValidation → existing compiler services).

### Layering Contract
```
Infrastructure → Normalization → Analysis → Diagnostics → Phase Entry
```
Prohibited: Downstream referencing upstream (e.g. Diagnostics calling Normalization internals) except through explicitly defined data contracts.

### Memory & Allocation Policy
- Prefer `Span<T>` / pooled arrays for interval accumulation and coverage unions.
- All temporary `List<T>` instances for hot-path analysis must come from a local object pool (simple stack or ring buffer) – no static globals.
- No LINQ in hot loops: normalization, interval intersection, coverage evaluation.
- Reuse canonical buffers across guard processing; clear instead of reallocate.
- Add performance synthetic scenarios to protect against regression (<5% overhead target).

### Atomic Constraint Canonicalization
- Provide a single deterministic formatter (e.g. `AtomicFormatter.Format(AtomicConstraint)`) used for:
	- Duplicate detection key generation
	- Traceability logging
	- Test assertions
- Stable ordering: `FieldPresence` < `Equality` < `Interval`; within category order by field / symbol name then literal/interval values.

### UNKNOWN Classification
- Introduce enum `UnknownReason { UnresolvableMember, NonConstantBinary, UnsupportedOperator, Other }` stored on `NormalizedGuard`.
- Future extensions require adding enum value + test update; reflection test enumerates handled reasons.

### Diagnostic Resolution / Precedence Table (Planned)
Final diagnostic emission applies precedence and gating:
1. E1005 (Multiple base overloads) – fail-fast: suppress completeness gap (E1001) because base semantics undefined; still compute W1101/W1102.
2. E1004 (Base overload not last) – reported; completeness (E1001) still evaluated if single base exists.
3. E1001 (Non-exhaustive guarded overloads) – may co‑emit with W1102, W1101.
4. W1002 (Unreachable guards) – emitted after completeness so gap highlighting unaffected.
5. W1102 (Excess UNKNOWN proportion) – uses unknownPercent; may accompany E1001.
6. W1101 (Overload count warning) – lowest precedence; never suppresses others.

Table will be mirrored in code as:
```csharp
internal static readonly DiagnosticCode[] GuardValidationPrecedence = new[] {
		DiagnosticCode.GUARD_MULTIPLE_BASE,    // E1005
		DiagnosticCode.GUARD_BASE_NOT_LAST,    // E1004
		DiagnosticCode.GUARD_INCOMPLETE,       // E1001
		DiagnosticCode.GUARD_UNREACHABLE,      // W1002
		DiagnosticCode.GUARD_UNKNOWN_EXPLOSION,// W1102
		DiagnosticCode.GUARD_OVERLOAD_COUNT    // W1101
};
```
An automated test will parse the spec’s ordered list and assert parity.

### Deterministic Secondary Notes Ordering
- Secondary notes sorted by (primarySpan.Start, guardIndex) to ensure stable output.

### Dual Highlight Structure
- Introduce `DiagnosticSpanSet` struct encapsulating primary and secondary span plus optional context span to avoid ad hoc tuple usage.

### Fail-Fast Gating Logic
- If E1005 is raised, skip gap detection (E1001) but still record UNKNOWN and guard metrics for W1102/W1101.
- Unit test will assert E1001 absence when E1005 present regardless of coverage gaps.

### Traceability Matrix
- JSON file `specs/002-guard-clause-overload-completeness/traceability.json` mapping each FR / AC to one or more test names plus classification (unit | integration | perf).
- Build/test step validates no FR/AC is left unmapped and all mapped tests exist.

### Instrumentation & Perf Scenarios
- Scenario definition file (JSON/YAML) enumerates synthetic groups: e.g. (N=10, avgAtoms=3, unknown=0%), (N=50, avgAtoms=6, unknown=30%), (N=100, avgAtoms=10, unknown=50%).
- Micro-bench harness compiles scenarios, comparing elapsed vs baseline compile (no validator) with threshold assertions.

### Documentation Additions
- Update spec with precedence table (mirrors code list) and memory policy summary.
- README section “Guard Validation Phase” describing responsibilities, layering, diagnostic taxonomy, performance constraints.

### Destructuring Test Mini-Spec (Deferred)
- Separate mini-spec drafted only after guard validation feature reaches green state.
- Captures: involved AST nodes, expected binding semantics, hypothesized fault (OrderOfEvaluation | FieldOffsetMapping | CodegenDivide), minimal reproduction sample.
- Avoids drive-by fix inside unrelated commits.

### Commit Strategy Reinforcement
- Each subsystem (Infrastructure, Normalization, Analysis, Diagnostics, Tests, Perf) enters main branch with all prior tests green.
- Destructuring fix occurs in its own commit referencing mini-spec ID.

### Risk Mitigations Recap
| Risk | Mitigation |
|------|------------|
| Namespace leakage | Reflection boundary test + single public phase entry |
| Diagnostic ordering drift | Spec/code parity test for precedence table |
| Allocation bloat | Object pooling + Span policy + perf scenarios |
| Non-deterministic output | Canonical atomic formatter + stable secondary ordering |
| UNKNOWN category creep | Explicit enum + test enumeration |
| Drive-by unrelated fixes | Deferred destructuring mini-spec + isolation commit |

These directives are now authoritative for subsequent implementation tasks and will be referenced in `README` and internal docs updates.

## Milestones (Execution Bundles)
Milestone A (Foundations): Tasks 1–9 (Phase skeleton, data structures, normalization, UNKNOWN enum, canonical atomic formatter, initial unit tests).
Milestone B (Core Analysis): Tasks 10–16 (Duplicate/Tautology, Interval Engine, Coverage, Unreachable, Base/Ordering diagnostics).
Milestone C (Diagnostics & Metrics): Tasks 17–22 + 32 (Diagnostic integration, Explosion & Count, Reporter, Resolution order table, Instrumentation, Fail-fast gating).
Milestone D (Quality & Traceability): Tasks 23–26 (Perf micro-bench, Integration tests, Edge intervals, Traceability matrix).
Milestone E (Finalization & Docs): Tasks 27–30 (Spec status updates, Documentation, Performance pass, Commit strategy validation).
Deferred (Separate Track): Task 31 (Destructuring mini-spec & fix) – only after Milestones A–D green and performance threshold met.

## Definition of Done (Per Task)
```
1. FR/AC references added/updated in traceability.json
2. New code internal (except allowed public phase entry/facade)
3. Precedence & traceability tests green
4. Determinism test hash unchanged (or updated intentionally with spec)
5. Benchmarks: median overhead ≤5% (or documented justified exception)
6. No LINQ in hot paths (Normalization, IntervalEngine, CoverageEvaluator) – enforced by code review & spot test
7. Pooling added only if threshold condition met (see below) or justified in commit message
8. All diagnostics emitted pass formatting tests (message + ordering)
9. If UNKNOWN reasons touched, enum + tests + spec appendix updated
10. Commit leaves repository buildable and all prior tests green
```

## Performance Threshold & Pooling Criteria
- Introduce pooling only if guard validation allocations >1% of total allocations (baseline synthetic scenario median) OR reduces guard validation allocations by ≥10% with negligible complexity cost.
- Benchmark methodology: 5 warmups + 10 measured runs; median used. Variance >10% triggers rerun or soft warning.
- Fail build if median overhead >5% unless an explicit waiver file `perf-waiver.md` (time-bound) justifies temporary exceedance.

## Instrumentation Output Contract
- Enabled via env var `FIFTH_GUARD_VALIDATION_PROFILE=1`.
- Output (stderr or dedicated logger): one JSON line per overload group:
	`{ "function": "name/arity", "overloads": n, "unknown": u, "unknownPercent": p, "elapsedMicros": t }`
- No output when flag absent. Never affects diagnostics.

## Review Checklist (PR Template)
- [ ] Milestone alignment correct (no scope bleed)
- [ ] Only allowed public types exported
- [ ] Traceability entries for all new FR/AC coverage
- [ ] Precedence constant matches spec table (test passes)
- [ ] Determinism test hash stable
- [ ] Perf benchmark within threshold (or waiver present)
- [ ] No pooling added without threshold justification
- [ ] UNKNOWN reason additions documented
- [ ] Diagnostic messages & ordering tests pass
- [ ] No drive-by changes (unrelated subsystems)

## Determinism Strategy
- Integration diagnostic suite executed twice sequentially; produce sorted signature: `code@primaryStart-secondaryCount` per diagnostic, hash with SHA256. Hashes must match.

## Gap Detection Debug Logging (Optional)
- When instrumentation enabled and E1001 emitted, log uncovered intervals list (canonical ordering) for developer debugging.

## Hazard / Future Enhancements Log
Maintained in README subsection; initial entries:
- OR guard simplification & CNF expansion (out of scope)
- Generic type parameter constraint coverage
- Pattern matching style destructuring expansions
- Data-flow derived constant range narrowing

## Precedence Change Policy
- Adding a new guard diagnostic requires: spec precedence table update, code constant update, new precedence parity test, rationale section in commit message referencing architectural intent.

## UNKNOWN Reason Policy
- New classification requires: enum addition, spec appendix line, example test case, traceability update; failure to do so blocks merge.

## Commit Message Conventions
`feat(validation):` new functionality
`fix(validation):` bug fix (not destructuring test unless mini-spec committed)
`perf(validation):` performance improvement with benchmark evidence
`test(validation):` tests only
`docs(validation):` documentation/spec updates
`refactor(validation):` internal change without behavior impact

## Waiver Process
- Temporary performance waiver stored in `specs/002-guard-clause-overload-completeness/perf-waiver.md` with expiration date and justification; CI rejects expired waivers.

## Success Metrics Summary
| Metric | Target | Measurement |
|--------|--------|-------------|
| Compile overhead | ≤5% median vs baseline | Micro-bench harness |
| Determinism | 100% identical hashes | Determinism test |
| Allocation share | ≤1% total or justified | Alloc profiler (optional) |
| Traceability coverage | 100% FR/AC mapped | Traceability test |
| Public API leakage | 0 unintended types | Reflection boundary test |


## Complexity Tracking
| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|--------------------------------------|
| (none) | | |

## Progress Tracking
**Phase Status**:
- [x] Phase 0: Research complete (/plan)
- [x] Phase 1: Design complete (/plan)
- [x] Phase 2: Task planning complete (approach defined)
 - [x] Phase 3: Tasks generated (/tasks command) – artifacts present: `research.md`, `data-model.md`, `quickstart.md`, `contracts/validator-contract.md`, `tasks.md`
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: PASS
- [x] Post-Design Constitution Check: PASS
- [x] All NEEDS CLARIFICATION resolved
- [ ] Complexity deviations documented (none required)

---
*Based on Constitution v2.1.1 - See `/specs/.specify/memory/constitution.md`*
