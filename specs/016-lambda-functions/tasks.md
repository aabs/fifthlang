# Task List: Lambda Functions & Higher-Order Functions

**Feature Branch**: `016-lambda-functions`
**Spec**: [spec.md](spec.md)
**Plan**: [plan.md](plan.md)

## Phase 1: AST & Parser (TDD)
- [ ] **1.1. Update AST Metamodel** <!-- id: 6 -->
  - [ ] Add `LambdaExp` (captures, body, generic parameters, constraints).
  - [ ] Add `FunctionType` (inputs, output).
  - [ ] Add `ClosureApplyExp` (closure instance, args).
  - [ ] Add `NewClosureInstanceExp` (generated class, captured values).
  - [ ] **File**: `src/ast-model/AstMetamodel.cs`
- [ ] **1.2. Regenerate AST** <!-- id: 7 -->
  - [ ] Run `just run-generator`.
- [ ] **1.3. Parser Tests** <!-- id: 5 -->
  - [ ] Add positive tests for lambda syntax (including generic constraints).
  - [ ] Add positive tests for function types.
  - [ ] **File**: `test/syntax-parser-tests/LambdaParserTests.cs`
- [ ] **1.4. Update Lexer** <!-- id: 2 -->
  - [ ] Add `FUN` ('fun') and `ARROW` ('->') tokens.
  - [ ] **File**: `src/parser/grammar/FifthLexer.g4`
- [ ] **1.5. Update Parser** <!-- id: 3 -->
  - [ ] Add `lambda_function` rule (including `constraint_clause`).
  - [ ] Add `function_type_signature` rule.
  - [ ] Update `expression` rule to include lambdas.
  - [ ] **File**: `src/parser/grammar/FifthParser.g4`
- [ ] **1.6. Update AST Builder** <!-- id: 4 -->
  - [ ] Update `AstBuilderVisitor.cs` to map parse tree (including constraints) to the new AST nodes.
  - [ ] Enforce parameter limit (FR-008) and emit `ERR_TOO_MANY_LF_PARAMETERS`.
  - [ ] **File**: `src/parser/AstBuilderVisitor.cs`

## Phase 2: Runtime & Type System
- [ ] **2.1. Define Runtime Interfaces** <!-- id: 1 -->
  - [ ] Create `IClosure<T>` and `IActionClosure` interfaces in `fifthlang.system`.
  - [ ] Ensure interfaces support up to 8 parameters (or as configured).
  - [ ] **File**: `src/fifthlang.system/Runtime/IClosure.cs`
- [ ] **2.2. Type System Updates** <!-- id: 8 -->
  - [ ] Update `TypeSystem` to support function types.
  - [ ] Implement type compatibility checks for function types.
  - [ ] **File**: `src/compiler/TypeSystem/TypeChecker.cs`

## Phase 3: Basic Execution & Captures (TDD)
- [ ] **3.1. US1: Basic Lambda Tests** <!-- id: 14 -->
  - [ ] Test definition, assignment, and invocation.
  - [ ] **File**: `test/runtime-integration-tests/LambdaTests.cs`
- [ ] **3.2. US3: Capture Tests** <!-- id: 16 -->
  - [ ] Test capturing local variables.
  - [ ] Test capturing modified variables (by value).
  - [ ] **File**: `test/runtime-integration-tests/ClosureTests.cs`
- [ ] **3.3. Closure Conversion Analysis** <!-- id: 9 -->
  - [ ] Implement `FreeVariableAnalysisVisitor` to identify captured variables.
  - [ ] **File**: `src/compiler/LanguageTransformations/FreeVariableAnalysisVisitor.cs`
- [ ] **3.4. Closure Conversion Rewriter** <!-- id: 10 -->
  - [ ] Implement `ClosureConversionRewriter` (inherits `DefaultAstRewriter`).
  - [ ] Generate `ClassDef` for closures (handling generics).
  - [ ] Replace `LambdaExp` with `NewClosureInstanceExp`.
  - [ ] **File**: `src/compiler/LanguageTransformations/ClosureConversionRewriter.cs`
- [ ] **3.5. Roslyn Generator Updates** <!-- id: 13 -->
  - [ ] Handle `NewClosureInstanceExp` (emit `new ClosureClass(...)`).
  - [ ] Handle `ClosureApplyExp` (emit `.Apply(...)`).
  - [ ] Ensure `IClosure` interfaces are referenced correctly.
  - [ ] **File**: `src/compiler/LoweredToRoslyn/LoweredAstToRoslynTranslator.cs`

## Phase 4: Higher Order Functions (TDD)
- [ ] **4.1. US2: HOF Tests** <!-- id: 15 -->
  - [ ] Test passing lambdas to functions.
  - [ ] Test returning lambdas from functions.
  - [ ] **File**: `test/runtime-integration-tests/HofTests.cs`
- [ ] **4.2. Defunctionalisation** <!-- id: 11 -->
  - [ ] Implement `DefunctionalisationRewriter`.
  - [ ] Transform HOF signatures to use `IClosure`/`IActionClosure`.
  - [ ] **File**: `src/compiler/LanguageTransformations/DefunctionalisationRewriter.cs`

## Phase 5: Advanced Features (TDD)
- [ ] **5.1. US4: Advanced Tests** <!-- id: 17 -->
  - [ ] Test generic lambdas.
  - [ ] Test deep recursion (TCO verification).
  - [ ] **File**: `test/runtime-integration-tests/AdvancedLambdaTests.cs`
- [ ] **5.2. TCO Support (Self-Recursion)** <!-- id: 12 -->
  - [ ] Implement `TailCallOptimizationRewriter`.
  - [ ] Detect self-recursion in tail position.
  - [ ] Rewrite to loop.
  - [ ] **File**: `src/compiler/LanguageTransformations/TailCallOptimizationRewriter.cs`
