# Quickstart: Using Triple Literals

```fifth
// Declare triples
personType: triple = <ex:Person, rdf:type, ex:Class>;
age42: triple = <ex:Alice, ex:age, 42>;
// IMPORTANT: A triple literal whose object is a list expands to multiple triples and
// therefore must produce a list of triples: [triple]
labels: [triple] = <ex:Alice, rdfs:label, ["Alice", "Ally"]>; // expands to two triples -> list length 2
// Empty list expansion yields zero triples (warning) – result is an empty [triple] list; binding to single triple is invalid.
emptyLabels: [triple] = <ex:Alice, ex:nothing, []>; // warning: expands to empty list
// Nested lists are invalid and produce a compile error (TRPL006)
// invalid: <ex:Alice, rdfs:label, [["Alice"], "Ally"]>;

// Graph composition
base: graph = <{ <ex:Alice, rdf:type, ex:Person>; }>; // existing assertion block
extended: graph = base + personType + age42; // chaining

// Mutating sugar (desugars to reassignment)
base += age42;
base -= personType;

// Using expansion inline instead of variable binding to a single triple:
// Using expansion inline: when used in a graph addition context, the [triple] result is implicitly folded into a graph union
extended2: graph = base + <ex:Alice, rdfs:label, ["Alice", "Ally"]>;

// Prefix policy: only declared prefixes usable; no implicit resolution.
```

## Build & Test
```
dotnet build fifthlang.sln
(dotnet test test/syntax-parser-tests/)
```
