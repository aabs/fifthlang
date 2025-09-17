# Tasks: Guarded Overload Completeness Validation & Destructuring Bonus Fix

**Feature Directory**: `specs/002-guard-clause-overload-completeness/`
**Input Docs**: plan.md, spec.md (FR-001..070, AC-001..038), addendum directives

## Execution Flow (Generated)
```
1. Load plan.md ✔
2. Load spec.md ✔ (enumerated FR/AC)
3. Derive tasks from milestones & Definition of Done
4. Order: Setup → Tests (TDD) → Core Infrastructure → Analysis Engines → Diagnostics → Instrumentation/Perf → Integration Tests → Traceability/Docs → Deferred Fix
5. Mark parallel-capable tasks [P] (different files, no dependency conflicts)
6. Include determinism, precedence, traceability, and perf gating tasks
```

## Phase 3.1: Setup & Skeleton
- [ ] T001 Create directory scaffold for GuardValidation (folders: `src/compiler/Validation/GuardValidation/{Infrastructure,Collection,Normalization,Analysis,Diagnostics,Instrumentation}`)
- [ ] T002 Add phase entry class `GuardOverloadValidationPhase.cs` (initial stub, no logic) in `src/compiler/Validation/GuardValidation/`
- [ ] T003 Wire phase into compiler pipeline (after symbol/type binding) modifying `src/compiler/ParserManager.cs` or appropriate orchestration point
- [ ] T004 Add namespace boundary README `src/compiler/Validation/GuardValidation/README.md` documenting layering & public surface (GuardOverloadValidationPhase only)
- [ ] T005 Add reflection boundary test `test/runtime-integration-tests/Validation/GuardValidationBoundaryTests.cs` asserting only allowed public types

## Phase 3.2: Test Scaffolding (Failing First) ⚠️ MUST FAIL BEFORE IMPLEMENTATION
- [ ] T006 [P] Unit test skeletons for normalization (`test/ast-tests/GuardValidation/NormalizationTests.cs`) covering FR-038/039/069/070 basics (empty now -> fail NotImplemented)
- [ ] T007 [P] Unit test skeletons for interval/empty detection (`test/ast-tests/GuardValidation/IntervalEngineTests.cs`) FR-040/041/042/059/060
- [ ] T008 [P] Unit test skeletons for duplicate/tautology handling (`test/ast-tests/GuardValidation/DuplicateDetectionTests.cs`) FR-056/070
- [ ] T009 [P] Unit test skeletons for unreachable detection (`test/ast-tests/GuardValidation/UnreachableAnalyzerTests.cs`) FR-005/030/042/056/060
- [ ] T010 [P] Unit test skeletons for UNKNOWN classification (`test/ast-tests/GuardValidation/UnknownClassificationTests.cs`) FR-038/039/043/057/058
- [ ] T011 [P] Unit test skeletons for diagnostic precedence & gating (`test/ast-tests/GuardValidation/PrecedenceTests.cs`) FR-053/066/067 + fail-fast E1005 gating
- [ ] T012 Integration test program: incomplete guards (E1001) `test/runtime-integration-tests/TestPrograms/Functions/Guards/incomplete_set.5th`
- [ ] T013 Integration test program: multiple base (E1005) `.../multiple_base.5th`
- [ ] T014 Integration test program: base not last (E1004) `.../base_not_last.5th`
- [ ] T015 Integration test program: unreachable guard (W1002) `.../unreachable_guard.5th`
- [ ] T016 Integration test program: overload count (W1101) `.../overload_count.5th`
- [ ] T017 Integration test program: unknown explosion (W1102) `.../unknown_explosion.5th`
- [ ] T018 Integration test program: combined E1001 + W1102 `.../incomplete_and_explosion.5th`
- [ ] T019 Integration test program: clean exhaustive via base `.../complete_with_base.5th`
- [ ] T020 Determinism harness integration test (run twice, hash) `test/runtime-integration-tests/GuardValidationDeterminismTests.cs`

## Phase 3.3: Core Infrastructure & Normalization
- [ ] T021 [P] Implement data structures `Infrastructure/GuardData.cs` (OverloadInfo, GuardOverloadGroup, NormalizedGuard, AtomicConstraint, Interval)
- [ ] T022 [P] Implement `Normalization/AtomicFormatter.cs` (canonical ordering & string) FR-041/Atomic ordering rules
- [ ] T023 [P] Implement `Normalization/GuardNormalizer.cs` (conjunction extraction, trivial true elimination FR-069, UNKNOWN reason enum) FR-038/039/069/070
- [ ] T024 Implement UNKNOWN reason enum + tests (hook into T010) `Normalization/UnknownReason.cs` FR-038/039
- [ ] T025 Implement `Analysis/IntervalEngine.cs` (interval struct ops, intersection, empty detection) FR-040/041/042/059/060
- [ ] T026 Implement object pool & span buffer helper `Infrastructure/Pooling.cs` (List<T> pool, span helpers) per memory policy
- [ ] T027 Update normalization tests to assert duplicate canonical forms (T006 enhancement)

## Phase 3.4: Analysis Engines & Coverage
- [ ] T028 Implement `Analysis/DuplicateDetector.cs` (duplicate & tautology detection precedence empty-first) FR-056/070
- [ ] T029 Implement `Analysis/UnreachableAnalyzer.cs` (subsumption, empty, duplicate unreachable) FR-005/030/042/056/060
- [ ] T030 Implement `Analysis/CoverageEvaluator.cs` (coverage union ignoring UNKNOWN) FR-002/065
- [ ] T031 Implement fail-fast gating logic inside orchestrator (skip coverage if E1005) FR-053/066
- [ ] T032 Update unit tests T009/T011 to cover fail-fast gating scenarios

## Phase 3.5: Diagnostics Layer
- [ ] T033 Implement `Diagnostics/GuardValidationReporter.cs` with `DiagnosticSpanSet` and secondary note formatting FR-036/037/029
- [ ] T034 Implement `Diagnostics/BaseOrderingRules.cs` E1004/E1005 detection sequencing FR-034/035/052/053/066
- [ ] T035 Implement `Diagnostics/ExplosionAnalyzer.cs` (W1101, W1102) FR-050/051/061/062/063/064/067/068
- [ ] T036 Integrate precedence constant + parity test (spec parse) `Diagnostics/PrecedenceParityTests.cs` FR-053/Precedence table
- [ ] T037 Hook diagnostics into phase orchestrator (apply ordering, dual highlighting) FR-037
- [ ] T038 Update integration test harness to assert primary+secondary spans presence (augment tests T012–T019)

## Phase 3.6: Orchestration & Pipeline Finalization
- [ ] T039 Implement phase orchestration logic (collect groups, run normalizer, analyses, diagnostics) `GuardOverloadValidationPhase.cs`
- [ ] T040 Wire instrumentation flag detection + JSON output (if env var set) FR Instrumentation Contract
- [ ] T041 Determinism test implementation referencing orchestration (T020)

## Phase 3.7: Performance & Instrumentation
- [ ] T042 Add synthetic scenario definitions `specs/002-guard-clause-overload-completeness/perf-scenarios.json`
- [ ] T043 Implement micro-bench harness `test/runtime-integration-tests/GuardValidationPerfTests.cs` (median-of-10, overhead <=5%)
- [ ] T044 Add allocation sampling (optional, skip if infra missing) with summary logging under instrumentation flag
- [ ] T045 Implement pooling activation threshold logic & document in README (tie to perf results) Memory Policy

## Phase 3.8: Integration & Edge Testing
- [ ] T046 Flesh out incomplete guard program (T012) with analyzable gaps for FR-002
- [ ] T047 Flesh out multiple base program (T013) with two unguarded + one after
- [ ] T048 Flesh out base not last program (T014) base placed mid sequence
- [ ] T049 Flesh out unreachable guard program (T015) include empty interval + duplicate case
- [ ] T050 Flesh out overload count program (T016) 33 overloads
- [ ] T051 Flesh out unknown explosion program (T017) >=8 with >50% UNKNOWN, no base
- [ ] T052 Flesh out combined program (T018) incomplete + >50% UNKNOWN
- [ ] T053 Flesh out exhaustive success program (T019) base final + prior guarded
- [ ] T054 Edge interval tests implementation (touching bounds, inversion) link to T007/T025
- [ ] T055 Update traceability.json mapping FR/AC to test names

## Phase 3.9: Traceability & Documentation
- [ ] T056 Create `traceability.json` initial mapping file (if absent)
- [ ] T057 Implement traceability parity test `test/ast-tests/GuardValidation/TraceabilityTests.cs`
- [ ] T058 Update spec.md with precedence table + execution status marks
- [ ] T059 Update README & compiler docs with new phase description & diagnostics taxonomy
- [ ] T060 Add hazard/future enhancements subsection to README

## Phase 3.10: Performance Pass & Cleanup
- [ ] T061 Optimize hot paths (remove residual LINQ, ensure pooling used) referencing perf deltas
- [ ] T062 Re-run perf harness; update/clear perf waiver if present
- [ ] T063 Determinism recheck after optimizations
- [ ] T064 Final traceability refresh (no FR/AC unmapped)

## Phase 3.11: Deferred Destructuring Mini-Spec & Fix
- [ ] T065 Draft mini-spec `specs/002-guard-clause-overload-completeness/destructuring-mini-spec.md` (AST capture, fault hypotheses)
- [ ] T066 Add failing focused test `test/runtime-integration-tests/TestPrograms/Functions/Guards/destructuring_bonus.5th` (if not already failing scenario isolated)
- [ ] T067 Implement fix (binding/order/codegen) with commit referencing mini-spec
- [ ] T068 Verify original `destructuring_example_ShouldReturn6000` passes & add traceability entry FR-014 linkage

## Phase 3.12: Finalization
- [ ] T069 Remove or reduce instrumentation noise (keep flag behavior only)
- [ ] T070 Final documentation sweep (ensure policies mirrored) & mark spec status to "Implemented"

## Dependencies Overview
- Setup (T001–T005) precedes all.
- Unit test skeletons (T006–T011) precede their implementations (T021–T038).
- Data structures (T021) precede normalization & analysis (T022–T031).
- Analysis (T028–T031) precede diagnostics integration (T033–T038).
- Orchestration (T039) depends on all prior infrastructure & diagnostics.
- Perf tasks (T042–T045) depend on orchestration & integration test scenarios.
- Integration fleshing (T046–T053) follows infrastructure to create meaningful failing/valid runs.
- Traceability & docs (T056–T060) after most tests exist.
- Destructuring mini-spec (T065+) only after core feature stable (post T064).

## Parallel Execution Examples
```
# Example 1: Run independent unit test skeleton creations in parallel (different files)
T006, T007, T008, T009, T010, T011

# Example 2: After T021 data structures, implement normalization & formatter in parallel
T022, T023 (share no files) while T024 enum is separate but depends on T023 partial

# Example 3: Analysis trio in parallel after intervals ready
T028, T029, T030

# Example 4: Diagnostic layer (ensure reporter first if desired sequencing)
T033, T034, T035 (independent) then T036, T037
```

## Validation Checklist
- All FR-001..070 mapped to at least one test in traceability.json by T064
- Precedence parity test passes (T036)
- Determinism hash stable (T041, T063)
- Perf median overhead ≤5% (T043, T062)
- No unintended public types (T005)
- UNKNOWN reasons enumerated & tested (T024)

## Notes
- [P] denotes parallel-suitable tasks (different files, no dependency conflict).
- TDD: Do not implement logic for a component before its failing tests are in place.
- Each commit message must reflect convention (`feat(validation):`, etc.).
- Destructuring fix isolated—no mixing with guard validation commits.

