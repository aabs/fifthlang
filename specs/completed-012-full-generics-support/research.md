# Phase 0: Research — Full Generics Support

**Feature**: `001-full-generics-support` | **Phase**: 0 (Research & Technical Unknowns) | **Date**: 2025-11-18

## Research Questions

### Q1: Type Inference Algorithm Selection
**Question**: Which type inference algorithm should Fifth use for generic type argument resolution?

**Options Evaluated**:
1. **Unification-based inference** (Hindley-Milner style)
   - Used by: Haskell, ML, Rust
   - Pros: Theoretically sound, complete for simple cases, well-understood
   - Cons: Can fail on complex overloads, requires explicit annotations for ambiguous cases
   
2. **Local type inference** (C# style)
   - Used by: C#, Java, TypeScript
   - Pros: Predictable, works well with method overloading, easier to implement incrementally
   - Cons: Less powerful than Hindley-Milner, more explicit annotations needed
   
3. **Bidirectional type checking** (modern approach)
   - Used by: TypeScript 4.0+, Flow
   - Pros: Best of both worlds, handles complex cases better
   - Cons: More complex to implement, harder to debug inference failures

**Decision**: **Local type inference (C# style)**

**Rationale**:
- Fifth already targets .NET and will map to C# generics — matching C#'s inference behavior minimizes surprises
- Predictable inference failures with clear error messages align with Fifth's diagnostics goals
- Simpler implementation allows faster iteration
- Can be enhanced later with bidirectional checking if needed (non-breaking evolution)
- Covers 80%+ of common generic usage patterns (collections, simple functions)

**Implementation Notes**:
- Start with method call inference: `identity(42)` infers `T=int`
- Expand to assignment inference: `x: List<int> = emptyList()` infers `T=int`
- Defer complex cases (higher-rank types, variance) to future iterations

---

### Q2: Type Constraint Validation Strategy
**Question**: When and how should type constraints be validated during compilation?

**Options Evaluated**:
1. **Early validation** (during type parameter resolution)
   - Validate constraints immediately when type arguments are provided
   - Pros: Fail fast, clearer error messages at call site
   - Cons: May require multiple passes if constraints reference other generics
   
2. **Late validation** (during IL generation)
   - Defer constraint checks until code generation
   - Pros: Simpler compiler pipeline, fewer passes
   - Cons: Later errors confuse users, harder to provide good diagnostics
   
3. **Hybrid validation** (check syntax early, semantics late)
   - Parse constraints early, validate satisfaction at instantiation time
   - Pros: Best error messages, catches issues at appropriate context
   - Cons: Slightly more complex implementation

**Decision**: **Early validation (during type parameter resolution pass)**

**Rationale**:
- Fifth's multi-pass philosophy already supports analysis passes before lowering
- Early validation enables better error messages with line/column of type argument usage
- Aligns with user expectation: error at `Stack<string>` instantiation, not deep in compiler
- Constraint satisfaction is a pure type check (no runtime behavior) — belongs in analysis phase

**Implementation Notes**:
- New `TypeParameterResolutionVisitor` validates constraints after resolving type arguments
- Check each constraint in order: base class → interfaces → constructor
- Cache validation results to avoid redundant checks for same instantiation
- Report diagnostic code `GEN003` for constraint violations

---

### Q3: Generic Type Instantiation and Caching
**Question**: How should Fifth handle repeated instantiations of the same generic type (e.g., multiple `List<int>` instances)?

**Options Evaluated**:
1. **No caching** (create new type instance every time)
   - Pros: Simplest implementation
   - Cons: Poor performance, wastes memory, breaks type equality checks
   
2. **Canonical type cache** (interning)
   - Cache instantiated types in global map: `(GenericType, TypeArgs[]) → CachedType`
   - Pros: Ensures type equality (`List<int>` is same type everywhere), fast lookups
   - Cons: Adds global state, requires cache invalidation strategy
   
3. **Structural equality** (no cache, but compare by structure)
   - Don't cache, but implement `Equals()` based on structure
   - Pros: No global state
   - Cons: Slower equality checks, still wastes memory

**Decision**: **Canonical type cache with structural hashing**

**Rationale**:
- Type equality is critical for type checking: `List<int>` must equal `List<int>` everywhere
- Fifth's type system already uses discriminated unions (`FifthType`) — cache fits naturally
- Performance: Generic instantiation can be expensive (constraint checking) — cache amortizes cost
- .NET also interns generic types — matching behavior aids Roslyn backend mapping

**Implementation Notes**:
- Add `GenericTypeCache` as singleton in type system module
- Hash generic instantiations: `Hash(GenericType.Name, TypeArgs.Select(t => t.Hash))`
- Cache lookup in `GenericTypeInferenceVisitor` after resolving type arguments
- No cache invalidation needed (types are immutable during compilation session)
- Cache cleared between compilation units (not persisted across builds)

---

### Q4: Roslyn Backend Mapping Strategy
**Question**: How should Fifth's generic types map to .NET/Roslyn generic types?

**Options Evaluated**:
1. **Erasure** (remove generics, use `object` everywhere)
   - Pros: Simplest, no .NET generic complexity
   - Cons: Loses type safety, poor performance (boxing), breaks interop
   
2. **Reification** (keep full generic type info at runtime)
   - Pros: Maximum type information, best interop
   - Cons: .NET already uses reification — not an option, just inherit
   
3. **Direct mapping** (Fifth generics → .NET generics 1:1)
   - Fifth `class Stack<T>` → C# `class Stack<T>`
   - Pros: Natural, preserves type safety, best interop with .NET libraries
   - Cons: Must handle .NET generic constraints (mapping Fifth constraints → C# constraints)

**Decision**: **Direct mapping (Fifth generics → .NET generics)**

**Rationale**:
- Fifth already targets .NET — leveraging .NET's generic system is natural
- Preserves type safety guarantees from Fifth source through to runtime
- Enables seamless interop: Fifth generics can call .NET generic APIs (`List<T>`, `Dictionary<K,V>`)
- Roslyn API supports generic type definitions (`TypeParameterSyntax`, `TypeConstraintClauseSyntax`)

**Implementation Notes**:
- In `RoslynBackend`, emit `TypeParameterSyntax` for each `TypeParameterDef`
- Map Fifth constraints to C# constraint clauses:
  - `InterfaceConstraint` → `TypeConstraint(interfaceType)`
  - `BaseClassConstraint` → `ClassOrStructConstraint` + `TypeConstraint(baseClass)`
  - `ConstructorConstraint` → `ConstructorConstraint()`
- Type argument substitution happens at IL transformation (before Roslyn) — Roslyn sees concrete types

---

### Q5: Grammar Ambiguity Resolution (Angle Brackets)
**Question**: How to handle ambiguity between generic syntax `<T>` and less-than operator `<`?

**Context**: Fifth grammar already supports `<` as less-than operator in expressions:
```fifth
x < y        // Less-than comparison
f<int>(x)    // Generic function call - AMBIGUOUS
```

**Options Evaluated**:
1. **Lookahead disambiguation** (ANTLR predicates)
   - Use syntactic predicates to distinguish `<` followed by type vs expression
   - Pros: Standard ANTLR approach, no syntax changes
   - Cons: Can be brittle, slow for complex cases
   
2. **Contextual keywords** (require `generic` keyword)
   - Syntax: `generic<T> class Stack { ... }`
   - Pros: No ambiguity
   - Cons: Verbose, breaks ergonomics
   
3. **Whitespace sensitivity** (require no space after `<`)
   - `f<int>` is generic, `f < int` is comparison
   - Pros: Intuitive for programmers
   - Cons: Violates Fifth's whitespace-insensitive grammar philosophy

**Decision**: **Lookahead disambiguation with grammar precedence**

**Rationale**:
- ANTLR 4 handles `<` ambiguity well with predicates: check if `<` is followed by `IDENTIFIER` (type name) vs `expression`
- Precedence: generic interpretation takes priority in type contexts (class definition, function return type), comparison takes priority in expression contexts
- No breaking changes to existing syntax — Fifth code without generics parses identically
- Testing can validate: parser tests with ambiguous cases ensure correct interpretation

**Implementation Notes**:
- Add predicate to `type_spec` rule: `{isTypeContext()}? IDENTIFIER LESS type_spec GREATER`
- In `AstBuilderVisitor`, context determines interpretation (already happens via grammar rule match)
- Grammar already disambiguates most cases: `class C<T>` can only be generic, `x < y` can only be comparison
- Edge case: function calls `f<T>(x)` — require explicit type arguments or inference

---

## Technical Unknowns Resolved

All major technical unknowns have been resolved with implementation decisions:

✅ **Type inference algorithm**: Local type inference (C# style)  
✅ **Constraint validation**: Early validation during type parameter resolution pass  
✅ **Type instantiation**: Canonical type cache with structural hashing  
✅ **Roslyn backend mapping**: Direct mapping (Fifth generics → .NET generics 1:1)  
✅ **Grammar ambiguity**: Lookahead disambiguation with precedence (no syntax changes)  

## Risks and Mitigations

### Risk 1: Type Inference Performance
**Risk**: Type inference for deeply nested generics (`List<List<Tuple<K,V>>>`) could be slow  
**Likelihood**: Medium  
**Impact**: Medium (compilation time regression)  
**Mitigation**: 
- Implement inference caching (cache inferred type arguments for call sites)
- Set inference depth limit (e.g., 10 levels) with diagnostic if exceeded
- Performance test with generated deeply-nested generic code in `test/perf/`

### Risk 2: Constraint Satisfaction Complexity
**Risk**: Complex constraint combinations (multiple interfaces + base class) may have subtle bugs  
**Likelihood**: Low  
**Impact**: High (type safety violation, wrong code generation)  
**Mitigation**:
- Comprehensive test suite with combinatorial constraint cases
- Validate against .NET's constraint rules (documentation: C# spec section 14.2.5)
- Add explicit tests for edge cases: `where T: IDisposable, IComparable<T>, new()`

### Risk 3: Roslyn Backend Complexity
**Risk**: Mapping Fifth constraints to C# constraints may have corner cases (e.g., struct constraint)  
**Likelihood**: Low  
**Impact**: Medium (incorrect interop with .NET generics)  
**Mitigation**:
- Start with minimal constraint types (interface, base class, constructor)
- Defer `struct`/`class`/`unmanaged` constraints to future iteration
- Add integration tests calling .NET generic APIs (`List<T>.Sort()`, `Dictionary<K,V>`)

### Risk 4: Grammar Ambiguity Regressions
**Risk**: Lookahead predicates could break existing expression parsing  
**Likelihood**: Low  
**Impact**: High (parser regression, breaks backward compatibility)  
**Mitigation**:
- Run full parser test suite (`test/syntax-parser-tests/`) after grammar changes
- Add regression tests for expressions with `<` operator (`x < y`, `f(a < b)`)
- Validate all existing `.5th` samples parse identically before/after changes

## Open Questions for Phase 1 Design

1. **Type parameter variance** (covariance/contravariance): Defer to future iteration or include in initial design?
   - **Recommendation**: Defer (not in spec's user stories, adds significant complexity)
   
2. **Type alias support for generic types**: Should `type IntList = List<int>;` be supported?
   - **Recommendation**: Include in Phase 1 design if time permits (low complexity, high value)
   
3. **Generic method overloading resolution**: How to choose between `f<T>(x: T)` and `f(x: int)` when `T=int`?
   - **Recommendation**: Specific type overload wins (standard C# behavior), document in `data-model.md`

## References

- **C# Language Specification**: [Generic type parameters (§14.2)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes#142-type-parameters)
- **Roslyn Generic Type APIs**: [`TypeParameterSyntax`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.syntax.typeparametersyntax), [`TypeConstraintClauseSyntax`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.codeanalysis.csharp.syntax.typeconstraintclausesyntax)
- **Type Inference Algorithms**: "A Theory of Type Qualifiers" (Foster et al.) — background on unification
- **ANTLR 4 Predicates**: [ANTLR 4 Reference](https://github.com/antlr/antlr4/blob/master/doc/predicates.md) — semantic predicates for disambiguation
- **Fifth Constitution**: `.specify/memory/constitution.md` — multi-pass compilation philosophy

---

**Phase 0 Complete** ✅  
Next: Proceed to **Phase 1** (Design: data-model.md, contracts/, quickstart.md)
