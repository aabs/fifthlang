---
id: steering-architecture
title: Architecture Rules
inclusion: always
---

# Architecture Rules

:::rule id="ARCH-001" severity="warning" category="governance" domain="architecture"
Architecture guidance for the Fifth compiler must be concrete and measurable. Every rule in this document must include a compliance check an agent can perform.
:::

## Dependency & Module Boundaries

:::rule id="ARCH-002" severity="error" category="dependency" domain="architecture"
Project references follow this strict DAG:

```text
ast-model -> ast_generator -> ast-generated -> parser -> compiler -> tests
                                               ^
                                          fifthlang.system
```

No `.csproj` under `src/` may contain a `<ProjectReference>` pointing backward in this ordering.

Verify: inspect project references under `src/` and reject any backward edge relative to this DAG.
:::

:::rule id="ARCH-003" severity="error" category="generation" domain="architecture"
Files under `src/ast-generated/` are output, not source. Any diff modifying `src/ast-generated/` must also change `src/ast-model/AstMetamodel.cs` or `src/ast_generator/Templates/`.

Verify: if `git diff --name-only` includes `src/ast-generated/`, it must also include `src/ast-model/` or `src/ast_generator/Templates/`.
:::

:::rule id="ARCH-004" severity="error" category="ast" domain="architecture"
All AST node types, fields, and inheritance are defined in `src/ast-model/AstMetamodel.cs`. No hand-written class outside `ast-model` may subclass `AstThing` or introduce new AST node types.

Verify: search `.cs` files outside `src/ast-model/` and `src/ast-generated/` for classes inheriting `AstThing`, `Expression`, `Statement`, or `TypeRef`. Any match is non-compliant.
:::

:::rule id="ARCH-005" severity="error" category="backend" domain="architecture"
`LoweredAstToRoslynTranslator` is the sole bridge to Roslyn. No phase or visitor under `src/compiler/LanguageTransformations/` or `src/compiler/Pipeline/Phases/` may reference `Microsoft.CodeAnalysis`. Roslyn types must not leak into the AST model or transformation layer.

Verify: `using Microsoft.CodeAnalysis` must not appear in `src/compiler/LanguageTransformations/`, `src/compiler/Pipeline/Phases/`, `src/ast-model/`, or `src/ast-generated/`.
:::

## Compiler Pipeline Rules

:::rule id="ARCH-006" severity="error" category="pipeline" domain="architecture"
Every compiler phase implements `ICompilerPhase` and declares:

- `DependsOn`: capability strings required from earlier phases.
- `ProvidedCapabilities`: capability strings this phase makes available.

`TransformationPipeline.RegisterPhase` enforces at registration time that every `DependsOn` entry is already provided. A phase that reads AST state from another phase without declaring the dependency is non-compliant.

Verify: for each phase under `src/compiler/Pipeline/Phases/`, confirm every visitor or rewriter it instantiates operates only on AST state guaranteed by its declared `DependsOn`.
:::

:::rule id="ARCH-007" severity="error" category="pipeline" domain="architecture"
Each `ICompilerPhase` performs exactly one category of work:

- Structural linking
- Symbol resolution
- Validation and diagnostics
- Lowering and desugaring
- Type annotation

A compound phase combining sub-steps must document each in its XML summary and must not mix unrelated concerns.

Verify: the `Transform` method should only instantiate visitors or rewriters serving its declared category. Distinct visitor types spanning multiple categories without XML-summary justification are non-compliant.
:::

:::rule id="ARCH-008" severity="error" category="pipeline" domain="architecture"
A phase receives `AstThing` and returns a new or mutated `AstThing` via `PhaseResult`. No phase may retain a reference to the input AST and mutate it after returning. The pipeline owns the AST reference between phases.

Verify: phase `Transform` methods must not store the input `ast` parameter in instance or static fields. `PhaseResult.TransformedAst` is the only valid output path.
:::

:::rule id="ARCH-009" severity="error" category="lowering" domain="architecture"
Transformation phases lower high-level AST constructs toward simpler forms consumable by `LoweredAstToRoslynTranslator`. No phase may introduce a higher-level construct than what it received.

Verify: for any rewriter phase, output node types must be equal to or simpler than input node types, where simpler means closer to what `LoweredAstToRoslynTranslator.TranslateStatement` or `TranslateExpression` directly handle.
:::

:::rule id="ARCH-010" severity="error" category="diagnostics" domain="architecture"
Phases report errors and warnings exclusively through `PhaseResult.Diagnostics` or `PhaseContext.Diagnostics`. Direct `Console.Error` writes are permitted only when `DebugHelpers.DebugEnabled` is true. No phase may write to `Console.Out`.

Verify: `Console.Error.WriteLine` in phase `Transform` methods must be guarded by `DebugHelpers.DebugEnabled`. `Console.WriteLine` or `Console.Out` calls are non-compliant.
:::

:::rule id="ARCH-011" severity="error" category="pipeline" domain="architecture"
The phase sequence in `TransformationPipeline.CreateDefault()` is the canonical compilation order. Phases must not be conditionally reordered at runtime. Skipping via `PipelineOptions.SkipPhases` is permitted, but reordering is not.

Verify: `CreateDefault()` contains only `RegisterPhase` calls in a fixed sequence with no conditional logic such as `if`, loops, or configuration-driven ordering.
:::

:::rule id="ARCH-012" severity="error" category="pipeline" domain="architecture"
Phases must not communicate through static mutable state, singletons, or thread-local storage. All inter-phase data flows through the AST or `PhaseContext`. The only exception is `DebugHelpers.DebugEnabled` as a read-only diagnostic flag.

Verify: phase classes under `src/compiler/Pipeline/Phases/` must not declare `static` mutable fields. Visitors or rewriters they instantiate must not read or write `static` mutable fields other than `DebugHelpers`.
:::

## Grammar & Parser Rules

:::rule id="ARCH-013" severity="error" category="parser" domain="architecture"
All parseable Fifth syntax is defined in `FifthLexer.g4` for tokens and `FifthParser.g4` for rules. No code outside the ANTLR grammar files may define new syntax. `AstBuilderVisitor` translates parse trees to AST but must not accept token sequences the grammar rejects.

Verify: every `Visit*` method suffix in `AstBuilderVisitor.cs` must match a named rule in `FifthParser.g4`.
:::

:::rule id="ARCH-014" severity="error" category="parser" domain="architecture"
Every named parser rule in `FifthParser.g4` that produces a semantic construct must have a corresponding `Visit*` method in `AstBuilderVisitor.cs`, and vice versa.

Verify: extract rule names from `FifthParser.g4` using lines matching `ruleName :`. Extract `Visit*` method names from `AstBuilderVisitor.cs`. The sets must align, allowing for ANTLR alternation labels such as `#labeledAlt`.
:::

## Visitor/Rewriter Pattern Selection

:::rule id="ARCH-015" severity="error" category="visitor" domain="architecture"
Use the visitor and rewriter base classes according to the operation being performed:

| Operation | Base class | When to use |
|---|---|---|
| Read-only analysis | `BaseAstVisitor` | Never modifies AST |
| Type-preserving AST modification | `DefaultRecursiveDescentVisitor` | Same node types in and out |
| Cross-type rewrites, statement hoisting, desugaring | `DefaultAstRewriter` | Returns `RewriteResult` with prologue or changes node types |

A visitor returning `RewriteResult` with non-empty prologue must extend `DefaultAstRewriter`. A visitor that only collects information must not extend `DefaultAstRewriter` or `DefaultRecursiveDescentVisitor`.

Verify: for each visitor or rewriter under `src/compiler/LanguageTransformations/`, confirm the base class matches the operation kind. A `BaseAstVisitor` subclass mutating AST nodes, or a `DefaultAstRewriter` that never returns prologue and never changes node types, is non-compliant.
:::