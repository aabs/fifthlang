---
description: code-generation-rules
inclusion: always
---
## Generation
- GEN-001: The AST generator is authoritative for all files under `src/ast-generated/`. These files must never be hand-edited.
- GEN-002: The primary generated files are:

- `builders.generated.cs` for builder pattern classes
- `visitors.generated.cs` for visitor pattern classes
- `rewriter.generated.cs` for lowering-oriented rewriters
- `typeinference.generated.cs` for type inference support
## Workflow
- GEN-003: To change generated AST output:

1. Edit `src/ast-model/AstMetamodel.cs`
2. Update templates under `src/ast_generator/Templates/` when template behavior must change
3. Regenerate with `just run-generator`
4. Build the full solution with `dotnet build fifthlang.sln`
## Design
- GEN-004: `AstMetamodel.cs` defines rich, high-level constructs that mirror source language features. These constructs are lowered through transformation passes before Roslyn code generation.
## Visitor
- GEN-005: Use `BaseAstVisitor` for read-only analysis such as symbol tables, diagnostics, and validation. This pattern must not modify the AST.
- GEN-006: Use `DefaultRecursiveDescentVisitor` for type-preserving AST modifications. This pattern must not change node types or hoist statements.
- GEN-007: Use `DefaultAstRewriter` for statement-level desugaring, cross-type rewrites, and expression hoisting. This is the preferred pattern for new lowering passes because it returns `RewriteResult` with a node and prologue.

Choose this pattern when introducing temporary variables, breaking down high-level constructs, transforming expression types, or performing any lowering that requires statement insertion.
## Reference
- GEN-008: Use `src/ast_generator/README.md` as the detailed reference for visitor and rewriter pattern selection.
## Review
- GEN-009: Any pull request modifying `src/ast-generated/` must include:

- The upstream metamodel or template changes
- The regeneration command used
- Confirmation that the generated files were not hand-edited
