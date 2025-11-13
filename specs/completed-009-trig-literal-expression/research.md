# Research: TriG Literal Expression Type

## Decisions

### D1: Default Type Mappings for Interpolation
- **Decision**: Apply sensible defaults (string → quoted+escaped; int/float/decimal → bare numeric; bool → true/false; datetime → quoted lexical form + `^^xsd:dateTime`).
- **Rationale**: Minimizes verbosity; aligns with standard RDF/Turtle usage and developer expectations.
- **Alternatives Considered**:
  - Explicit wrappers only (rejected: verbose, raises barrier for common cases)
  - Hybrid with optional overrides (accepted implicitly via potential future wrappers; not required now)

### D2: IRI Representation in Interpolation
- **Decision**: Developers provide either prefixed names (e.g., `ex:Person`) or absolute IRIs `<http://example.org/Person>` directly inside TriG literal text; interpolation that yields an IRI must return a formatted string already in one of these forms.
- **Rationale**: Avoids new surface syntax or wrapper calls; leverages standards-compliant TriG notation.
- **Alternatives Considered**:
  - `iri(expr)` wrapper (rejected for MVP; can be added later for clarity)
  - Heuristic detection from string shape (rejected: risk of misclassification)

### D3: Escaping Literal Braces
- **Decision**: Use triple braces: `{{{` → literal `{{`; `}}}` → literal `}}`.
- **Rationale**: Simple, deterministic; mirrors patterns in other templating syntaxes; avoids escape character complexities.
- **Alternatives Considered**:
  - Backslash escapes (rejected: interacts poorly with C# raw strings and could require additional parsing layer)
  - Verbatim fenced segments (rejected: increases grammar complexity)

### D4: Interpolation Evaluation Timing
- **Decision**: Evaluate interpolated expressions at runtime (post variable initialization) prior to TriG parsing; compile-time constants may be folded.
- **Rationale**: Keeps semantics consistent with other expression evaluations; avoids introducing a compile-time partial interpreter.
- **Alternatives Considered**:
  - Pure compile-time expansion (rejected: complexity; requires constant analysis of arbitrary expressions)

### D5: Error Diagnostics Granularity
- **Decision**: Diagnostics reference line/column relative to source file; mapping inside literal retains actual positions (no offset normalization beyond standard parser location tracking).
- **Rationale**: Preserves developer mental model; aligns with existing diagnostic style.
- **Alternatives Considered**:
  - Virtual sub-range coordinates (rejected: adds confusion, no clear benefit)

### D6: Performance Boundaries
- **Decision**: No special streaming parser for large literals; rely on existing memory parsing as TriG literals expected to be modest (<100KB typical). Document potential future streaming optimization.
- **Rationale**: Simplicity; typical use cases small; premature optimization avoided.
- **Alternatives Considered**:
  - Custom incremental parser (rejected for MVP complexity)

## Unresolved Items
None. All clarifications from the specification resolved.

## Future Considerations
- Add optional helpers (`iri()`, `typed(value, datatype)`) if developer ergonomics indicate need.
- Potential optimization path: streaming parsing for very large datasets.
- Consider caching interpolation serialization results for repeated identical expressions.

## References
- W3C TriG: https://www.w3.org/TR/trig/
- W3C Turtle: https://www.w3.org/TR/turtle/
