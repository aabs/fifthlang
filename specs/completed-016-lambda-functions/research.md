# Research: Lambda Functions Implementation

## Decisions & Rationale

### 1. Closure Representation
**Decision**: Use "Class-per-Closure" strategy for internal representation, and `System.Func<>`/`System.Action<>` delegates for public API boundaries.
**Rationale**: 
- **Class-per-Closure**: Allows full control over captured variables (fields) and provides a concrete type for the `Apply` method. This is necessary for the "Defunctionalisation" pass where we replace function types with `IClosure<T>` interfaces. It also aligns with how C# and Java implement lambdas under the hood.
- **Public Delegates**: .NET interoperability requires standard delegates (`Func`, `Action`). Wrapper functions will bridge the gap between the internal `IClosure` world and the external `Delegate` world.

### 2. Tail Call Optimization (TCO)
**Decision**: Implement "Self-Recursion Elimination" (transform to `while` loop) only.
**Rationale**: 
- Full TCO (Trampolines) incurs significant runtime overhead for all function calls. 
- Self-recursion covers the vast majority of functional programming use cases (e.g., `map`, `filter`, `fold` on lists).
- This is a pragmatic trade-off between performance and feature completeness.

### 3. Void Return Handling
**Decision**: Map `void` return type to `TVoidType` in the type system and use `IAction` interfaces at runtime.
**Rationale**: 
- `IClosure<T>` implies a return value. Forcing a `Unit` return type would require boxing/unboxing or dummy values in all void functions.
- `IAction` maps directly to `System.Action`, simplifying the Roslyn backend emission.

### 4. AST Lowering Strategy
**Decision**: Use `DefaultAstRewriter` for Closure Conversion and Defunctionalisation.
**Rationale**: 
- These transformations require replacing nodes with blocks of code (e.g., replacing a `LambdaExp` with a class definition *and* an instantiation expression).
- `DefaultAstRewriter` supports `RewriteResult` which allows returning a node plus a "prologue" (statements to insert before), which is perfect for hoisting closure class definitions.

## Alternatives Considered

### A. Dynamic/Delegate-based Closures (Internal)
- **Idea**: Use `Func<...>` everywhere internally.
- **Rejected**: Makes "Defunctionalisation" impossible or redundant. We want to support a "Worker/Wrapper" model where the worker is optimized and explicit about its closure usage. It also makes TCO harder as we can't easily inspect/modify the delegate target.

### B. Full Trampoline TCO
- **Idea**: Return a `Thunk` from every function.
- **Rejected**: Too slow for a general-purpose language. Requires changing the calling convention of *every* function, not just recursive ones.

### C. Type Inference for Lambdas
- **Idea**: Allow `fun(x) { ... }`.
- **Rejected**: Adds significant complexity to the parser and type checker (bidirectional type checking or unification). Explicit types (`fun(x: int)`) are simpler to implement and consistent with the current language design.
