---

description: "Task list for TriG Literal Expression feature"
---

# Tasks: TriG Literal Expression Type

**Input**: Design documents from `/specs/001-trig-literal-expression/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included per repository constitution (Test-First). Tests are listed before implementation tasks in each story.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify environment and establish a green baseline

- [ ] T001 Restore solution and verify SDKs via `fifthlang.sln`
- [ ] T002 Build solution for baseline timing via `fifthlang.sln`
- [ ] T003 Run fast smoke tests via `test/ast-tests/ast_tests.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core scaffolding needed before implementing user stories

- [ ] T004 Add generator guard: verify no hand-edits under `src/ast-generated/` and document regeneration command in PR body
- [ ] T005 Create test sample folder for TriG literals in `src/parser/grammar/test_samples/trig_literals/`
- [ ] T006 [P] Add feature docs cross-link in `specs/001-trig-literal-expression/quickstart.md`
- [ ] T007 Ensure generator entry is available at `src/ast_generator/ast_generator.csproj`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Initialize Store from TriG literal (Priority: P1) 🎯 MVP

**Goal**: Declare and initialize a `Store` from an inline `@< ... >` TriG literal without interpolation.

**Independent Test**: A .5th program with a TriG literal compiles and populates a `Store`.

### Tests for User Story 1 (write first)

- [ ] T008 [P] [US1] Add positive parser sample `.5th` at `src/parser/grammar/test_samples/trig_literals/valid_basic.5th`
- [ ] T009 [P] [US1] Add negative parser sample (malformed TriG) at `src/parser/grammar/test_samples/trig_literals/invalid_unbalanced.5th`
- [ ] T010 [US1] Add syntax tests referencing samples in `test/syntax-parser-tests/`
- [ ] T011 [US1] Add runtime integration test that asserts triples loaded in `test/runtime-integration-tests/`
- [ ] T012 [US1] Add whitespace preservation test (no trimming/newline loss) in `test/runtime-integration-tests/`
- [ ] T013 [US1] Add mixed-construct sample (graph `{}` + `@<...>`) and disambiguation syntax test in `test/syntax-parser-tests/`

### Implementation for User Story 1

- [ ] T014 [US1] Add `TriGLiteralExpression` node to metamodel in `src/ast-model/AstMetamodel.cs`
- [ ] T015 [P] [US1] Regenerate AST using `src/ast_generator/ast_generator.csproj` (outputs under `src/ast-generated/`)
- [ ] T016 [US1] Update lexer to recognize `@<` introducer in `src/parser/grammar/FifthLexer.g4`
- [ ] T017 [US1] Add parser rule for TriG literal block in `src/parser/grammar/FifthParser.g4`
- [ ] T018 [US1] Build AST in `src/parser/AstBuilderVisitor.cs` (construct `TriGLiteralExpression` with raw payload span)
- [ ] T019 [US1] Introduce lowering pass `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T020 [US1] Register lowering pass in `src/compiler/ParserManager.cs`
- [ ] T021 [US1] Preserve whitespace/newlines policy (no trimming) in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T022 [US1] Implement minimal runtime load (string → Store) in lowering in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T023 [US1] Wire diagnostic locations to literal span in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T024 [US1] Build full solution via `fifthlang.sln`

**Checkpoint**: US1 compiles and runs; Store initialized from literal with correct triples.

---

## Phase 4: User Story 2 - Interpolate expressions into TriG (Priority: P2)

**Goal**: Support `{{ expression }}` interpolation with default type mappings and brace escaping.

**Independent Test**: Program interpolates values of types string/int/float/bool/datetime and produces valid RDF terms.

### Tests for User Story 2 (write first)

- [ ] T025 [P] [US2] Add positive interpolation sample `.5th` at `src/parser/grammar/test_samples/trig_literals/valid_interpolation_basic.5th`
- [ ] T026 [US2] Add syntax tests for interpolation forms in `test/syntax-parser-tests/`
- [ ] T027 [US2] Add runtime integration tests per type (string/int/float/bool/datetime) in `test/runtime-integration-tests/`
- [ ] T028 [P] [US2] Add negative sample for unsupported interpolation value at `src/parser/grammar/test_samples/trig_literals/invalid_unsupported_value.5th`
- [ ] T029 [US2] Add diagnostic test asserting unsupported interpolation error with span in `test/syntax-parser-tests/`
- [ ] T030 [US2] Add brace escaping tests asserting `{{{`→`{{` and `}}}`→`}}` in `test/runtime-integration-tests/`
- [ ] T031 [US2] Add runtime-evaluation timing/scope test (value mutation before literal evaluation) in `test/runtime-integration-tests/`

### Implementation for User Story 2

- [ ] T032 [US2] Extend metamodel to include interpolation entries on `TriGLiteralExpression` in `src/ast-model/AstMetamodel.cs`
- [ ] T033 [P] [US2] Regenerate AST using `src/ast_generator/ast_generator.csproj`
- [ ] T034 [US2] Extend lexer/parser to recognize `{{ ... }}` and triple-brace escaping in `src/parser/grammar/FifthLexer.g4`
- [ ] T035 [US2] Add parser rules to capture interpolation spans and literal text in `src/parser/grammar/FifthParser.g4`
- [ ] T036 [US2] Populate interpolation nodes in `src/parser/AstBuilderVisitor.cs`
- [ ] T037 [US2] Implement serialization rules in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T038 [US2] Implement brace escaping mapping in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T039 [US2] Handle IRIs: absolute `<...>` and prefixed names pass-through in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T040 [US2] Emit diagnostics for unsupported interpolation values in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T041 [US2] Build and run test suites via `fifthlang.sln`

**Checkpoint**: US2 interpolations serialize correctly; tests validate each supported type.

---

## Phase 5: User Story 3 - Robust delimiter handling (Priority: P3)

**Goal**: Ensure nested `<...>` IRIs do not prematurely terminate the literal and diagnostics are precise for delimiter issues.

**Independent Test**: Literals with multiple IRIs and nested blocks parse; unbalanced cases produce targeted diagnostics.

### Tests for User Story 3 (write first)

- [ ] T042 [P] [US3] Add negative samples for delimiter errors under `src/parser/grammar/test_samples/trig_literals/invalid_*.5th`
- [ ] T043 [US3] Add syntax tests asserting diagnostic spans in `test/syntax-parser-tests/`

### Implementation for User Story 3

- [ ] T044 [US3] Update parser termination logic to top-level `>` matching in `src/parser/grammar/FifthParser.g4`
- [ ] T045 [US3] Ensure lexer mode/rule actions do not consume nested `<...>` prematurely in `src/parser/grammar/FifthLexer.g4`
- [ ] T046 [US3] Improve diagnostic mapping for unbalanced delimiters in `src/parser/AstBuilderVisitor.cs`
- [ ] T047 [US3] Build and run full test suite via `fifthlang.sln`

**Checkpoint**: US3 delimiter robustness and diagnostics validated.

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T048 [P] Update feature docs with examples in `specs/001-trig-literal-expression/quickstart.md`
- [ ] T049 [P] Performance check with ~100KB literal sample in `src/parser/grammar/test_samples/trig_literals/large_literal.5th`
- [ ] T050 Validate examples via script `scripts/validate-examples.fish`
- [ ] T051 [P] Add developer notes to `docs/knowledge-graphs.md` if needed
- [ ] T052 Final build + full tests via `fifthlang.sln`

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1) → Foundational (Phase 2) → User Stories (Phase 3+)
- User stories proceed in priority order P1 → P2 → P3; after Foundational, different story phases can be staffed in parallel if desired.

### User Story Dependencies

- US1: None beyond foundational
- US2: Depends on US1 metamodel and basic literal support
- US3: Can proceed after US1 grammar structure; largely independent of US2

### Parallel Opportunities

- [P] tasks within different files: regeneration (T015/T033), docs updates, sample additions, and test files can run in parallel.
- Tests for different stories can be implemented independently once their parser hooks exist.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational (T001–T007)
2. Implement US1 (T008–T024)
3. Validate US1 independently (build + targeted tests)

### Incremental Delivery

- Add US2, validate interpolation paths
- Add US3, validate robustness and diagnostics
---

description: "Task list for TriG Literal Expression feature"
---

# Tasks: TriG Literal Expression Type

**Input**: Design documents from `/specs/001-trig-literal-expression/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Included per repository constitution (Test-First). Write failing tests before implementation where applicable.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify environment and establish a green baseline

- [ ] T001 Restore solution and verify SDKs via `fifthlang.sln`
- [ ] T002 Build solution for baseline timing via `fifthlang.sln`
- [ ] T003 Run fast smoke tests via `test/ast-tests/ast_tests.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core scaffolding needed before implementing user stories

- [ ] T004 Create test sample folder for TriG literals in `src/parser/grammar/test_samples/trig_literals/`
- [ ] T005 [P] Add feature docs cross-link in `specs/001-trig-literal-expression/quickstart.md`
- [ ] T006 Ensure generator entry is available at `src/ast_generator/ast_generator.csproj`

**Checkpoint**: Foundation ready - user story implementation can now begin

---

## Phase 3: User Story 1 - Initialize Store from TriG literal (Priority: P1) 🎯 MVP

**Goal**: Declare and initialize a `Store` from an inline `@< ... >` TriG literal without interpolation.

**Independent Test**: A .5th program with a TriG literal compiles and populates a `Store`.

### Implementation for User Story 1

- [ ] T007 Add `TriGLiteralExpression` node to metamodel in `src/ast-model/AstMetamodel.cs`
- [ ] T008 [P] Regenerate AST artifacts using `src/ast_generator/ast_generator.csproj` (outputs under `src/ast-generated/`)
- [ ] T009 Update lexer to recognize `@<` introducer in `src/parser/grammar/FifthLexer.g4`
- [ ] T010 Add parser rule for TriG literal block in `src/parser/grammar/FifthParser.g4`
- [ ] T011 Build AST from parse tree in `src/parser/AstBuilderVisitor.cs` (construct `TriGLiteralExpression` with raw payload span)
- [ ] T012 Introduce lowering pass `TriGLiteralLoweringRewriter.cs` in `src/compiler/LanguageTransformations/`
- [ ] T013 Register lowering pass in pipeline in `src/compiler/ParserManager.cs`
- [ ] T014 Preserve whitespace/newlines policy in lowering (no trimming) in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T015 Implement minimal runtime load (string → Store) in lowering using existing system hooks in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T016 Wire diagnostic locations to literal span in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T017 Add positive parser sample `.5th` under `src/parser/grammar/test_samples/trig_literals/valid_basic.5th`
- [ ] T018 Add negative parser sample (malformed TriG) under `src/parser/grammar/test_samples/trig_literals/invalid_unbalanced.5th`
- [ ] T019 Add syntax tests referencing samples in `test/syntax-parser-tests/` files
- [ ] T020 Add runtime integration test that asserts triples loaded in `test/runtime-integration-tests/` files
- [ ] T021 Build full solution via `fifthlang.sln`

**Checkpoint**: US1 compiles and runs; Store initialized from literal with correct triples.

---

## Phase 4: User Story 2 - Interpolate expressions into TriG (Priority: P2)

**Goal**: Support `{{ expression }}` interpolation with default type mappings and brace escaping.

**Independent Test**: Program interpolates values of types string/int/float/bool/datetime and produces valid RDF terms.

### Implementation for User Story 2

- [ ] T022 Extend metamodel to include interpolation entries on `TriGLiteralExpression` in `src/ast-model/AstMetamodel.cs`
- [ ] T023 [P] Regenerate AST artifacts using `src/ast_generator/ast_generator.csproj`
- [ ] T024 Extend lexer/parser to recognize `{{ ... }}` and triple-brace escaping in `src/parser/grammar/FifthLexer.g4`
- [ ] T025 Add parser rules to capture interpolation spans and literal text in `src/parser/grammar/FifthParser.g4`
- [ ] T026 Populate interpolation nodes in `src/parser/AstBuilderVisitor.cs`
- [ ] T027 Implement serialization rules in lowering for strings/numbers/bools/datetime in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T028 Implement brace escaping mapping `{{{`→`{{`, `}}}`→`}}` in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T029 Handle IRIs: absolute `<...>` and prefixed names pass-through in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T030 Emit diagnostics for unsupported interpolation values in `src/compiler/LanguageTransformations/TriGLiteralLoweringRewriter.cs`
- [ ] T031 Add positive interpolation sample `.5th` under `src/parser/grammar/test_samples/trig_literals/valid_interpolation_basic.5th`
- [ ] T032 Add syntax tests for interpolation forms in `test/syntax-parser-tests/` files
- [ ] T033 Add runtime integration tests for each base type in `test/runtime-integration-tests/` files
- [ ] T034 Build and run test suites via `fifthlang.sln`

**Checkpoint**: US2 interpolations serialize correctly; tests validate each supported type.

---

## Phase 5: User Story 3 - Robust delimiter handling (Priority: P3)

**Goal**: Ensure nested `<...>` IRIs do not prematurely terminate the literal and diagnostics are precise for delimiter issues.

**Independent Test**: Literals with multiple IRIs and nested blocks parse; unbalanced cases produce targeted diagnostics.

### Implementation for User Story 3

- [ ] T035 Update parser termination logic to top-level `>` matching in `src/parser/grammar/FifthParser.g4`
- [ ] T036 Ensure lexer mode or rule actions do not consume nested `<...>` prematurely in `src/parser/grammar/FifthLexer.g4`
- [ ] T037 Improve diagnostic mapping for unbalanced delimiters in `src/parser/AstBuilderVisitor.cs`
- [ ] T038 Add negative samples for delimiter errors under `src/parser/grammar/test_samples/trig_literals/invalid_*.5th`
- [ ] T039 Add syntax tests asserting diagnostic spans in `test/syntax-parser-tests/` files
- [ ] T040 Build and run full test suite via `fifthlang.sln`

**Checkpoint**: US3 delimiter robustness and diagnostics validated.

---

## Phase N: Polish & Cross-Cutting Concerns

- [ ] T041 [P] Update feature docs with examples in `specs/001-trig-literal-expression/quickstart.md`
- [ ] T042 [P] Performance check with ~100KB literal sample in `src/parser/grammar/test_samples/trig_literals/large_literal.5th`
- [ ] T043 Validate examples via script `scripts/validate-examples.fish`
- [ ] T044 [P] Add developer notes to `docs/knowledge-graphs.md` if needed
- [ ] T045 Final build + full tests via `fifthlang.sln`

---

## Dependencies & Execution Order

### Phase Dependencies

- Setup (Phase 1) → Foundational (Phase 2) → User Stories (Phase 3+)
- User stories proceed in priority order P1 → P2 → P3; after Foundational, different story phases can be staffed in parallel if desired.

### User Story Dependencies

- US1: None beyond foundational
- US2: Depends on US1 metamodel and basic literal support
- US3: Can proceed after US1 grammar structure; largely independent of US2

### Parallel Opportunities

- [P] tasks within different files: regeneration (T008/T023), docs updates, sample additions, and test files can run in parallel.
- Tests for different stories can be implemented independently once their parser hooks exist.

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Setup and Foundational (T001–T006)
2. Implement US1 (T007–T021)
3. Validate US1 independently (build + targeted tests)

### Incremental Delivery

- Add US2, validate interpolation paths
- Add US3, validate robustness and diagnostics

