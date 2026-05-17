---
id: steering-code-generation
title: AST Code Generation Rules
inclusion: fileMatch
fileMatchPattern: "src/ast-model/**,src/ast-generated/**,src/ast_generator/**"
---

# AST Code Generation Rules

## Generator-as-Source-of-Truth

:::rule id="GEN-001" category="generation" 
The AST generator is authoritative for all files under `src/ast-generated/`. These files must never be hand-edited.
:::

:::rule id="GEN-002" mandatory="false" category="generation" 
The primary generated files are:

- `builders.generated.cs` for builder pattern classes
- `visitors.generated.cs` for visitor pattern classes
- `rewriter.generated.cs` for lowering-oriented rewriters
- `typeinference.generated.cs` for type inference support
:::

## How to Change Generated Code

:::rule id="GEN-003" category="workflow" 
To change generated AST output:

1. Edit `src/ast-model/AstMetamodel.cs`
2. Update templates under `src/ast_generator/Templates/` when template behavior must change
3. Regenerate with `just run-generator`
4. Build the full solution with `dotnet build fifthlang.sln`
:::

## AST Design

:::rule id="GEN-004" mandatory="false" category="design" 
`AstMetamodel.cs` defines rich, high-level constructs that mirror source language features. These constructs are lowered through transformation passes before Roslyn code generation.
:::

## Visitor/Rewriter Pattern Selection

:::rule id="GEN-005" category="visitor" 
Use `BaseAstVisitor` for read-only analysis such as symbol tables, diagnostics, and validation. This pattern must not modify the AST.
:::

:::rule id="GEN-006" category="visitor" 
Use `DefaultRecursiveDescentVisitor` for type-preserving AST modifications. This pattern must not change node types or hoist statements.
:::

:::rule id="GEN-007" category="visitor" 
Use `DefaultAstRewriter` for statement-level desugaring, cross-type rewrites, and expression hoisting. This is the preferred pattern for new lowering passes because it returns `RewriteResult` with a node and prologue.

Choose this pattern when introducing temporary variables, breaking down high-level constructs, transforming expression types, or performing any lowering that requires statement insertion.
:::

:::rule id="GEN-008" mandatory="false" category="reference" 
Use `src/ast_generator/README.md` as the detailed reference for visitor and rewriter pattern selection.
:::

## PR Requirements

:::rule id="GEN-009" category="review" 
Any pull request modifying `src/ast-generated/` must include:

- The upstream metamodel or template changes
- The regeneration command used
- Confirmation that the generated files were not hand-edited
:::
