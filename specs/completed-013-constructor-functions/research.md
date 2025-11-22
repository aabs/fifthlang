# Research & Decisions: Constructor Functions

**Date**: 2025-11-19
**Scope**: Overload resolution, definite assignment, inheritance chaining, diagnostics, performance strategy.

## Decisions Summary

| Topic | Decision | Rationale | Alternatives Considered |
|-------|----------|-----------|--------------------------|
| Overload Ranking | Exact match > convertible > widening (stable priority) | Simple, predictable, mirrors existing function overload semantics | Cost-based scoring (complex), full type unification (not needed) |
| Ambiguity Handling | First pass collects best rank set; >1 → CTOR002 | Clear cutoff; avoids heuristic tie-breaking | Implicit picking by declaration order (non-deterministic) |
| Definite Assignment | Single forward CFG pass with conservative merging (intersection of assigned sets at join points) | Guarantees safety; easy to explain; linear complexity | SAT/constraint solver (overkill), backward analysis (redundant) |
| Required Field Detection | Non-nullable & no default initializer | Aligns with language semantics; matches spec | Using attribute markers (not defined) |
| Base Constructor Resolution | Require explicit `: base(...)` if no parameterless base | Removes guesswork; keeps inheritance explicit | Implicit chaining (hidden behavior) |
| Cycle Detection | DFS ancestry stack check | Minimal code; low runtime cost | Global graph pre-computation (unnecessary) |
| Synthesized Constructor | Only when zero ctors and all fields defaulted/nullable | Prevent accidental partially initialized objects | Always synthesize (could hide missing assignments) |
| Shadowed Names | Require `this.` to disambiguate field assignment | Avoid silent self-assignment bugs | Implicit resolution precedence (error-prone) |
| Null Generic Arg Handling | Standard compatibility rules; no special inference | Keeps semantics consistent with other calls | Special null inference heuristics (adds complexity) |
| Caching Strategy | LRU(5000) on (ClassTypeId, ParamSigHash) | Limits memory growth; high hit rate expected | Unlimited growth (risk memory), random eviction |
| Performance Measurement | Benchmark synthetic class (200 overloads) + typical project | Captures worst-case & real-case behavior | Sole reliance on microbenchmarks (unrepresentative) |
| Diagnostics Format | Structured fields + hint per code (CTOR001–CTOR008) | Consistency with existing system; machine-readable | Freeform text only (hard to parse) |

## Detailed Rationale

### Overload Resolution
Chosen minimal ranking ensures deterministic behavior. Conversion tiers mapped from existing implicit conversion rules; no per-parameter weighting to avoid complexity. Ambiguity surfaces early (CTOR002) prompting code clarity.

### Definite Assignment Analysis
Forward data-flow: each statement updates assigned set; branching merges via intersection (fields must be set on all paths). Conservative approach may produce false positives in rare cases but never false negatives—prefer safety now; can relax later by tracking conditional guarantees.

### Inheritance & Base Chaining
Explicitness requirement matches spec philosophy (no hidden behavior). Parameterless base enables optional omission; any other base shape forces constructor author to express intent. Cycle detection via stack ensures O(N) in depth.

### Diagnostics
Each code includes actionable hint to reduce time-to-fix. Signature formatting `ClassName(type1,type2)` avoids ambiguity. Hints tuned: adjust arguments, add base call, assign fields, remove duplicate, etc.

### Performance
Arity pre-filter + leading parameter type hash drastically reduces candidate set. LRU ensures heavy reuse for repeated instantiations. No speculative inlining at this feature stage—defer until profiling justifies.

### Risk Mitigation Summary
- Ambiguity: deterministic error rather than heuristic pick.
- Assignment complexity: conservative pass rather than sophisticated solver.
- Memory growth: capped cache with small struct keys.
- Future extensibility: data structures anticipate possible `this(...)` chaining (reserved field placeholders).

## Open Questions
None – all clarifications resolved in spec; no blockers.

## Implementation Notes (Non-Binding)
- Introduce `ConstructorDef` in metamodel; regeneration must follow.
- Extend parser rule validating class-name & no return type.
- Add new lowering rewriter; integrate before existing ClassCtorInserter modifications (or adjust order: synthesis first, then lowering).

## Next Phase Inputs
Outputs here seed `data-model.md` and contract interfaces. No pending clarifications.
