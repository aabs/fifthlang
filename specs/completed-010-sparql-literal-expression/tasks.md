# Tasks: Embedded SPARQL Queries

**Feature**: 001-sparql-literal-expression  
**Input**: Design documents from `/specs/001-sparql-literal-expression/`  
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/, quickstart.md

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- File paths are absolute from repository root

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Verify prerequisites and prepare build environment

- [ ] T001 Verify .NET 8.0.118 SDK and Java 17+ installed per global.json
- [ ] T002 [P] Verify dotNetRDF packages available via nuget restore
- [ ] T003 [P] Create test sample directory at src/parser/grammar/test_samples/ (if not exists)
- [ ] T004 Run baseline full build: `dotnet build fifthlang.sln` to establish clean state

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core grammar and AST infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Grammar Foundation

- [ ] T005 Add SPARQL_START token `'?<'` to src/parser/grammar/FifthLexer.g4
- [ ] T006 Add SPARQL_CLOSE_ANGLE token `'>'` to src/parser/grammar/FifthLexer.g4
- [ ] T007 Add lexer mode SparqlMode to src/parser/grammar/FifthLexer.g4 for content between `?<` and `>`
- [ ] T008 Define sparqlLiteral rule in src/parser/grammar/FifthParser.g4: `sparqlLiteral: SPARQL_START sparqlLiteralContent* SPARQL_CLOSE_ANGLE;`
- [ ] T009 Add sparqlLiteral alternative to literal rule in src/parser/grammar/FifthParser.g4

### AST Foundation

- [ ] T010 Define SparqlLiteralExpression record in src/ast-model/AstMetamodel.cs with properties: SparqlText, Bindings, Interpolations, SourceSpan
- [ ] T011 Define VariableBinding record in src/ast-model/AstMetamodel.cs with properties: Name, ResolvedExpression, Type, SpanInLiteral
- [ ] T012 Define Interpolation record in src/ast-model/AstMetamodel.cs with properties: Position, Length, Expression, ResultType
- [ ] T013 Regenerate AST builders and visitors: `just run-generator` or `dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated`
- [ ] T014 Verify generated files include SparqlLiteralExpressionBuilder and visitor methods in src/ast-generated/

### System Type Foundation

- [ ] T015 [P] Create Query.cs in src/fifthlang.system/ with sealed class wrapping VDS.RDF.Query.SparqlQuery
- [ ] T016 [P] Define QueryType enum in src/fifthlang.system/Query.cs: Select, Construct, Ask, Describe, Update
- [ ] T017 [P] Define ParameterInfo record in src/fifthlang.system/Query.cs with Name, FifthType, RdfNodeType
- [ ] T018 [P] Define NodeType enum in src/fifthlang.system/Query.cs: Iri, Literal, BlankNode, Variable

**Checkpoint**: Foundation ready - grammar defined, AST node created, system type scaffolded. User story implementation can now begin.

---

## Phase 3: User Story 1 - Author SPARQL as a literal (Priority: P1) 🎯 MVP

**Goal**: Enable `?< ... >` syntax that compiles to Query type

**Independent Test**: Create a minimal .5th file with `q: Query = ?<SELECT * WHERE { }>;` and verify it compiles without errors and produces correct AST node.

### Grammar Test Samples

- [ ] T019 [P] [US1] Create src/parser/grammar/test_samples/sparql-literal-empty.5th with empty literal: `q: Query = ?<>;`
- [ ] T020 [P] [US1] Create src/parser/grammar/test_samples/sparql-literal-select.5th with basic SELECT query
- [ ] T021 [P] [US1] Create src/parser/grammar/test_samples/sparql-literal-construct.5th with CONSTRUCT query
- [ ] T022 [P] [US1] Create src/parser/grammar/test_samples/sparql-literal-invalid.5th with malformed SPARQL (negative test)

### Parser Tests

- [ ] T023 [P] [US1] Create test/syntax-parser-tests/SparqlLiteralTests.cs test class
- [ ] T024 [US1] Add test ParsesEmptySparqlLiteral in test/syntax-parser-tests/SparqlLiteralTests.cs
- [ ] T025 [US1] Add test ParsesBasicSelectQuery in test/syntax-parser-tests/SparqlLiteralTests.cs
- [ ] T026 [US1] Add test ParsesConstructQuery in test/syntax-parser-tests/SparqlLiteralTests.cs
- [ ] T027 [US1] Add test RejectsMalformedSparql in test/syntax-parser-tests/SparqlLiteralTests.cs

### AST Builder Implementation

- [ ] T028 [US1] Implement VisitSparqlLiteral method in src/parser/AstBuilderVisitor.cs to extract SPARQL text and create SparqlLiteralExpression
- [ ] T029 [US1] Add size limit check (1MB) in VisitSparqlLiteral with diagnostic emission for oversized literals

### AST Tests

- [ ] T030 [P] [US1] Create test/ast-tests/SparqlLiteralAstTests.cs test class
- [ ] T031 [US1] Add test CreatesCorrectAstNodeForEmptyLiteral in test/ast-tests/SparqlLiteralAstTests.cs
- [ ] T032 [US1] Add test CreatesCorrectAstNodeForSelectQuery in test/ast-tests/SparqlLiteralAstTests.cs
- [ ] T033 [US1] Add test PreservesSparqlTextVerbatim in test/ast-tests/SparqlLiteralAstTests.cs
- [ ] T034 [US1] Add test CapturesSourceSpanCorrectly in test/ast-tests/SparqlLiteralAstTests.cs

### Type Checking

- [ ] T035 [US1] Update TypeAnnotationVisitor.cs in src/compiler/LanguageTransformations/ to map SparqlLiteralExpression → Query type
- [ ] T036 [US1] Add test AnnotatesSparqlLiteralExpressionAsQueryType in test/ast-tests/SparqlLiteralAstTests.cs

### Diagnostic Codes Definition

- [ ] T036a [P] [US1] Define SPARQL diagnostic codes in src/compiler/LanguageTransformations/SparqlDiagnostics.cs: SPARQL001 (malformed SPARQL syntax), SPARQL002 (unknown variable), SPARQL003 (incompatible type), SPARQL004 (nested interpolation), SPARQL005 (non-constant interpolation), SPARQL006 (oversized literal)
- [ ] T036b [P] [US1] Document diagnostic format: "SPARQL00X: message text" with line/column offset within literal body

### SPARQL Validation

- [ ] T037 [US1] Create SparqlValidationVisitor.cs in src/compiler/LanguageTransformations/ to parse SPARQL text via dotNetRDF
- [ ] T038 [US1] Implement error diagnostic emission using SPARQL001 code for invalid SPARQL syntax with line/column offset within literal
- [ ] T039 [US1] Add SparqlValidationVisitor to transformation pipeline in src/compiler/ParserManager.cs after TypeAnnotationVisitor
- [ ] T040 [US1] Add test EmitsDiagnosticForInvalidSparql in test/ast-tests/SparqlLiteralAstTests.cs verifying SPARQL001 code
- [ ] T041 [US1] Add test ReportsCorrectLocationForSparqlError in test/ast-tests/SparqlLiteralAstTests.cs

### Integration Tests

- [ ] T042 [P] [US1] Create test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs test class
- [ ] T043 [US1] Add test CompilesEmptyLiteralToQueryInstance in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T044 [US1] Add test CompilesBasicSelectToQueryInstance in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T045 [US1] Add test QueryTypeIsCorrectForSelect in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T046 [US1] Add test QueryTypeIsCorrectForConstruct in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs

**Checkpoint**: At this point, User Story 1 should be fully functional - SPARQL literals parse, type-check, and compile to Query instances.

---

## Phase 4: User Story 2 - Bind variables via parameters (Priority: P1) 🎯 MVP

**Goal**: Enable safe variable binding via parameter insertion (no string concatenation)

**Independent Test**: Create a .5th file with `age: int = 42; q: Query = ?<SELECT * WHERE { ?s ex:age age }>;` and verify the bound parameter exists and compiles without diagnostic errors.

### Variable Resolution Visitor

- [ ] T047 [P] [US2] Create SparqlVariableBindingVisitor.cs in src/compiler/LanguageTransformations/ extending BaseAstVisitor
- [ ] T048 [US2] Implement VisitSparqlLiteralExpression in SparqlVariableBindingVisitor.cs to scan SPARQL text for identifiers
- [ ] T049 [US2] Implement identifier-to-symbol resolution against symbol table in SparqlVariableBindingVisitor.cs
- [ ] T050 [US2] Populate Bindings list with resolved VariableBinding instances in SparqlVariableBindingVisitor.cs
- [ ] T051 [US2] Emit diagnostic SPARQL002 for unknown variables in SparqlVariableBindingVisitor.cs with format "SPARQL002: Unknown variable 'name' in SPARQL literal"
- [ ] T052 [US2] Add SparqlVariableBindingVisitor to transformation pipeline in src/compiler/ParserManager.cs after SymbolTableBuilderVisitor

### Test Samples

- [ ] T053 [P] [US2] Create src/parser/grammar/test_samples/sparql-literal-binding.5th with variable references
- [ ] T054 [P] [US2] Create src/parser/grammar/test_samples/sparql-literal-unknown-var.5th with unknown variable (negative test)
- [ ] T055 [P] [US2] Create src/parser/grammar/test_samples/sparql-literal-multiple-bindings.5th with multiple variables

### Variable Binding Tests

- [ ] T056 [P] [US2] Add test ResolvesInScopeVariable in test/ast-tests/SparqlLiteralAstTests.cs
- [ ] T057 [P] [US2] Add test EmitsDiagnosticForUnknownVariable in test/ast-tests/SparqlLiteralAstTests.cs
- [ ] T058 [P] [US2] Add test ResolvesMultipleVariables in test/ast-tests/SparqlLiteralAstTests.cs
- [ ] T059 [P] [US2] Add test HandlesVariableShadowing in test/ast-tests/SparqlLiteralAstTests.cs

### Lowering Rewriter

- [ ] T060 [US2] Create SparqlLoweringRewriter.cs in src/compiler/LanguageTransformations/ extending DefaultAstRewriter
- [ ] T061 [US2] Implement VisitSparqlLiteralExpression in SparqlLoweringRewriter.cs to emit query construction code
- [ ] T062 [US2] Generate SparqlParameterizedString construction in SparqlLoweringRewriter.cs
- [ ] T063 [US2] Generate SetLiteral calls for each binding in SparqlLoweringRewriter.cs with type mapping (int→xsd:integer, string→xsd:string, float→xsd:float, double→xsd:double, bool→xsd:boolean, decimal→xsd:decimal)
- [ ] T064 [US2] Generate Query constructor call with parameters dictionary in SparqlLoweringRewriter.cs
- [ ] T065 [US2] Add SparqlLoweringRewriter to transformation pipeline in src/compiler/ParserManager.cs after SparqlValidationVisitor

### Type Compatibility Validation

- [ ] T066 [US2] Implement type compatibility check in TypeAnnotationVisitor.cs for bound variables (int, string, float, etc.)
- [ ] T067 [US2] Emit diagnostic SPARQL003 for incompatible types (Graph, Triple) in TypeAnnotationVisitor.cs with format "SPARQL003: Type 'typename' cannot be bound to SPARQL parameter"
- [ ] T068 [US2] Add test EmitsDiagnosticForIncompatibleType in test/ast-tests/SparqlLiteralAstTests.cs

### Integration Tests

- [ ] T069 [P] [US2] Add test CompilesLiteralWithIntBinding in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T070 [P] [US2] Add test CompilesLiteralWithStringBinding in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T071 [P] [US2] Add test CompilesLiteralWithMultipleBindings in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T072 [P] [US2] Add test BoundParametersHaveCorrectTypes in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T073 [P] [US2] Add test BoundParametersHaveCorrectValues in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T073a [P] [US2] Create comprehensive binding test matrix covering int/string/float/double/bool/decimal types × direct-scope/nested-scope/shadowed scenarios (18 test cases total) to validate SC-003 (95%+ binding coverage)

### Security Tests

- [ ] T074 [P] [US2] Add test PreventsSqlInjectionViaBinding in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T075 [P] [US2] Add test PreventsSparqlInjectionViaBinding in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T076 [P] [US2] Add test NoRawStringConcatenationInLoweredCode in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - literals compile with safe variable binding.

---

## Phase 5: User Story 3 - Interpolation placeholders (Priority: P1) 🎯 MVP

**Goal**: Enable `{{expr}}` syntax for computed value injection

**Independent Test**: Create a .5th file with `age: int = 42; q: Query = ?<SELECT * WHERE { ?s ex:age {{age}} }>;` and verify the resulting query is valid and safe.

**Note**: This phase is now mandatory for MVP as interpolation provides essential computed value support.

### Lexer Extension

- [ ] T077 [P] [US3] Add SPARQL_INTERPOLATION_START token `'{{'` to SparqlMode in src/parser/grammar/FifthLexer.g4
- [ ] T078 [P] [US3] Add SPARQL_INTERPOLATION_END token `'}}'` to SparqlMode in src/parser/grammar/FifthLexer.g4
- [ ] T079 [US3] Update sparqlLiteral rule in src/parser/grammar/FifthParser.g4 to recognize interpolation syntax

### Parser Extension

- [ ] T080 [US3] Update VisitSparqlLiteral in src/parser/AstBuilderVisitor.cs to extract interpolation expressions
- [ ] T081 [US3] Populate Interpolations list in SparqlLiteralExpression during parsing
- [ ] T082 [US3] Add validation for nested interpolation (emit diagnostic SPARQL004 with format "SPARQL004: Nested interpolation not allowed")

### Test Samples

- [ ] T083 [P] [US3] Create src/parser/grammar/test_samples/sparql-literal-interpolation.5th with {{expr}} syntax
- [ ] T084 [P] [US3] Create src/parser/grammar/test_samples/sparql-literal-nested-interpolation.5th (negative test)

### Interpolation Tests

- [ ] T085 [P] [US3] Add test ParsesInterpolationExpression in test/syntax-parser-tests/SparqlLiteralTests.cs
- [ ] T086 [P] [US3] Add test RejectsNestedInterpolation in test/syntax-parser-tests/SparqlLiteralTests.cs
- [ ] T087 [P] [US3] Add test InterpolationCapturesPosition in test/ast-tests/SparqlLiteralAstTests.cs

### Lowering Extension

- [ ] T088 [US3] Extend SparqlLoweringRewriter.cs to handle interpolation expressions
- [ ] T089 [US3] Implement safe serialization for interpolated IRIs in SparqlLoweringRewriter.cs
- [ ] T090 [US3] Implement safe serialization for interpolated literals in SparqlLoweringRewriter.cs
- [ ] T091 [US3] Add IRI syntax validation for interpolated values in SparqlLoweringRewriter.cs

### Type Checking Extension

- [ ] T092 [US3] Extend TypeAnnotationVisitor.cs to infer types for interpolation expressions
- [ ] T093 [US3] Emit diagnostic SPARQL005 for non-constant interpolation expressions with format "SPARQL005: Interpolation expression must be compile-time constant"
- [ ] T094 [US3] Add test InterpolationTypesInferredCorrectly in test/ast-tests/SparqlLiteralAstTests.cs

### Integration Tests

- [ ] T095 [P] [US3] Add test CompilesLiteralWithInterpolation in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T096 [P] [US3] Add test InterpolatedIriIsValidSyntax in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T097 [P] [US3] Add test InterpolatedLiteralIsSafelySerialized in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs

**Checkpoint**: All user stories (US1, US2, US3) should now be independently functional - literals with interpolation compile safely. MVP is complete.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Documentation, validation, and refinements across all user stories

### Documentation

- [ ] T098 [P] Update docs/knowledge-graphs.md to include SPARQL literal syntax and examples
- [ ] T099 [P] Add SPARQL literal examples to docs/examples/learnfifth.5th
- [ ] T100 [P] Verify quickstart.md examples compile successfully

### Edge Cases

- [ ] T101 [P] Add test EmitsDiagnosticForOversizedLiteral (>1MB) in test/ast-tests/SparqlLiteralAstTests.cs verifying SPARQL006 code with format "SPARQL006: SPARQL literal exceeds 1MB size limit; consider external file"
- [ ] T102 [P] Add test HandlesEmptyLiteralCorrectly in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs
- [ ] T103 [P] Add test HandlesMultilineQueriesWithWhitespace in test/runtime-integration-tests/SparqlLiteralIntegrationTests.cs

### Performance Validation

- [ ] T104 [P] Add performance test for large literal (<5KB, <50ms overhead) in test/runtime-integration-tests/ (baseline: GitHub Actions standard runner, 2-core, 7GB RAM)
- [ ] T105 [P] Add performance test for many bindings (>10 variables) in test/runtime-integration-tests/ (baseline: GitHub Actions standard runner)

### Build Validation

- [ ] T106 Run full solution build: `dotnet build fifthlang.sln` and verify <2 minutes total time
- [ ] T107 Run all tests: `dotnet test fifthlang.sln` and verify all pass
- [ ] T108 Verify no hand-edited files in src/ast-generated/ (check git diff)

### Constitution Compliance

- [ ] T109 Verify grammar changes documented in AGENTS.md or constitution
- [ ] T110 Verify AST metamodel changes follow partial record pattern
- [ ] T111 Verify system type follows sealed class pattern
- [ ] T112 Verify transformation visitors follow single-responsibility principle

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - **BLOCKS all user stories**
- **User Stories (Phase 3-5)**: All depend on Foundational phase completion
  - US1 (P1): Can start immediately after Foundational
  - US2 (P1): Can start after Foundational; depends on US1 grammar/AST but can develop in parallel
  - US3 (P2): Can start after Foundational; optional for MVP
- **Polish (Phase 6)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Independent - only depends on Foundational
- **User Story 2 (P1)**: Builds on US1 grammar/AST but independently testable
- **User Story 3 (P2)**: Optional; builds on US1/US2 but independently testable

### Within Each Phase

**Phase 2 (Foundational)**:
- Grammar tasks (T005-T009) must complete before AST tasks (T010-T014)
- AST tasks must complete before T013 (regeneration)
- System type tasks (T015-T018) can proceed in parallel with grammar/AST

**Phase 3 (US1)**:
- Grammar samples (T019-T022) can proceed in parallel
- Parser tests (T023-T027) can proceed in parallel after samples exist
- AST builder (T028-T029) must complete before AST tests (T030-T034)
- Type checking (T035-T036) can proceed after AST builder
- SPARQL validation (T037-T041) requires dotNetRDF integration
- Integration tests (T042-T046) require all above to be complete

**Phase 4 (US2)**:
- Variable binding visitor (T047-T052) is core dependency for all US2 tasks
- Test samples (T053-T055) can proceed in parallel
- Binding tests (T056-T059) can proceed in parallel after visitor exists
- Lowering rewriter (T060-T065) requires visitor to be complete
- Type compatibility (T066-T068) can proceed after visitor exists
- Integration tests (T069-T073) require lowering to be complete
- Security tests (T074-T076) require lowering to be complete

**Phase 5 (US3)**:
- Lexer/parser extension (T077-T082) must complete first
- Test samples (T083-T084) can proceed in parallel after parser extension
- Tests (T085-T087) can proceed after samples exist
- Lowering/type checking extensions (T088-T094) require parser extension
- Integration tests (T095-T097) require all above to be complete

### Parallel Opportunities

**Phase 1 (Setup)**: T002 and T003 can run in parallel

**Phase 2 (Foundational)**:
- Within grammar: T005-T009 are sequential
- System type tasks: T015-T018 can all run in parallel
- Grammar and system type phases can proceed in parallel

**Phase 3 (US1)**:
- Grammar samples: T019-T022 all parallel
- Parser tests: T023-T027 all parallel (after samples)
- AST tests: T030-T034 all parallel (after builder)
- Integration tests: T042-T046 all parallel (after pipeline)

**Phase 4 (US2)**:
- Test samples: T053-T055 all parallel
- Binding tests: T056-T059 all parallel
- Integration tests: T069-T073 all parallel
- Security tests: T074-T076 all parallel

**Phase 5 (US3)**:
- Lexer tokens: T077-T078 parallel
- Test samples: T083-T084 parallel
- Tests: T085-T087 parallel
- Integration tests: T095-T097 parallel

**Phase 6 (Polish)**:
- Documentation: T098-T100 all parallel
- Edge cases: T101-T103 all parallel
- Performance: T104-T105 parallel
- Constitution checks: T109-T112 parallel

---

## Parallel Example: User Story 1 (Phase 3)

```bash
# After T028-T029 (AST builder) complete, launch all AST tests together:
Task T030: "CreatesCorrectAstNodeForEmptyLiteral"
Task T031: "CreatesCorrectAstNodeForSelectQuery"
Task T032: "PreservesSparqlTextVerbatim"
Task T033: "CapturesSourceSpanCorrectly"

# After full pipeline (T037-T039) complete, launch all integration tests together:
Task T042: "CompilesEmptyLiteralToQueryInstance"
Task T043: "CompilesBasicSelectToQueryInstance"
Task T044: "QueryTypeIsCorrectForSelect"
Task T045: "QueryTypeIsCorrectForConstruct"
```

---

## Implementation Strategy

### MVP First (User Stories 1 + 2 Only)

1. Complete Phase 1: Setup (~30 minutes)
2. Complete Phase 2: Foundational (~4-6 hours) - **CRITICAL - blocks all stories**
3. Complete Phase 3: User Story 1 (~6-8 hours) - Basic literal parsing
4. Complete Phase 4: User Story 2 (~8-10 hours) - Variable binding
5. **STOP and VALIDATE**: Test US1+US2 independently
6. Deploy/demo if ready (functional SPARQL literals with safe binding)

**Estimated MVP Time**: 2-3 days for core functionality

### Incremental Delivery

1. Complete Setup + Foundational (foundation ready) - **Day 1**
2. Add User Story 1 → Test independently → Demo (literals parse!) - **Day 2**
3. Add User Story 2 → Test independently → Deploy (binding works!) - **Day 3**
4. Add User Story 3 → Test independently → Deploy (interpolation!) - **Optional Day 4**
5. Polish phase → Final validation - **Day 4-5**

### Parallel Team Strategy

With multiple developers (after Foundational phase complete):

1. **Developer A**: User Story 1 (grammar samples, parser tests, AST builder, type checking)
2. **Developer B**: User Story 2 (variable binding visitor, lowering rewriter, security tests)
3. **Developer C**: System type + documentation (Query.cs refinements, docs updates, quickstart validation)

After each story completes, developers can assist with integration testing and polish.

---

## Task Summary

- **Total Tasks**: 115 (updated from 112)
- **Setup Tasks**: 4
- **Foundational Tasks**: 14 (BLOCKING)
- **User Story 1 Tasks**: 30 (P1 - MVP critical, includes diagnostic definition tasks)
- **User Story 2 Tasks**: 31 (P1 - MVP critical, includes comprehensive binding matrix)
- **User Story 3 Tasks**: 21 (P1 - MVP critical, interpolation is mandatory)
- **Polish Tasks**: 15

**MVP Scope**: All phases 1-5 (US1 + US2 + US3) = 100 tasks
**Full Feature Scope**: All phases = 115 tasks

**Parallel Opportunities Identified**: 45 tasks marked [P] can run in parallel within their phase

**Independent Test Criteria**:
- **US1**: Create minimal .5th with literal, verify compilation and AST node
- **US2**: Create .5th with variable binding, verify parameter exists and no diagnostics
- **US3**: Create .5th with interpolation, verify safe serialization

**Suggested MVP**: Complete Setup + Foundational + US1 + US2, then validate independently before proceeding to US3 or polish.

---

## Notes

- [P] tasks = different files, no dependencies, can run in parallel
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Tests are written FIRST and should FAIL before implementation begins
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Follow TDD: Red (test fails) → Green (test passes) → Refactor
- Constitution compliance validated in polish phase
