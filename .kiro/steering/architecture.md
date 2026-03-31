---
inclusion: always
---

# Architecture Rules

Concrete, measurable rules for the Fifth compiler. Every rule includes a compliance check an agent can perform.

## Dependency & Module Boundaries

### 1. Single-Direction Dependency Flow

Project references follow this strict DAG:

```
ast-model → ast_generator → ast-generated → parser → compiler → tests
                                               ↑
                                          fifthlang.system
```

No `.csproj` under `src/` may contain a `<ProjectReference>` pointing backward in this ordering.

### 2. Generated Code Is Read-Only

Files under `src/ast-generated/` are output, not source. Any diff modifying `src/ast-generated/` MUST also change `src/ast-model/AstMetamodel.cs` or `src/ast_generator/Templates/`.

Verify: if `git diff --name-only` includes `src/ast-generated/`, it MUST also include `src/ast-model/` or `src/ast_generator/Templates/`.

### 8. Metamodel Is the Single Source of AST Shape

All AST node types, fields, and inheritance are defined in `src/ast-model/AstMetamodel.cs`. No hand-written class outside `ast-model` may subclass `AstThing` or introduce new AST node types.

Verify: search `.cs` files outside `src/ast-model/` and `src/ast-generated/` for classes inheriting `AstThing`, `Expression`, `Statement`, or `TypeRef`. Any match is non-compliant.

### 9. Backend Isolation

`LoweredAstToRoslynTranslator` is the sole bridge to Roslyn. No phase or visitor under `src/compiler/LanguageTransformations/` or `src/compiler/Pipeline/Phases/` may reference `Microsoft.CodeAnalysis`. Roslyn types must not leak into the AST model or transformation layer.

Verify: `using Microsoft.CodeAnalysis` must not appear in `src/compiler/LanguageTransformations/`, `src/compiler/Pipeline/Phases/`, `src/ast-model/`, or `src/ast-generated/`.

## Compiler Pipeline Rules

### 3. Phase Contract Integrity

Every compiler phase implements `ICompilerPhase` and declares:
- `DependsOn`: capability strings required from earlier phases (empty for root phases).
- `ProvidedCapabilities`: capability strings this phase makes available.

`TransformationPipeline.RegisterPhase` enforces at registration time that every `DependsOn` entry is already provided. A phase that reads AST state from another phase without declaring the dependency is non-compliant.

Verify: for each phase under `src/compiler/Pipeline/Phases/`, confirm every visitor/rewriter it instantiates operates only on AST state guaranteed by its declared `DependsOn`.

### 4. Phase Single-Responsibility

Each `ICompilerPhase` performs exactly one category:
- Structural linking
- Symbol resolution
- Validation/diagnostics
- Lowering/desugaring
- Type annotation

A compound phase combining sub-steps MUST document each in its XML summary and must not mix unrelated concerns.

Verify: the `Transform` method should only instantiate visitors/rewriters serving its declared category. Distinct visitor types spanning multiple categories without XML-summary justification is non-compliant.

### 5. AST Immutability Between Phases

A phase receives `AstThing` and returns a new or mutated `AstThing` via `PhaseResult`. No phase may retain a reference to the input AST and mutate it after returning. The pipeline owns the AST reference between phases.

Verify: phase `Transform` methods must not store the input `ast` parameter in instance or static fields. `PhaseResult.TransformedAst` is the only valid output path.

### 6. Lowering Direction Is Strictly Downward

Transformation phases lower high-level AST constructs toward simpler forms consumable by `LoweredAstToRoslynTranslator`. No phase may introduce a higher-level construct than what it received.

Verify: for any rewriter phase, output node types must be equal to or simpler than input node types ("simpler" = closer to what `LoweredAstToRoslynTranslator.TranslateStatement`/`TranslateExpression` directly handle).

### 10. Diagnostics Through Structured Channels Only

Phases report errors/warnings exclusively through `PhaseResult.Diagnostics` or `PhaseContext.Diagnostics`. Direct `Console.Error` writes are permitted only when `DebugHelpers.DebugEnabled` is true. No phase may write to `Console.Out`.

Verify: `Console.Error.WriteLine` in phase `Transform` methods must be guarded by `DebugHelpers.DebugEnabled`. `Console.WriteLine` or `Console.Out` calls are unconditionally non-compliant.

### 11. Deterministic Pipeline Ordering

The phase sequence in `TransformationPipeline.CreateDefault()` is the canonical compilation order. Phases must not be conditionally reordered at runtime (skipping via `PipelineOptions.SkipPhases` is permitted; reordering is not).

Verify: `CreateDefault()` contains only `RegisterPhase` calls in a fixed sequence with no conditional logic (`if`, loops, or config-driven ordering).

### 14. No Implicit State Sharing Between Phases

Phases must not communicate through static mutable state, singletons, or thread-local storage. All inter-phase data flows through the AST or `PhaseContext`. Exception: `DebugHelpers.DebugEnabled` (read-only diagnostic flag).

Verify: phase classes under `src/compiler/Pipeline/Phases/` must not declare `static` mutable fields. Visitors/rewriters they instantiate must not read/write `static` mutable fields other than `DebugHelpers`.

## Grammar & Parser Rules

### 7. Grammar Is the Single Source of Syntax Truth

All parseable Fifth syntax is defined in `FifthLexer.g4` (tokens) and `FifthParser.g4` (rules). No code outside the ANTLR grammar files may define new syntax. `AstBuilderVisitor` translates parse trees to AST but must not accept token sequences the grammar rejects.

Verify: every `Visit*` method suffix in `AstBuilderVisitor.cs` must match a named rule in `FifthParser.g4`.

### 13. Parser ↔ AST Builder Correspondence

Every named parser rule in `FifthParser.g4` producing a semantic construct MUST have a corresponding `Visit*` method in `AstBuilderVisitor.cs`, and vice versa.

Verify: extract rule names from `FifthParser.g4` (lines matching `ruleName :`). Extract `Visit*` method names from `AstBuilderVisitor.cs`. The sets must align (allowing for ANTLR alternation labels like `#labeledAlt`).

## Visitor/Rewriter Pattern Selection

### 12. Pattern Matches Operation Kind

| Operation | Base class | When to use |
|---|---|---|
| Read-only analysis (symbol tables, diagnostics, validation) | `BaseAstVisitor` | Never modifies AST |
| Type-preserving AST modification | `DefaultRecursiveDescentVisitor` | Same node types in/out |
| Cross-type rewrites, statement hoisting, desugaring | `DefaultAstRewriter` | Returns `RewriteResult` with prologue or changes node types |

A visitor returning `RewriteResult` with non-empty prologue MUST extend `DefaultAstRewriter`. A visitor that only collects information MUST NOT extend `DefaultAstRewriter` or `DefaultRecursiveDescentVisitor`.

Verify: for each visitor/rewriter under `src/compiler/LanguageTransformations/`, confirm the base class matches the operation kind. A `BaseAstVisitor` subclass mutating AST nodes, or a `DefaultAstRewriter` that never returns prologue and never changes node types, is non-compliant.


## C#/.NET Systems Programming Rules

Rules 15–28 govern allocation efficiency, async correctness, and thread safety across `src/`. These are measurable constraints grounded in .NET runtime behavior.

### 15. No Sync-Over-Async

MUST NOT call `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on `Task`/`Task<T>`. These block the thread and risk deadlocks. Refactor callers to `async` or use a dedicated synchronous API path.

Verify: search `src/**/*.cs` for `\.Result`, `\.Wait()`, `\.GetAwaiter().GetResult()` on Task expressions. Known violation: `ast_generator/Program.cs` `.Wait()` (to be remediated).

### 16. Async Methods Must Accept CancellationToken

Every `public`/`internal` method returning `Task`/`Task<T>` MUST accept a `CancellationToken` parameter (defaulting to `default` is fine). The token MUST be forwarded to every awaited call that accepts one.

Verify: for each `async` method in `src/`, confirm a `CancellationToken` parameter exists and is forwarded to awaited callees.

### 17. ConfigureAwait(false) in Library Code

All `await` expressions in library projects (`src/ast-model`, `src/ast_generator`, `src/parser`, `src/compiler`, `src/fifthlang.system`) MUST use `.ConfigureAwait(false)`. Exempt: `Program.cs` files and LSP handler methods under `src/language-server/Handlers/`.

Verify: in listed library projects, every `await` without `.ConfigureAwait(false)` is non-compliant (unless exempt).

### 18. No Unbounded Collection Growth in Visitors

Visitors/rewriters under `src/compiler/LanguageTransformations/` MUST NOT allocate `List<T>`, `Dictionary<K,V>`, or `HashSet<T>` without bounded initial capacity when the upper bound is knowable from the input AST (e.g., `ctx.Params.Count`).

Verify: `new List<T>()` without capacity where surrounding code iterates a known-size collection is non-compliant. Acceptable only when final size is genuinely unpredictable.

### 19. Span/stackalloc for Short-Lived Buffers ≤ 256 Bytes

Methods allocating `byte[]` or `char[]` of ≤256 bytes for purely local, non-escaping use MUST use `Span<T>` with `stackalloc`. Arrays that escape the method are exempt.

Verify: find `new byte[n]`/`new char[n]` where n ≤ 256 and the array doesn't escape. These should be `stackalloc` + `Span<T>`.

### 20. ArrayPool for Transient Buffers > 256 Bytes

Transient `T[]` allocations >256 bytes (not stored beyond method/`using` scope) MUST rent from `ArrayPool<T>.Shared` and return in a `finally` block or via `IDisposable`. Reference pattern: `PooledList<T>` in `compiler/Validation/GuardValidation/Infrastructure/`.

Verify: find `new T[n]` where n > 256 (or variable-length) and the array doesn't escape. These should use `ArrayPool<T>.Shared.Rent`/`Return`.

### 21. No LINQ Materialization in Per-Node Visitor Methods

`Visit*`/`Rewrite*` methods executing per AST node MUST NOT call `.ToList()`, `.ToArray()`, `.ToDictionary()` on sequences consumed lazily or iterated exactly once. Materialization is permitted only when the result is stored, indexed, or iterated multiple times.

Verify: in `Visit*`/`Rewrite*` methods under `src/compiler/LanguageTransformations/`, `.ToList()`/`.ToArray()`/`.ToDictionary()` on a collection iterated exactly once and never stored is non-compliant.

### 22. String Building via Interpolation or StringBuilder

String construction in loops MUST use `StringBuilder` (or `string.Create`/`DefaultInterpolatedStringHandler`). Repeated `+` or `string.Concat` inside loop bodies is non-compliant (each iteration allocates a new string).

Verify: find `for`/`foreach`/`while` bodies with `string` `+` or `string.Concat` accumulating across iterations.

### 23. ConcurrentDictionary Requires Documented Contention Strategy

Every `ConcurrentDictionary<K,V>` field MUST have an XML doc or inline comment explaining: (a) which threads read/write it, (b) whether `GetOrAdd`/`AddOrUpdate` factories have side effects, (c) expected contention level.

Verify: each `ConcurrentDictionary` field in `src/` must have an adjacent comment addressing thread access and contention.

### 24. Lock Scope Minimization

`lock` blocks MUST contain only minimum statements for the invariant. No I/O, `await`, unbounded method calls, allocations (including LINQ materialization or string interpolation) inside `lock`. Reference: `GenericTypeCache._lruLock` pattern.

Verify: `lock` blocks in `src/` must not contain `await`, `Console.*`, `File.*`, `.ToList()`/`.ToArray()`, or `$"..."`.

### 25. No Fire-and-Forget Tasks

`async` methods MUST NOT be called without awaiting, storing, or explicitly discarding the returned `Task`. Minimum acceptable pattern: `_ = SomeAsync(); // fire-and-forget: <reason>`.

Verify: call sites of `Task`/`Task<T>`-returning methods where the return is neither awaited, assigned, nor discarded with `_ =` are non-compliant.

### 26. IDisposable Resources Must Use `using`

Every local `IDisposable`/`IAsyncDisposable` variable MUST use `using` (declaration or block form). Manual `try/finally/Dispose` is acceptable only with documented conditional disposal logic.

Verify: local variables of `IDisposable` types (`Stream`, `StreamReader`, `HttpClient`, `PooledList<T>`, etc.) not declared with `using` and not disposed in `finally` are non-compliant.

### 27. Immutable AST Node Construction via Builders or `with`

AST nodes (from `AstMetamodel.cs` / `ast-generated/`) MUST be constructed using generated builders or C# `with` expressions. Direct property mutation after construction is non-compliant. Exempt mutable infrastructure properties: `Parent`, `SymbolTable`, `Annotations`, `Type`, `Location`.

Verify: in `src/compiler/` and `src/parser/`, assignment to AST node properties outside a builder's `Build()` or `with` expression (excluding exempt properties) is non-compliant.

### 28. No Closure Allocation in Hot Loops

Lambdas/local functions inside loops iterating AST node collections (e.g., `foreach (var stmt in block.Statements)`) MUST NOT capture outer variables. Captured variables cause per-iteration closure allocation. Refactor to `static` local functions or explicit-parameter methods.

Verify: in visitor/rewriter methods under `src/compiler/LanguageTransformations/`, lambdas inside `for`/`foreach`/`while` referencing variables declared outside the loop are non-compliant.
