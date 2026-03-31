---
inclusion: auto
---

# Architecture Principles

Concrete, measurable rules governing the Fifth compiler architecture. Every rule has a compliance test an agent can perform.

## 1. Single-Direction Dependency Flow

Project references MUST follow this strict DAG:

```
ast-model → ast_generator → ast-generated → parser → compiler → tests
                                               ↑
                                          fifthlang.system
```

Compliance check: no `.csproj` under `src/` may contain a `<ProjectReference>` that points backward in this ordering (e.g., `ast-model` referencing `parser`). 

## 2. Generated Code Is Read-Only Output

Files under `src/ast-generated/` are compiler output, not source. Any diff that modifies a file in `src/ast-generated/` MUST also contain a corresponding change in `src/ast-model/AstMetamodel.cs` or `src/ast_generator/Templates/`.

Compliance check: if `git diff --name-only` includes any path under `src/ast-generated/`, it MUST also include at least one path under `src/ast-model/` or `src/ast_generator/Templates/`.

## 3. Phase Contract Integrity

Every compiler phase MUST implement `ICompilerPhase` and declare:
- `DependsOn`: capability strings required from earlier phases (empty for root phases).
- `ProvidedCapabilities`: capability strings this phase makes available to later phases.

`TransformationPipeline.RegisterPhase` enforces at registration time that every `DependsOn` entry is already provided. A phase that silently reads AST state established by another phase without declaring the dependency is non-compliant.

Compliance check: for each phase class under `src/compiler/Pipeline/Phases/`, verify that every visitor or rewriter it instantiates operates only on AST state guaranteed by its declared `DependsOn` capabilities.

## 4. Phase Single-Responsibility

Each `ICompilerPhase` MUST perform exactly one logical operation category:
- structural linking, OR
- symbol resolution, OR
- validation/diagnostics, OR
- lowering/desugaring, OR
- type annotation.

A compound phase (e.g., `TypeAnnotationPhase`) that combines sub-steps MUST document each sub-step in its XML summary and MUST not mix unrelated concerns (e.g., a lowering phase must not also perform unrelated validation).

Compliance check: the `Transform` method of a phase should only instantiate visitors/rewriters that serve its declared category. Count distinct visitor/rewriter types; if they span more than one category without documented justification in the XML summary, the phase is non-compliant.

## 5. AST Immutability Between Phases

A phase receives an `AstThing` and returns a new or mutated `AstThing` via `PhaseResult`. No phase may retain a reference to the input AST and mutate it after returning. The pipeline owns the AST reference between phases.

Compliance check: phase `Transform` methods must not store the input `ast` parameter in instance or static fields. The returned `PhaseResult.TransformedAst` is the only valid output path.

## 6. Lowering Direction Is Strictly Downward

Transformation phases MUST lower high-level AST constructs toward simpler forms consumable by `LoweredAstToRoslynTranslator`. No phase may introduce a higher-level construct than what it received (e.g., a phase must not synthesize a `DestructuringExp` from a `VarDeclStatement`).

Compliance check: for any rewriter phase, the output node types must be equal to or simpler than the input node types. "Simpler" means closer to what `LoweredAstToRoslynTranslator.TranslateStatement` and `TranslateExpression` directly handle.

## 7. Grammar Is the Single Source of Syntax Truth

All parseable Fifth syntax is defined exclusively in `FifthLexer.g4` (tokens) and `FifthParser.g4` (rules). No code outside the ANTLR grammar files may define new syntax forms. `AstBuilderVisitor` translates parse trees to AST but must not accept token sequences that the grammar rejects.

Compliance check: every parser rule referenced in `AstBuilderVisitor.cs` must correspond to a named rule in `FifthParser.g4`. Any `Visit*` method in `AstBuilderVisitor` whose suffix does not match a parser rule name is non-compliant.

## 8. Metamodel Is the Single Source of AST Shape

All AST node types, their fields, and their inheritance hierarchy are defined in `src/ast-model/AstMetamodel.cs`. No hand-written class outside `ast-model` may subclass `AstThing` or introduce new AST node types.

Compliance check: search all `.cs` files outside `src/ast-model/` and `src/ast-generated/` for classes inheriting from `AstThing`, `Expression`, `Statement`, or `TypeRef`. Any match is non-compliant.

## 9. Backend Isolation

`LoweredAstToRoslynTranslator` is the sole bridge between the AST domain and Roslyn. No compiler phase or visitor under `src/compiler/LanguageTransformations/` or `src/compiler/Pipeline/Phases/` may reference `Microsoft.CodeAnalysis` namespaces. Roslyn types must not leak into the AST model or transformation layer.

Compliance check: `using Microsoft.CodeAnalysis` must not appear in any file under `src/compiler/LanguageTransformations/`, `src/compiler/Pipeline/Phases/`, `src/ast-model/`, or `src/ast-generated/`.

## 10. Diagnostic Output Through Structured Channels Only

Compiler phases MUST report errors and warnings exclusively through `PhaseResult.Diagnostics` (returned from `Transform`) or `PhaseContext.Diagnostics`. Direct writes to `Console.Error` are permitted only when `DebugHelpers.DebugEnabled` is true. No phase may write to `Console.Out`.

Compliance check: in phase `Transform` methods, `Console.Error.WriteLine` calls must be guarded by `DebugHelpers.DebugEnabled`. `Console.WriteLine` or `Console.Out` calls are unconditionally non-compliant.

## 11. Deterministic Pipeline Ordering

The phase sequence in `TransformationPipeline.CreateDefault()` is the canonical compilation order. Phases must not be conditionally reordered at runtime (skipping via `PipelineOptions.SkipPhases` is permitted; reordering is not). Adding a new phase requires inserting it at a specific position with a comment indicating its phase number.

Compliance check: `CreateDefault()` must contain only `RegisterPhase` calls in a fixed sequence with no conditional logic (no `if`, no loops, no configuration-driven ordering).

## 12. Visitor Pattern Selection Matches Operation Kind

- Read-only analysis (symbol tables, diagnostics, validation) → `BaseAstVisitor`
- Type-preserving AST modification → `DefaultRecursiveDescentVisitor`
- Cross-type rewrites, statement hoisting, or desugaring → `DefaultAstRewriter`

A visitor that returns `RewriteResult` with non-empty prologue MUST extend `DefaultAstRewriter`. A visitor that only collects information without modifying the AST MUST NOT extend `DefaultAstRewriter` or `DefaultRecursiveDescentVisitor`.

Compliance check: for each visitor/rewriter under `src/compiler/LanguageTransformations/`, verify the base class matches the operation kind. A `BaseAstVisitor` subclass that mutates AST nodes is non-compliant. A `DefaultAstRewriter` subclass that never returns prologue statements and never changes node types should be a `DefaultRecursiveDescentVisitor` instead.

## 13. Parser ↔ AST Builder Correspondence

Every named parser rule in `FifthParser.g4` that produces a semantic construct (not a pure grouping/alternation rule) MUST have a corresponding `Visit*` method in `AstBuilderVisitor.cs`. Conversely, every `Visit*` method in `AstBuilderVisitor.cs` MUST correspond to a parser rule.

Compliance check: extract rule names from `FifthParser.g4` (lines matching `ruleName :`). Extract `Visit*` method names from `AstBuilderVisitor.cs`. The sets must align (allowing for ANTLR-generated alternation labels like `#labeledAlt`).

## 14. No Implicit State Sharing Between Phases

Phases must not communicate through static mutable state, ambient singletons, or thread-local storage. All inter-phase data flows through the AST itself or through `PhaseContext`. The only exception is `DebugHelpers.DebugEnabled` (a read-only diagnostic flag).

Compliance check: phase classes under `src/compiler/Pipeline/Phases/` must not declare `static` mutable fields. Visitor/rewriter classes they instantiate must not read from or write to `static` mutable fields other than `DebugHelpers`.

---

## C#/.NET Systems Programming Rules

Rules 15–28 govern allocation efficiency, async correctness, and thread safety across the entire `src/` tree. These are measurable constraints grounded in .NET runtime behaviour.

## 15. No Sync-Over-Async

Code MUST NOT call `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on a `Task` or `Task<T>`. These block the calling thread and risk deadlocks under `SynchronizationContext`. If a synchronous call site needs an async result, refactor the caller to be `async` or use a dedicated synchronous API path that never constructs a `Task`.

Compliance check: search all `.cs` files under `src/` for `\.Result`, `\.Wait()`, and `\.GetAwaiter().GetResult()`. Any match on a `Task`/`Task<T>` expression is non-compliant. (The existing `ast_generator/Program.cs` `.Wait()` call is a known violation to be remediated.)

## 16. Async Methods Must Accept CancellationToken

Every `public` or `internal` method returning `Task` or `Task<T>` MUST accept a `CancellationToken` parameter (defaulting to `default` is acceptable). The token MUST be forwarded to every awaited call that accepts one. Methods that ignore the token after accepting it are non-compliant.

Compliance check: for each `async` method in `src/`, verify a `CancellationToken` parameter exists. Then verify every `await` expression within the method body passes the token to the callee (where the callee's signature accepts one).

## 17. ConfigureAwait(false) in Library Code

All `await` expressions in library projects (`src/ast-model`, `src/ast_generator`, `src/parser`, `src/compiler`, `src/fifthlang.system`) MUST use `.ConfigureAwait(false)`. Only application entry points (`Program.cs`) and LSP handler methods (which require `SynchronizationContext` capture) are exempt.

Compliance check: in the listed library projects, every `await` expression that does not chain `.ConfigureAwait(false)` is non-compliant, unless the containing method is in a file named `Program.cs` or under `src/language-server/Handlers/`.

## 18. No Unbounded Collection Growth in Visitors

Visitor and rewriter classes under `src/compiler/LanguageTransformations/` MUST NOT allocate collections (`List<T>`, `Dictionary<K,V>`, `HashSet<T>`) without a bounded initial capacity when the upper bound is knowable from the input AST. When the element count is available (e.g., `ctx.Params.Count`, `ctx.Functions.Count`), pass it as the capacity argument.

Compliance check: for each `new List<T>()` (no capacity argument) in a visitor/rewriter, determine whether the surrounding code iterates a collection of known size to populate it. If so, the missing capacity hint is non-compliant. `new List<T>()` is acceptable only when the final size is genuinely unpredictable.

## 19. Prefer Span/stackalloc for Short-Lived Buffers ≤ 256 Bytes

Methods that allocate `byte[]` or `char[]` arrays of 256 bytes or fewer for purely local, non-escaping use MUST use `Span<T>` with `stackalloc` instead. Arrays that escape the method (returned, stored in fields, passed to APIs requiring `T[]`) are exempt.

Compliance check: find `new byte[n]` or `new char[n]` where `n ≤ 256` and the array does not escape the declaring method. These should be `stackalloc` + `Span<T>`.

## 20. ArrayPool for Transient Buffers > 256 Bytes

Methods that allocate `T[]` arrays larger than 256 bytes for transient use (not stored beyond the method or a `using` scope) MUST rent from `ArrayPool<T>.Shared` and return the buffer in a `finally` block or via `IDisposable`. The existing `PooledList<T>` in `compiler/Validation/GuardValidation/Infrastructure/` is the reference pattern.

Compliance check: find `new T[n]` where `n > 256` (or variable-length) and the array does not escape. These should use `ArrayPool<T>.Shared.Rent`/`Return`.

## 21. No LINQ Materialisation in Per-Node Visitor Methods

Visitor `Visit*` and rewriter `Rewrite*`/`Visit*` methods that execute once per AST node MUST NOT call `.ToList()`, `.ToArray()`, or `.ToDictionary()` on sequences that could be consumed lazily or whose result is immediately iterated exactly once. Materialisation is permitted only when the result is stored, indexed, or iterated multiple times.

Compliance check: in each `Visit*`/`Rewrite*` method under `src/compiler/LanguageTransformations/`, find `.ToList()`, `.ToArray()`, `.ToDictionary()` calls. If the materialised collection is iterated exactly once and never stored or indexed, it is non-compliant.

## 22. String Building via Interpolation or StringBuilder, Not Concatenation

Code that builds strings in a loop MUST use `StringBuilder` or equivalent (`string.Create`, `DefaultInterpolatedStringHandler`). Repeated `+` or `string.Concat` inside a loop body is non-compliant because each iteration allocates a new `string`.

Compliance check: find `for`/`foreach`/`while` loop bodies that contain `string` `+` or `string.Concat` where the result accumulates across iterations. These must use `StringBuilder`.

## 23. ConcurrentDictionary Requires Documented Contention Strategy

Every `ConcurrentDictionary<K,V>` field MUST have an XML doc comment or inline comment explaining: (a) which threads read/write it, (b) whether `GetOrAdd`/`AddOrUpdate` value factories have side effects, and (c) the expected contention level. Undocumented concurrent collections are non-compliant.

Compliance check: for each `ConcurrentDictionary` field declaration in `src/`, verify an adjacent comment or XML doc exists that addresses thread access pattern and contention.

## 24. Lock Scope Minimisation

`lock` blocks MUST contain only the minimum statements required to maintain the invariant. No I/O, no `await`, no method calls with unbounded execution time, and no allocations (including LINQ materialisation or string interpolation) inside a `lock` body. The `GenericTypeCache._lruLock` pattern is the reference: lock protects only linked-list pointer manipulation and dictionary mutation.

Compliance check: for each `lock` block in `src/`, verify the body contains no `await`, no `Console.*`, no `File.*`, no `.ToList()`/`.ToArray()`, and no string interpolation (`$"..."`).

## 25. No Fire-and-Forget Tasks

`async` methods MUST NOT be called without awaiting, storing, or explicitly discarding the returned `Task` with a comment explaining why. An unawaited `Task` silently swallows exceptions. `_ = SomeAsync()` with a `// fire-and-forget: <reason>` comment is the minimum acceptable pattern.

Compliance check: find call sites of methods returning `Task`/`Task<T>` where the return value is neither `await`ed, assigned, nor explicitly discarded with `_ =`. Any such call is non-compliant.

## 26. IDisposable Resources Must Use `using`

Every local variable whose type implements `IDisposable` or `IAsyncDisposable` MUST be declared with `using` (declaration or block form). Manual `try/finally/Dispose` is acceptable only when conditional disposal logic is required and documented.

Compliance check: find local variable declarations of `IDisposable` types (e.g., `Stream`, `StreamReader`, `StreamWriter`, `HttpClient`, `PooledList<T>`) that are not declared with `using` and not disposed in a `finally` block.

## 27. Immutable AST Node Construction via Builders or `with`

AST nodes (types defined in `AstMetamodel.cs` and generated in `ast-generated/`) MUST be constructed using their generated builder or C# `with` expressions. Direct property mutation after construction (e.g., `node.Foo = bar;` outside a builder or `with`) is non-compliant because it breaks referential transparency assumptions in the pipeline.

Compliance check: in `src/compiler/` and `src/parser/`, find assignment statements targeting properties of AST node types (classes from `ast` or `ast_generated` namespaces) that are not inside a builder's `Build()` method or a `with` expression. Assignments to `Parent`, `SymbolTable`, `Annotations`, `Type`, and `Location` are exempt (these are explicitly mutable infrastructure properties).

## 28. No Closure Allocation in Hot Loops

Lambda expressions and local functions MUST NOT capture outer variables when used inside loops that iterate over AST node collections (e.g., `foreach (var stmt in block.Statements)`). Captured variables cause the compiler to allocate a closure object per iteration. Extract the lambda to a `static` local function or a method that takes parameters explicitly.

Compliance check: in visitor/rewriter methods under `src/compiler/LanguageTransformations/`, find lambda expressions inside `for`/`foreach`/`while` bodies that reference variables declared outside the loop. These must be refactored to `static` lambdas or explicit-parameter methods.
