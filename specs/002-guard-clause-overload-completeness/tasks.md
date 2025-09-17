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
- [ ] T001 Create validation folder structure `src/compiler/Validation/GuardValidation/{Infrastructure,Collection,Normalization,Analysis,Diagnostics,Instrumentation}`
- [ ] T002 Add phase entry `GuardOverloadValidationPhase` (public) + pipeline hook (temporarily no-op) in `src/compiler/ParserManager.cs` (or appropriate orchestrator) – ensure build passes
- [ ] T003 Add README boundaries `src/compiler/Validation/GuardValidation/README.md` (namespace + layering + memory policy)
- [ ] T004 Reflection boundary test `test/runtime-integration-tests/Validation/GuardValidation/PublicSurfaceTests.cs` (fail if extra public types)
- [ ] T005 Add initial `traceability.json` with empty mappings `specs/002-guard-clause-overload-completeness/traceability.json`

## Phase 3.2: Tests First (Normalization & Classification) ⚠️ MUST FAIL INITIALLY
- [ ] T006 [P] Unit test: tautology & unguarded detection `test/ast-tests/Validation/Guards/Normalization/TautologyTests.cs`
- [ ] T007 [P] Unit test: trivial true elimination `.../Normalization/TrueEliminationTests.cs`
- [ ] T008 [P] Unit test: UNKNOWN classification cases (OR, cross-identifier, generic param) `.../Normalization/UnknownClassificationTests.cs`
- [ ] T009 [P] Unit test: atomic equality & interval formation `.../Normalization/IntervalFormationTests.cs`
- [ ] T010 [P] Unit test: empty/inverted interval detection `.../Normalization/EmptyIntervalTests.cs`
- [ ] T011 [P] Unit test: canonical atomic formatter determinism `.../Normalization/AtomicFormatterDeterminismTests.cs`

## Phase 3.3: Core Normalization Implementation
- [ ] T012 Implement `Infrastructure/Models.cs` (OverloadInfo, NormalizedGuard, AtomicConstraint, enums) & wire into phase (no logic yet)
- [ ] T013 Implement `Normalization/GuardNormalizer.cs` (conjunction-only parsing, tautology, trivial true elimination, interval merging, UNKNOWN classification)
- [ ] T014 Implement `Infrastructure/AtomicFormatter.cs` (stable ordering + composite key)
- [ ] T015 Implement `Analysis/IntervalEngine.cs` (Intersect, IsEmpty, Subsumes) w/out LINQ
- [ ] T016 Update normalization tests to pass (re-run; all previous failing tests now green)

## Phase 3.4: Tests First (Analysis & Diagnostics) ⚠️ ADD BEFORE IMPLEMENTATION
- [ ] T017 [P] Unit test: duplicate detection → later guard unreachable `.../Analysis/DuplicateDetectionTests.cs`
- [ ] T018 [P] Unit test: empty precedence over duplicate (FR-070) `.../Analysis/EmptyVsDuplicatePrecedenceTests.cs`
- [ ] T019 [P] Unit test: interval subsumption unreachable `.../Analysis/IntervalSubsumptionTests.cs`
- [ ] T020 [P] Unit test: boolean exhaustive pair (no base) completeness success `.../Analysis/BooleanExhaustiveTests.cs`
- [ ] T021 [P] Unit test: incomplete guards (no base) → E1001 `.../Analysis/IncompletenessTests.cs`
- [ ] T022 [P] Unit test: multiple base precedence (E1005 suppresses E1001) `.../Diagnostics/MultipleBasePrecedenceTests.cs`
- [ ] T023 [P] Unit test: base-not-last still allows E1001 `.../Diagnostics/BaseNotLastCoverageTests.cs`
- [ ] T024 [P] Unit test: unreachable after base (should still warn if analyzable) `.../Diagnostics/UnreachableAfterBaseTests.cs`
- [ ] T025 [P] Unit test: explosion warning thresholds (just below / above) `.../Diagnostics/ExplosionThresholdTests.cs`
- [ ] T026 [P] Unit test: overload count warning threshold (32 vs 33) `.../Diagnostics/OverloadCountWarningTests.cs`
- [ ] T027 [P] Unit test: deterministic ordering & hash stability `.../Determinism/DeterminismHashTests.cs`

## Phase 3.5: Analysis & Diagnostic Implementations
- [ ] T028 Implement `Collection/OverloadCollector.cs` (grouping logic) & integrate into phase
- [ ] T029 Implement `Analysis/DuplicateDetector.cs`
- [ ] T030 Implement `Analysis/CoverageEvaluator.cs` (boolean completeness + analyzable union tracking)
- [ ] T031 Implement `Analysis/UnreachableAnalyzer.cs` (uses IntervalEngine + duplicate & empty info)
- [ ] T032 Implement `Diagnostics/GuardValidationReporter.cs` (emission helpers + precedence application)
- [ ] T033 Implement `Diagnostics/BaseOrderingRules.cs` (E1004/E1005 logic + gating)
- [ ] T034 Implement `Analysis/ExplosionAnalyzer.cs` (unknownPercent) & `Analysis/CountAnalyzer.cs`
- [ ] T035 Integrate analyses in phase run sequence (collector → normalizer → analyzers → reporter)
- [ ] T036 Make previously added analysis/diagnostic tests pass

## Phase 3.6: Integration Tests (Quickstart Scenarios) ⚠️ ADD BEFORE RUNNING
- [ ] T037 [P] Integration: missing base (E1001) `test/runtime-integration-tests/TestPrograms/Functions/Guards/missing_base.5th`
- [ ] T038 [P] Integration: boolean exhaustive pair success `.../boolean_exhaustive.5th`
- [ ] T039 [P] Integration: multiple base (E1005) `.../multiple_base.5th`
- [ ] T040 [P] Integration: base not last (E1004) `.../base_not_last.5th`
- [ ] T041 [P] Integration: duplicate unreachable (W1002) `.../duplicate_unreachable.5th`
- [ ] T042 [P] Integration: interval subsumption unreachable (W1002) `.../interval_subsumed.5th`
- [ ] T043 [P] Integration: explosion + incomplete (E1001 + W1102) `.../explosion_incomplete.5th`
- [ ] T044 [P] Integration: overload count warning (W1101) `.../overload_count.5th`
- [ ] T045 [P] Integration: tautology base equivalence `.../tautology_base.5th`
- [ ] T046 [P] Integration: empty interval precedence over duplicate `.../empty_vs_duplicate.5th`

## Phase 3.7: Instrumentation & Performance
- [ ] T047 Add `Instrumentation/InstrumentationLogger.cs` (env var detection + JSON output)
- [ ] T048 Add synthetic scenario definitions `specs/002-guard-clause-overload-completeness/perf-scenarios.json`
- [ ] T049 Add micro-benchmark harness `test/runtime-integration-tests/Performance/GuardValidationBenchTests.cs` (measure overhead)
- [ ] T050 Implement optional local pooling (guarded by allocation threshold) & benchmark delta
- [ ] T051 Performance assertion test (fail if >5% overhead unless waiver file exists)

## Phase 3.8: Traceability & Parity
- [ ] T052 Populate `traceability.json` mapping FR-001..070 & AC-001..038 to tests (unit/integration/perf)
- [ ] T053 Add traceability validation test `test/ast-tests/Validation/Guards/Traceability/TraceabilityCoverageTests.cs`
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
