---
id: steering-transformation-pipeline
title: Language Transformation Pipeline
inclusion: fileMatch
fileMatchPattern: "src/compiler/LanguageTransformations/**,src/compiler/ParserManager.cs"
---

# Language Transformation Pipeline

## Transformation Pass Order

:::rule id="PIPE-001" severity="error" category="pipeline" domain="transformations"
The compiler applies these passes sequentially in `ParserManager.cs`:

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
:::

## Design Principles

:::rule id="PIPE-002" severity="error" category="design" domain="transformations"
Each visitor or rewriter in the transformation pipeline must have one well-defined responsibility.
:::

:::rule id="PIPE-003" severity="error" category="dependency" domain="transformations"
Transformation passes are order-dependent, and later passes may rely on invariants established by earlier passes.
:::

:::rule id="PIPE-004" severity="warning" category="design" domain="transformations"
Prefer several simple, comprehensible passes over a single pass that mixes unrelated transformation logic.
:::

:::rule id="PIPE-005" severity="warning" category="documentation" domain="transformations"
Document dependencies between transformation passes whenever later stages rely on earlier ones.
:::

:::rule id="PIPE-006" severity="error" category="correctness" domain="transformations"
Each transformation pass must preserve AST validity and type safety.
:::

:::rule id="PIPE-007" severity="warning" category="design" domain="transformations"
Prefer expressing language adaptation as AST transformations rather than pushing additional complexity into code generation.
:::

## Adding a New Transformation

:::rule id="PIPE-008" severity="error" category="workflow" domain="transformations"
To add a new transformation:

1. Create the visitor or rewriter in `src/compiler/LanguageTransformations/`
2. Choose the correct base pattern using the code-generation steering guidance
3. Register the transformation in `src/compiler/ParserManager.cs`
4. Add tests in `test/ast-tests/` or `test/runtime-integration-tests/`
5. Build the solution and run the full test suite
:::

## Code Generation

:::rule id="PIPE-009" severity="error" category="code-generation" domain="transformations"
The compiler uses `LoweredAstToRoslynTranslator` to emit C# syntax trees from the lowered AST for Roslyn compilation and PE or PDB emission. Roslyn-generated PDBs must include full line-and-column sequence points so debugging fidelity is preserved.
:::
