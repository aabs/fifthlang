# Data Model: Triple Primitive & Related Constructs

## Entities

### TripleLiteralExp (Existing / Confirmed)
| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Subject | Expression (IRIRefExp | VarRefExp) | Must resolve to IRI type | Variables require static IRI type |
| Predicate | Expression (IRIRefExp | VarRefExp) | Must resolve to IRI type | Same rules as Subject |
| Object | Expression | Must resolve to IRI or primitive literal; may be single-level ListLiteralExp | List triggers expansion; nested lists invalid |
| IsExpanded | bool | Derived | Marks nodes produced by list expansion pass |

### Triple (AST SymbolKind.Triple)
Represents semantic triple instance (post-lowering). Structural equality on (Subject, Predicate, Object).

### Graph (AST SymbolKind.Graph)
Collection (set) of Triple values. No ordering guarantee. Duplicate insert suppressed.

## Relationships
- TripleLiteralExp → produces one or more Triple nodes (after expansion transformation)
- Graph operations `+` / `-` consume Triple(s) and produce new Graph
- Mutating forms (`+=`, `-=`) rewrite to Assert/Retract (or Graph construction) operations

## Invariants
| Invariant | Enforcement Stage |
|-----------|-------------------|
| Triple literal has exactly 3 top-level components | Parser rule validation |
| List object elements all valid object node forms (no nested lists) | Expansion visitor (error TRPL006 if nested) |
| Empty list object => zero triples + warning | Expansion visitor + diagnostic emission |
| Graph union addition does not duplicate existing triple | Operator lowering + KG helper semantics |
| Structural equality consistent across additions/removals | Test suite (property-based) |

## State Transitions
| State | Event | Result |
|-------|-------|--------|
| TripleLiteralExp (with list object) | ExpansionVisitor | Multiple TripleLiteralExp (IsExpanded=true) or graph composition expression |
| Graph + Triple | LoweringVisitor | New GraphConstructionExp (KG helper calls) |
| Graph - Triple | LoweringVisitor | New GraphConstructionExp with filtered set |
| Graph += Triple | MutationDesugar (same lowering visitor) | Expression equivalent to re-assignment via KG.Assert sequence |

## Type Inference Notes
- `triple` primitive maps to `VDS.RDF.Triple`
- Triple literal expression type: `triple`
- Triple literal with list object: When used standalone: yields `graph` (union of expanded triples) or sequence lowered to graph-building expression (decision: expansion yields intermediate synthetic Graph-building node, then type = graph)
- Operators:
  - `graph + triple` → `graph`
  - `triple + triple` → `graph`
  - `triple + graph` → `graph`
  - `graph - triple` → `graph`
  - Mutating forms evaluate to `graph` but are not expressions returning value unless allowed in assignment context (desugared to assignment expression).

## Diagnostics (Identifiers)
| Code | Condition | Message Sketch |
|------|-----------|----------------|
| TRPL001 | Wrong arity in triple literal | "Triple literal must have exactly 3 components" |
| TRPL002 | Subject/predicate not IRI-typed | "Subject/predicate must be IRI or IRI-typed variable" |
| TRPL003 | Invalid object element in list | "Invalid object element; expected IRI or primitive literal" |
| TRPL004 | Empty list expansion | "Empty triple list expands to no triples" (warning) |
| TRPL005 | Duplicate suppressed (optional info) | "Duplicate triple ignored (set semantics)" (info) |
| TRPL006 | Nested list in triple object | "Nested list expansion not permitted (single-level only)" |

## Open Modeling Considerations
Prefix resolution: rely solely on existing declared prefixes; no implicit defaults (FR-023).
Performance: ensure expansion visitor linear in number of list elements; no recursive flattening required due to nested prohibition.
- Representation for list-expansion intermediate: Option A: keep original TripleLiteralExp with Object=List; expansion pass replaces with multiple sibling TripleLiteralExp; Option B: introduce `TripleExpansionGroup` synthetic node. (Chosen: A for simplicity.)
- Decide whether mutating operators produce value (likely yes returning resulting graph, matching KG helper chaining semantics).
