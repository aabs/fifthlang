# Research for Graph Assertion Block

## Decisions
- Grammar: New `graphAssertionBlock` rule; usable as statement and primary expression; delimiters `<{` and `}>`; nesting allowed with other blocks.
- Semantics: Explicit persistence via `graph += graphValue` and `store += graphValue`; standalone statement targets default graph; l-values transactional; default store non-transactional with no rollback; set semantics dedup identical triples; open-world for conflicts.
- Types: Introduce `store`, `graph`, `triple`, `iri` types; define typing for `+=` on `graph` and `store`; inference for block value as `graph`.
- Built-ins: Expose `Fifth.System.KG` surface in symbol table; rely on `dotNetRdf` for backing; allow remote via `SparqlHttpProtocolConnector`.
- Lowering: Block lowers to calls to KG functions; no auto-persist in expression form.

## Rationale
- Distinct delimiters avoid grammar ambiguity and keep normal blocks unchanged.
- Explicit persistence keeps side effects predictable and testable.
- Type additions are minimal and match runtime primitives already present in `Fifth.System`.
- Lowering to built-ins reuses existing IL path and reduces compiler complexity.

## Alternatives Considered
- Auto-persist always: Rejected for lack of control and testability.
- Transactional default graph: Rejected; many stores do not support it; spec favors open-world.
- New project for KG runtime: Rejected; `Fifth.System` is sufficient.
