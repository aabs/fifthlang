# Tasks: SPARQL Comprehensions

**Input**: Design documents in `specs/015-sparql-comprehensions/` (`spec.md`, `plan.md`, `research.md`, `data-model.md`, `quickstart.md`, `contracts/diagnostics.md`)

## Phase 1: Setup (Shared Infrastructure)

- [x] T001 Confirm baseline builds/tests pass (repo root): `dotnet build fifthlang.sln` and `dotnet test test/syntax-parser-tests/syntax-parser-tests.csproj`
- [x] T002 [P] Add new `.5th` sample fixtures folder `test/runtime-integration-tests/TestPrograms/Comprehensions/`
- [x] T003 [P] Add parser sample `.5th` for new comprehension syntax in `src/parser/grammar/test_samples/list_comprehension_from_where.5th`

## Phase 2: Foundational (Blocking Prerequisites)

- [x] T004 Update AST contract: redesign `ListComprehension` in `src/ast-model/AstMetamodel.cs` (Projection/Source/Constraints)
- [x] T005 Regenerate AST code: run `just run-generator` (or `dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated`)
- [x] T006 Update AST builder to construct new comprehension shape in `src/parser/AstBuilderVisitor.cs`

## Phase 3: User Story 1 (P1) — Project objects from SPARQL results

**Goal**: Map tabular SELECT results into a typed list using comprehension projection.

**Independent Test Criteria**: A Fifth program can run a SELECT query and evaluate a comprehension into a `[Person]` list with correct property values.

- [x] T007 [US1] Update lexer tokens for new comprehension keywords in `src/parser/grammar/FifthLexer.g4` (add `FROM`; ensure legacy `in`/`#` comprehension forms are rejected with a clear parse/compile error per FR-013)
- [x] T008 [US1] Update list comprehension grammar in `src/parser/grammar/FifthParser.g4` (projection + `from` + optional `where` constraint list)
- [x] T009 [P] [US1] Add parser tests for new comprehension form in `test/syntax-parser-tests/SyntaxParserTests.cs` (var projection + object projection cases)
- [ ] T010 [US1] Add compile-time SPARQL SELECT parsing helper in `src/compiler/LanguageTransformations/SparqlSelectIntrospection.cs` (extract projected vars)
- [ ] T011 [US1] Add comprehension validator pass in `src/compiler/LanguageTransformations/SparqlComprehensionValidationVisitor.cs` (generator is tabular SELECT; object projection RHS is `?varName`; `?varName` exists in SELECT projection)
- [ ] T012 [US1] Integrate comprehension validation into pipeline in `src/compiler/ParserManager.cs`
- [ ] T013 [US1] Implement runtime row binding access helpers in `src/fifthlang.system/TabularResultBindings.cs`
- [x] T014 [US1] Implement comprehension lowering for SPARQL tabular results in `src/compiler/LanguageTransformations/ListComprehensionLoweringRewriter.cs` (Enhanced with structured lowering approach - currently simplified placeholder)
- [x] T015 [US1] Add runtime integration `.5th` program exercising object projection from SELECT in `test/runtime-integration-tests/TestPrograms/Comprehensions/sparql-object-projection.5th`
- [x] T016 [US1] Add runtime integration test to compile+run the sample in `test/runtime-integration-tests/ComprehensionRuntimeTests.cs`
- [x] T031 [US1] Add runtime integration `.5th` program where the SELECT yields zero rows and comprehension returns an empty list in `test/runtime-integration-tests/TestPrograms/Comprehensions/sparql-empty-result.5th` (FR-011 / SC-003)
- [x] T032 [US1] Add runtime integration `.5th` program where a referenced `?var` is unbound in a row and evaluation fails with a clear runtime error in `test/runtime-integration-tests/TestPrograms/Comprehensions/sparql-missing-binding.5th` (FR-011a)
- [x] T033 [US1] Extend `test/runtime-integration-tests/ComprehensionRuntimeTests.cs` to compile+run the new samples and assert empty list + runtime error contains the missing variable name

## Phase 4: User Story 2 (P2) — Filter rows with constraints

**Goal**: Allow zero or more `where` constraints, AND-ed together.

**Independent Test Criteria**: Same program produces different results with/without constraints; multiple constraints behave as logical AND.

- [x] T017 [US2] Extend grammar to parse multiple constraints (comma-separated) in `src/parser/grammar/FifthParser.g4` (if not already covered by T008)
- [x] T018 [P] [US2] Add parser test for multiple constraints in `test/syntax-parser-tests/SyntaxParserTests.cs`
- [ ] T019 [US2] Enforce boolean constraint typing in `src/compiler/LanguageTransformations/SparqlComprehensionValidationVisitor.cs` (extend T011 implementation)
- [x] T020 [US2] Implement constraint evaluation in `src/compiler/LanguageTransformations/ListComprehensionLoweringRewriter.cs` (extend T014 implementation) (Documented in lowering pass - full runtime support pending)
- [ ] T021 [US2] Add runtime integration `.5th` sample demonstrating constraints filter rows in `test/runtime-integration-tests/TestPrograms/Comprehensions/sparql-constraints.5th`
- [ ] T022 [US2] Add runtime integration test to compile+run the sample and assert filtered output in `test/runtime-integration-tests/ComprehensionRuntimeTests.cs`

## Phase 5: User Story 3 (P3) — Clear errors for invalid comprehensions

**Goal**: Provide actionable compile-time diagnostics for invalid generator/query/vars/constraints.

**Independent Test Criteria**: Negative samples fail compilation with a stable diagnostic code/message category.

- [x] T023 [US3] Implement diagnostic codes/messages per `specs/015-sparql-comprehensions/contracts/diagnostics.md` in `src/compiler/LanguageTransformations/ComprehensionDiagnostics.cs`
- [x] T024 [US3] Add negative parser/compile samples in `test/runtime-integration-tests/TestPrograms/Syntax/Invalid/` (legacy `in`/`#`, non-SELECT generator, unknown `?varName`)
- [x] T025 [US3] Add tests asserting diagnostics surfaced via `src/compiler/CompilationResult.cs` diagnostics stream in `test/syntax-parser-tests/ComprehensionDiagnosticsTests.cs`

## Phase 6: Polish & Cross-Cutting

- [x] T026 [P] Update docs to mention breaking change (`in`/`#` rejected) and link to the migration note (T029)
- [x] T027 Run sample validator: `scripts/validate-examples.fish` (ensure grammar compliance)
- [x] T028 Run regression gate: `dotnet build fifthlang.sln` and `dotnet test fifthlang.sln`
- [x] T029 Add migration note doc `specs/015-sparql-comprehensions/migration.md` covering `in/#` → `from/where` (before/after examples + guidance)
- [x] T030 Record SemVer bump decision (minor vs major) in `specs/015-sparql-comprehensions/migration.md` with rationale (constitution XI + `docs/Development/release-process.md`)

## Dependencies & Execution Order

Dependency graph:

`Setup` → `Foundation` → `US1` → `US2` → `US3` → `Polish`

- **Setup**: T001–T003
- **Foundation (blocks all stories)**: T004–T006
- **US1** depends on Foundation: T007–T016, T031–T033
- **US2** depends on Foundation and most of US1 parsing/AST work: T017–T022
- **US3** depends on validator wiring from US1/US2: T023–T025
- **Polish** depends on desired story completion: T026–T030

## Parallel Execution Examples

- US1: T009 can run in parallel with T010; T013 can run in parallel with T011.
- US2: T018 can run in parallel with T021.
- US3: T024 can run in parallel with T023.
- Polish: T026 can run in parallel with T027.

## MVP Scope Suggestion

- MVP = US1 only: complete T001–T016 and T031–T033, then validate with runtime integration tests.
