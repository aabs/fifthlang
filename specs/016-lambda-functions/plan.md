# Implementation Plan: Lambda Functions

**Branch**: `016-lambda-functions` | **Date**: 2026-01-04 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/016-lambda-functions/spec.md`

## Summary

Implement support for lambda functions as first-class citizens: anonymous function expressions (LF), assignment to variables and members, passing as parameters, returning as results, generic LFs, closure capture, defunctionalisation of HOFs, worker/wrapper generation, and TCO support for self-recursion.

## Technical Context

**Language/Version**: C# 14, .NET 8.0
**Primary Dependencies**: ANTLR 4.8, Microsoft.CodeAnalysis (Roslyn), TUnit, FluentAssertions, Dunet, Vogen
**Storage**: N/A
**Testing**: TUnit
**Target Platform**: Cross-platform (dotnet)
**Project Type**: Compiler Library
**Performance Goals**: N/A
**Constraints**: Must use AST lowering pipeline; generated code must not be hand-edited.
**Scale/Scope**: Major language feature implementation.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

- [x] **Library-First**: Feature implemented in `src/compiler` and `src/ast-model`.
- [x] **Contracts-First**: AST metamodel updates defined in `data-model.md`.
- [x] **Generator-as-Source-of-Truth**: AST updates via `AstMetamodel.cs`.
- [x] **Test-First**: Plan includes comprehensive testing strategy.
- [x] **Simplicity**: Using `DefaultAstRewriter` for lowering; TCO limited to self-recursion.
- [x] **Multi-Pass Compilation**: Adding specific passes for Closure Conversion and Defunctionalisation.

## Project Structure

### Documentation (this feature)

```text
specs/016-lambda-functions/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (AST definitions)
└── tasks.md             # Phase 2 output
```

### Source Code (repository root)

```text
src/
├── ast-model/          # AST Metamodel updates
├── ast-generated/      # Generated AST classes
├── parser/             # Grammar updates
├── compiler/           # Lowering passes
└── fifthlang.system/   # Runtime interfaces (IClosure)

test/
├── ast-tests/          # AST unit tests
├── syntax-parser-tests/# Parser tests
└── runtime-integration-tests/ # End-to-end tests
```

**Structure Decision**: Standard Fifth Language Compiler structure.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| N/A | | |

## Phase 1: Grammar & AST Infrastructure
- [ ] **Grammar Updates**:
    - Update `src/parser/grammar/FifthLexer.g4` with `FUN` keyword and `->` operator.
    - Update `src/parser/grammar/FifthParser.g4` with `lambda_function` and `function_type_signature` rules.
    - Ensure `type_name` rule includes `void` or handle it in `function_type_signature`.
- [ ] **AST Metamodel**:
    - Modify `src/ast-model/AstMetamodel.cs`:
        - Add `FunctionType` (Type).
        - Add `LambdaExp` (Expression).
        - Add `ClosureApplyExp` (Expression).
        - Add `WorkerFunctionDef` (Definition).
        - Add `WrapperFunctionDef` (Definition).
    - Update `src/ast-model/TypeSystem/FifthType.cs`:
        - Update `TFunc` to support multiple input types (e.g., `List<FifthType> InputTypes`).
    - Run `just run-generator` to update AST classes.
- [ ] **Parser Implementation**:
    - Update `src/parser/AstBuilderVisitor.cs` to handle new grammar rules and build `LambdaExp` and `FunctionType`.
- [ ] **Parser Tests**:
    - Add syntax tests in `test/syntax-parser-tests/` to verify parsing of LFs and function types.

## Phase 2: Runtime Support
- [ ] **Runtime Interfaces**:
    - Define `IClosure` interfaces (generic variants) in `src/fifthlang.system/` or a new runtime project.
    - Define `IActionClosure` interfaces for void-returning LFs.
    - Define standard `Closure` base classes if needed.

## Phase 3: Lowering & Transformations
- [ ] **Closure Conversion Pass**:
    - Create `src/compiler/LanguageTransformations/ClosureConversionRewriter.cs`.
    - Implement variable capture analysis (free variables).
    - Generate closure classes/structs.
    - Rewrite `LambdaExp` to closure instantiation.
    - Handle `void` return types using `IActionClosure`.
- [ ] **Defunctionalisation Pass**:
    - Create `src/compiler/LanguageTransformations/DefunctionalisationRewriter.cs`.
    - Replace `FunctionType` usages with `IClosure<...>` or `IActionClosure<...>`.
    - Rewrite function calls `f(...)` to `f.Apply(...)`.
- [ ] **TCO Pass (Self-Recursion)**:
    - Create `src/compiler/LanguageTransformations/TailCallOptimizationRewriter.cs`.
    - Detect self-recursive tail calls.
    - Rewrite to `while` loops.

## Phase 4: Code Generation (Roslyn)
- [ ] **Roslyn Backend Updates**:
    - Update `src/compiler/LoweredToRoslyn/LoweredAstToRoslynTranslator.cs`.
    - Handle `IClosure`/`IActionClosure` types.
    - Emit generated closure classes.
    - Emit `WorkerFunctionDef` and `WrapperFunctionDef`.
    - Map `void` return to `void` in C#.

## Phase 5: Verification
- [ ] **Unit Tests**:
    - Add tests for each rewriter pass.
- [ ] **Integration Tests**:
    - Implement User Story 1 (Basic LFs).
    - Implement User Story 2 (HOFs).
    - Implement User Story 3 (Captures).
    - Implement User Story 4 (Generics & TCO).
    - Verify void-returning LFs.
