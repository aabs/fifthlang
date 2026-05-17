---
description: transformation-pipeline
inclusion: always
---
## Pipeline
- PIPE-001: The compiler applies these passes sequentially in `ParserManager.cs`:

1. `TreeLinkageVisitor` for parent-child relationships
2. `BuiltinInjectorVisitor` for built-in function definitions
3. `ClassCtorInserter` for default constructors
4. `SymbolTableBuilderVisitor` for scoping symbol tables
5. `PropertyToFieldExpander` for property syntax expansion
6. `OverloadGatheringVisitor` for overload grouping
7. `OverloadTransformingVisitor` for guard and subclause transformation
8. `DestructuringVisitor` for destructuring property references
9. `DestructuringLoweringRewriter` for lowering destructuring into variable declarations
10. `TypeAnnotationVisitor` for type inference and annotation
## Design
- PIPE-002: Each visitor or rewriter in the transformation pipeline must have one well-defined responsibility.
- PIPE-004: Prefer several simple, comprehensible passes over a single pass that mixes unrelated transformation logic.
- PIPE-007: Prefer expressing language adaptation as AST transformations rather than pushing additional complexity into code generation.
## Dependency
- PIPE-003: Transformation passes are order-dependent, and later passes may rely on invariants established by earlier passes.
## Documentation
- PIPE-005: Document dependencies between transformation passes whenever later stages rely on earlier ones.
## Correctness
- PIPE-006: Each transformation pass must preserve AST validity and type safety.
## Workflow
- PIPE-008: To add a new transformation:

1. Create the visitor or rewriter in `src/compiler/LanguageTransformations/`
2. Choose the correct base pattern using the code-generation steering guidance
3. Register the transformation in `src/compiler/ParserManager.cs`
4. Add tests in `test/ast-tests/` or `test/runtime-integration-tests/`
5. Build the solution and run the full test suite
## Code Generation
- PIPE-009: The compiler uses `LoweredAstToRoslynTranslator` to emit C# syntax trees from the lowered AST for Roslyn compilation and PE or PDB emission. Roslyn-generated PDBs must include full line-and-column sequence points so debugging fidelity is preserved.
