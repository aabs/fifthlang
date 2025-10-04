# Transformation Contract: Triple Feature

## Pass 1: TripleLiteralExpansionVisitor
| Input Node | Condition | Output | Notes |
|------------|-----------|--------|-------|
| TripleLiteralExp (object is list with N>0) | list elements valid | Replace node with N sibling TripleLiteralExp (each object element) | Preserve source location for diagnostics |
| TripleLiteralExp (object is empty list) | size == 0 | Remove node; emit TRPL004 warning | No triples generated |

## Pass 2: GraphTripleOperatorLoweringVisitor
| Pattern | Replacement (High-Level Pseudocode) | Notes |
|---------|-------------------------------------|-------|
| graph + triple | `KG.Assert(graph, KG.CreateTriple(...))` returning new graph (copy semantics) | If immutability required: clone graph first (helper) |
| triple + triple | `KG.CreateGraph().Assert(t1).Assert(t2)` (dedupe by structural set union) | Or dedicated union helper |
| triple + graph | `KG.Assert(graph, t)` with union semantics | |
| graph - triple | `KG.Retract(graph, t)` returning new graph (copy semantics) | Copy semantics preserved |
| graph += triple | Desugar to assignment: `g = g + triple` | Maintain chaining semantics |
| graph -= triple | Desugar to assignment: `g = g - triple` | |

## Structural Equality Enforcement
- Lowering assumes structural equality implemented during KG operations (or pre-check set membership).

## Diagnostics Emission Points
| Code | Phase | Trigger |
|------|-------|---------|
| TRPL001 | Parser validation | Wrong component count |
| TRPL002 | Type inference | Subject/predicate not IRI typed |
| TRPL003 | ExpansionVisitor | Invalid list object element |
| TRPL004 | ExpansionVisitor | Empty list object |
| TRPL005 | LoweringVisitor | Duplicate suppression informational (optional) |
| TRPL006 | ExpansionVisitor | Nested list encountered (disallowed) |

## Type Mapping
- TripleLiteralExp -> `triple`
- Expanded multiple TripleLiteralExp used in addition context fold into `graph` type.

## Ordering
- Lowering does not guarantee stable order; visitors must not rely on iteration order for correctness.

## Immutability Policy
- For non-mutating operators, implement a helper `KG.CopyGraph(IGraph g)` if necessary to produce a logical new graph prior to Assert/Retract (or treat underlying graph as persistent structure if library supports). Decision: Do logical copy only if underlying operations are mutating; else treat operations as pure wrappers returning `IGraph`.

## Prefix Resolution
- No additional resolution introduced; rely entirely on existing declared prefixes. Failure to resolve emits existing unresolved-prefix diagnostic (outside scope of new TRPL codes).
