# Quickstart: System KG Types

## Goal
Use `graph`, `triple`, and `store` without importing `Fifth.System`, backed by system library types.

## Example (Fifth)
```fifth
main(): int {
  g: graph;
  t: triple;
  s: store = sparql_store(<http://example.org/store>);
  g += t;
  return 0;
}
```

## Interop Check (C#)
```csharp
// Receives a Graph from Fifth and asserts underlying interop surface
public static int CheckGraph(object g)
{
    // Should be Fifth.System.Graph and expose/wrap dotNetRDF IGraph
    return 0;
}
```

## Validate
```fish
# Build and run existing suites
just build-all
scripts/validate-examples.fish
 dotnet test test/kg-smoke-tests/kg-smoke-tests.csproj -v minimal
 dotnet test test/runtime-integration-tests/runtime-integration-tests.csproj -v minimal --filter FullyQualifiedName~KG_
```
