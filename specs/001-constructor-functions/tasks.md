---
description: "Task list for Constructor Functions feature implementation"
---

# Tasks: Constructor Functions

**Input**: Design documents from `/specs/001-constructor-functions/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: This feature requires comprehensive test coverage before implementation (TDD approach).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

This is a compiler feature spanning multiple projects in the Fifth language monorepo:
- `src/ast-model/` - AST metamodel definitions
- `src/ast-generated/` - Auto-generated AST builders/visitors (NEVER hand-edit)
- `src/parser/grammar/` - ANTLR grammar files
- `src/parser/` - Parser and AST builder visitor
- `src/compiler/LanguageTransformations/` - AST transformation passes
- `test/syntax-parser-tests/` - Parser tests
- `test/runtime-integration-tests/` - End-to-end runtime tests
- `test/ast-tests/` - AST-level tests

---

## Phase 1: Setup (Shared Infrastructure) ✅ COMPLETE

**Purpose**: Project initialization and basic structure

- [ ] T001 Update lowering.md contract with constructor synthesis preconditions in specs/001-constructor-functions/contracts/lowering.md
- [x] T002 [P] Create sample .5th files demonstrating constructor forms in src/parser/grammar/test_samples/constructors/ (commit e8daff9)
- [x] T003 [P] Add negative test samples for invalid constructor patterns in src/parser/grammar/test_samples/constructors/invalid/ (commit e8daff9)

---

## Phase 2: Foundational (Blocking Prerequisites) ✅ COMPLETE

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Foundational Tests (Write FIRST - Must FAIL before implementation)

- [x] T003a [P] Parser test expecting failure for minimal constructor syntax in test/syntax-parser-tests/ConstructorParsingTests.cs (commit 5966414)
- [x] T003b [P] Parser test expecting failure for base constructor call in test/syntax-parser-tests/ConstructorParsingTests.cs (commit 5966414)
- [x] T003c [P] Verify tests fail due to missing grammar/metamodel support (commit 5966414)

### Foundational Implementation (After tests fail)

- [x] T004 Add ConstructorDef to AstMetamodel.cs in src/ast-model/AstMetamodel.cs (reused FunctionDef with IsConstructor flag, commit e8daff9)
- [x] T005 Add BaseConstructorCall to AstMetamodel.cs in src/ast-model/AstMetamodel.cs (commit e8daff9)
- [ ] T006 Add InstantiationExpression updates to AstMetamodel.cs in src/ast-model/AstMetamodel.cs (deferred - needs new keyword support)
- [x] T007 Regenerate AST builders and visitors via just run-generator (commit e8daff9)
- [x] T008 Add constructor syntax rule to FifthParser.g4 in src/parser/grammar/FifthParser.g4 (commit 5966414)
- [x] T009 Update FifthLexer.g4 if new keywords needed in src/parser/grammar/FifthLexer.g4 (BASE keyword added, commit 5966414)
- [x] T010 Implement VisitConstructorDeclaration in AstBuilderVisitor.cs in src/parser/AstBuilderVisitor.cs (commit 5966414)
- [x] T011 Implement VisitBaseConstructorCall in AstBuilderVisitor.cs in src/parser/AstBuilderVisitor.cs (commit e8daff9)
- [x] T012 Create diagnostic codes CTOR001-CTOR010 in src/compiler/Diagnostics/ following existing pattern (commit e8daff9)
- [x] T013 Build solution to validate metamodel and grammar changes: dotnet build fifthlang.sln (validated across all commits)

### Additional Foundational Tests (Value Return Prohibition)

- [ ] T013a [P] Parser negative test for `return expr;` in constructor in test/syntax-parser-tests/ConstructorParsingTests.cs (deferred - parser allows, semantic check catches)
- [ ] T013b [P] Semantic test expecting CTOR009 for value return in test/runtime-integration-tests/ConstructorDiagnosticTests.cs (deferred - needs runtime support)

### Additional Foundational Tests (Parameter Shadowing)

- [ ] T013c [P] Parser test for shadowed parameter with/without this qualification in test/syntax-parser-tests/ConstructorParsingTests.cs (deferred)
- [ ] T013d [P] Semantic test verifying field uninitialized when unqualified in test/ast-tests/ShadowingTests.cs (new file) (deferred)

### Additional Foundational Tests (Forbidden Modifiers)

- [ ] T013e [P] Parser negative test for async constructor in test/syntax-parser-tests/ConstructorParsingTests.cs (deferred - async not yet in metamodel)
- [x] T013f [P] Parser negative test for static constructor in test/syntax-parser-tests/ConstructorParsingTests.cs (unit test in a80c8bb)
- [ ] T013g [P] Semantic test expecting CTOR010 for each forbidden modifier in test/runtime-integration-tests/ConstructorDiagnosticTests.cs (deferred - needs runtime support)

### Foundational Validation (Verify tests now PASS)

- [x] T013h Verify T003a-T003c now pass after implementation (all 6 parser tests passing)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel ✅

---

## Phase 3: User Story 1 - Basic Explicit Construction (Priority: P1) 🎯 MVP - PARTIAL ⚠️

**Goal**: Enable developers to define a constructor to initialize mandatory fields with explicit and safe object creation.

**Independent Test**: Create a class with two required fields, define one constructor assigning both, instantiate, verify no diagnostics and values set.

### Tests for User Story 1

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [x] T014 [P] [US1] Parser test for basic constructor syntax in test/syntax-parser-tests/ConstructorParsingTests.cs (commit 5966414 - 6 tests)
- [x] T015 [P] [US1] Parser test for constructor with multiple parameters in test/syntax-parser-tests/ConstructorParsingTests.cs (commit 5966414)
- [ ] T016 [P] [US1] Runtime test for simple constructor instantiation in test/runtime-integration-tests/BasicConstructorTests.cs (BLOCKED: needs code generation)
- [ ] T017 [P] [US1] Runtime test verifying field values after construction in test/runtime-integration-tests/BasicConstructorTests.cs (BLOCKED: needs code generation)

### Implementation for User Story 1

- [x] T018 [US1] Implement ClassCtorInserter synthesis logic in src/compiler/LanguageTransformations/ClassCtorInserter.cs (commit 27c0eff)
- [x] T019 [US1] Add constructor synthesis detection for classes without explicit constructors in src/compiler/LanguageTransformations/ClassCtorInserter.cs (commit 27c0eff)
- [ ] T020 [US1] Implement basic constructor overload resolution in src/compiler/SemanticAnalysis/ConstructorResolver.cs (new file) (BLOCKED: needs symbol table - Phase 5)
- [ ] T021 [US1] Add constructor resolution to InstantiationExpression in src/compiler/SemanticAnalysis/ConstructorResolver.cs (BLOCKED: needs symbol table - Phase 5)
- [ ] T022 [US1] Emit CTOR001 diagnostic for no matching constructor in src/compiler/SemanticAnalysis/ConstructorResolver.cs (BLOCKED: needs symbol table - Phase 5)
- [x] T023 [US1] Emit CTOR005 diagnostic when synthesis not possible in src/compiler/LanguageTransformations/ClassCtorInserter.cs (commit 27c0eff)
- [x] T024 [US1] Update ParserManager.cs pipeline to include constructor resolution pass in src/compiler/ParserManager.cs (commit 27c0eff, b97a3f4, ce0e396 - TODO added)
- [x] T025 [US1] Validate tests pass: dotnet test test/syntax-parser-tests/ --filter FullyQualifiedName~BasicConstructor (all passing)
- [ ] T026 [US1] Validate runtime tests pass: dotnet test test/runtime-integration-tests/ --filter FullyQualifiedName~BasicConstructor (BLOCKED: needs code generation)
- [x] T026a [US1] Implement CTOR009 emission for value return in src/compiler/SemanticAnalysis/ConstructorValidator.cs (new file) (commit b97a3f4)
- [x] T026b [US1] Implement CTOR010 emission for forbidden modifiers in src/compiler/SemanticAnalysis/ConstructorValidator.cs (commit b97a3f4)
- [x] T026c [US1] Implement `new` keyword support for object instantiation (commit 018d251)
- [x] T026d [US1] Create ConstructorResolver stub with TODO for Phase 5 integration (commit ce0e396)

**Status**: Constructor synthesis, validation, and `new` keyword support complete. Constructor resolution stub created with documented dependency on symbol table infrastructure (Phase 5). Runtime tests blocked by missing code generation/lowering.

**Blockers**: 
- Constructor overload resolution requires symbol table for class lookup (Phase 5 dependency)
- Runtime integration tests require lowering and code generation implementation

**Checkpoint**: Parser tests passing (12/12), synthesis and validation working (5/5 tests). ConstructorResolver stub in place with clear path forward once symbol table is available.

---

## Phase 4: User Story 2 - Field Safety & Definite Assignment (Priority: P1) - DEFERRED ⏸️

**Goal**: Ensure all required fields are assigned before construction completes with clear diagnostics.

**Independent Test**: Define a constructor omitting one required field assignment; expect a definite assignment diagnostic listing missing fields.

**Status**: DEFERRED - Depends on completed Phase 3 (constructor resolution) and Phase 5 (symbol table). Definite assignment analysis requires:
1. Resolved constructor calls to know which constructor is being invoked
2. Symbol table to look up field definitions and their types
3. Control flow graph (CFG) analysis infrastructure

### Tests for User Story 2

- [ ] T027 [P] [US2] Parser test for constructor with missing field assignment in test/syntax-parser-tests/ConstructorParsingTests.cs (DEFERRED)
- [ ] T028 [P] [US2] AST test for RequiredFieldSet computation in test/ast-tests/DefiniteAssignmentTests.cs (new file) (DEFERRED)
- [ ] T029 [P] [US2] AST test for definite assignment analysis in test/ast-tests/DefiniteAssignmentTests.cs (DEFERRED)
- [ ] T030 [P] [US2] Runtime test expecting CTOR003 diagnostic for unassigned field in test/runtime-integration-tests/DefiniteAssignmentTests.cs (new file) (DEFERRED)

### Implementation for User Story 2

- [ ] T031 [US2] Create RequiredFieldSet analysis data structure in src/compiler/SemanticAnalysis/RequiredFieldSet.cs (new file) (DEFERRED)
- [ ] T032 [US2] Implement field requirement detection (non-nullable, no default) in src/compiler/SemanticAnalysis/RequiredFieldSet.cs (DEFERRED)
- [ ] T033 [US2] Implement single-pass forward CFG data-flow analysis in src/compiler/SemanticAnalysis/DefiniteAssignmentAnalyzer.cs (new file) (DEFERRED)
- [ ] T034 [US2] Add conservative merging at control flow join points in src/compiler/SemanticAnalysis/DefiniteAssignmentAnalyzer.cs (DEFERRED)
- [ ] T035 [US2] Emit CTOR003 diagnostic with missing field list in src/compiler/SemanticAnalysis/DefiniteAssignmentAnalyzer.cs (DEFERRED)
- [ ] T036 [US2] Integrate definite assignment pass into ParserManager.cs pipeline in src/compiler/ParserManager.cs (DEFERRED)
- [ ] T037 [US2] Validate tests pass: dotnet test test/ast-tests/ --filter FullyQualifiedName~DefiniteAssignment (DEFERRED)
- [ ] T038 [US2] Validate runtime tests pass: dotnet test test/runtime-integration-tests/ --filter FullyQualifiedName~DefiniteAssignment (DEFERRED)

**Dependency Chain**: Phase 3 completion → Phase 5 (symbol table) → Phase 4 (definite assignment)

**Checkpoint**: User Story 2 cannot be completed until User Story 1 is fully implemented with constructor resolution working

---

## Phase 5: User Story 3 - Overload Resolution (Priority: P2)

**Goal**: Enable multiple constructor signatures with precise, unambiguous selection on `new` calls.

**Independent Test**: Define two overloads differing by parameter types; call each and verify correct resolution; add a third causing ambiguity to trigger an error.

### Tests for User Story 3

- [ ] T039 [P] [US3] Parser test for multiple constructor overloads in test/syntax-parser-tests/ConstructorParsingTests.cs
- [ ] T040 [P] [US3] Runtime test for exact match overload selection in test/runtime-integration-tests/OverloadResolutionTests.cs (new file)
- [ ] T041 [P] [US3] Runtime test for convertible parameter overload selection in test/runtime-integration-tests/OverloadResolutionTests.cs
- [ ] T042 [P] [US3] Runtime test for ambiguous overload producing CTOR002 in test/runtime-integration-tests/OverloadResolutionTests.cs
- [ ] T043 [P] [US3] Runtime test for duplicate signature producing CTOR006 in test/runtime-integration-tests/OverloadResolutionTests.cs

### Implementation for User Story 3

- [ ] T044 [US3] Implement overload ranking (exact > convertible > widening) in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T045 [US3] Implement arity pre-filtering in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T046 [US3] Add parameter type hash for fast lookup in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T047 [US3] Implement ambiguity detection (>1 best rank) in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T048 [US3] Emit CTOR002 diagnostic with candidate signatures sorted lexicographically in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T049 [US3] Implement duplicate signature detection in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T050 [US3] Emit CTOR006 diagnostic for duplicate constructors in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T051 [US3] Add LRU cache (5000 entries) for resolved constructors in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T052 [US3] Validate tests pass: dotnet test test/runtime-integration-tests/ --filter FullyQualifiedName~OverloadResolution

**Checkpoint**: All overload resolution scenarios should now work correctly

---

## Phase 6: User Story 4 - Inheritance Base Chaining (Priority: P2)

**Goal**: Enable derived classes to initialize base state explicitly when required.

**Independent Test**: Base class with parameterized constructor; derived class without base call should fail; add base call; succeeds.

### Tests for User Story 4

- [ ] T053 [P] [US4] Parser test for base constructor call syntax in test/syntax-parser-tests/ConstructorParsingTests.cs
- [ ] T054 [P] [US4] Runtime test for successful base chaining in test/runtime-integration-tests/InheritanceConstructorTests.cs (new file)
- [ ] T055 [P] [US4] Runtime test for missing base call producing CTOR004 in test/runtime-integration-tests/InheritanceConstructorTests.cs
- [ ] T056 [P] [US4] Runtime test for inheritance cycle producing CTOR008 in test/runtime-integration-tests/InheritanceConstructorTests.cs

### Implementation for User Story 4

- [ ] T057 [US4] Implement base constructor requirement detection in src/compiler/SemanticAnalysis/BaseConstructorValidator.cs (new file)
- [ ] T058 [US4] Add parameterless base constructor detection in src/compiler/SemanticAnalysis/BaseConstructorValidator.cs
- [ ] T059 [US4] Emit CTOR004 diagnostic when base call missing in src/compiler/SemanticAnalysis/BaseConstructorValidator.cs
- [ ] T060 [US4] Implement inheritance cycle detection via DFS ancestry stack in src/compiler/SemanticAnalysis/BaseConstructorValidator.cs
- [ ] T061 [US4] Emit CTOR008 diagnostic with cycle path in src/compiler/SemanticAnalysis/BaseConstructorValidator.cs
- [ ] T062 [US4] Integrate base constructor validation into ParserManager.cs pipeline in src/compiler/ParserManager.cs
- [ ] T063 [US4] Validate tests pass: dotnet test test/runtime-integration-tests/ --filter FullyQualifiedName~InheritanceConstructor

**Checkpoint**: Inheritance scenarios should now be properly validated

---

## Phase 7: User Story 5 - Generic Class Construction (Priority: P3)

**Goal**: Enable instantiation of generic classes with constructors referencing class type parameters.

**Independent Test**: Generic class `Box<T>` with constructor using `T`; instantiate with different concrete types; verify type-correct initialization.

### Tests for User Story 5

- [ ] T064 [P] [US5] Parser test for generic class constructor in test/syntax-parser-tests/ConstructorParsingTests.cs
- [ ] T065 [P] [US5] Runtime test for generic constructor with int in test/runtime-integration-tests/GenericConstructorTests.cs (new file)
- [ ] T066 [P] [US5] Runtime test for generic constructor with string in test/runtime-integration-tests/GenericConstructorTests.cs
- [ ] T067 [P] [US5] Runtime test for generic constructor with null unconstrained parameter in test/runtime-integration-tests/GenericConstructorTests.cs

### Implementation for User Story 5

- [ ] T068 [US5] Implement type parameter substitution for constructor parameters in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T069 [US5] Validate constructor parameter types reference only class-level type parameters in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T070 [US5] Emit CTOR007 diagnostic for invalid constructor-level type parameters in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T071 [US5] Update overload resolution to handle generic parameter substitution in src/compiler/SemanticAnalysis/ConstructorResolver.cs
- [ ] T072 [US5] Validate tests pass: dotnet test test/runtime-integration-tests/ --filter FullyQualifiedName~GenericConstructor

### Additional Generic Constructor Tests

- [ ] T072a [P] [US5] Semantic test verifying type parameters bound before field type resolution in test/ast-tests/TypeBindingTests.cs (new file)
- [ ] T072b [P] [US5] Integration test for generic constructor accessing generic field types in test/runtime-integration-tests/GenericConstructorTests.cs

**Checkpoint**: Generic class constructors should work correctly with type substitution

---

## Phase 8: Lowering & Code Generation

**Purpose**: Transform high-level constructor calls into allocation + initialization sequences

### Tests for Lowering

- [ ] T073 [P] AST test for lowering InstantiationExpression in test/ast-tests/ConstructorLoweringTests.cs (new file)
- [ ] T074 [P] AST test for lowering with base call in test/ast-tests/ConstructorLoweringTests.cs
- [ ] T075 [P] Integration test for lowered code execution in test/runtime-integration-tests/LoweredConstructorTests.cs (new file)

### Implementation for Lowering

- [ ] T076 Create ConstructorLoweringRewriter extending DefaultAstRewriter in src/compiler/LanguageTransformations/ConstructorLoweringRewriter.cs (new file)
- [ ] T077 Implement VisitInstantiationExpression lowering logic in src/compiler/LanguageTransformations/ConstructorLoweringRewriter.cs
- [ ] T078 Generate allocation temp statement in src/compiler/LanguageTransformations/ConstructorLoweringRewriter.cs
- [ ] T079 Inline field default assignments in src/compiler/LanguageTransformations/ConstructorLoweringRewriter.cs
- [ ] T080 Add base constructor invocation if present in src/compiler/LanguageTransformations/ConstructorLoweringRewriter.cs
- [ ] T081 Inline constructor body statements in src/compiler/LanguageTransformations/ConstructorLoweringRewriter.cs
- [ ] T082 Generate definite assignment validation if needed in src/compiler/LanguageTransformations/ConstructorLoweringRewriter.cs
- [ ] T083 Replace InstantiationExpression with temp reference in src/compiler/LanguageTransformations/ConstructorLoweringRewriter.cs
- [ ] T084 Integrate ConstructorLoweringRewriter into ParserManager.cs pipeline in src/compiler/ParserManager.cs
- [ ] T085 Validate lowering tests pass: dotnet test test/ast-tests/ --filter FullyQualifiedName~ConstructorLowering
- [ ] T086 Validate integration tests pass: dotnet test test/runtime-integration-tests/ --filter FullyQualifiedName~LoweredConstructor

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T087 [P] Update learn5thInYMinutes.md with constructor examples in docs/learn5thInYMinutes.md
- [ ] T088 [P] Add constructor section to developer guide in docs/
- [ ] T089 [P] Validate all example .5th files parse correctly via scripts/validate-examples.fish
- [ ] T090 Create performance benchmark for 200-constructor synthetic class in test/perf/ConstructorBenchmarks.cs (new file)
- [ ] T091 Run performance benchmarks and validate <5% compile time increase
- [ ] T092 [P] Add negative diagnostic tests for all CTOR001-CTOR010 codes in test/runtime-integration-tests/ConstructorDiagnosticTests.cs (new file)

### Diagnostic Formatting Tests

- [ ] T092a [P] Test CTOR001 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092b [P] Test CTOR002 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092c [P] Test CTOR003 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092d [P] Test CTOR004 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092e [P] Test CTOR005 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092f [P] Test CTOR006 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092g [P] Test CTOR007 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092h [P] Test CTOR008 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092i [P] Test CTOR009 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs
- [ ] T092j [P] Test CTOR010 JSON output structure in test/runtime-integration-tests/ConstructorDiagnosticTests.cs

- [ ] T093 Run full test suite: dotnet test fifthlang.sln
- [ ] T094 Validate quickstart.md instructions in specs/001-constructor-functions/quickstart.md
- [ ] T095 Update AGENTS.md with constructor feature notes if needed in AGENTS.md

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-7)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Lowering (Phase 8)**: Depends on User Stories 1-4 completion
- **Polish (Phase 9)**: Depends on all previous phases being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - Extends US1 but independently testable
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - Extends US1 but independently testable
- **User Story 5 (P3)**: Can start after Foundational (Phase 2) - Extends US1/US3 but independently testable

### Within Each User Story

- Tests MUST be written and FAIL before implementation
- All [P] marked tests can run in parallel
- Implementation tasks follow test tasks
- Validation tasks must complete before moving to next story

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel (T002, T003)
- Tests within each user story marked [P] can run in parallel
- User Stories 1 and 2 can be worked on in parallel (both P1 priority)
- User Stories 3 and 4 can be worked on in parallel (both P2 priority)
- Documentation tasks in Polish phase (T087, T088, T092) can run in parallel

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task T014: "Parser test for basic constructor syntax"
Task T015: "Parser test for constructor with multiple parameters"
Task T016: "Runtime test for simple constructor instantiation"
Task T017: "Runtime test verifying field values after construction"

# After tests are written and failing, proceed with implementation sequentially
```

---

## Implementation Strategy

### MVP First (User Stories 1 & 2 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1 (Basic Construction)
4. Complete Phase 4: User Story 2 (Field Safety)
5. **STOP and VALIDATE**: Test User Stories 1 & 2 independently
6. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Basic constructors working
3. Add User Story 2 → Test independently → Field safety enforced (MVP!)
4. Add User Story 3 → Test independently → Overloads working
5. Add User Story 4 → Test independently → Inheritance working
6. Add User Story 5 → Test independently → Generics working
7. Complete Lowering → Full feature implementation
8. Polish → Production ready

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Basic Construction)
   - Developer B: User Story 2 (Field Safety)
3. After US1/US2 complete:
   - Developer A: User Story 3 (Overloads)
   - Developer B: User Story 4 (Inheritance)
   - Developer C: User Story 5 (Generics)
4. Team completes Lowering together
5. Parallel polish tasks

---

## Summary

- **Total Tasks**: 113
- **MVP Tasks (US1 + US2)**: ~34 tasks (T001-T026b plus foundational tests)
- **Full Feature**: 113 tasks across 9 phases
- **Parallel Opportunities**: 30+ tasks marked [P]
- **Independent Test Criteria**: Each user story has specific acceptance tests
- **Suggested MVP Scope**: User Stories 1 & 2 (basic construction + field safety)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- **CRITICAL**: Never hand-edit files in src/ast-generated/
- **CRITICAL**: Never cancel build/test operations (can take 1-2 minutes)
- Run `just run-generator` after any metamodel changes
