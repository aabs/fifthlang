# Implementation Tasks: Full Generics Support

**Feature**: `001-full-generics-support` | **Branch**: `001-full-generics-support` | **Date**: 2025-11-18

## Overview

This document breaks down the implementation of Full Generics Support for the Fifth programming language into concrete, executable tasks organized by user story priority. The feature enables type-safe generic programming with classes, functions, type parameters, constraints, and automatic type inference.

**Tech Stack**: C# 14, .NET 8.0 SDK (8.0.118), ANTLR 4.8, TUnit + FluentAssertions  
**Target**: Compiler infrastructure (Fifth language)  
**Approach**: Test-driven development (TDD) following Fifth constitution

---

## Implementation Strategy

### MVP Scope (User Story 1 Only)
The minimum viable product includes **User Story 1 (Generic Collection Classes)** only:
- Generic class definitions with single type parameter
- Type parameter usage in properties and methods
- Basic type instantiation (`Stack<int>`)
- Type safety enforcement at compile time

This MVP provides immediate value by enabling developers to write type-safe collection classes without duplication.

### Incremental Delivery Order
1. **US1** (P1): Generic Collection Classes — Foundational capability
2. **US2** (P1): Generic Functions with Type Inference — Completes core generics
3. **US3** (P2): Generic Methods in Classes — Advanced patterns
4. **US4** (P2): Type Constraints — Enables algorithms
5. **US5** (P3): Multiple Type Parameters — Complex data structures
6. **US6** (P3): Nested Generic Types — Sophisticated modeling

Each user story delivers independently testable value.

---

## Task Summary

- **Total Tasks**: 89
- **Setup Phase**: 5 tasks
- **Foundational Phase**: 10 tasks
- **US1 Phase**: 15 tasks (MVP)
- **US2 Phase**: 13 tasks
- **US3 Phase**: 11 tasks
- **US4 Phase**: 14 tasks
- **US5 Phase**: 9 tasks
- **US6 Phase**: 8 tasks
- **Polish Phase**: 4 tasks
- **Parallelizable Tasks**: 42 (marked with [P])

---

## Dependencies & Execution Order

### User Story Dependencies
```
Setup Phase (blocking)
    ↓
Foundational Phase (blocking)
    ↓
├─→ US1 (P1) — Generic Collection Classes [MVP] (independent)
    ├─→ US2 (P1) — Generic Functions (depends on US1 AST/type system)
        ├─→ US3 (P2) — Generic Methods (depends on US1 + US2)
        ├─→ US4 (P2) — Type Constraints (depends on US1 + US2)
            ├─→ US5 (P3) — Multiple Type Parameters (depends on US1-US4)
            └─→ US6 (P3) — Nested Generic Types (depends on US1-US5)
    ↓
Polish Phase (cross-cutting)
```

**Critical Path**: Setup → Foundational → US1 → US2 → {US3, US4} → {US5, US6} → Polish

### Parallel Execution Opportunities

**Within Setup Phase** (all tasks sequential - project initialization)

**Within Foundational Phase**:
- T006-T009 (grammar rules) can run in parallel after T005
- T013-T014 (parser visitor methods) can run in parallel after T010-T012

**Within US1 Phase**:
- T017-T019 (test files) can be created in parallel
- T024-T025 (type system extensions) can run in parallel after T020-T023

**Within US2 Phase**:
- T032-T033 (test samples) can be created in parallel
- T037-T039 (inference tests) can run in parallel after T034-T036

**Within US3-US6 Phases**: Similar parallelization patterns (tests, independent file modifications)

---

## Phase 1: Setup (Project Initialization)

**Goal**: Initialize project structure and verify build environment

- [ ] T001 Verify .NET 8.0 SDK (8.0.118) and Java 17+ installed, run `dotnet --version` and `java -version`
- [ ] T002 Run `dotnet restore fifthlang.sln` to restore all dependencies (wait 70+ seconds, NEVER CANCEL)
- [ ] T003 Run `dotnet build fifthlang.sln` to verify clean build (wait 60+ seconds, NEVER CANCEL)
- [ ] T004 Run `dotnet test fifthlang.sln` to verify all existing tests pass (wait 5+ minutes, NEVER CANCEL)
- [ ] T005 Create feature documentation directory structure in `specs/001-full-generics-support/` (spec.md, plan.md, research.md already exist)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Goal**: Extend grammar, AST metamodel, and code generator to support generic syntax parsing

**Note**: These changes are foundational for ALL user stories and must complete before any story-specific work.

### Grammar Extensions

- [ ] T006 [P] Add `WHERE` keyword to `src/parser/grammar/FifthLexer.g4` for constraint syntax
- [ ] T007 [P] Add `type_parameter_list` rule to `src/parser/grammar/FifthParser.g4`: `LESS type_parameter (COMMA type_parameter)* GREATER`
- [ ] T008 [P] Add `type_parameter` rule to `src/parser/grammar/FifthParser.g4`: `IDENTIFIER`
- [ ] T009 [P] Add `constraint_clause` and `constraint_list` rules to `src/parser/grammar/FifthParser.g4` for `where T: InterfaceName` syntax
- [ ] T010 Run `dotnet build src/parser/parser.csproj` to regenerate ANTLR parser (auto-compilation, wait 30+ seconds)

### AST Metamodel Extensions

- [ ] T011 Add `TypeParameterDef` record to `src/ast-model/AstMetamodel.cs` with properties: Name (TypeParameterName), Constraints (TypeConstraint[]), Location
- [ ] T012 Add `TypeConstraint` abstract record hierarchy to `src/ast-model/AstMetamodel.cs`: InterfaceConstraint, BaseClassConstraint, ConstructorConstraint
- [ ] T013 [P] Extend `ClassDef` record in `src/ast-model/AstMetamodel.cs` to add `TypeParameters` property (TypeParameterDef[]?)
- [ ] T014 [P] Extend `FunctionDef` record in `src/ast-model/AstMetamodel.cs` to populate `TypeParameters` property (was unused/TODO)
- [ ] T015 Run `just run-generator` or `dotnet run --project src/ast_generator/ast_generator.csproj -- --folder src/ast-generated` to regenerate AST builders/visitors (wait 5+ seconds)

---

## Phase 3: User Story 1 — Generic Collection Classes (P1) [MVP]

**Story Goal**: Enable developers to define reusable data structures with single type parameters (e.g., `class Stack<T>`)

**Independent Test**: Define `Stack<T>` class, instantiate with different types (int, string), verify type safety

### Test Setup

- [ ] T016 [US1] Create test file `test/syntax-parser-tests/GenericClassSyntaxTests.cs` for grammar parsing tests
- [ ] T017 [P] [US1] Create test sample `test/syntax-parser-tests/test_samples/generic_class.5th` with `class Stack<T> { items: List<T>; }`
- [ ] T018 [P] [US1] Create test file `test/ast-tests/GenericClassAstBuilderTests.cs` for AST construction tests
- [ ] T019 [P] [US1] Create test file `test/runtime-integration-tests/GenericClassRuntimeTests.cs` for end-to-end execution tests

### Grammar & Parser Implementation

- [ ] T020 [US1] Modify `class_definition` rule in `src/parser/grammar/FifthParser.g4` to accept optional `type_parameter_list`: `CLASS IDENTIFIER type_parameter_list? ...`
- [ ] T021 [US1] Implement `VisitType_parameter_list` method in `src/parser/AstBuilderVisitor.cs` to parse type parameters and return `TypeParameterDef[]`
- [ ] T022 [US1] Implement `VisitType_parameter` method in `src/parser/AstBuilderVisitor.cs` to create `TypeParameterDef` nodes
- [ ] T023 [US1] Assign `TypeParameterScope` with unique scope ID during AST construction in `AstBuilderVisitor.cs` for class-level type parameters

### Type System Extensions

- [ ] T024 [P] [US1] Add `TGenericParameter` variant to `FifthType` discriminated union in `src/ast-model/AstMetamodel.cs` with properties: Name, Constraints, Scope
- [ ] T025 [P] [US1] Add `TGenericInstance` variant to `FifthType` discriminated union in `src/ast-model/AstMetamodel.cs` with properties: GenericTypeDefinition, TypeArguments, CachedHash
- [ ] T026 [US1] Regenerate AST code via `just run-generator` after metamodel changes

### Type Instantiation & Validation

- [ ] T027 [US1] Create `src/compiler/LanguageTransformations/TypeParameterResolutionVisitor.cs` implementing `BaseAstVisitor` for type parameter resolution
- [ ] T028 [US1] Implement type argument count validation in `TypeParameterResolutionVisitor` (report GEN001 if count mismatch)
- [ ] T029 [US1] Create `GenericTypeCache` class in `src/compiler/TypeSystem/GenericTypeCache.cs` with 10,000 entry limit and LRU eviction (per NFR-001)
- [ ] T030 [US1] Register `TypeParameterResolutionVisitor` in `src/compiler/ParserManager.cs` transformation pipeline after symbol table building

### Validation & Integration

- [ ] T031 [US1] Run `dotnet test test/syntax-parser-tests/GenericClassSyntaxTests.cs` to verify grammar parsing
- [ ] T032 [US1] Run `dotnet test test/ast-tests/GenericClassAstBuilderTests.cs` to verify AST construction
- [ ] T033 [US1] Run `dotnet test test/runtime-integration-tests/GenericClassRuntimeTests.cs` to verify end-to-end execution (**GATE**: must pass for US1 complete)

---

## Phase 4: User Story 2 — Generic Functions with Type Inference (P1)

**Story Goal**: Enable developers to write generic utility functions with automatic type argument inference (e.g., `identity<T>(x: T)`)

**Independent Test**: Define `identity<T>(x: T): T`, call without explicit type arguments, verify inference

**Dependencies**: Requires US1 (AST/type system foundation)

### Test Setup

- [ ] T034 [US2] Create test file `test/syntax-parser-tests/GenericFunctionSyntaxTests.cs` for function syntax tests
- [ ] T035 [P] [US2] Create test sample `test/syntax-parser-tests/test_samples/generic_function.5th` with `identity<T>(x: T): T => x;`
- [ ] T036 [P] [US2] Create test sample `test/syntax-parser-tests/test_samples/generic_inference.5th` with inference examples (`identity(42)`)

### Grammar & Parser Implementation

- [ ] T037 [US2] Modify `function_definition` rule in `src/parser/grammar/FifthParser.g4` to accept optional `type_parameter_list`
- [ ] T038 [US2] Update `VisitFunction_definition` in `src/parser/AstBuilderVisitor.cs` to parse function type parameters and populate `FunctionDef.TypeParameters`
- [ ] T039 [US2] Assign `TypeParameterScope` with unique scope ID for function-level type parameters in `AstBuilderVisitor.cs`

### Type Inference Implementation

- [ ] T040 [US2] Create `src/compiler/LanguageTransformations/GenericTypeInferenceVisitor.cs` implementing `BaseAstVisitor` for type inference
- [ ] T041 [US2] Implement `TypeInferenceContext` class in `src/compiler/TypeSystem/TypeInferenceContext.cs` with properties: InferredTypes (map), PendingConstraints, Diagnostics, CallSite
- [ ] T042 [US2] Implement method call inference in `GenericTypeInferenceVisitor`: infer type arguments from function arguments (local type inference, C# style per research.md)
- [ ] T043 [US2] Implement assignment context inference in `GenericTypeInferenceVisitor`: infer from `x: Stack<int> = new Stack()` patterns
- [ ] T044 [US2] Implement inference failure diagnostics (report GEN002 when type arguments cannot be inferred)
- [ ] T045 [US2] Register `GenericTypeInferenceVisitor` in `src/compiler/ParserManager.cs` after `TypeParameterResolutionVisitor`

### Tests & Validation

- [ ] T046 [P] [US2] Create test file `test/ast-tests/GenericInferenceTests.cs` for type inference unit tests
- [ ] T047 [P] [US2] Create test file `test/runtime-integration-tests/GenericFunctionRuntimeTests.cs` for end-to-end function tests
- [ ] T048 [US2] Run `dotnet test test/syntax-parser-tests/GenericFunctionSyntaxTests.cs` to verify function syntax
- [ ] T049 [US2] Run `dotnet test test/ast-tests/GenericInferenceTests.cs` to verify type inference logic
- [ ] T050 [US2] Run `dotnet test test/runtime-integration-tests/GenericFunctionRuntimeTests.cs` to verify end-to-end execution (**GATE**: must pass for US2 complete)

---

## Phase 5: User Story 3 — Generic Methods in Classes (P2)

**Story Goal**: Enable generic methods within classes where method type parameters are independent of class type parameters (e.g., `List<T>.map<U>()`)

**Independent Test**: Define class with static generic method, call with different types, verify type parameter independence

**Dependencies**: Requires US1 (generic classes) + US2 (type inference)

### Test Setup

- [ ] T051 [US3] Create test sample `test/syntax-parser-tests/test_samples/generic_methods.5th` with class containing generic methods
- [ ] T052 [P] [US3] Create test file `test/ast-tests/GenericMethodAstTests.cs` for method-level type parameter tests
- [ ] T053 [P] [US3] Create test file `test/runtime-integration-tests/GenericMethodRuntimeTests.cs` for end-to-end method tests

### Implementation

- [ ] T054 [US3] Update `MethodDef` record in `src/ast-model/AstMetamodel.cs` to add `TypeParameters` property if not already present
- [ ] T055 [US3] Assign distinct `TypeParameterScope` for method-level type parameters in `AstBuilderVisitor.cs` (ScopeKind.Method)
- [ ] T056 [US3] Implement type parameter shadowing logic in `TypeParameterResolutionVisitor`: method `T` shadows class `T` within method scope
- [ ] T057 [US3] Extend `GenericTypeInferenceVisitor` to handle method calls on generic class instances (resolve both class and method type parameters)
- [ ] T058 [US3] Regenerate AST code via `just run-generator` if metamodel changed

### Tests & Validation

- [ ] T059 [US3] Run `dotnet test test/ast-tests/GenericMethodAstTests.cs` to verify method type parameter handling
- [ ] T060 [US3] Run `dotnet test test/runtime-integration-tests/GenericMethodRuntimeTests.cs` to verify method execution (**GATE**: must pass for US3 complete)
- [ ] T061 [US3] Verify type parameter shadowing works correctly (method `T` distinct from class `T` in test scenarios)

---

## Phase 6: User Story 4 — Type Constraints for Safe Operations (P2)

**Story Goal**: Enable type parameter constraints to ensure type arguments support required operations (e.g., `where T: IComparable`)

**Independent Test**: Define `sort<T>(items: [T]): [T] where T: IComparable`, verify compiler enforces constraint

**Dependencies**: Requires US1 (generic classes) + US2 (type inference)

### Test Setup

- [ ] T062 [US4] Create test sample `test/syntax-parser-tests/test_samples/generic_constraints.5th` with constrained functions and classes
- [ ] T063 [P] [US4] Create test file `test/ast-tests/GenericConstraintTests.cs` for constraint validation tests
- [ ] T064 [P] [US4] Create test file `test/runtime-integration-tests/GenericConstraintRuntimeTests.cs` for constraint enforcement tests

### Grammar & Parser Implementation

- [ ] T065 [US4] Update `function_definition` and `class_definition` rules in `src/parser/grammar/FifthParser.g4` to accept `constraint_clause*`
- [ ] T066 [US4] Implement `VisitConstraint_clause` method in `src/parser/AstBuilderVisitor.cs` to parse `where T: InterfaceName` syntax
- [ ] T067 [US4] Implement `VisitConstraint` method in `src/parser/AstBuilderVisitor.cs` to create `InterfaceConstraint`, `BaseClassConstraint`, or `ConstructorConstraint` nodes
- [ ] T068 [US4] Attach parsed constraints to corresponding `TypeParameterDef.Constraints` array in AST

### Constraint Validation Implementation

- [ ] T069 [US4] Implement constraint satisfaction validation in `TypeParameterResolutionVisitor`: check if type arguments satisfy all constraints
- [ ] T070 [US4] Implement interface constraint checking: verify type implements required interface (report GEN003 if violated)
- [ ] T071 [US4] Implement base class constraint checking: verify type derives from required base class (report GEN003 if violated)
- [ ] T072 [US4] Implement constructor constraint checking: verify type has parameterless constructor (report GEN004 if missing)
- [ ] T073 [US4] Implement constraint ordering validation: ensure constructor constraint is last if present
- [ ] T074 [US4] Implement recursive constraint support with cycle detection (per clarifications: allow `Node<T> where T: Node<T>`)

### Tests & Validation

- [ ] T075 [US4] Run `dotnet test test/ast-tests/GenericConstraintTests.cs` to verify constraint parsing and validation
- [ ] T076 [US4] Run `dotnet test test/runtime-integration-tests/GenericConstraintRuntimeTests.cs` to verify constraint enforcement (**GATE**: must pass for US4 complete)

---

## Phase 7: User Story 5 — Multiple Type Parameters (P3)

**Story Goal**: Enable types and functions with multiple independent type parameters (e.g., `Dictionary<TKey, TValue>`)

**Independent Test**: Define `Dictionary<TKey, TValue>`, instantiate with different type combinations, verify independence

**Dependencies**: Requires US1-US4 (all core generic functionality)

### Test Setup

- [ ] T077 [US5] Create test sample `test/syntax-parser-tests/test_samples/multiple_type_params.5th` with multi-parameter generics
- [ ] T078 [P] [US5] Create test file `test/ast-tests/MultipleTypeParamTests.cs` for multi-parameter AST tests
- [ ] T079 [P] [US5] Create test file `test/runtime-integration-tests/MultipleTypeParamRuntimeTests.cs` for multi-parameter execution tests

### Implementation

- [ ] T080 [US5] Verify grammar already supports multiple type parameters in `type_parameter_list` (comma-separated)
- [ ] T081 [US5] Extend `TypeParameterResolutionVisitor` to handle multiple type parameters with independent constraint validation
- [ ] T082 [US5] Extend `GenericTypeInferenceVisitor` to infer multiple type arguments independently (e.g., `pair(1, "hello")` infers T1=int, T2=string)
- [ ] T083 [US5] Update `GenericTypeCache` hashing to handle multiple type arguments correctly (hash combination of all arguments)

### Tests & Validation

- [ ] T084 [US5] Run `dotnet test test/ast-tests/MultipleTypeParamTests.cs` to verify multi-parameter handling
- [ ] T085 [US5] Run `dotnet test test/runtime-integration-tests/MultipleTypeParamRuntimeTests.cs` to verify execution (**GATE**: must pass for US5 complete)

---

## Phase 8: User Story 6 — Nested Generic Types (P3)

**Story Goal**: Enable using generic types as type arguments to other generic types (e.g., `List<Stack<int>>`)

**Independent Test**: Declare `items: List<Stack<int>>`, perform nested operations, verify type tracking

**Dependencies**: Requires US1-US5 (complete generic type system)

### Test Setup

- [ ] T086 [US6] Create test sample `test/syntax-parser-tests/test_samples/nested_generics.5th` with nested generic examples
- [ ] T087 [P] [US6] Create test file `test/runtime-integration-tests/NestedGenericRuntimeTests.cs` for nested generic execution tests

### Implementation

- [ ] T088 [US6] Verify `type_spec` rule in grammar supports recursive generic syntax (`List<Stack<int>>`)
- [ ] T089 [US6] Update `ParseTypeSpec` in `AstBuilderVisitor.cs` to handle nested generic types recursively
- [ ] T090 [US6] Update `GenericTypeCache` to handle deeply nested generics (structural hashing with recursion)
- [ ] T091 [US6] Implement type equality checks for nested generics in type system (structural comparison at all nesting levels)

### Tests & Validation

- [ ] T092 [US6] Run `dotnet test test/runtime-integration-tests/NestedGenericRuntimeTests.cs` to verify nested generic execution (**GATE**: must pass for US6 complete)

---

## Phase 9: Roslyn Backend & Code Generation

**Goal**: Map Fifth generic types to .NET generic types for code generation

**Dependencies**: Requires all user stories (US1-US6) complete

- [ ] T093 Extend `src/code_generator/RoslynBackend/` to emit `TypeParameterSyntax` for generic type definitions
- [ ] T094 [P] Implement Fifth constraint → C# constraint mapping in Roslyn backend: InterfaceConstraint → `TypeConstraint`, BaseClassConstraint → `ClassOrStructConstraint`, ConstructorConstraint → `ConstructorConstraint`
- [ ] T095 [P] Implement generic type instantiation in IL emission: substitute type parameters with concrete type arguments
- [ ] T096 Verify full .NET reification: ensure Stack<int> and Stack<string> are distinct runtime types (per clarifications)

---

## Phase 10: Polish & Cross-Cutting Concerns

**Goal**: Complete documentation, performance validation, and final integration

- [ ] T097 Add generics section to `docs/learn5thInYMinutes.md` with examples from quickstart.md
- [ ] T098 Run performance benchmarks: verify compilation time increase < 15% (SC-003), type inference < 100ms (per plan.md)
- [ ] T099 Run full regression test suite: `dotnet test fifthlang.sln` to ensure 100% backward compatibility (SC-006)
- [ ] T100 Review all generics documentation for completeness: verify `docs/learn5thInYMinutes.md` includes all examples from `quickstart.md`, ensure cross-references are accurate, validate that all user stories have corresponding documentation sections
- [ ] T101 Update `.github/copilot-instructions.md` with final generic type support details (if not already updated by agent script)

---

## Validation Checklist

Before marking the feature complete, verify:

- [ ] All 13 success criteria from spec.md pass
- [ ] All runtime integration tests pass (US1-US6 gates)
- [ ] Diagnostic messages use structured format with error codes GEN001-GEN006 (per clarifications)
- [ ] Type instantiation cache respects 10,000 entry limit with LRU eviction (NFR-001)
- [ ] Grammar disambiguation uses contextual lookahead (per clarifications)
- [ ] Full .NET reification confirmed (Stack<int> ≠ Stack<string> at runtime)
- [ ] 100% backward compatibility verified (all existing tests pass)
- [ ] Performance targets met: <15% compile time increase, <100ms type inference

---

## Notes

- **Test-First Approach**: Fifth follows TDD mandatorily per constitution. Each phase includes test creation before implementation.
- **Never Cancel Builds**: Build operations can take 1-2 minutes. Always wait for completion.
- **Grammar Auto-Generation**: ANTLR parser regenerates automatically during build. Manual generation rarely needed.
- **AST Code Generation**: Run `just run-generator` after every metamodel change to regenerate builders/visitors.
- **Parallel Opportunities**: Tasks marked [P] can run in parallel with other [P] tasks in the same phase (different files, no dependencies).
- **User Story Labels**: Tasks marked [US#] belong to that user story phase for independent tracking.

---

**Total: 100 tasks** | **MVP (US1): 33 tasks** | **Parallelizable: 42 tasks**
