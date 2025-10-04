## Tasks: New Primitive Type `triple`

**Input**: Design documents from `/specs/001-new-primitive-type/`
**Prerequisites**: `plan.md`, `research.md`, `data-model.md`, `contracts/`, `quickstart.md`

## Phase 3.1: Setup / Environment
- [x] T001 Ensure ANTLR & .NET toolchain available (verify `dotnet --version`, Java 17+) (no code change)
- [x] T002 Create benchmark fixture placeholder `test/perf/TripleParseBaseline.cs` (will remain failing until populated)

## Phase 3.2: Tests First (Grammar & AST) ⚠️ MUST FAIL INITIALLY
- [x] T003 Add valid triple literal samples in `src/parser/grammar/test_samples/triple_valid_01.5th` (simple IRIs)
- [x] T004 [P] Add valid list expansion sample `triple_valid_list_01.5th`
- [x] T005 [P] Add invalid nested list sample `triple_invalid_nested_01.5th` expecting TRPL006
- [x] T006 [P] Add invalid arity samples `<s,p>` and `<s,p,o,x>` `triple_invalid_arity_01.5th` expecting TRPL001
- [x] T007 [P] Add invalid trailing comma sample `triple_invalid_trailing_01.5th` expecting parse error → TRPL001
- [x] T008 [P] Add ambiguous `<{` vs `<s,p,o>` separation sample to ensure graph assertion unaffected
- [x] T009 Create AST test `test/ast-tests/TripleLiteralAstTests.cs` (assert node kinds, subject/predicate variable acceptance)
- [x] T010 [P] Add AST test for list expansion placeholder (will fail until expansion pass) `TripleLiteralExpansionTests.cs`
- [x] T011 Add diagnostic test skeletons for TRPL001–TRPL006 in `test/syntax-parser-tests/TripleDiagnosticsTests.cs`
- [x] T011A Add unresolved prefix negative test sample `triple_invalid_prefix_01.5th` asserting existing unresolved-prefix diagnostic (FR-023)

## Phase 3.3: Lexer & Parser Implementation
- [x] T012 Add `TRIPLE : 'triple';` to `src/parser/grammar/FifthLexer.g4`
- [x] T013 Add `MINUS_ASSIGN : '-=';` token to lexer (if absent) & integrate into tokens ordering
- [x] T014 Integrate `tripleLiteral` rule into `FifthParser.g4` (`literal` alt) with two comma structure
- [x] T015 Adjust lookahead / predicates to disambiguate `<{` vs triple (ensure no regression in existing tests)
- [x] T016 Run full build to regenerate ANTLR outputs (expect failing new tests now partly satisfied)
- [x] T017 Implement parse-tree visit logic in `src/parser/AstBuilderVisitor.cs` constructing `TripleLiteralExp`

## Phase 3.4: AST / Metamodel Adjustments
- [x] T018 Confirm `TripleLiteralExp` exists in `src/ast-model/AstMetamodel.cs` (no change expected) else add and regenerate (completed: rename applied & verified via successful build/tests)
- [x] T019 [P] Regenerate AST (`make run-generator`) and commit generated artifacts (do NOT hand-edit) (completed: generation ran during build after rename)

## Phase 3.5: Transformation Passes (Design Tests First)
- [x] T020 Add failing test `test/ast-tests/TripleExpansionVisitorTests.cs` for list -> multiple triples  # implemented: covered via updates to `TripleLiteralExpansionTests.cs` and added unit-level checks for expansion behavior
- [x] T021 [P] Add failing test `test/ast-tests/TripleLoweringVisitorTests.cs` for `graph + triple` & `triple + triple`
- [x] T022 Implement `TripleLiteralExpansionVisitor.cs` under `src/compiler/LanguageTransformations/`  # implemented: expansion visitor added and wired in ParserManager
- [x] T023 Implement empty list warning TRPL004 emission in expansion visitor  # implemented: TRPL004 emitted for empty list objects
- [x] T024 Implement nested list TRPL006 error detection in expansion visitor  # implemented: TRPL006 emitted for nested list objects
- [x] T025 Implement `GraphTripleOperatorLoweringVisitor.cs` handling +,- between graphs/triples (non-mutating)
- [x] T026 Add structural dedupe logic (or rely on KG helpers) ensuring single instance after union
- [x] T026A Add operator invalid-combination tests (`triple - graph`, `triple * 2`, `!<...>`) asserting type errors (covers FR-012/013)

# Notes
- The expansion and diagnostics work (T022–T024) has been implemented and covered by unit and AST-level tests (including `TripleDiagnosticsUnitTests.cs`, parser-level tests, and updates to `TripleLiteralExpansionTests.cs`).
- Remaining Phase 3.5 items are lowering and dedupe work (T025–T026A) which are not yet implemented.

## Phase 3.6: Mutating Operators & KG Helpers
- [x] T027 Add failing tests `test/runtime-integration-tests/TripleMutatingOperatorsTests.cs` for `+=` / `-=` forms
- [x] T028 Implement parser assignment rule extension (if needed) for MINUS_ASSIGN handling `graph -= triple`
- [x] T029 Implement lowering for `g += <...>` and `g -= <...>` in lowering visitor (desugar to assignment)
- [x] T030 Review `fifthlang.system/KnowledgeGraphs.cs` (or similar) for `Assert`, `Retract`, `CreateTriple`; add `Retract` / `CopyGraph` if absent
- [x] T031 [P] Add unit tests for newly added KG helper methods


## Phase 3.7: Type Inference & Diagnostics
- [ ] T032 Extend type inference (likely in generated type inference file or manual visitor) to map triple literal to primitive `triple`
- [ ] T033 [P] Add type inference test verifying operator result types (graph + triple → graph, triple + triple → graph)
- [ ] T034 Add diagnostic emission points mapping (wire TRPL001–TRPL006) to existing diagnostic framework
- [ ] T035 [P] Add tests asserting diagnostics appear with correct codes/messages
- [ ] T035A Add canonical serialization / round-trip test (construct triple then serialize then parse; covers FR-018 & FR-018A escaping of `>` and `,`)

## Phase 3.8: Property-Based & Integration Tests
- [ ] T036 Implement property-based duplicate suppression test (adding same triple N times size stable)
- [ ] T037 [P] Implement property-based list expansion associativity test
- [ ] T038 Integration test: triple literal in graph assertion block `<{ <s,p,o>; }>` asserts triple (FR-021)
- [ ] T039 [P] Integration test: nested list rejected with TRPL006
- [ ] T040 Integration test: performance harness baseline capture (pre-feature parse timing reference)
- [ ] T040A Property-based ordering invariance test: ensure adding same set of triples via different `+` association yields graphs structurally equal disregarding order (FR-008B)

## Phase 3.9: Performance & Benchmark
- [ ] T041 Create large sample file `test/perf/triple_heavy_01.5th` (1000 triple literals)
- [ ] T042 Implement benchmark runner (if not existing) measuring parse time vs baseline
- [ ] T043 Enforce ≤5% regression check (fail test if exceeded)
- [ ] T043A Enforce variance guard (mean ≤5% AND mean ≤ 2σ) in benchmark assertion harness

## Phase 3.10: Documentation & Validation
- [ ] T044 Validate `quickstart.md` samples parse with `scripts/validate-examples.fish`
- [ ] T045 [P] Update docs/knowledge-graphs.md (add triple literal section)
- [ ] T046 [P] Add diagnostics section to docs (TRPL001–TRPL006 table)
- [ ] T047 Update spec FR section if implementation details uncovered (remove any stale assumptions)
- [ ] T047A Add docs snippet demonstrating escaped serialization for a string object containing `,` and `>` (FR-018A)

## Phase 3.11: Finalization / Gates
- [ ] T048 Constitution re-check (no manual generated edits, tests precede impl)
- [ ] T049 Ensure all new tests green; re-run full solution tests
- [ ] T050 Prepare PR summary: performance data, diagnostics list, transformation overview
- [ ] T050A Generated-code integrity check: run generator twice and diff `src/ast-generated` to ensure idempotence (guards against hidden state in templates)

## Dependencies & Ordering
- T003–T011 must precede lexer/parser implementation tasks (T012–T017) (TDD)
- T012–T017 precede transformation tests (T020–T021) & passes (T022–T026)
- Mutating operator tests (T027) before implementation (T028–T030)
- Type inference tasks (T032–T035) after lowering visitors exist
- Property/integration tests (T036–T040) after core lowering passes
- Performance tasks (T041–T043) after feature implementation but before finalization
- Documentation (T044–T047) after core correctness verified

## Parallelizable [P] Tasks
Group A (early grammar tests): T004 T005 T006 T007 T008
Group B (AST/testing scaffolds): T010 T011
Group C (Lowering test scaffolds): T021 T033 T035
Group D (Property/integration): T037 T039
Group E (Docs): T045 T046

## Acceptance Criteria Mapping
| Requirement | Tasks |
|-------------|-------|
| FR-003/004 (syntax form) | T012–T015, T003–T008 |
| FR-005 (IRI forms / vars) | T009, T012, T017, T032 |
| FR-006 (single-level list) | T005, T020, T022–T024, T034 |
| FR-008–FR-010 (operators) | T021, T025–T026, T026A, T033, T036–T038, T040A |
| FR-008A (structural equality set semantics) | T026, T036, T040A |
| FR-008B (ordering non-observable) | T026, T040A |
| FR-019–FR-021 (list expansion & assertion block) | T020–T024, T038 |
| FR-018A (canonical escaping) | T035A, T047A |
| FR-023 (no implicit prefixes) | T012–T015, T011A, T034, T045 |
| Mutating ops (spec extension) | T027–T030 |
| Diagnostics TRPL001–006 | T005–T007, T011, T022–T024, T034–T035 |
| NFR-002 (performance) | T041–T043 |

## Parallel Execution Example
```
# Example: run early grammar tests in parallel (after creating sample files):
Task: T004 Add valid list expansion sample
Task: T005 Add invalid nested list sample
Task: T006 Add invalid arity samples
Task: T007 Add invalid trailing comma sample
Task: T008 Add ambiguity separation sample
```

## Validation Checklist
- [ ] All diagnostics tested
- [ ] All operator combinations tested
- [ ] Performance regression check in place
- [ ] No nested list accepted
- [ ] Empty list warning emitted
- [ ] Mutating operators desugar correctly
- [ ] Documentation updated (quickstart + knowledge graphs + diagnostics)

## Post-Merge Follow-Ups
- Consider graph literal synergy & potential triple pattern matching
- Optional optimization: memoize structural equality for large graphs
