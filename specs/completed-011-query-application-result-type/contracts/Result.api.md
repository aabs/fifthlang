# API Contract: Result Type

**Namespace**: Fifth.System  
**Assembly**: fifthlang.system.dll  
**Version**: 1.0.0

## Overview

`Result` is a discriminated union type representing the outcome of SPARQL query application to RDF stores. It provides type-safe access to tabular data (SELECT), graph data (CONSTRUCT/DESCRIBE), or boolean values (ASK) depending on the query form executed.

## Type Signature

```csharp
namespace Fifth.System
{
    [Union]
    public abstract partial class Result
    {
        partial record TabularResult(SparqlResultSet ResultSet) : Result;
        partial record GraphResult(Store GraphStore) : Result;
        partial record BooleanResult(bool Value) : Result;
    }
}
```

## Cases

### TabularResult

**Purpose**: Represents SELECT query results as tabular data with named SPARQL variables and rows.

**Constructor**:
```csharp
public TabularResult(SparqlResultSet resultSet)
```

**Properties**:
```csharp
public SparqlResultSet ResultSet { get; }
public IEnumerable<SparqlResult> Rows { get; }
public IReadOnlyList<string> Variables { get; }
public int RowCount { get; }
```

**Methods**:
```csharp
public T GetValue<T>(int rowIndex, string varName)
public IEnumerable<T> GetColumn<T>(string varName)
public bool TryGetValue<T>(int rowIndex, string varName, out T value)
```

**Example Usage**:
```csharp
// Fifth syntax (after lowering):
result: Result = ?<SELECT ?name ?age WHERE { ... }> <- myStore;

result switch {
    TabularResult t => {
        foreach (var row in t.Rows) {
            var name = row["name"].ToString();
            var age = row["age"].AsInteger();
            std.print($"{name} is {age} years old");
        }
    },
    _ => std.print("Not tabular result")
}
```

**Invariants**:
- `ResultSet` is never null
- `Rows` may be empty but never null
- `Variables` contains unique SPARQL variable names (without '?' prefix)
- `RowCount` returns non-negative integer (may force materialization)

**Performance Characteristics**:
- `Rows` enumeration is lazy (streaming friendly)
- `RowCount` may force full materialization
- `GetColumn<T>()` iterates all rows (O(n))

---

### GraphResult

**Purpose**: Represents CONSTRUCT/DESCRIBE query results as an RDF graph wrapped in a Store.

**Constructor**:
```csharp
public GraphResult(Store graphStore)
```

**Properties**:
```csharp
public Store GraphStore { get; }
public int TripleCount { get; }
```

**Methods**:
```csharp
public string ToTriG()
public IEnumerable<Triple> GetTriples(string? graphName = null)
```

**Example Usage**:
```csharp
// Fifth syntax:
result: Result = ?<CONSTRUCT { ?s ?p ?o } WHERE { ... }> <- myStore;

result switch {
    GraphResult g => {
        var constructedStore = g.GraphStore;
        std.print(g.ToTriG());  // Serialize as TriG
        
        // Further query the constructed graph
        var nextResult = ?<SELECT ?s WHERE { ?s a <Person> }> <- constructedStore;
    },
    _ => std.print("Not graph result")
}
```

**Invariants**:
- `GraphStore` is never null
- `TripleCount` returns non-negative integer (counts across all named graphs)
- `ToTriG()` always produces valid TriG syntax

**Performance Characteristics**:
- `TripleCount` is O(1) if Store maintains count, O(n) otherwise
- `ToTriG()` serialization is O(n) in triple count

---

### BooleanResult

**Purpose**: Represents ASK query results as a boolean truth value.

**Constructor**:
```csharp
public BooleanResult(bool value)
```

**Properties**:
```csharp
public bool Value { get; }
```

**Example Usage**:
```csharp
// Fifth syntax:
result: Result = ?<ASK WHERE { ?s a <Person> }> <- myStore;

result switch {
    BooleanResult b => std.print(if b.Value "Found persons" else "No persons"),
    _ => std.print("Not boolean result")
}
```

**Invariants**:
- `Value` is always true or false (no null state)

**Performance Characteristics**:
- O(1) access, no lazy evaluation

---

## Pattern Matching

Result supports exhaustive pattern matching in Fifth:

```csharp
process_result(result: Result): string {
    result switch {
        TabularResult t => format_table(t.Rows, t.Variables),
        GraphResult g => g.ToTriG(),
        BooleanResult b => b.Value.ToString()
    }
}
```

**Exhaustiveness Checking**: Compiler enforces all 3 cases are handled. Non-exhaustive matches produce compile-time error.

## Error Handling

Result represents **successful** query execution. Failures are communicated via QueryError (separate contract).

```csharp
// Runtime API signature:
public static Result Execute(Query query, Store store, CancellationToken? token = null)
    // Returns Result on success
    // Throws exception with QueryError details on failure
```

## Type Inference

Fifth compiler infers Result type from `<-` operator:

```csharp
// Explicit type annotation (optional):
result: Result = query <- store;

// Inferred type (recommended):
result = query <- store;  // Compiler infers Result type
```

## Serialization

Result types are runtime values; no serialization to/from JSON/TriG is provided in MVP. To persist results:

- **TabularResult**: Iterate `Rows` and serialize manually (e.g., to CSV, JSON array)
- **GraphResult**: Use `ToTriG()` to export RDF graph
- **BooleanResult**: Serialize `Value` as JSON boolean

## Compatibility

- **Minimum .NET Version**: .NET 8.0
- **Dependencies**: VDS.RDF (dotNetRDF) 3.x for SparqlResultSet
- **Breaking Changes**: None (new type introduction)

## Best Practices

1. **Prefer pattern matching over type tests**: Use `switch` expressions instead of `is` checks
2. **Stream tabular results**: Iterate `Rows` lazily; avoid calling `RowCount` unless needed
3. **Handle all cases**: Ensure exhaustive pattern matching to catch unexpected result types
4. **Use try-catch for execution**: Wrap query application in exception handling to catch QueryError details

## Security Considerations

- Result instances are immutable after construction
- No user-controlled code execution in accessor methods
- GraphResult.GraphStore may contain sensitive data; apply access control before exposing

## Testing Strategy

- **Unit Tests**: Mock SparqlResultSet and Store for each case
- **Integration Tests**: Execute real queries against test stores, verify result types
- **Property Tests**: Verify invariants (non-null fields, positive counts, lazy enumeration behavior)

## Future Enhancements

- Pagination API for TabularResult (`GetPage(int pageSize, int pageNumber)`)
- Async enumeration support (`IAsyncEnumerable<SparqlResult>` for Rows)
- Result caching/memoization for repeated access
- Structured export methods (ToJson(), ToCsv() for TabularResult)
