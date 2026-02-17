# Phase 0 Research: SPARQL Comprehensions

## Inputs

- Feature spec: `specs/015-sparql-comprehensions/spec.md`
- Constitution: `.specify/memory/constitution.md`

## Repository Findings

### Current list comprehension state

- **Parsing**: `[` `var` `in` `<expr>` (`#` `<expr>`)? `]` in `src/parser/grammar/FifthParser.g4`.
- **AST**: `ListComprehension : List` exists in `src/ast-model/AstMetamodel.cs`, but it is *stringly-typed* (`SourceName: string`) and supports only one optional constraint.
- **Implementation**: comprehension execution/lowering is currently **not implemented**; existing tests are skipped.

### Current SPARQL support state

- Fifth surface SPARQL literal `?<...>` is parsed as a raw literal with interpolation (`src/parser/grammar/FifthLexer.g4` + `FifthParser.g4`).
- Runtime SPARQL parsing/execution uses dotNetRDF (`VDS.RDF.Query`) via `src/fifthlang.system/QueryApplicationExecutor.cs`.
- Result type is a discriminated union `Result` with `TabularResult(SparqlResultSet)` for SELECT queries (`src/fifthlang.system/Result.cs`).
- An ANTLR SPARQL grammar exists in `src/parser/grammar/SparqlParser.g4` and is generated into `src/parser/grammar/sparql-grammar/*`.

## Decisions

### Decision: Represent list comprehensions as a real AST expression (not strings)

- **Chosen**: Update `ListComprehension` to store `Source: Expression` and `Projection: Expression`, plus `Constraints: List<Expression>`.
- **Rationale**: The current `SourceName: string` cannot be typechecked, transformed, or lowered deterministically. Multi-constraint filtering is required by the spec.
- **Alternatives considered**:
  - Keep `SourceName` as text and re-parse later → duplicates parsing and loses source locations.
  - Special-case SPARQL comprehensions only → violates FR-014 (alternate forms of same feature).

### Decision: Compile-time SPARQL validation uses the existing generated SPARQL parser

- **Chosen**: Parse the SPARQL literal body with `SparqlLexer/SparqlParser` (ANTLR) during compilation to:
  - validate the query is a SELECT query (tabular result)
  - extract projected variables for comprehension validation
- **Rationale**: Spec requires compile-time errors for non-SELECT and unknown variable usage; runtime-only checks are too late.
- **Alternatives considered**:
  - Use dotNetRDF parse at compile time → adds heavier dependency into compilation phases and may differ from the ANTLR grammar requirement.
  - Do not parse; rely on dynamic runtime behavior → violates FR-005 and FR-009a.

### Decision: Comprehension lowering uses AST rewriting (not codegen hacks)

- **Chosen**: Implement lowering as a dedicated pass using the project’s rewriting patterns (prefer `DefaultAstRewriter` when statement hoisting is needed).
- **Rationale**: Constitution prefers AST lowering over codegen complexity; comprehensions naturally desugar into list construction + iteration.
- **Alternatives considered**:
  - Emit special-case C# in Roslyn translator → brittle, duplicates semantics, harder to test.

### Decision: Constraint scoping for object projections needs an explicit rule

- **Chosen (proposed for Phase 1 design)**: Introduce an implicit variable (e.g. `it`) bound to the projected element, and require constraints over object projections to use `it.<prop>` member access.
- **Rationale**: Allows reuse of existing expression grammar and symbol resolution rules without introducing “bare property name” binding magic.
- **Alternatives considered**:
  - Allow bare `Prop` inside constraint to mean `it.Prop` → requires new name-resolution behavior and risks ambiguity.

## Open Questions / Needs Clarification

1. **Constraint scoping final syntax**: Is `it.<prop>` acceptable, or must constraints allow bare property names? (This is currently left open in the spec.)
2. **Row binding conversion**: What are the coercion rules from SPARQL term values → Fifth property types (string/int/iri/etc.)? Existing SPARQL execution returns dotNetRDF nodes; a conversion surface likely belongs in `src/fifthlang.system/`.
3. **Diagnostics codes**: Which diagnostic code prefix should be used for this feature (e.g., `LCOMP00X` vs reuse `SPARQL00X`)?

## Implications

- This feature effectively completes the currently-unimplemented list comprehension runtime semantics, then extends it with SPARQL-specific compile-time checks and object projection restrictions.
