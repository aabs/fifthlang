# Task List: Lambda Functions & Higher-Order Functions

**Feature Branch**: `016-lambda-functions`
**Spec**: [spec.md](spec.md)
**Plan**: [plan.md](plan.md)

## Phase 1: AST & Parser (TDD)
- [x] **1.1. Update AST Metamodel** <!-- id: 6 -->
  - [x] Confirm `LambdaExp` already exists in the current metamodel (no new AST nodes needed for this feature’s representation).
  - [x] Represent function types via `FifthType.TFunc` (multi-input list) rather than introducing a dedicated `FunctionType` AST node.
  - [x] Update `FifthType.TFunc` in `src/ast-model/TypeSystem/FifthType.cs` to support multiple inputs.
  - [x] **Files**: `src/ast-model/TypeSystem/FifthType.cs`, `src/ast-model/TypeSystem/TypeInference.cs`, `src/compiler/TypeSystem/GenericTypeCache.cs`
- [x] **1.2. Regenerate AST** <!-- id: 7 -->
  - [x] Not required for this stage (no changes were made under `src/ast-model/AstMetamodel.cs`).
- [ ] **1.3. Parser Tests** <!-- id: 5 -->
  - [x] Add positive tests for lambda syntax.
  - [ ] Add positive tests for lambda generic constraints.
  - [x] Add positive tests for function types.
  - [x] **File**: `test/syntax-parser-tests/LambdaSyntaxTests.cs`
- [x] **1.4. Update Lexer** <!-- id: 2 -->
  - [x] Add `FUN` ('fun') and `ARROW` ('->') tokens.
  - [x] **File**: `src/parser/grammar/FifthLexer.g4`
- [x] **1.5. Update Parser** <!-- id: 3 -->
  - [x] Add lambda expression parsing.
  - [x] Enforce block body syntax `{ ... }` (no expression bodies).
  - [x] Add function type signature parsing (`[T1, T2] -> R`).
  - [x] Update expression parsing to include lambdas.
  - [x] **File**: `src/parser/grammar/FifthParser.g4`
- [ ] **1.6. Update AST Builder** <!-- id: 4 -->
  - [x] Update `AstBuilderVisitor.cs` to map parse tree to `LambdaExp` + function type specs.
  - [ ] Map lambda generic constraints end-to-end (parse → AST model → type system).
  - [x] Enforce parameter limit (FR-008) and emit `ERR_TOO_MANY_LF_PARAMETERS`.
  - [x] **Files**: `src/parser/AstBuilderVisitor.cs`, `src/compiler/LanguageTransformations/LambdaDiagnostics.cs`, `src/compiler/LanguageTransformations/LambdaValidationVisitor.cs`, `src/compiler/ParserManager.cs`
- [x] **1.7. Validate Documentation & Examples** <!-- id: 18 -->
  - [x] Run `scripts/validate-examples.fish` to ensure all docs and samples parse with the new grammar.
  - [x] Update any broken examples in `docs/` or `test/ast-tests/CodeSamples`.

## Phase 2: Runtime & Type System
- [x] **2.1. Define Runtime Interfaces** <!-- id: 1 -->
  - [x] Create `IClosure<...>` and `IActionClosure<...>` interfaces in `fifthlang.system`.
  - [x] Ensure interfaces support up to 8 parameters.
  - [x] **File**: `src/fifthlang.system/Runtime/IClosure.cs`
- [x] **2.2. Type System Updates** <!-- id: 8 -->
  - [x] Update type representation and inference for multi-argument function types.
  - [x] Update generic type caching/keying for multi-arg function types.
  - [x] **Files**: `src/ast-model/TypeSystem/TypeInference.cs`, `src/compiler/TypeSystem/GenericTypeCache.cs`

## Phase 3: Basic Execution & Captures (TDD)
- [ ] **3.1. US1: Basic Lambda Tests** <!-- id: 14 -->
  - [ ] Test definition, assignment, and invocation.
  - [ ] **File**: `test/runtime-integration-tests/LambdaTests.cs`
- [ ] **3.2. US3: Capture Tests** <!-- id: 16 -->
  - [ ] Test capturing local variables.
  - [ ] Test capturing modified variables (by value).
  - [ ] **File**: `test/runtime-integration-tests/ClosureTests.cs`
- [ ] **3.3. Closure Conversion Analysis** <!-- id: 9 -->
  - [ ] Implement `FreeVariableAnalysisVisitor` to identify captured variables and `this`.
  - [ ] Validate no shadowing of outer variables.
  - [ ] Validate captured variables are not reassigned (read-only).
  - [ ] **File**: `src/compiler/LanguageTransformations/FreeVariableAnalysisVisitor.cs`
- [ ] **3.4. Closure Conversion Rewriter** <!-- id: 10 -->
  - [ ] Implement `ClosureConversionRewriter` (inherits `DefaultAstRewriter`).
  - [ ] Generate `ClassDef` for closures (handling generics).
  - [ ] Replace `LambdaExp` with explicit object instantiation (using `ObjectInitializerExp` or similar).
  - [ ] Replace invocations with standard method calls to `.Apply(...)`.
  - [ ] **File**: `src/compiler/LanguageTransformations/ClosureConversionRewriter.cs`
- [ ] **3.5. Roslyn Generator Updates** <!-- id: 13 -->
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
