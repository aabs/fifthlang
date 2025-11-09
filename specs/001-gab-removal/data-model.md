# Data Model: Remove GAB

This feature removes constructs; it does not introduce new persistent entities. Existing RDF-related entities remain unchanged and are documented for regression focus.

## Entities (Affected / Unchanged)

### Former GAB Constructs (Removed)
- Graph Assertion Block: (Removed) Previously allowed inline triple assertions; no replacement entity; elimination reduces AST surface.

### Triple Literal (Unchanged)
- Purpose: Represents an RDF triple in code.
- Attributes: Subject (IRI or blank), Predicate (IRI), Object (Literal | IRI | Blank).
- Validation: Grammar ensures correct forms; no changes in this feature.

### Store Declaration (Unchanged)
- Purpose: Declares an RDF store (default or named).
- Attributes: Name (optional 'default'), Backend URI.
- Validation: Must follow canonical forms; unchanged.

## Relationships
- Triple Literals may appear in RDF-related expressions/statements; unaffected.
- Store Declaration sets target persistence for graph operations; unaffected.

## State Transitions
- None introduced. Removal does not add lifecycle states.

## Impact Summary
- AST model shrinks: remove node definitions and generated builders/visitors for GAB.
- No data migration required; no serialization impact.
