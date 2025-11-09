# Contracts: System KG Types

This folder documents the public surface that the compiler lowering/translator will target in `Fifth.System`.

## Graph (C#-ish contract)
- Namespace: `Fifth.System`
- Type: `public sealed class Graph` (thin wrapper over dotNetRDF `IGraph`)
- Members:
  - `public void Add(Triple t)`
  - `public int Count { get; }`
  - `public IEnumerable<Triple> Triples { get; }`
  - `public VDS.RDF.IGraph ToVds()`
  - `public static Graph FromVds(VDS.RDF.IGraph g)`
  - Binary operators (non-mutating):
    - `public static Graph operator +(Graph g, Triple t)`
    - `public static Graph operator -(Graph g, Triple t)`
    - `public static Graph operator +(Graph g1, Graph g2)`
    - `public static Graph operator -(Graph g1, Graph g2)`
  - Compound assignment (mutating):
    - `public static Graph operator +=(Graph g, Triple t)`
    - `public static Graph operator -=(Graph g, Triple t)`
    - `public static Graph operator +=(Graph g1, Graph g2)`
    - `public static Graph operator -=(Graph g1, Graph g2)`
  - Semantics Note:
    - Binary `+`/`-` MUST NOT mutate operands; they return a new `Graph`.
    - Copy semantics (v1): returned `Graph` MUST be a deep copy (triples cloned/merged) to avoid aliasing with operands.
    - Compound `+=`/`-=` MUST mutate the LHS and return it.

## Triple
- Type: `public sealed class Triple` (thin wrapper over dotNetRDF nodes/`Triple`)
- Members:
  - `public object Subject { get; }` (or a specific node type abstraction)
  - `public object Predicate { get; }`
  - `public object Object { get; }`
  - `public VDS.RDF.Triple ToVdsTriple()`
  - `public static Triple FromVds(VDS.RDF.Triple t)`
  - Binary operators (non-mutating):
    - (Removed) `Triple + Triple` returning `Triple` — superseded by clarified semantics.
    - `public static Graph operator +(Triple a, Triple b)` (returns new Graph containing both triples)
    - (Undefined) `Triple - Triple` — no operator provided.
  - With Graph (non-mutating result Graph): handled as graph overloads above (i.e., `public static Graph operator +(Triple t, Graph g)` if needed for commutativity)

## Store
- Type: `public sealed class Store` (wrapper/adapter over dotNetRDF store abstractions)
- Members:
  - `public Graph CreateGraph()`
  - `public void AddGraph(Graph g)`
  - `public Graph GetDefaultGraph()`
  - `public object ExecuteQuery(string sparql)`
  - Compound assignment (mutating):
    - `public static Store operator +=(Store s, Graph g)`
    - `public static Store operator -=(Store s, Graph g)`

## Factory
- Function in `Fifth.System` surface: `store sparql_store(iri)` (exposed to Fifth)
- C# binding: static method or function exported by system assembly returning `Store`

## Compiler Binding Contract
- No dedicated lowering for KG operators; rely on operator resolution (`+=`/`-=` become rebinding with returned LHS per C# semantics).
- Type names `graph|triple|store` are globally predeclared and resolve to these `Fifth.System` types during binding.
