# Research: Remove Graph Assertion Block (GAB)

## Decisions

- Decision: No special diagnostics for GAB
  - Rationale: "Leave no trace" requirement; standard syntax errors simplify maintenance and avoid hinting at removed constructs
  - Alternatives considered: Custom error message; deprecation warning → Rejected per spec

- Decision: Preserve RDF features as-is
  - Rationale: Existing users rely on triple literals, RDF datatypes, and canonical store declarations; changes must not regress behavior
  - Alternatives considered: Broaden removal scope → Rejected; out of spec

- Decision: Remove AST nodes for GAB via metamodel change + regeneration
  - Rationale: Generator-as-source-of-truth; guarantees consistency across visitors/rewriters
  - Alternatives considered: Keep dead nodes unused → Rejected; increases maintenance burden

- Decision: Remove grammar tokens/rules for GAB from lexer/parser
  - Rationale: Prevents acceptance of GAB; aligns syntax with docs and examples
  - Alternatives considered: Keep grammar with hidden feature-flag → Rejected; scope says unconditional removal

## Open Questions (none)

All clarifications resolved in spec; no remaining unknowns for planning.
