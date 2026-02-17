# Tasks: Query Application and Result Type

**Feature Branch**: `011-query-application-result-type`  
**Input**: Design documents from `/specs/011-query-application-result-type/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/  
**Date**: 2025-11-15

**Tests**: TDD approach is MANDATORY per constitution. All test tasks must be completed before implementation.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and AST generator setup

- [ ] T001 Run AST code generator to establish baseline in src/ast-generated/
- [ ] T002 [P] Create test sample directory src/parser/grammar/test_samples/ for query application examples
- [ ] T003 [P] Verify ANTLR 4.8 toolchain and .NET 8.0.118 SDK per global.json

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core grammar, type system, and runtime infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Grammar Foundation

- [ ] T004 Add QUERY_APPLICATION_OP token '<-' to src/parser/grammar/FifthLexer.g4 before comparison operators
- [ ] T005 Add queryApplicationExpr parser rule to src/parser/grammar/FifthParser.g4
- [ ] T006 Build parser project to trigger ANTLR grammar compilation

### AST Foundation

- [ ] T007 Add QueryApplicationExp node to src/ast-model/AstMetamodel.cs with Query/Store/InferredType fields
- [ ] T008 [P] Add ResultType discriminated union to src/ast-model/AstMetamodel.cs (TabularResult|GraphResult|BooleanResult cases)
- [ ] T009 Run AST code generator to regenerate src/ast-generated/ with new nodes
- [ ] T010 Update src/parser/AstBuilderVisitor.cs to handle VisitQueryApplicationExpr creating QueryApplicationExp nodes

### Runtime Foundation (Fifth.System)

- [ ] T011 [P] Create src/fifthlang.system/ResultType.cs with Result discriminated union using dunet
- [ ] T012 [P] Create src/fifthlang.system/QueryError.cs with ErrorKind enum (8 values) and structured fields
- [ ] T013 [P] Create src/fifthlang.system/SourceSpan.cs value struct (Line, Column, Length, FilePath)
- [ ] T014 Create src/fifthlang.system/QueryApplicationExecutor.cs with Execute(Query, Store, CancellationToken?) signature
- [ ] T015 Implement dotNetRDF exception-to-QueryError mapping in QueryApplicationExecutor per research.md table

### Transformation Foundation

- [ ] T016 [P] Create src/compiler/LanguageTransformations/QueryApplicationTypeCheckVisitor.cs stub
- [ ] T017 [P] Create src/compiler/LanguageTransformations/SparqlSecurityValidator.cs stub with 6 validation rules
- [ ] T018 [P] Create src/compiler/LanguageTransformations/QueryApplicationLoweringRewriter.cs stub extending DefaultAstRewriter
- [ ] T019 Register transformation passes in src/compiler/ParserManager.cs transformation pipeline

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Apply SELECT Query to Store (Priority: P1) 🎯 MVP

**Goal**: Enable developers to apply SPARQL SELECT queries to stores using `<-` operator, returning tabular results with variable access

**Independent Test**: Create store with sample triples, apply SELECT query using `<-`, verify Result.TabularResult contains expected rows/variables

### Tests for User Story 1

**Grammar/Parser Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T020 [P] [US1] Create test/syntax-parser-tests/QueryApplicationGrammarTests.cs testing `<-` token recognition
- [ ] T021 [P] [US1] Add test case verifying SELECT query literal `<-` store syntax parses correctly
- [ ] T022 [P] [US1] Add test case verifying operator precedence (function calls before `<-`)
- [ ] T023 [P] [US1] Create src/parser/grammar/test_samples/query_application_select.5th example

**AST Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T024 [P] [US1] Create test/ast-tests/QueryApplicationTypeCheckTests.cs with test for Query/Store type validation
- [ ] T025 [P] [US1] Add test case rejecting non-Query LHS operand (compile-time error)
- [ ] T026 [P] [US1] Add test case rejecting non-Store RHS operand (compile-time error)
- [ ] T027 [P] [US1] Add test case verifying Result type inference from QueryApplicationExp

**Lowering Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T028 [P] [US1] Create test/ast-tests/QueryApplicationLoweringTests.cs verifying AST transformation to FuncCallExp
- [ ] T029 [P] [US1] Add test case verifying lowered call targets Fifth.System.QueryApplicationExecutor.Execute
- [ ] T030 [P] [US1] Add test case verifying Query/Store arguments preserved in lowered call

**Integration Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T031 [P] [US1] Create test/runtime-integration-tests/QueryApplicationSelectTests.cs
- [ ] T032 [P] [US1] Add test: basic SELECT query returns TabularResult with expected variables (SC-001, SC-002)
- [ ] T033 [P] [US1] Add test: TabularResult.Rows iteration yields correct row count and values (SC-006)
- [ ] T034 [P] [US1] Add test: SELECT with FILTER clause filters rows correctly
- [ ] T035 [P] [US1] Add test: SELECT with zero results returns empty TabularResult
- [ ] T036 [P] [US1] Add test: SELECT query with 10,000 rows completes without memory issues (SC-006 baseline)

### Implementation for User Story 1

**Grammar Implementation**:

- [ ] T037 [US1] Verify QUERY_APPLICATION_OP token in FifthLexer.g4 (from T004 - should already pass grammar tests)
- [ ] T038 [US1] Verify queryApplicationExpr rule in FifthParser.g4 (from T005 - should already pass grammar tests)

**AST Builder Implementation**:

- [ ] T039 [US1] Implement VisitQueryApplicationExpr in AstBuilderVisitor.cs returning QueryApplicationExp node

**Type Checking Implementation**:

- [ ] T040 [US1] Implement QueryApplicationTypeCheckVisitor.VisitQueryApplicationExp validating Query/Store types
- [ ] T041 [US1] Add diagnostic emission for type mismatches (non-Query LHS, non-Store RHS)
- [ ] T042 [US1] Set InferredType to Result on successful type check

**Runtime Implementation**:

- [ ] T043 [US1] Implement TabularResult properties in ResultType.cs (Rows, Variables, RowCount accessors)
- [ ] T044 [US1] Implement TabularResult.GetValue<T>(rowIndex, varName) method
- [ ] T045 [US1] Implement QueryApplicationExecutor.Execute for SELECT queries creating TabularResult from SparqlResultSet
- [ ] T046 [US1] Add streaming/lazy enumeration support for TabularResult.Rows (IEnumerable<SparqlResult>)

**Lowering Implementation**:

- [ ] T047 [US1] Implement QueryApplicationLoweringRewriter.VisitQueryApplicationExp transforming to FuncCallExp
- [ ] T048 [US1] Verify lowered code calls Fifth.System.QueryApplicationExecutor.Execute with Query/Store arguments

**Validation**:

- [ ] T049 [US1] Run grammar tests - all should PASS
- [ ] T050 [US1] Run AST tests - all should PASS
- [ ] T051 [US1] Run integration tests - all should PASS
- [ ] T052 [US1] Build solution with dotnet build fifthlang.sln --no-restore
- [ ] T053 [US1] Run subset tests: dotnet test test/ast-tests/ast_tests.csproj

**Checkpoint**: At this point, User Story 1 (SELECT queries) should be fully functional and testable independently

---

## Phase 4: User Story 2 - Apply CONSTRUCT/DESCRIBE Query to Store (Priority: P2)

**Goal**: Enable developers to apply CONSTRUCT/DESCRIBE queries returning GraphResult with Store access

**Independent Test**: Create store with sample triples, apply CONSTRUCT query using `<-`, verify Result.GraphResult contains constructed Store

### Tests for User Story 2

**Grammar/Parser Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T054 [P] [US2] Add test case in QueryApplicationGrammarTests.cs for CONSTRUCT query syntax
- [ ] T055 [P] [US2] Add test case for DESCRIBE query syntax
- [ ] T056 [P] [US2] Create src/parser/grammar/test_samples/query_application_construct.5th example
- [ ] T057 [P] [US2] Create src/parser/grammar/test_samples/query_application_describe.5th example

**Integration Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T058 [P] [US2] Create test/runtime-integration-tests/QueryApplicationConstructTests.cs
- [ ] T059 [P] [US2] Add test: CONSTRUCT query returns GraphResult with Store (SC-002)
- [ ] T060 [P] [US2] Add test: GraphResult.GraphStore contains constructed triples
- [ ] T061 [P] [US2] Add test: GraphResult.ToTriG() produces valid TriG serialization
- [ ] T062 [P] [US2] Add test: GraphResult.TripleCount returns correct count
- [ ] T063 [P] [US2] Add test: DESCRIBE query returns GraphResult with described resources
- [ ] T064 [P] [US2] Add test: GraphResult.GraphStore can be further queried (chaining scenario)
- [ ] T065 [P] [US2] Add test: CONSTRUCT with zero matching patterns returns empty GraphResult

### Implementation for User Story 2

**Runtime Implementation**:

- [ ] T066 [US2] Implement GraphResult properties in ResultType.cs (GraphStore, TripleCount accessors)
- [ ] T067 [US2] Implement GraphResult.ToTriG() serialization method
- [ ] T068 [US2] Implement GraphResult.GetTriples(graphName?) enumeration method
- [ ] T069 [US2] Update QueryApplicationExecutor.Execute to handle CONSTRUCT queries creating GraphResult
- [ ] T070 [US2] Update QueryApplicationExecutor.Execute to handle DESCRIBE queries creating GraphResult
- [ ] T071 [US2] Add Store wrapping logic converting dotNetRDF IGraph to Fifth.System.Store

**Validation**:

- [ ] T072 [US2] Run grammar tests - all should PASS
- [ ] T073 [US2] Run integration tests - all should PASS
- [ ] T074 [US2] Verify User Story 1 (SELECT) still works independently

**Checkpoint**: At this point, User Stories 1 (SELECT) AND 2 (CONSTRUCT/DESCRIBE) should both work independently

---

## Phase 5: User Story 3 - Handle Query Execution Errors (Priority: P3)

**Goal**: Provide clear, structured error diagnostics when query execution fails (syntax errors, timeouts, etc.)

**Independent Test**: Attempt to apply malformed/invalid queries, verify QueryError with appropriate Kind/Message/Suggestion

### Tests for User Story 3

**Integration Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T075 [P] [US3] Create test/runtime-integration-tests/QueryApplicationErrorTests.cs
- [ ] T076 [P] [US3] Add test: SyntaxError for malformed SPARQL query (SC-004, SC-010, SC-013)
- [ ] T077 [P] [US3] Add test: ValidationError for undefined variable in SELECT clause (SC-010, SC-013)
- [ ] T078 [P] [US3] Add test: ExecutionError for function evaluation failure (SC-010, SC-013)
- [ ] T079 [P] [US3] Add test: Timeout error for query exceeding time limit (SC-011, SC-013)
- [ ] T080 [P] [US3] Add test: Cancellation error when CancellationToken triggered (SC-011, SC-013)
- [ ] T081 [P] [US3] Add test: ResourceLimit error for memory-exhausting query (SC-013)
- [ ] T082 [P] [US3] Add test: SourceSpan populated for SyntaxError with line/column info (SC-010)
- [ ] T083 [P] [US3] Add test: Suggestion field populated for ≥80% of errors (SC-010)
- [ ] T084 [P] [US3] Add test: UnderlyingExceptionType captures dotNetRDF exception name (SC-010)
- [ ] T085 [P] [US3] Create src/parser/grammar/test_samples/query_application_errors.5th with invalid examples

**Security Validation Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T086 [P] [US3] Create test/ast-tests/SparqlSecurityValidatorTests.cs
- [ ] T087 [P] [US3] Add test: SecurityWarning for unbalanced braces (SC-008, SC-013)
- [ ] T088 [P] [US3] Add test: SecurityWarning for duplicate PREFIX declarations (SC-008)
- [ ] T089 [P] [US3] Add test: SecurityWarning for trailing comment escape (SC-008)
- [ ] T090 [P] [US3] Add test: SecurityWarning for rogue DROP/LOAD commands (SC-008)
- [ ] T091 [P] [US3] Add test: SecurityWarning for mid-token backslash (SC-008)
- [ ] T092 [P] [US3] Add test: SecurityWarning for unstructured concatenation (compiler warning) (SC-008)
- [ ] T093 [P] [US3] Add test: Legitimate queries pass validation (≥95% false positive rate per SC-008)
- [ ] T094 [P] [US3] Add ≥30 injection attack test cases covering all 6 validation rules (SC-008)

### Implementation for User Story 3

**Error Handling Implementation**:

- [ ] T095 [US3] Implement QueryError factory method FromException(Exception, queryText) with switch-based mapping
- [ ] T096 [US3] Implement QueryError.SecurityWarning factory method
- [ ] T097 [US3] Implement SourceSpan extraction from RdfParseException line/column info
- [ ] T098 [US3] Implement Suggestion generation for each ErrorKind (target ≥80% coverage)
- [ ] T099 [US3] Add QueryExecutionException wrapping QueryError for throw semantics
- [ ] T100 [US3] Update QueryApplicationExecutor.Execute with try-catch mapping dotNetRDF exceptions

**Security Validation Implementation**:

- [ ] T101 [US3] Implement SparqlSecurityValidator.Validate(queryText) with 6 validation rules
- [ ] T102 [US3] Add unbalanced braces detection (count '{' vs '}')
- [ ] T103 [US3] Add duplicate PREFIX detection via regex/parsing
- [ ] T104 [US3] Add trailing comment escape detection (# without newline)
- [ ] T105 [US3] Add rogue command detection (DROP, LOAD, CREATE, CLEAR, etc.)
- [ ] T106 [US3] Add mid-token backslash detection (outside string literals/URIs)
- [ ] T107 [US3] Add unstructured concatenation warning emission
- [ ] T108 [US3] Integrate SparqlSecurityValidator into QueryApplicationExecutor.Execute pre-execution

**Validation**:

- [ ] T109 [US3] Run error handling tests - all should PASS
- [ ] T110 [US3] Run security validation tests - all should PASS (SC-008: ≥30 injection rejections, ≥95% legit queries pass)
- [ ] T111 [US3] Verify all 8 ErrorKind values covered in tests (SC-013)
- [ ] T112 [US3] Verify ≥80% of errors have Suggestion field (SC-010)
- [ ] T113 [US3] Verify User Stories 1-2 still work independently

**Checkpoint**: All error handling and security validation functional

---

## Phase 6: User Story 4 - Type Safety for Result Access (Priority: P2)

**Goal**: Provide compile-time type safety and pattern matching for Result discriminated union

**Independent Test**: Write code accessing wrong Result case, verify compile-time or runtime pattern match errors

### Tests for User Story 4

**ASK Query Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T114 [P] [US4] Create test/runtime-integration-tests/QueryApplicationAskTests.cs
- [ ] T115 [P] [US4] Add test: ASK query returns BooleanResult with true for matching pattern (SC-007)
- [ ] T116 [P] [US4] Add test: ASK query returns BooleanResult with false for non-matching pattern (SC-007)
- [ ] T117 [P] [US4] Add test: ASK with empty store returns false (SC-007)
- [ ] T118 [P] [US4] Add test: ASK with GRAPH clause returns correct boolean (SC-007)
- [ ] T119 [P] [US4] Add test: ASK with FILTER edge cases (SC-007)
- [ ] T120 [P] [US4] Add ≥20 ASK query test scenarios (SC-007)
- [ ] T121 [P] [US4] Create src/parser/grammar/test_samples/query_application_ask.5th example

**Pattern Matching Tests** (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T122 [P] [US4] Add test in QueryApplicationSelectTests.cs: pattern match Result extracting TabularResult case
- [ ] T123 [P] [US4] Add test in QueryApplicationConstructTests.cs: pattern match Result extracting GraphResult case
- [ ] T124 [P] [US4] Add test in QueryApplicationAskTests.cs: pattern match Result extracting BooleanResult case
- [ ] T125 [P] [US4] Add test: non-exhaustive pattern match produces compile-time warning/error

### Implementation for User Story 4

**Runtime Implementation**:

- [ ] T126 [US4] Implement BooleanResult properties in ResultType.cs (Value accessor)
- [ ] T127 [US4] Update QueryApplicationExecutor.Execute to handle ASK queries creating BooleanResult
- [ ] T128 [US4] Add mapping from dotNetRDF SparqlResultsType.Boolean to BooleanResult case

**Pattern Matching Support**:

- [ ] T129 [US4] Verify dunet generates exhaustiveness checking for Result discriminated union
- [ ] T130 [US4] Add pattern matching examples to quickstart.md if not already present

**Validation**:

- [ ] T131 [US4] Run ASK query tests - all should PASS (SC-007: ≥20 scenarios)
- [ ] T132 [US4] Run pattern matching tests - all should PASS
- [ ] T133 [US4] Verify Result discriminated union handles all 4 SPARQL query forms correctly (SC-002)

**Checkpoint**: All user stories (1-4) should now be independently functional

---

## Phase 7: Performance & Concurrency (Cross-Cutting)

**Goal**: Validate performance targets (FR-013: streaming, FR-015: cancellation, FR-016: concurrency)

### Performance Tests (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T134 [P] Add test in QueryApplicationSelectTests.cs: SELECT 100k rows streaming with <1.5× memory (SC-009)
- [ ] T135 [P] Add test: SELECT 100k rows throughput <10% degradation vs direct dotNetRDF (SC-009)
- [ ] T136 [P] Add test: TabularResult.Rows iteration is lazy (no full materialization)

### Cancellation Tests (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T137 [P] Create test/runtime-integration-tests/QueryCancellationTests.cs
- [ ] T138 [P] Add test: CancellationToken triggers query termination <200ms (SC-011)
- [ ] T139 [P] Add test: Cancellation produces QueryError with Cancellation Kind (SC-013)
- [ ] T140 [P] Add test: Absent CancellationToken maintains synchronous semantics

### Concurrency Tests (TDD - WRITE FIRST, ENSURE FAIL):

- [ ] T141 [P] Create test/runtime-integration-tests/QueryConcurrencyTests.cs
- [ ] T142 [P] Add test: ≥25 parallel read queries show no data corruption (SC-012)
- [ ] T143 [P] Add test: Parallel queries show ≤5% latency variance vs isolated (SC-012)
- [ ] T144 [P] Add test: ReaderWriterLockSlim allows concurrent readers

### Implementation:

- [ ] T145 Implement CancellationToken parameter in QueryApplicationExecutor.Execute (already stubbed in T014)
- [ ] T146 Propagate CancellationToken to dotNetRDF SparqlQueryEvaluationOptions
- [ ] T147 Add cooperative cancellation checks (ThrowIfCancellationRequested) in execution loop
- [ ] T148 Implement ReaderWriterLockSlim in Store wrapper for concurrency isolation
- [ ] T149 Add AcquireReadLock() method in Store used by QueryApplicationExecutor
- [ ] T150 Update QueryApplicationLoweringRewriter to pass cancellation token if available in scope

### Validation:

- [ ] T151 Run performance tests - verify SC-009 criteria (<1.5× memory, <10% throughput)
- [ ] T152 Run cancellation tests - verify SC-011 criteria (<200ms termination)
- [ ] T153 Run concurrency tests - verify SC-012 criteria (≤5% variance)
- [ ] T154 Run full test suite: dotnet test fifthlang.sln

---

## Phase 8: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, validation, and final integration checks

- [ ] T155 [P] Update docs/knowledge-graphs.md with query application examples using `<-` operator
- [ ] T156 [P] Add query application examples to docs/learn5thInYMinutes.md
- [ ] T157 [P] Validate all .5th example files parse correctly via scripts/validate-examples.fish
- [ ] T158 [P] Add query application operator to docs/syntax-samples-readme.md
- [ ] T159 Code cleanup: remove debug logging, commented code, TODO markers
- [ ] T160 Run full build: dotnet build fifthlang.sln --no-restore (timeout 120s)
- [ ] T161 Run full test suite: dotnet test fifthlang.sln (timeout 5min)
- [ ] T162 Verify constitution compliance: all gates still pass per plan.md
- [ ] T163 Run quickstart.md validation: execute examples from specs/011-query-application-result-type/quickstart.md
- [ ] T164 Generate final AST code via just run-generator
- [ ] T165 Verify no hand-edits in src/ast-generated/ (constitution violation check)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phases 3-6)**: All depend on Foundational phase completion
  - User Story 1 (SELECT - Phase 3): Can start after Phase 2 - MVP target
  - User Story 2 (CONSTRUCT/DESCRIBE - Phase 4): Can start after Phase 2 - Independent of US1
  - User Story 3 (Error Handling - Phase 5): Can start after Phase 2 - Independent of US1/US2 but enhances both
  - User Story 4 (Type Safety/ASK - Phase 6): Can start after Phase 2 - Independent of US1-US3
- **Performance & Concurrency (Phase 7)**: Depends on US1 completion (needs SELECT baseline)
- **Polish (Phase 8)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1 - SELECT)**: MVP - Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2 - CONSTRUCT/DESCRIBE)**: Can start after Foundational (Phase 2) - Independent of US1, shares runtime executor
- **User Story 3 (P3 - Error Handling)**: Can start after Foundational (Phase 2) - Enhances US1/US2 but independently testable
- **User Story 4 (P2 - Type Safety/ASK)**: Can start after Foundational (Phase 2) - Independent of US1-US3, completes Result union

### Within Each User Story

- Tests MUST be written and FAIL before implementation (TDD discipline per constitution)
- Grammar/parser tests before AST tests
- AST tests before lowering tests
- All tests before implementation
- Runtime types before executor implementation
- Transformation passes before lowering integration
- Story complete before moving to next priority

### Parallel Opportunities

#### Phase 1 (Setup)
- T002 (test samples) || T003 (SDK verification) can run in parallel

#### Phase 2 (Foundational)
- T008 (ResultType AST) || T007 (QueryApplicationExp AST) after T006 completes
- T011 (ResultType.cs) || T012 (QueryError.cs) || T013 (SourceSpan.cs) in runtime layer
- T016 (TypeCheck stub) || T017 (SecurityValidator stub) || T018 (Lowering stub) transformation passes

#### Phase 3 (User Story 1 - SELECT)
**All tests in parallel (T020-T036)**:
- Grammar tests (T020-T023)
- AST tests (T024-T030)
- Integration tests (T031-T036)

**After tests complete, implementation can proceed with some parallelism**:
- T043 (TabularResult properties) || T044 (GetValue method) || T046 (streaming) can run in parallel

#### Phase 4 (User Story 2 - CONSTRUCT/DESCRIBE)
**All tests in parallel (T054-T065)**:
- Grammar tests (T054-T057)
- Integration tests (T058-T065)

**Implementation**:
- T066 (GraphResult properties) || T067 (ToTriG) || T068 (GetTriples) can run in parallel

#### Phase 5 (User Story 3 - Error Handling)
**All tests in parallel (T075-T094)**:
- Integration tests (T075-T085)
- Security validation tests (T086-T094)

**Implementation**:
- T102-T107 (6 security validation rules) can run in parallel after T101

#### Phase 6 (User Story 4 - Type Safety/ASK)
**All tests in parallel (T114-T125)**:
- ASK query tests (T114-T121)
- Pattern matching tests (T122-T125)

#### Phase 7 (Performance & Concurrency)
**All tests in parallel (T134-T144)**:
- Performance tests (T134-T136)
- Cancellation tests (T137-T140)
- Concurrency tests (T141-T144)

#### Phase 8 (Polish)
- T155-T158 (documentation updates) can run in parallel

### Parallel Example: Foundational Phase (Phase 2)

```bash
# After grammar foundation (T004-T006) completes, launch in parallel:
Task T008: "Add ResultType to AstMetamodel.cs"
Task T007: "Add QueryApplicationExp to AstMetamodel.cs"

# After T009 (AST regeneration), launch in parallel:
Task T011: "Create ResultType.cs"
Task T012: "Create QueryError.cs"
Task T013: "Create SourceSpan.cs"

# Separately, launch transformation stubs in parallel:
Task T016: "Create QueryApplicationTypeCheckVisitor.cs"
Task T017: "Create SparqlSecurityValidator.cs"
Task T018: "Create QueryApplicationLoweringRewriter.cs"
```

### Parallel Example: User Story 1 Tests (Phase 3)

```bash
# Launch all grammar/parser tests together:
Task T020: "Grammar test for <- token"
Task T021: "Grammar test for SELECT <- syntax"
Task T022: "Grammar test for operator precedence"
Task T023: "Create query_application_select.5th sample"

# Launch all AST tests together:
Task T024: "Create QueryApplicationTypeCheckTests.cs"
Task T025: "Test rejecting non-Query LHS"
Task T026: "Test rejecting non-Store RHS"
Task T027: "Test Result type inference"

# Launch all integration tests together:
Task T031: "Create QueryApplicationSelectTests.cs"
Task T032-T036: "All integration test cases"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only - Fastest Path to Value)

1. **Complete Phase 1**: Setup (T001-T003) → ~10 minutes
2. **Complete Phase 2**: Foundational (T004-T019) → ~4 hours (CRITICAL - blocks all stories)
3. **Complete Phase 3**: User Story 1 (T020-T053) → ~8 hours (TDD: tests first, then implementation)
4. **STOP and VALIDATE**: Test User Story 1 independently
   - Run: `dotnet test test/syntax-parser-tests/ --filter QueryApplicationGrammar`
   - Run: `dotnet test test/ast-tests/ --filter QueryApplicationTypeCheck`
   - Run: `dotnet test test/runtime-integration-tests/ --filter QueryApplicationSelect`
   - Execute: `./scripts/validate-examples.fish` to check test samples
5. **Deploy/Demo**: SELECT queries working end-to-end (satisfies SC-001, SC-002, SC-006)

**Estimated Time**: ~12 hours for MVP (SELECT queries only)

### Incremental Delivery (Recommended)

1. **Complete Setup + Foundational** (Phases 1-2) → Foundation ready
2. **Add User Story 1** (Phase 3: SELECT) → Test independently → **Deploy/Demo MVP!**
3. **Add User Story 2** (Phase 4: CONSTRUCT/DESCRIBE) → Test independently → Deploy/Demo
4. **Add User Story 3** (Phase 5: Error Handling) → Test independently → Deploy/Demo
5. **Add User Story 4** (Phase 6: Type Safety/ASK) → Test independently → Deploy/Demo
6. **Add Performance/Concurrency** (Phase 7) → Validate SC-009, SC-011, SC-012
7. **Polish** (Phase 8) → Final validation

**Estimated Total Time**: ~40 hours for complete feature with all user stories

### Parallel Team Strategy (3+ Developers)

With multiple developers after Foundational phase completes:

**Week 1**:
- **Developer A**: User Story 1 (SELECT - Phase 3) → MVP
- **Developer B**: User Story 2 (CONSTRUCT/DESCRIBE - Phase 4)
- **Developer C**: User Story 3 (Error Handling - Phase 5)

**Week 2**:
- **Developer A**: User Story 4 (ASK/Type Safety - Phase 6)
- **Developer B**: Performance & Concurrency (Phase 7)
- **Developer C**: Polish & Documentation (Phase 8)

**Integration**: Each developer completes and tests their story independently, then merge to main

**Estimated Total Time**: ~2 weeks with parallel development

---

## Validation Checklist (Before PR/Merge)

### Constitution Compliance
- [ ] All tests written FIRST (TDD discipline)
- [ ] Grammar changes follow split lexer/parser pattern
- [ ] AST nodes defined in AstMetamodel.cs, generated via `just run-generator`
- [ ] No hand-edits in src/ast-generated/
- [ ] All .5th examples validated via scripts/validate-examples.fish
- [ ] Build completes within 120s timeout
- [ ] Test suite completes within 5min timeout

### Success Criteria Validation
- [ ] SC-001: Query application in ≤2 lines (`query <- store`)
- [ ] SC-002: 100% correct Result discrimination (4 SPARQL forms)
- [ ] SC-003: 100% compile-time type checking (invalid operands rejected)
- [ ] SC-004: Runtime errors include detail for <2min diagnosis
- [ ] SC-005: ≥50% syntactic noise reduction vs C# dotNetRDF
- [ ] SC-006: 10k row result sets handled without degradation
- [ ] SC-007: ASK queries yield correct boolean (≥20 test scenarios)
- [ ] SC-008: ≥30 injection attempts rejected, ≥95% legit queries pass
- [ ] SC-009: 100k rows: <1.5× memory, <10% throughput degradation
- [ ] SC-010: 100% QueryError Kind + Message, ≥80% Suggestion
- [ ] SC-011: Cancellation <200ms termination (95th percentile)
- [ ] SC-012: ≥25 parallel queries: no corruption, ≤5% latency variance
- [ ] SC-013: All 8 ErrorKind values covered in tests

### Final Commands
```bash
# Full build validation
dotnet restore fifthlang.sln
dotnet build fifthlang.sln --no-restore

# Full test suite
dotnet test fifthlang.sln

# Example validation
./scripts/validate-examples.fish

# AST generator verification
just run-generator
git diff src/ast-generated/  # Should be no changes if already committed
```

---

## Notes

- **[P] tasks**: Different files, no dependencies, safe to run in parallel
- **[Story] labels**: Map task to specific user story for traceability
- **TDD Discipline**: ALL test tasks must be completed and FAILING before implementation starts
- **Independent Stories**: Each user story should be independently completable and testable
- **Checkpoints**: Stop at each checkpoint to validate story independently before proceeding
- **MVP Focus**: Phase 3 (User Story 1 - SELECT) is the minimum viable product
- **Constitution**: Follow all constraints in AGENTS.md and .github/copilot-instructions.md
- **Commit Strategy**: Commit after each logical task group or phase completion
- **Avoid**: Vague tasks, same-file conflicts, cross-story dependencies that break independence
