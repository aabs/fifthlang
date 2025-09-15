# Knowledge Graphs in Fifth

This doc summarizes the canonical store declaration syntax and graph assertion blocks, and how they lower to the built-in `Fifth.System.KG` helpers.

## Canonical Store Declarations
- Use the colon form exclusively:
  - `name : store = sparql_store(<http://example.org/store>);`
  - `store default = sparql_store(<http://example.org/store>);` (sets the default store)

The `sparql_store` function is a built-in alias for connecting to a remote SPARQL endpoint. It returns a `VDS.RDF.Storage.IStorageProvider`.

## Graph Assertion Blocks
- Statement-form saves to the default store:
```fifth
store default = sparql_store(<http://example.org/store>);

main(): int {
    <{
        <http://ex/s> <http://ex/p> "o";
    }>;
    return 0;
}
```

- Expression-form yields a graph value:
```fifth
main(): int {
    g: graph = <{
        <http://ex/s> <http://ex/p> 42;
    }>;
    return g.CountTriples();
}
```

## Literal Support in Object Position
Graph blocks accept object literals for:
- Strings, booleans, chars
- Signed/unsigned integers: `sbyte`, `byte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`
- Floating point: `float`, `double`
- Precise decimals: `decimal`

Literals are lowered to typed RDF literals using the appropriate XSD datatype (e.g., `xsd:int`, `xsd:decimal`).

## Lowering Strategy
Graph assertion blocks lower to calls on `Fifth.System.KG`:
- Build nodes via `CreateUri`/`CreateLiteral`
- Create triples with `CreateTriple`
- Assert with `Assert`
- Save with `SaveGraph(store, graph[, uri])`

The statement-form implicitly resolves the default store and calls `SaveGraph`. The expression-form simply returns the constructed `IGraph`.

## Raw API Quickstart (Equivalent)
You can also use the raw API directly:
```fifth
main(): int {
    KG.SaveGraph(
        KG.sparql_store("http://example.org/store"),
        KG.Assert(
            KG.CreateGraph(),
            KG.CreateTriple(
                KG.CreateUri(KG.CreateGraph(), "http://ex/s"),
                KG.CreateUri(KG.CreateGraph(), "http://ex/p"),
                KG.CreateLiteral(KG.CreateGraph(), 1.23m)
            )
        ),
        "http://example.org/graph"
    );
    return 0;
}
```

See tests under `test/runtime-integration-tests/*GraphAssertionBlock*` for more examples.
