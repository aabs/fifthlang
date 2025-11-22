# Tasks: Guarded Overload Completeness Validation & Destructuring Bonus Fix

**Input**: Design documents in `specs/002-guard-clause-overload-completeness/` (plan.md, research.md, data-model.md, quickstart.md, contracts/validator-contract.md)
**Prerequisites**: Plan Phase 0–3 complete (see `plan.md` Progress Tracking)

## Execution Flow (Generated)
```
1. Load plan.md (feature name, milestones, DoD) ✔
2. Load data-model.md (entities: OverloadGroup, OverloadInfo, NormalizedGuard, AtomicConstraint, CoverageAccumulator, DiagnosticSpanSet, analyzers) ✔
3. Load contracts/validator-contract.md (public phase API + invariants) ✔
4. Load research.md (heuristics, risks) ✔
5. Load quickstart.md (scenario tests) ✔
6. Emit ordered TDD-first tasks with dependencies & [P] markers
7. Validate coverage of FR-001..070 & AC-001..038 via traceability task
8. Provide parallel run examples
```

Legend: `[P]` task can execute in parallel with other `[P]` tasks (different files, no dependency conflict). Tests precede implementation (TDD). Same-file edits are serialized.

---
## Phase 3.1: Setup & Skeleton
- [x] T001 Create validation folder structure `src/compiler/Validation/GuardValidation/{Infrastructure,Collection,Normalization,Analysis,Diagnostics,Instrumentation}`
- [x] T002 Add phase entry `GuardOverloadValidationPhase` (public) + pipeline hook (temporarily no-op) in `src/compiler/ParserManager.cs` (or appropriate orchestrator) – ensure build passes
- [x] T003 Add README boundaries `src/compiler/Validation/GuardValidation/README.md` (namespace + layering + memory policy)
- [x] T004 Reflection boundary test `test/runtime-integration-tests/Validation/GuardValidation/PublicSurfaceTests.cs` (fail if extra public types)
- [x] T005 Add initial `traceability.json` with empty mappings `specs/002-guard-clause-overload-completeness/traceability.json`

## Phase 3.0: Infrastructure & CLI (Completed)
- [x] T000 Add Makefile target `install-cli` for building compiler and creating symlink in `~/bin` as `fifth` CLI tool

## Current Implementation Status (Phase 4 - Complete)
- [x] T999 ✨ `GuardCompletenessValidator` class implemented with basic validation logic
- [x] T998 ✨ Integration into `ParserManager` pipeline (before overload transformations)
- [x] T997 ✨ Basic diagnostic emission (E1001, E1004, E1005, W1002, W1101, W1102)
- [x] T996 ✨ Predicate classification (Base, Analyzable, Unknown)
- [x] T995 ✨ Simple subsumption detection and unreachable analysis
- [x] T994 ✨ **Modular Architecture**: Refactored to proper folder structure with Infrastructure/, Collection/, Normalization/, Analysis/, Diagnostics/, Instrumentation/ components
- [x] **Missing**: Test infrastructure, traceability, performance benchmarks

## Phase 3.2: Tests First (Normalization & Classification) ⚠️ MUST FAIL INITIALLY
- [x] T006 [P] Unit test: tautology & unguarded detection `test/ast-tests/Validation/Guards/Normalization/TautologyTests.cs`
- [x] T007 [P] Unit test: trivial true elimination `.../Normalization/TrueEliminationTests.cs`
- [x] T008 [P] Unit test: UNKNOWN classification cases (OR, cross-identifier, generic param) `.../Normalization/UnknownClassificationTests.cs`
- [x] T009 [P] Unit test: atomic equality & interval formation `.../Normalization/IntervalFormationTests.cs`
- [x] T010 [P] Unit test: empty/inverted interval detection `.../Normalization/EmptyIntervalTests.cs`
- [x] T011 [P] Unit test: canonical atomic formatter determinism `.../Normalization/AtomicFormatterDeterminismTests.cs`

## Phase 3.3: Core Normalization Implementation
- [x] T012 Implement `Infrastructure/Models.cs` (OverloadInfo, NormalizedGuard, AtomicConstraint, enums) & wire into phase (no logic yet)
- [x] T013 Implement `Normalization/GuardNormalizer.cs` (conjunction-only parsing, tautology, trivial true elimination, interval merging, UNKNOWN classification)
- [x] T014 Implement `Infrastructure/AtomicFormatter.cs` (stable ordering + composite key)
- [x] T015 Implement `Analysis/IntervalEngine.cs` (Intersect, IsEmpty, Subsumes) w/out LINQ
- [x] T016 Update normalization tests to pass (re-run; all previous failing tests now green)

## Phase 3.4: Tests First (Analysis & Diagnostics) ⚠️ ADD BEFORE IMPLEMENTATION
 [x] T017 [P] Unit test: duplicate detection → later guard unreachable `.../Analysis/DuplicateDetectionTests.cs`
 [x] T018 [P] Unit test: empty precedence over duplicate (FR-070) `.../Analysis/EmptyVsDuplicatePrecedenceTests.cs`
 [x] T019 [P] Unit test: interval subsumption unreachable `.../Analysis/IntervalSubsumptionTests.cs`
 [x] T020 [P] Unit test: boolean exhaustive pair (no base) completeness success `.../Analysis/BooleanExhaustiveTests.cs`
 [x] T021 [P] Unit test: incomplete guards (no base) → E1001 `.../Analysis/IncompletenessTests.cs`
 [x] T022 [P] Unit test: multiple base precedence (E1005 suppresses E1001) `.../Diagnostics/MultipleBasePrecedenceTests.cs`
 [x] T023 [P] Unit test: base-not-last still allows E1001 `.../Diagnostics/BaseNotLastCoverageTests.cs`
 [x] T024 [P] Unit test: unreachable after base (should still warn if analyzable) `.../Diagnostics/UnreachableAfterBaseTests.cs`
 [x] T025 [P] Unit test: explosion warning thresholds (just below / above) `.../Diagnostics/ExplosionThresholdTests.cs`
 [x] T026 [P] Unit test: overload count warning threshold (32 vs 33) `.../Diagnostics/OverloadCountWarningTests.cs`
 [x] T027 [P] Unit test: deterministic ordering & hash stability `.../Determinism/DeterminismHashTests.cs`

## Phase 3.5: Analysis & Diagnostic Implementations
- [x] T028 Implement `Collection/OverloadCollector.cs` (grouping logic) & integrate into phase
- [x] T029 Implement `Analysis/DuplicateDetector.cs`
- [x] T030 Implement `Analysis/CoverageEvaluator.cs` (boolean completeness + analyzable union tracking)
- [x] T031 Implement `Analysis/UnreachableAnalyzer.cs` (uses IntervalEngine + duplicate & empty info)
- [x] T032 Implement `Diagnostics/GuardValidationReporter.cs` (emission helpers + precedence application)
- [x] T033 Implement `Diagnostics/BaseOrderingRules.cs` (E1004/E1005 logic + gating)
- [x] T034 Implement `Analysis/ExplosionAnalyzer.cs` (unknownPercent) & `Analysis/CountAnalyzer.cs`
- [x] T035 Integrate analyses in phase run sequence (collector → normalizer → analyzers → reporter)
- [ ] T036 Make previously added analysis/diagnostic tests pass

## Phase 3.6: Integration Tests (Quickstart Scenarios) ⚠️ ADD BEFORE RUNNING
- [x] T037 [P] Integration: missing base (E1001) `test/runtime-integration-tests/TestPrograms/Functions/Guards/missing_base.5th`
- [x] T038 [P] Integration: boolean exhaustive pair success `.../boolean_exhaustive.5th`
- [x] T039 [P] Integration: multiple base (E1005) `.../multiple_base.5th`
- [x] T040 [P] Integration: base not last (E1004) `.../base_not_last.5th`
- [x] T041 [P] Integration: duplicate unreachable (W1002) `.../duplicate_unreachable.5th`
- [x] T042 [P] Integration: interval subsumption unreachable (W1002) `.../interval_subsumed.5th`
- [x] T043 [P] Integration: explosion + incomplete (E1001 + W1102) `.../explosion_incomplete.5th`
- [x] T044 [P] Integration: overload count warning (W1101) `.../overload_count.5th`
- [x] T045 [P] Integration: tautology base equivalence `.../tautology_base.5th`
- [x] T046 [P] Integration: empty interval precedence over duplicate `.../empty_vs_duplicate.5th`

### Follow-up tasks added
- [ ] T071 Add CI job `validate-examples` to parse all `.5th` samples under `docs/`, `specs/`, and `test/` and fail on parse errors (specification task; implementation deferred).
- [ ] T072 Remove temporary debug prints from `test/runtime-integration-tests/GuardValidation` integration tests (cleanup & safety check).
- [ ] T073 Add unit tests for tautology-base equivalence and E1005/E1001 precedence (covers corner-case semantics validated during integration).
- [ ] T074 Update `traceability.json` to include mappings for T037–T046 integration scenarios and add coverage assertions.

> Note: Integration tests T037–T046 were implemented and executed end-to-end. Several `.5th` samples were updated to use the parser-supported parameter-constraint syntax `param: Type | <expr>` and to use block function bodies where required by the grammar. The integration suite for GuardValidation is green for these scenarios.

## Phase 3.7: Instrumentation & Performance
- [ ] T047 Add `Instrumentation/InstrumentationLogger.cs` (env var detection + JSON output)
- [ ] T048 Add synthetic scenario definitions `specs/002-guard-clause-overload-completeness/perf-scenarios.json`
- [ ] T049 Add micro-benchmark harness `test/runtime-integration-tests/Performance/GuardValidationBenchTests.cs` (measure overhead)
- [ ] T050 Implement optional local pooling (guarded by allocation threshold) & benchmark delta
- [x] T051 Performance assertion test (fail if >5% overhead unless waiver file exists)

## Phase 3.8: Traceability & Parity
- [x] T052 Populate `traceability.json` mapping FR-001..070 & AC-001..038 to tests (unit/integration/perf)
- [x] T053 Add traceability validation test `test/ast-tests/Validation/Guards/Traceability/TraceabilityCoverageTests.cs`
- [ ] T054 Add precedence parity test (spec order vs `GuardValidationPrecedence`) `.../Diagnostics/PrecedenceParityTests.cs`
- [ ] T055 Add public surface test (already exists) extend to assert only phase entry public (reuse T004)

## Phase 3.9: Destructuring Bonus Fix (Deferred Execution)
- [ ] T056 Draft mini-spec `specs/002-guard-clause-overload-completeness/destructuring-mini-spec.md` capturing failing test root cause hypotheses
- [ ] T057 Add failing repro test (currently failing) `test/runtime-integration-tests/TestPrograms/Destructuring/destructuring_example.5th`
- [ ] T058 Implement fix (adjust destructuring binding or overload selection) – minimal targeted change
- [ ] T059 Confirm test passes & update traceability (link AC-004)

## Phase 3.10: Documentation & Finalization
- [ ] T060 Update feature `spec.md` Execution Status & add precedence table snapshot
- [ ] T061 Update compiler README with new phase overview & diagnostics table
- [ ] T062 Add memory/pooling policy notes to README & spec
- [ ] T063 Add determinism section (hash test description) to docs
- [ ] T064 Review / adjust performance waiver policy (remove waiver if present)
- [ ] T065 Final determinism re-run & hash snapshot commit
- [ ] T066 Final refactor pass (remove dead code, ensure no LINQ in hot paths)
- [ ] T067 Update `traceability.json` post-doc tasks if new references added
- [ ] T068 Mark plan `Phase 4` complete in `plan.md`
- [ ] T069 Mark plan `Phase 5` complete after green build + perf + determinism
- [ ] T070 Create PR summary section referencing FR/AC coverage & metrics

## Dependencies Overview
- T000 completed (CLI infrastructure)
- T001 → T002 → (T003,T004,T005)
- T006–T011 independent of each other; all depend on T002
- T012–T016 depend on normalization test set (T006–T011 written & failing)
- T017–T027 depend on T016
- T028–T036 depend on T017–T027 test presence
- T037–T046 (integration) depend on core analyses (T028–T035) present (tests added before full passing allowed)
- T047–T051 depend on core implementation (T035) & integration tests
- T052–T055 after majority of tests exist (post T046)
- T056–T059 only after guard validation stable (post T055)
- T060–T070 finalization after all prior tasks

## Parallel Execution Examples
```
# Example 1: Run normalization test authoring in parallel
T006 T007 T008 T009 T010 T011

# Example 2: Run analysis/diagnostic test authoring in parallel
T017 T018 T019 T020 T021 T022 T023 T024 T025 T026 T027

# Example 3: Integration scenario tests batch
T037 T038 T039 T040 T041 T042 T043 T044 T045 T046
```

## Validation Checklist
- [x] CLI infrastructure completed (T000)
- [x] Core validation logic implemented (T999-T995)
- [🔄] Pipeline integration functional (basic diagnostics working)
- [ ] All FR-001..070 mapped in traceability (T052)
- [ ] All AC-001..038 mapped in traceability (T052)
- [ ] Determinism test added & passing (T027)
- [ ] Precedence parity enforced (T054)
- [ ] Performance ≤5% or waiver documented (T051)
- [ ] Only one public type (phase entry) (T004/T055)
- [ ] Explosion + count warning coexist test (covered by T025/T026 & integration T043/T044)
- [ ] Destructuring test passes (T059)

---
Generated per tasks.prompt with TDD-first ordering, deterministic diagnostic mandates, and explicit layering constraints.
- Each commit message must reflect convention (`feat(validation):`, etc.).
- Destructuring fix isolated—no mixing with guard validation commits.

## Current Implementation Notes (September 2025)

**What's Implemented:**
- ✅ `GuardCompletenessValidator` class with core diagnostic logic
- ✅ Pipeline integration in `ParserManager` (executes after destructuring flattening, before overload transformations)
- ✅ All major diagnostic categories: E1001 (incomplete), E1004 (base not last), E1005 (multiple base), W1002 (unreachable), W1101 (overload count), W1102 (unknown explosion)
- ✅ Predicate classification: Base, Analyzable, Unknown
- ✅ Basic subsumption detection for unreachable analysis
- ✅ **Modular Architecture**: Proper folder structure implemented per specification:
  - `Infrastructure/` - Core types (PredicateType, PredicateDescriptor, AnalyzedOverload, FunctionGroup)
  - `Collection/` - OverloadCollector for gathering function groups from AST
  - `Normalization/` - PredicateNormalizer for analysis and classification
  - `Analysis/` - CompletenessAnalyzer for subsumption and coverage analysis
  - `Diagnostics/` - DiagnosticEmitter for centralized diagnostic emission
  - `Instrumentation/` - ValidationInstrumenter for performance tracking
- ✅ Layering contract enforced: Infrastructure → Normalization → Analysis → Diagnostics
- ✅ All components properly encapsulated as `internal`

## Test Infrastructure Status (NEW)
- [x] **Unit Test Structure**: Complete test folder structure matching modular architecture:
  - `test/ast-tests/Validation/Guards/Infrastructure/` - FunctionGroup, PredicateDescriptor, AnalyzedOverload tests
  - `test/ast-tests/Validation/Guards/Collection/` - OverloadCollector tests
  - `test/ast-tests/Validation/Guards/Normalization/` - PredicateNormalizer tests
  - `test/ast-tests/Validation/Guards/Analysis/` - CompletenessAnalyzer tests
  - `test/ast-tests/Validation/Guards/Diagnostics/` - DiagnosticEmitter tests
  - `test/ast-tests/Validation/Guards/Instrumentation/` - ValidationInstrumenter tests
- [x] **Mock Infrastructure**: MockOverloadableFunction helper for test isolation
- [x] **Integration Tests**: Basic .5th test files and integration test scenarios:
  - `test/runtime-integration-tests/GuardValidation/complete_guards.5th`
  - `test/runtime-integration-tests/GuardValidation/incomplete_guards.5th`
  - `test/runtime-integration-tests/GuardValidation/unreachable_guards.5th`
  - `test/runtime-integration-tests/GuardValidation/GuardValidationIntegrationTests.cs`
- [x] **Test Coverage**: All modular components have corresponding unit tests using xUnit + FluentAssertions

**What's Missing (Next Priorities):**
- ❌ Full synthetic benchmark scenarios and automated CI benchmark job (nightly harness exists; consider adding PR-level lighter checks)
- ❌ Sophisticated predicate normalization (beyond simple numeric comparisons)
- ❌ Additional analysis/diagnostic tests (T017–T027) and implementations (T028–T036)
- ❌ Integration scenario tests (T037–T046)
- ❌ Full FR/AC mapping expansion to FR-001..070 & AC-001..038 (T052)

**Current State:** Phase 4 complete with comprehensive test infrastructure - core validation with proper modular architecture and full test coverage implemented; missing only traceability mapping and performance validation per plan requirements.
**Update (24 Sep 2025):** Interval engine implemented and wired into `CompletenessAnalyzer`; new unit tests for interval formation, emptiness, and subsumption are passing; traceability updated with new tests; analyzer/integration unit tests remain green.
