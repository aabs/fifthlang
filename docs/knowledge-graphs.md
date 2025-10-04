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

## Triple Literals

Fifth supports concise triple literal syntax for constructing individual RDF triples:

```fifth
// Basic triple literal syntax: <subject, predicate, object>
personType: triple = <ex:Person, rdf:type, rdfs:Class>;
age: triple = <ex:Alice, ex:age, 42>;
```

### Triple Literal Syntax Rules

- **Form**: `<subject, predicate, object>` with exactly three comma-separated components
- **Subject/Predicate**: Must be IRIs (either full `<http://...>` or prefixed `ex:name`)
- **Object**: Can be an IRI, primitive literal (string, number, boolean), or variable reference

### List Expansion

Triple literals support list expansion in the object position:

```fifth
// List in object position expands to multiple triples
labels: [triple] = <ex:Alice, rdfs:label, ["Alice", "Ally"]>; 
// Expands to two triples: <ex:Alice, rdfs:label, "Alice"> and <ex:Alice, rdfs:label, "Ally">

// Empty list produces warning and zero triples
emptyLabels: [triple] = <ex:Alice, ex:nothing, []>; // Warning: TRPL004
```

**Note**: Nested lists are not allowed and will produce a compile error (TRPL006).

### Triple Operations

Triples compose with graphs using `+` and `-` operators:

```fifth
base: graph = <{ <ex:Alice, rdf:type, ex:Person>; }>;

// Add a triple to a graph (returns new graph)
extended: graph = base + <ex:Alice, ex:age, 42>;

// Chaining operations
g2: graph = base + personType + age;

// Combine triples into a graph
g3: graph = <ex:s1, ex:p1, ex:o1> + <ex:s2, ex:p2, ex:o2>;

// Remove a triple from a graph
g4: graph = extended - <ex:Alice, ex:age, 42>;
```

### Mutating Assignment Operators

Triple literals support compound assignment operators for graphs:

```fifth
base: graph = <{ <ex:Alice, rdf:type, ex:Person>; }>;

// Add triple to existing graph (mutating syntax, desugars to reassignment)
base += <ex:Alice, ex:age, 42>;

// Remove triple from graph
base -= <ex:Alice, ex:age, 42>;
```

### Triple Literals in Graph Assertion Blocks

Triple literals can be used directly inside graph assertion blocks:

```fifth
<{
    <ex:Alice, rdf:type, ex:Person>;  // Triple literal asserted into the block's graph
    <ex:Alice, ex:age, 42>;
}>;
```

### Escaping in Serialization

When triple literals are serialized (e.g., in debugging or logging), special characters are escaped:
- The characters `>` and `,` inside string literal objects are preceded by a backslash
- Exactly one space follows each comma
- Example: `<ex:s, ex:p, "value\, with comma">` 

## Literal Support in Object Position
Graph blocks and triple literals accept object literals for:
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

## Diagnostics

### Triple Literal Diagnostics (TRPL001-TRPL006)

The compiler emits specific diagnostic codes for triple literal errors:

| Code | Severity | Description | Example |
|------|----------|-------------|---------|
| TRPL001 | Error | Triple literal must have exactly three components (subject, predicate, object) | `<ex:s, ex:p>` (too few), `<ex:s, ex:p, ex:o, ex:x>` (too many) |
| TRPL002 | Error | Triple literal subject must be an IRI | Using a string literal as subject |
| TRPL003 | Error | Triple literal predicate must be an IRI | Using a number as predicate |
| TRPL004 | Warning | Triple literal with empty list object expands to zero triples | `<ex:s, ex:p, []>` |
| TRPL005 | Error | Invalid type in triple literal object position | Using unsupported types in object position |
| TRPL006 | Error | Nested lists are not allowed in triple literal object position (only single-level lists) | `<ex:s, ex:p, [[ex:o1], ex:o2]>` |

**Note**: IRI-related errors (such as unresolved prefixes) continue to use existing diagnostic codes and are not specific to triple literals.
