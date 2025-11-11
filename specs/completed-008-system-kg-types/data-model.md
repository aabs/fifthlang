# Data Model: System KG Types

## Entities

### Graph
- Purpose: Container of triples with add/query operations.
- Underlying: Thin wrapper over dotNetRDF `IGraph` (composition).
- Key Members (proposed):
  - `Add(Triple t)`
  - `IEnumerable<Triple> Triples { get; }`
  - `Count { get; }`
  - `ToVds()` / `FromVds(IGraph)` bridge
  - Operators:
    - Binary non-mutating: `+`/`-` with `(Graph, Triple)` and `(Graph, Graph)` returning new `Graph`
    - Compound mutating: `+=`/`-=` with `(Graph, Triple)` and `(Graph, Graph)` mutating LHS and returning it

### Triple
- Purpose: Represents RDF statement (subject, predicate, object).
- Underlying: Thin wrapper over dotNetRDF nodes/`Triple`.
- Key Members (proposed):
  - `Subject` (IRI or node)
  - `Predicate` (IRI or node)
  - `Object` (literal or node)
  - `ToVdsTriple()` / `FromVds(Triple)`
  - Operators (non-mutating): `+` and `-` between `Triple` values yield new `Triple`

### Store
- Purpose: Abstraction for querying/persisting graphs (SPARQL-capable).
- Underlying: Wrapper/adapter over `IInMemoryQueryableStore` or SPARQL endpoint client.
- Key Members (proposed):
  - `CreateGraph()`
  - `ExecuteQuery(string sparql)`
  - `AddGraph(Graph g)`
  - `GetDefaultGraph()`
  - Operators (mutating): `+=` and `-=` with `Graph` modify store contents and return the same store

## Relationships
- `Graph` contains many `Triple`.
- `Store` may contain many `Graph` instances.

## Constraints
- Duplicate triple handling: follow dotNetRDF semantics (likely set-like; reasssert is idempotent). If different, document behavior. (NEEDS CLARIFICATION)
- Thread safety: unspecified (NEEDS CLARIFICATION; default non-thread-safe). Consider optional sync wrapper later.

## State Transitions
- Graph: empty → populated (via Add/`+=`).
- Store: uninitialized default graph → default graph set via `store default`.

## Notes
- Performance metric: operations should remain within ≤5% baseline.
- Global type names map to these entities without import.
