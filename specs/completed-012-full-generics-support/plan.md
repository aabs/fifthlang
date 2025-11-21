# Implementation Plan: Full Generics Support

**Branch**: `001-full-generics-support` | **Date**: 2025-11-18 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-full-generics-support/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Implement complete generic type support for the Fifth programming language, including generic classes with type parameters, generic functions with type inference, type constraints (interface, base class, constructor), and nested generic type instantiation. This enables developers to write type-safe reusable code without duplication, with automatic type inference reducing boilerplate in 80% of common cases.

**Technical Approach**: Extend AST metamodel with `TypeParameterDef` and constraint nodes, enhance grammar to support generic syntax (`<T>`, `where` clauses), implement unification-based type inference in the type system, and map to .NET generic types in the Roslyn backend. Follow multi-pass compilation philosophy: parse → build high-level AST with generics → type check/infer → lower to IL AST with substituted types → generate .NET generics.

## Technical Context

**Language/Version**: C# 14, .NET 8.0 SDK (global.json pins 8.0.118)  
**Primary Dependencies**: Antlr4.Runtime.Standard (4.8+), RazorLight (code generation), TUnit + FluentAssertions (testing), dunet (discriminated unions), Vogen (value objects)  
**Storage**: N/A (compiler/type system - all in-memory AST and type information)  
**Testing**: TUnit with FluentAssertions in `test/ast-tests/`, `test/syntax-parser-tests/`, `test/runtime-integration-tests/`  
**Target Platform**: .NET 8.0 cross-platform (compiler runs on Windows/macOS/Linux, generates PE assemblies)  
**Project Type**: Compiler infrastructure (single solution with multiple libraries)  
**Performance Goals**: Compilation time increase < 15% for programs using generics, type inference resolution < 100ms for typical function calls  
**Constraints**: Must maintain 100% backward compatibility with existing Fifth code, generic type instantiation must be deterministic and reproducible, type safety must be enforced at compile time with zero runtime type errors  
**Scale/Scope**: Support arbitrary type parameter counts, unlimited nesting depth for generic types, handle complex constraint combinations (multiple interfaces + base class + constructor)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Library-First, Contracts-First
- **Status**: PASS
- **Rationale**: Changes are focused on existing libraries (`ast-model`, `parser`, `compiler`) with clear contracts expressed through AST metamodels, grammar files, and generated builders/visitors. No new organizational libraries needed.

### ✅ CLI and Text I/O Discipline
- **Status**: PASS
- **Rationale**: No CLI changes required. Existing `compiler` CLI handles new syntax via grammar extensions. Diagnostic messages for generic errors will follow existing stderr patterns.

### ✅ Generator-as-Source-of-Truth
- **Status**: PASS
- **Rationale**: All AST extensions (`TypeParameterDef`, `TypeConstraint` nodes) will be added to `AstMetamodel.cs`. Generated code in `ast-generated/` will be regenerated via `just run-generator`. No hand-editing of generated files.

### ✅ Test-First (Non-Negotiable)
- **Status**: PASS with NOTE
- **Rationale**: TDD approach required. Tests will be added in order:
  1. Grammar tests for parsing generic syntax
  2. AST builder tests for creating generic nodes
  3. Type inference tests for type argument resolution
  4. Runtime integration tests for end-to-end generic code execution
- **NOTE**: Feature is NOT complete until runtime integration tests pass with actual Fifth generic syntax (e.g., `Stack<int>`, `identity<T>(x)`), not just C# interop tests.

### ✅ Reproducible Builds & Toolchain Discipline
- **Status**: PASS
- **Rationale**: Uses existing .NET 8.0 SDK (8.0.118) and ANTLR 4.8 toolchain. Grammar changes will auto-regenerate during build. No new external dependencies.

### ✅ Simplicity, Minimal Surface, and Safety
- **Status**: PASS with JUSTIFICATION
- **Rationale**: Generics inherently add complexity, but this is essential language feature complexity, not incidental. Design follows standard generic type system patterns (type parameters, constraints, inference) proven in C#/Java/TypeScript. Minimized by:
  - Reusing existing type system infrastructure
  - No variance or higher-kinded types in initial version
  - Standard unification-based inference (well-understood algorithm)

### ✅ Multi-Pass Compilation & AST Lowering Philosophy
- **Status**: PASS
- **Rationale**: Follows multi-pass architecture perfectly:
  1. **Parse Phase**: Grammar recognizes generic syntax → high-level AST with `TypeParameterDef`
  2. **Analysis Phase**: New `TypeParameterResolutionVisitor` and `GenericTypeInferenceVisitor` passes
  3. **Lowering Phase**: Type parameter substitution before IL transformation
  4. **Code Generation**: Map to .NET generic types in Roslyn backend
- No new lowering patterns needed; uses existing transformation visitor infrastructure.

### ✅ AST Design & Transformation Strategy
- **Status**: PASS
- **Rationale**: High-level AST gets rich generic constructs (`TypeParameterDef`, constraints). IL AST remains simple - generics are resolved/substituted before IL transformation. Type parameter resolution is a new analysis pass, not a lowering pass. Uses `BaseAstVisitor` for type checking (read-only analysis).

### ✅ Parser & Grammar Integrity
- **Status**: PASS
- **Rationale**: Grammar extensions are backward compatible:
  - Type parameters are optional: `class Name<T>?`
  - Constraints use new `WHERE` keyword (not currently used)
  - Angle brackets already supported in expression context (less-than operator)
  - Tests in `test_samples/*.5th` will verify grammar compliance
- All existing Fifth code remains valid (no breaking changes).

### ✅ Observability & Diagnostics
- **Status**: PASS
- **Rationale**: New diagnostics (GEN001-GEN006) will follow existing diagnostic pattern with structured output to stderr. Type inference failures will report line/column, expected vs inferred types.

### ✅ Versioning & Backward Compatibility
- **Status**: PASS
- **Rationale**: 100% backward compatible. Existing `[T]` and `T[]` syntax continues to work. Current hardcoded `list<T>`/`array<T>` handling becomes proper generics without breaking changes.

### Post-Phase 1 Re-check Required?
**YES** - After Phase 1 design, re-validate:
- Type inference algorithm complexity is justified
- Generic type instantiation caching doesn't violate simplicity
- Number of new transformation passes is minimal

All gates PASSED. Ready to proceed to Phase 0: Research.

## Project Structure

### Documentation (this feature)

```text
specs/[###-feature]/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── ast-model/              # AST metamodel definitions
│   ├── AstMetamodel.cs     # High-level AST nodes (WILL MODIFY - add TypeParameterDef, TypeConstraint)
│   └── ILMetamodel.cs      # IL AST nodes (no changes)
├── ast-generated/          # Auto-generated (WILL REGENERATE via 'just run-generator')
│   ├── builders.generated.cs
│   ├── visitors.generated.cs
│   └── rewriter.generated.cs
├── ast_generator/          # Code generator (no changes to logic)
├── parser/
│   ├── grammar/
│   │   ├── FifthLexer.g4   # Token definitions (WILL MODIFY - add WHERE keyword)
│   │   └── FifthParser.g4  # Syntax rules (WILL MODIFY - add type_parameter_list, constraint_clause)
│   └── AstBuilderVisitor.cs # Parser→AST transformation (WILL MODIFY - handle new grammar rules)
├── compiler/
│   ├── LanguageTransformations/  # AST passes
│   │   ├── TypeParameterResolutionVisitor.cs   # NEW - resolves type parameters to concrete types
│   │   └── GenericTypeInferenceVisitor.cs      # NEW - infers type arguments from usage
│   └── ParserManager.cs    # Transformation pipeline (WILL MODIFY - register new passes)
└── code_generator/
    └── RoslynBackend/      # IL generation (WILL MODIFY - emit .NET generic types)

test/
├── syntax-parser-tests/    # Grammar parsing tests
│   ├── GenericSyntaxTests.cs                   # NEW - test generic parsing
│   └── test_samples/
│       ├── generic_class.5th                   # NEW - sample: class Stack<T>
│       ├── generic_function.5th                # NEW - sample: identity<T>(x: T)
│       ├── generic_constraints.5th             # NEW - sample: where T: IComparable
│       └── generic_inference.5th               # NEW - sample: implicit type args
├── ast-tests/              # AST builder tests
│   └── GenericAstBuilderTests.cs               # NEW - test AST construction for generics
└── runtime-integration-tests/  # End-to-end tests
    └── GenericRuntimeTests.cs                  # NEW - test end-to-end generic code execution
```

**Structure Decision**: This feature modifies existing compiler infrastructure (ast-model, parser, compiler) and adds new transformation passes. No new organizational libraries needed. All changes follow Fifth's existing multi-project structure with AST metamodel as source of truth, ANTLR grammar for parsing, and visitor-based transformation pipeline.

**Key Modifications**:
1. **AST Model** (`AstMetamodel.cs`): Add `TypeParameterDef`, `TypeConstraint` hierarchy, extend `ClassDef`/`FunctionDef` with type parameters, add `TGenericParameter`/`TGenericInstance` to `FifthType` discriminated union
2. **Grammar** (`FifthLexer.g4`, `FifthParser.g4`): Add `WHERE` keyword, `type_parameter_list`, `constraint_clause` rules
3. **Parser** (`AstBuilderVisitor.cs`): Remove hardcoded list/array logic in `VisitGeneric_type_spec`, add methods to build generic AST nodes
4. **Compiler**: Two new transformation passes (type parameter resolution, type inference) inserted after symbol table building in pipeline
5. **Code Generator**: Extend Roslyn backend to emit .NET generic types with proper metadata

**Files That Will NOT Change**:
- `src/fifthlang.system/` - No built-in generic functions initially
- `global.json`, `fifthlang.sln`, `Justfile` - No build system changes

## Complexity Tracking

> **No constitution violations detected**

All complexity is **essential language feature complexity** (generic type system), not incidental organizational complexity. Constitution gates passed with justification documented in Constitution Check section above.

---

## Phase 0: Research

**Deliverables**: [research.md](research.md)

**Status**: ✅ Complete

**Summary**: Resolved 5 major technical unknowns:
1. **Type inference algorithm**: Local type inference (C# style) — predictable, matches .NET semantics
2. **Constraint validation**: Early validation during type parameter resolution pass — best error messages
3. **Type instantiation**: Canonical type cache with structural hashing — ensures type equality
4. **Roslyn mapping**: Direct mapping (Fifth generics → .NET generics 1:1) — natural interop
5. **Grammar ambiguity**: Lookahead disambiguation — no syntax changes needed

**Risks Identified**:
- Type inference performance (mitigated: caching + depth limits)
- Constraint satisfaction complexity (mitigated: comprehensive tests)
- Roslyn backend corner cases (mitigated: start minimal, defer advanced constraints)
- Grammar regressions (mitigated: full test suite validation)

---

## Phase 1: Design

**Deliverables**:
- [data-model.md](data-model.md) — AST extensions, type system additions, grammar rules
- [quickstart.md](quickstart.md) — Fifth language examples demonstrating generic syntax
- [contracts/ast-nodes.md](contracts/ast-nodes.md) — Public API contracts for new AST nodes

**Status**: ✅ Complete

**Summary**:
- **AST Model Extensions**: 4 new node types (`TypeParameterDef`, 3 constraint types), 2 modified nodes (`ClassDef`, `FunctionDef`)
- **Type System Extensions**: 2 new `FifthType` variants (`TGenericParameter`, `TGenericInstance`), `TypeParameterScope`, `TypeInferenceContext`, `GenericTypeCache`
- **Grammar Extensions**: 5 new rules (`type_parameter_list`, `type_parameter`, `constraint_clause`, `constraint_list`, `constraint`), 1 new token (`WHERE`)
- **Visitor Extensions**: Generated methods for `BaseAstVisitor`, `DefaultRecursiveDescentVisitor`
- **Diagnostics**: 6 new error codes (`GEN001` through `GEN006`)

**Key Design Decisions**:
- Generic type instances are interned (canonical cache) for fast equality checks
- Type inference uses C# semantics (local inference, not Hindley-Milner)
- Constraints validated early (during type parameter resolution pass) for better error messages
- Direct mapping to .NET generics in Roslyn backend (no erasure)

---

## Phase 2: Implementation Plan Structure

**NOTE**: Detailed task breakdown is created by `/speckit.tasks` command (not part of `/speckit.plan`).

**Recommended Implementation Order** (from research and design):

### 2.1: AST Metamodel Extensions (Foundation)
- Add `TypeParameterDef`, `TypeConstraint` hierarchy to `src/ast-model/AstMetamodel.cs`
- Extend `ClassDef` and `FunctionDef` with `TypeParameters` property
- Add `TGenericParameter` and `TGenericInstance` to `FifthType` discriminated union
- Regenerate code: `just run-generator`
- **Tests**: AST builder unit tests (`test/ast-tests/GenericAstBuilderTests.cs`)

### 2.2: Grammar Extensions (Syntax)
- Add `WHERE` keyword to `src/parser/grammar/FifthLexer.g4`
- Add `type_parameter_list`, `constraint_clause` rules to `src/parser/grammar/FifthParser.g4`
- Modify `class_definition`, `function_definition` rules to accept type parameters
- Build project (ANTLR auto-generates parser)
- **Tests**: Grammar parsing tests (`test/syntax-parser-tests/GenericSyntaxTests.cs`)

### 2.3: Parser Visitor Updates (AST Construction)
- Implement `VisitType_parameter_list`, `VisitConstraint_clause` in `src/parser/AstBuilderVisitor.cs`
- Modify `VisitGeneric_type_spec` to remove hardcoded list/array logic
- Assign `TypeParameterScope` during AST construction
- **Tests**: Parser tests with `.5th` samples (`generic_class.5th`, `generic_function.5th`)

### 2.4: Type Parameter Resolution Pass (Analysis)
- Create `src/compiler/LanguageTransformations/TypeParameterResolutionVisitor.cs`
- Implement constraint validation logic (interface, base class, constructor)
- Replace `TGenericParameter` with `TGenericInstance` after validation
- **Tests**: Type resolution unit tests (constraint violation tests)

### 2.5: Type Inference Pass (Analysis)
- Create `src/compiler/LanguageTransformations/GenericTypeInferenceVisitor.cs`
- Implement `TypeInferenceContext` and inference algorithm (local inference)
- Implement `GenericTypeCache` for type interning
- **Tests**: Type inference unit tests (function call inference, return type inference)

### 2.6: Compiler Pipeline Integration
- Register new passes in `src/compiler/ParserManager.cs` transformation pipeline
- Insert passes after symbol table building, before IL lowering
- **Tests**: End-to-end compiler tests (parse → analyze → lower → generate)

### 2.7: Roslyn Backend Extensions (Code Generation)
- Extend `src/code_generator/RoslynBackend/` to emit `TypeParameterSyntax`
- Map Fifth constraints to C# constraint clauses
- Handle generic type instantiation in IL emission
- **Tests**: Roslyn backend tests (verify emitted C# syntax tree)

### 2.8: Runtime Integration Tests (Validation)
- Create `test/runtime-integration-tests/GenericRuntimeTests.cs`
- Test end-to-end: Fifth source → compiled assembly → execution → correct results
- Test interop: Fifth generics calling .NET generic APIs (`List<T>`, `Dictionary<K,V>`)
- **GATE**: Feature is NOT complete until all runtime tests pass

### 2.9: Performance Validation
- Add benchmarks to `test/perf/` for generic type inference
- Measure compilation time increase (target: <15% for generic code)
- Measure type inference resolution time (target: <100ms per call site)
- **GATE**: Must meet performance goals before merging

### 2.10: Documentation and Examples
- Update `docs/learn5thInYMinutes.md` with generics section
- Add example programs to `docs/examples/`
- Update language specification (if formal spec exists)

---

## Post-Phase 1 Constitution Re-check

**Required**: YES (per Constitution Check section)

**Re-check After Phase 1 Design**:

### ✅ Type Inference Algorithm Complexity
- **Decision**: Local type inference (C# style)
- **Justification**: Well-understood algorithm, predictable behavior, matches .NET semantics
- **Complexity**: Justified — essential for ergonomic generic usage

### ✅ Generic Type Instantiation Caching
- **Decision**: Canonical type cache (`GenericTypeCache`)
- **Justification**: Ensures type equality (`List<int>` is same type everywhere), amortizes constraint checking cost
- **Complexity**: Justified — necessary for correct type system semantics

### ✅ Number of New Transformation Passes
- **Decision**: 2 new passes (`TypeParameterResolutionVisitor`, `GenericTypeInferenceVisitor`)
- **Justification**: Follows multi-pass compilation philosophy, clean separation of concerns
- **Complexity**: Justified — minimal pass count for feature scope

**Result**: All post-Phase 1 re-checks PASS. No constitution violations introduced by design.

---

## Agent Context Updates

**Files to Update**:
1. **`.github/copilot-instructions.md`**:
   - Add generic type support to "Active Technologies" section
   - Document new AST nodes (`TypeParameterDef`, constraints)
   - Document new FifthType variants (`TGenericParameter`, `TGenericInstance`)
   
2. **`AGENTS.md`**:
   - Add section on generic type system
   - Document new transformation passes (type parameter resolution, type inference)
   - Add validation checklist for generic feature usage
   
3. **`.specify/memory/constitution.md`** (if applicable):
   - Update "Type System" section with generic type extensions
   - Update "Multi-Pass Compilation" section with new analysis passes

**Content to Add**:
```markdown
## Generic Type Support

Fifth supports full generic type programming:

- **Generic classes**: `class Stack<T> { ... }`
- **Generic functions**: `identity<T>(x: T): T => x`
- **Type constraints**: `where T: IComparable, new()`
- **Type inference**: Automatic type argument resolution (80% of cases)
- **.NET interop**: Direct mapping to .NET generic types

**AST Nodes**:
- `TypeParameterDef` — type parameter declaration
- `InterfaceConstraint`, `BaseClassConstraint`, `ConstructorConstraint` — constraint types

**Type System**:
- `TGenericParameter` — unresolved type parameter (e.g., `T` in `class Stack<T>`)
- `TGenericInstance` — resolved generic instantiation (e.g., `Stack<int>`)

**Transformation Passes**:
- `TypeParameterResolutionVisitor` — validates constraints, resolves type parameters
- `GenericTypeInferenceVisitor` — infers type arguments from usage

**Diagnostics**: `GEN001` through `GEN006` (type argument errors, inference failures, constraint violations)
```

---

## Success Criteria Validation

**From spec.md** — all success criteria must be testable:

1. ✅ **SC-001**: Generic class instantiation compiles and runs → `test/runtime-integration-tests/GenericRuntimeTests.cs`
2. ✅ **SC-002**: Type inference success rate ≥80% → Measured in inference tests (add metrics)
3. ✅ **SC-003**: Constraint violations caught at compile time → `test/ast-tests/` constraint tests
4. ✅ **SC-004**: Nested generics compile → `test/runtime-integration-tests/` nested generic tests
5. ✅ **SC-005**: .NET generic interop works → `test/runtime-integration-tests/` interop tests
6. ✅ **SC-006**: Error messages include location + expected/actual types → Diagnostic tests
7. ✅ **SC-007**: Compilation time increase <15% → `test/perf/` benchmarks
8. ✅ **SC-008**: Type inference resolution <100ms → `test/perf/` benchmarks
9. ✅ **SC-009**: 100% backward compatible → Run full existing test suite
10. ✅ **SC-010**: Zero type safety violations → Runtime tests must not throw type errors
11. ✅ **SC-011**: Deterministic instantiation → Type cache equality tests
12. ✅ **SC-012**: Parser accepts all valid syntax → Grammar tests with edge cases
13. ✅ **SC-013**: Documentation examples compile and run → `docs/examples/` test harness

**All success criteria are testable and mapped to test artifacts.**

---

## Summary

**Phase 0 Complete**: ✅ All technical unknowns resolved  
**Phase 1 Complete**: ✅ AST model, grammar, contracts designed  
**Ready for Phase 2**: ✅ Implementation plan structure defined

**Next Steps**:
1. Run `/speckit.tasks` to generate detailed task breakdown
2. Begin Phase 2.1: AST Metamodel Extensions
3. Follow TDD workflow: Write test → Implement → Validate → Commit

**Artifacts Created**:
- [plan.md](plan.md) (this file) — Implementation plan
- [research.md](research.md) — Technical unknowns resolution
- [data-model.md](data-model.md) — AST and type system design
- [quickstart.md](quickstart.md) — Fifth language examples
- [contracts/ast-nodes.md](contracts/ast-nodes.md) — Public API contracts

**Branch**: `001-full-generics-support` (ready for implementation)
